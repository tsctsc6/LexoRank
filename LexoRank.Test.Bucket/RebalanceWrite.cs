using System.Data;
using LexoRank.Core;
using LexoRank.Test.Bucket.Database;
using LexoRank.Test.Bucket.Database.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LexoRank.Test.Bucket;

/// <summary>
/// SqliteConnection does not support nested transactions.
/// So here just an example.
/// </summary>
public class RebalanceWrite : IAsyncLifetime
{
    private SqliteConnection connection;
    private LexoRankBucketManager lexoRankBucketManager;

    public async ValueTask InitializeAsync()
    {
        lexoRankBucketManager = new LexoRankBucketManager(
            CommonCharacterSets.Digits,
            '|',
            ["0", "1", "2"]
        );
        connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Normal()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>().UseSqlite(connection).Options;
        await using var dbContext = new MyDbContext(options);
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        await Preparation.SeedingDataAsync(lexoRankBucketManager, dbContext);

        // Exact values are not needed
        // Here just simply use CountAsync method
        var count = await dbContext.Posts.CountAsync(TestContext.Current.CancellationToken);
        lexoRankBucketManager.BeginRebalance(count, 0.7);

        var state = lexoRankBucketManager.GetState();
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

        var writeThread = new Thread(async () => await WriteThreadAsync());

        // Logically here is while(true)
        for (var i = 0; i < 0xff; i++)
        {
            if (i == 2)
                writeThread.Start();
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                TestContext.Current.CancellationToken
            );
            var post = await dbContext
                .Posts.Where(x => x.SortingValue.StartsWith(lexoRankBucketManager.CurrentBucket))
                .OrderByDescending(x => x.SortingValue)
                .FirstOrDefaultAsync(TestContext.Current.CancellationToken);
            if (post is null)
                break;
            post.SortingValue = lexoRankBucketManager.GetNewLexoRank();
            dbContext.Posts.Update(post);

            lexoRankBucketManager.SubmitNext();
            var newState = lexoRankBucketManager.GetState();
            lexoRankData.LastLexoRankValueNumerator = newState.LastLexoRankValueNumerator;
            dbContext.LexoRankData.Update(lexoRankData);

            await Task.Delay(100, TestContext.Current.CancellationToken);

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            await transaction.CommitAsync(TestContext.Current.CancellationToken);
        }

        writeThread.Join();

        var result = await dbContext
            .Posts.AsNoTracking()
            .OrderBy(p => p.SortingValue)
            .ToArrayAsync(TestContext.Current.CancellationToken);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(4, result[1].Id);
        Assert.Equal(3, result[2].Id);
        Assert.Equal(2, result[3].Id);
        foreach (var r in result)
        {
            Assert.StartsWith("1", r.SortingValue);
        }
    }

    private async Task WriteThreadAsync()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>().UseSqlite(connection).Options;
        await using var dbContext = new MyDbContext(options);
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        await using (
            var transaction = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                TestContext.Current.CancellationToken
            )
        )
        {
            var postToUpdate = await dbContext.Posts.SingleOrDefaultAsync(
                x => x.Id == 1,
                TestContext.Current.CancellationToken
            );
            var post4 = await dbContext
                .Posts.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == 4, TestContext.Current.CancellationToken);

            for (var i = 0; i < 3; i++)
            {
                try
                {
                    postToUpdate.SortingValue = lexoRankBucketManager.Between(
                        string.Empty,
                        post4.SortingValue
                    );
                    break;
                }
                catch (ArgumentException)
                {
                    await Task.Delay(100, TestContext.Current.CancellationToken);
                    if (i == 2)
                        throw;
                }
            }
            dbContext.Posts.Update(postToUpdate);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            await transaction.CommitAsync(TestContext.Current.CancellationToken);
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
