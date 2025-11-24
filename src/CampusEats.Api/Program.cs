using CampusEats.Api.Common.Interfaces;
using CampusEats.Api.Common.Services;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Extensions;
using CampusEats.Api.Features.Users;
using CampusEats.Api.Features.Menu;
using CampusEats.Api.Features.Kitchen;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.InventoryReports;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var dbHost = Environment.GetEnvironmentVariable("DB_Host");
var dbPort = Environment.GetEnvironmentVariable("DB_Port");
var dbName = Environment.GetEnvironmentVariable("DB_Name");
var dbUser = Environment.GetEnvironmentVariable("DB_User");
var dbPassword = Environment.GetEnvironmentVariable("DB_Password");

var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

builder.Services.AddDbContext<CampusDbContext>(opt =>
    opt.UseNpgsql(connectionString));

builder.Services.AddDbContext<IdentityDbContext>(opt =>
    opt.UseNpgsql(connectionString));

builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var campusDb = scope.ServiceProvider.GetRequiredService<CampusDbContext>();
    await campusDb.Database.MigrateAsync();
    
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
// Map feature endpoints
app.MapMenuEndpoints();

app.MapGet("/api/menuitems", async (CampusDbContext db) =>
    await db.MenuItems.ToListAsync())
    .WithName("GetMenuItems")
    .WithTags("MenuItems");

app.MapGet("/api/menuitems/{id:guid}", async (CampusDbContext db, Guid id) =>
    await db.MenuItems.FindAsync(id) is MenuItem item ? Results.Ok(item) : Results.NotFound())
    .WithName("GetMenuItemById")
    .WithTags("MenuItems");

app.MapPut("/api/menuitems/{id:guid}", async (CampusDbContext db, Guid id, MenuItem update) =>
{
    var item = await db.MenuItems.FindAsync(id);
    if (item == null) return Results.NotFound();

    item.Name = update.Name;
    item.Description = update.Description;
    item.Price = update.Price;
    item.CategoryId = update.CategoryId;
    item.ImageUrl = update.ImageUrl;
    item.PreparationTimeMinutes = update.PreparationTimeMinutes;
    item.IsAvailable = update.IsAvailable;
    item.Calories = update.Calories;
    item.UpdatedAt = DateTimeOffset.UtcNow;

    await db.SaveChangesAsync();
    return Results.NoContent();
})
    .WithName("UpdateMenuItem")
    .WithTags("MenuItems");

app.MapDelete("/api/menuitems/{id:guid}", async (CampusDbContext db, Guid id) =>
{
    var item = await db.MenuItems.FindAsync(id);
    if (item == null) return Results.NotFound();
    db.MenuItems.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
    .WithName("DeleteMenuItem")
    .WithTags("MenuItems");

// Kitchen endpoints
app.MapKitchenEndpoints();

// Inventory endpoints
app.MapInventoryEndpoints();

app.Run();
