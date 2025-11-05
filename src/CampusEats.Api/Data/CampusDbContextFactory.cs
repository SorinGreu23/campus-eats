using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CampusEats.Api.Data;

public class CampusDbContextFactory : IDesignTimeDbContextFactory<CampusDbContext>
{
    public CampusDbContext CreateDbContext(string[] args)
    {
        DotNetEnv.Env.Load();
        
        var optionsBuilder = new DbContextOptionsBuilder<CampusDbContext>();
        
        var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
        var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var postgresDb = Environment.GetEnvironmentVariable("POSTGRES_DB");
        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        
        var connectionString = string.IsNullOrEmpty(postgresHost)
            ? "Host=localhost;Port=5432;Database=campuseats;Username=postgres;Password=postgres" // Fallback
            : $"Host={postgresHost};Port={postgresPort};Database={postgresDb};Username={postgresUser};Password={postgresPassword}";
        
        optionsBuilder.UseNpgsql(connectionString);

        return new CampusDbContext(optionsBuilder.Options);
    }
}
