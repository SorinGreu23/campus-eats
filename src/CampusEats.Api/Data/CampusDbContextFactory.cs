using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DotNetEnv;

namespace CampusEats.Api.Data;

public class CampusDbContextFactory : IDesignTimeDbContextFactory<CampusDbContext>
{
    public CampusDbContext CreateDbContext(string[] args)
    {
        Env.Load();

        var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");
        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

        var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

        var optionsBuilder = new DbContextOptionsBuilder<CampusDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new CampusDbContext(optionsBuilder.Options);
    }
}
