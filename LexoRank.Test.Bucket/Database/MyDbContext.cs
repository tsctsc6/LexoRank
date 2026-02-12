using LexoRank.Test.Bucket.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace LexoRank.Test.Bucket.Database;

public class MyDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Post> Posts { get; set; }
    public DbSet<LexoRankData> LexoRankData { get; set; }
}
