using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LexoRank.Test.Bucket.Nested.Database;

public class MyDbContextDesignTimeFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    private readonly IConfigurationRoot _config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: true)
        .Build();
    

    public MyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseNpgsql(_config["ConnectionStrings:Default"])
            .Options;
        return new MyDbContext(options);
    }
}
