using LexoRank.Core;
using LexoRank.Test.Bucket.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LexoRank.Test.Bucket;

public class InsertData
{
    [Fact]
    public async Task Normal()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
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

        var result = await dbContext
            .Posts.OrderBy(p => p.SortingValue)
            .ToArrayAsync(TestContext.Current.CancellationToken);
        Assert.Equal(4, result[0].Id);
        Assert.Equal(1, result[1].Id);
        Assert.Equal(3, result[2].Id);
        Assert.Equal(2, result[3].Id);
    }
}
