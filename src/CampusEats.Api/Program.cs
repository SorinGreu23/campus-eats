using CampusEats.Api.Common.Interfaces;
using CampusEats.Api.Common.Services;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Extensions;
using CampusEats.Api.Features.Allergens;
using CampusEats.Api.Features.DietaryRestrictions;
using CampusEats.Api.Features.Inventory;
using CampusEats.Api.Features.LoyaltyPoints;
using CampusEats.Api.Features.Menu;
using CampusEats.Api.Features.Orders;
using CampusEats.Api.Features.Users;
using DotNetEnv;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Load .env file - check local directory first, then solution root
var localEnvPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(localEnvPath))
{
    Env.Load(localEnvPath);
}
else
{
    var solutionEnvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
    if (File.Exists(solutionEnvPath))
    {
        Env.Load(solutionEnvPath);
    }
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter your JWT token in the format: Bearer {token}",
        }
    );
    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});
builder.Services.AddOpenApi();

var dbHost = Environment.GetEnvironmentVariable("DB_Host");
var dbPort = Environment.GetEnvironmentVariable("DB_Port");
var dbName = Environment.GetEnvironmentVariable("DB_Name");
var dbUser = Environment.GetEnvironmentVariable("DB_User");
var dbPassword = Environment.GetEnvironmentVariable("DB_Password");

var connectionString =
    $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

builder.Services.AddDbContext<CampusDbContext>(opt => opt.UseNpgsql(connectionString));
builder.Services.AddDbContext<IdentityDbContext>(opt => opt.UseNpgsql(connectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:4201", "http://localhost:4202")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var campusDb = scope.ServiceProvider.GetRequiredService<CampusDbContext>();

    // Add missing columns if they don't exist
    try
    {
        await campusDb.Database.ExecuteSqlRawAsync(@"
            DO $$ 
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.columns 
                    WHERE table_name = 'inventory_items' AND column_name = 'UpdatedAt'
                ) THEN
                    ALTER TABLE inventory_items ADD COLUMN ""UpdatedAt"" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;
                END IF;

                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.columns 
                    WHERE table_name = 'inventory_transactions' AND column_name = 'CreatedAt'
                ) THEN
                    ALTER TABLE inventory_transactions ADD COLUMN ""CreatedAt"" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;
                END IF;

                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.columns 
                    WHERE table_name = 'inventory_items' AND column_name = 'IsOutOfStock'
                ) THEN
                    ALTER TABLE inventory_items ADD COLUMN ""IsOutOfStock"" boolean DEFAULT false;
                END IF;
            END $$;
        ");

        // Update IsOutOfStock flag for all items based on current quantity
        await campusDb.Database.ExecuteSqlRawAsync(@"
            UPDATE inventory_items 
            SET ""IsOutOfStock"" = (""CurrentQuantity"" <= 0);
        ");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Column addition warning: {ex.Message}");
    }

    try
    {
        await LoyaltyRewardsSeeder.SeedLoyaltyRewards(campusDb);
        await AllergensAndDietaryRestrictionsSeeder.SeedAllergensAndDietaryRestrictions(campusDb);
        await CategoriesSeeder.SeedCategories(campusDb);
        await MenuItemsSeeder.SeedMenuItems(campusDb);
        await InventorySeeder.SeedInventory(campusDb);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seeder warning: {ex.Message}");
    }

    var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await identityDb.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseCors("AllowClientApp");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapUserEndpoints();
app.MapLoyaltyPointsEndpoints();
app.MapMenuEndpoints();
app.MapAllergenEndpoints();
app.MapDietaryRestrictionEndpoints();
app.MapInventoryEndpoints();
app.MapOrdersEndpoints();

app.Run();
