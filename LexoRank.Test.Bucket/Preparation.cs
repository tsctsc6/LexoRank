using LexoRank.Core;
using LexoRank.Test.Bucket.Database;
using LexoRank.Test.Bucket.Database.Entities;
using Xunit;

namespace LexoRank.Test.Bucket;

public static class Preparation
{
    public static async Task SeedingDataAsync(
        LexoRankBucketManager lexoRankBucketManager,
        MyDbContext dbContext
    )
    {
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
    }
}
