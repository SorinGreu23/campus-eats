using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CampusEats.Api.Data;

public class CampusDbContextFactory : IDesignTimeDbContextFactory<CampusDbContext>
{
    public CampusDbContext CreateDbContext(string[] args)
    {
        DotNetEnv.Env.Load();
        
        var optionsBuilder = new DbContextOptionsBuilder<CampusDbContext>();
        
        var postgresHost = Environment.GetEnvironmentVariable("DB_Host");
        var postgresPort = Environment.GetEnvironmentVariable("DB_Port") ?? "5432";
        var postgresDb = Environment.GetEnvironmentVariable("DB_Name");
        var postgresUser = Environment.GetEnvironmentVariable("DB_User");
        var postgresPassword = Environment.GetEnvironmentVariable("DB_Password");
        
        var connectionString = string.IsNullOrEmpty(postgresHost)
            ? "Host=localhost;Port=5432;Database=campuseats;Username=postgres;Password=postgres" // Fallback
            : $"Host={postgresHost};Port={postgresPort};Database={postgresDb};Username={postgresUser};Password={postgresPassword}";
        
        optionsBuilder.UseNpgsql(connectionString);

        return new CampusDbContext(optionsBuilder.Options);
    }
}
