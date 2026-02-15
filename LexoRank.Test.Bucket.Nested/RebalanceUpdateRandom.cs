using System.Data;
using LexoRank.Core;
using LexoRank.Test.Bucket.Nested.Database;
using LexoRank.Test.Bucket.Nested.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Xunit;

namespace LexoRank.Test.Bucket.Nested;

public class RebalanceUpdateRandom : IAsyncLifetime
{
    private readonly LexoRankBucketManager _lexoRankBucketManager = new LexoRankBucketManager(
        CommonCharacterSets.Digits,
        '|',
        ["0", "1", "2"]
    );

    private readonly IConfigurationRoot _config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: true)
        .Build();

    private readonly string _dbName = "lexorank_test_" + Guid.NewGuid().ToString("N");

    public async ValueTask InitializeAsync()
    {
        await using var conn = new NpgsqlConnection(_config["ConnectionStrings:Postgres"]);
        await conn.OpenAsync();
        await new NpgsqlCommand(
            $"CREATE DATABASE {_dbName} WITH TABLESPACE = {_config["VirtualTableSpace"]};",
            conn
        ).ExecuteNonQueryAsync();
        try
        {
            await using var dbContext = new MyDbContext(
                new DbContextOptionsBuilder<MyDbContext>()
                    .UseNpgsql(_config["ConnectionStrings:Default"])
                    .Options
            );
            await dbContext.Database.EnsureCreatedAsync();
        }
        catch (Exception)
        {
            await DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await using var dbContext = new MyDbContext(
            new DbContextOptionsBuilder<MyDbContext>()
                .UseNpgsql($"{_config["ConnectionStrings:Default"]}Database={_dbName}")
                .Options
        );
        await dbContext.Database.EnsureDeletedAsync();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Normal()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseNpgsql($"{_config["ConnectionStrings:Default"]}Database={_dbName}")
            .Options;
        await using var dbContext = new MyDbContext(options);
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        await Preparation.SeedingDataAsync(_lexoRankBucketManager, dbContext);

        // Exact values are not needed
        // Here just simply use CountAsync method
        var count = await dbContext.Posts.CountAsync(TestContext.Current.CancellationToken);
        _lexoRankBucketManager.BeginRebalance(count, 0.7);

        var state = _lexoRankBucketManager.GetState();
        var lexoRankData = new LexoRankData()
        {
            CharacterSet = state.CharacterSet,
            Separator = state.Separator,
            Buckets = state.Buckets,
            CurrentBucket = state.CurrentBucket,
            NextBucket = state.NextBucket,
            IsDesc = state.IsDesc,
            DenominatorBase = state.DenominatorBase,
            DenominatorExponent = state.DenominatorExponent,
            StepSizeNumerator = state.StepSizeNumerator,
            LastLexoRankValueNumerator = state.LastLexoRankValueNumerator,
            IsRebalancing = state.IsRebalancing,
        };
        await dbContext.LexoRankData.AddAsync(lexoRankData, TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var writeThread = Task.CompletedTask;

        // Logically here is while(true)
        for (var i = 0; i < 0xff; i++)
        {
            if (i == 2)
                writeThread = Task.Run(WriteThreadAsync);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.RepeatableRead,
                TestContext.Current.CancellationToken
            );
            var post = await dbContext
                .Posts.Where(x => x.SortingValue.StartsWith(_lexoRankBucketManager.CurrentBucket))
                .OrderByDescending(x => x.SortingValue)
                .FirstOrDefaultAsync(TestContext.Current.CancellationToken);
            if (post is null)
                break;
            post.SortingValue = _lexoRankBucketManager.GetNewLexoRank();
            dbContext.Posts.Update(post);

            _lexoRankBucketManager.SubmitNext();
            var newState = _lexoRankBucketManager.GetState();
            lexoRankData.LastLexoRankValueNumerator = newState.LastLexoRankValueNumerator;
            dbContext.LexoRankData.Update(lexoRankData);

            await Task.Delay(Random.Shared.Next(0, 50), TestContext.Current.CancellationToken);
            try
            {
                await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
                await transaction.CommitAsync(TestContext.Current.CancellationToken);
            }
            catch (InvalidOperationException)
            {
                continue;
            }
        }

        _lexoRankBucketManager.FinishRebalance();
        state = _lexoRankBucketManager.GetState();
        await using var transaction2 = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.RepeatableRead,
            TestContext.Current.CancellationToken
        );
        lexoRankData.IsRebalancing = state.IsRebalancing;
        lexoRankData.CurrentBucket = state.CurrentBucket;
        dbContext.LexoRankData.Update(lexoRankData);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await transaction2.CommitAsync(TestContext.Current.CancellationToken);

        var result = await dbContext
            .Posts.AsNoTracking()
            .OrderBy(p => p.SortingValue)
            .ToArrayAsync(TestContext.Current.CancellationToken);
        foreach (var r in result)
        {
            Assert.StartsWith("1", r.SortingValue);
        }

        await writeThread;
    }

    private async Task WriteThreadAsync()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseNpgsql($"{_config["ConnectionStrings:Default"]}Database={_dbName}")
            .Options;
        await using var dbContext = new MyDbContext(options);
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        for (var i1 = 0; i1 < 3; i1++)
        {
            try
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.RepeatableRead,
                    TestContext.Current.CancellationToken
                );

                var postToUpdate = await dbContext.Posts.SingleAsync(
                    x => x.Id == 1,
                    TestContext.Current.CancellationToken
                );
                var post4 = await dbContext
                    .Posts.AsNoTracking()
                    .SingleAsync(x => x.Id == 4, TestContext.Current.CancellationToken);

                postToUpdate.SortingValue = _lexoRankBucketManager.Between(
                    string.Empty,
                    post4.SortingValue
                );
                dbContext.Posts.Update(postToUpdate);
                await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
                await transaction.CommitAsync(TestContext.Current.CancellationToken);
                break;
            }
            catch (Exception e) when (e is InvalidOperationException or ArgumentException)
            {
                await Task.Delay(Random.Shared.Next(0, 50), TestContext.Current.CancellationToken);
                if (i1 == 2)
                    throw;
            }
        }

        var result = await dbContext
            .Posts.AsNoTracking()
            .OrderBy(p => p.SortingValue)
            .ToArrayAsync(TestContext.Current.CancellationToken);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(4, result[1].Id);
        Assert.Equal(3, result[2].Id);
        Assert.Equal(2, result[3].Id);
    }
}
