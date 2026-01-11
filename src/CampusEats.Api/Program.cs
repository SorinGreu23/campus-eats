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
using CampusEats.Api.Features.Payments;
using CampusEats.Api.Features.Users;
using DotNetEnv;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Load .env file - search current directory and parent directories up to solution root
var currentDir = Directory.GetCurrentDirectory();
string? envFilePath = null;

while (envFilePath == null && currentDir != null)
{
    var envPath = Path.Combine(currentDir, ".env");
    if (File.Exists(envPath))
    {
        envFilePath = envPath;
        // Load into environment variables for other parts of the app
        Env.Load(envPath, new LoadOptions(setEnvVars: true));
        Console.WriteLine($"Loaded .env from: {envPath}");
    }
    else
    {
        currentDir = Directory.GetParent(currentDir)?.FullName;
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add .env file to configuration if found
if (envFilePath != null)
{
    builder.Configuration.AddInMemoryCollection(
        Env.Load(envFilePath).ToDictionary(kv => kv.Key, kv => kv.Value)!
    );
}

builder.Configuration.AddEnvironmentVariables();
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

builder.Services.AddDbContext<CampusDbContext>(opt => 
    opt.UseNpgsql(connectionString, 
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public")));
        
builder.Services.AddDbContext<IdentityDbContext>(opt => 
    opt.UseNpgsql(connectionString, 
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistoryIdentity", "public")));

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

// Configure Stripe settings from environment variables
builder.Configuration["Stripe:SecretKey"] = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY") 
    ?? builder.Configuration["Stripe:SecretKey"];
builder.Configuration["Stripe:PublishableKey"] = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY") 
    ?? builder.Configuration["Stripe:PublishableKey"];
builder.Configuration["Stripe:WebhookSecret"] = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET") 
    ?? builder.Configuration["Stripe:WebhookSecret"];

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IStripePaymentService, StripePaymentService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var campusDb = scope.ServiceProvider.GetRequiredService<CampusDbContext>();

    // Apply new migrations
    await campusDb.Database.MigrateAsync();

    try
    {
        // Ensure database is migrated first
        await campusDb.Database.MigrateAsync();
        
        await LoyaltyRewardsSeeder.SeedLoyaltyRewards(campusDb);
        await AllergensAndDietaryRestrictionsSeeder.SeedAllergensAndDietaryRestrictions(campusDb);
        await CategoriesSeeder.SeedCategories(campusDb);
        await MenuItemsSeeder.SeedMenuItems(campusDb);
        await InventorySeeder.SeedInventory(campusDb);
        await MenuItemImageUpdater.UpdateMenuItemImages(campusDb);
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
app.MapPaymentsEndpoints();

await app.RunAsync();
