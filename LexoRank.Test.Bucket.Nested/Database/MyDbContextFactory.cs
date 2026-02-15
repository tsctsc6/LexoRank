using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LexoRank.Test.Bucket.Nested.Database;

public class MyDbContextFactory
{
    private readonly IConfigurationRoot _config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: true)
        .Build();

    public string DbName { get; } = "lexorank_test_" + Guid.NewGuid().ToString("N");

    public MyDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseNpgsql(
                $"{_config["ConnectionStrings:Default"]}Database={DbName}"
                // npgsqlOptions =>
                // {
                //     npgsqlOptions.EnableRetryOnFailure(
                //         maxRetryCount: 3,
                //         maxRetryDelay: TimeSpan.FromMilliseconds(50),
                //         errorCodesToAdd: ["40001"]
                //     );
                // }
            )
            .Options;
        return new MyDbContext(options);
    }
}
