using LexoRank.Core;
using LexoRank.Test.Bucket.Database;
using LexoRank.Test.Bucket.Database.Entities;
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

        var post1 = new Post()
        {
            Id = 1,
            Title = "Post1",
            SortingValue = lexoRankBucketManager.Between(string.Empty, string.Empty),
        };
        var post2 = new Post()
        {
            Id = 2,
            Title = "Post2",
            SortingValue = lexoRankBucketManager.Between(post1.SortingValue, string.Empty),
        };
        var post3 = new Post()
        {
            Id = 3,
            Title = "Post3",
            SortingValue = lexoRankBucketManager.Between(post1.SortingValue, post2.SortingValue),
        };
        var post4 = new Post()
        {
            Id = 4,
            Title = "Post4",
            SortingValue = lexoRankBucketManager.Between(string.Empty, post1.SortingValue),
        };
        await dbContext.Posts.AddRangeAsync(post1, post2, post3, post4);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await dbContext
            .Posts.OrderBy(p => p.SortingValue)
            .ToArrayAsync(TestContext.Current.CancellationToken);
        Assert.Equal(4, result[0].Id);
        Assert.Equal(1, result[1].Id);
        Assert.Equal(3, result[2].Id);
        Assert.Equal(2, result[3].Id);
    }
}
