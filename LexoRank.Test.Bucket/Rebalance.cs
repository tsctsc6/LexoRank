using LexoRank.Core;
using LexoRank.Test.Bucket.Database;
using LexoRank.Test.Bucket.Database.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LexoRank.Test.Bucket;

public class Rebalance
{
    [Fact]
    public async Task Normal()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        //await using var connection = new SqliteConnection("DataSource=test.db");
        connection.Open();
        var options = new DbContextOptionsBuilder<MyDbContext>().UseSqlite(connection).Options;
        await using var dbContext = new MyDbContext(options);
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var lexoRankBucketManager = new LexoRankBucketManager(
            CommonCharacterSets.Digits,
            '|',
            ["0", "1", "2"]
        );

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

        // Logically here is while(true)
        for (var i = 0; i < 0xff; i++)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
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

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            await transaction.CommitAsync(TestContext.Current.CancellationToken);
        }

        var result = await dbContext
            .Posts.OrderBy(p => p.SortingValue)
            .ToArrayAsync(TestContext.Current.CancellationToken);
        Assert.Equal(4, result[0].Id);
        Assert.Equal(1, result[1].Id);
        Assert.Equal(3, result[2].Id);
        Assert.Equal(2, result[3].Id);
        foreach (var r in result)
        {
            Assert.StartsWith("1", r.SortingValue);
        }
    }
}
