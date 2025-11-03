using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// Configure DbContext: use InMemory during development or when UseInMemory=true is set in config
var useInMemory = builder.Configuration.GetValue<bool?>("UseInMemory") ?? false;
if (useInMemory || builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<CampusDbContext>(options =>
        options.UseInMemoryDatabase("CampusEatsInMemory"));
}
else
{
    builder.Services.AddDbContext<CampusDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}

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

// If using in-memory DB, seed some sample data to make Swagger/test interactions simpler
if (useInMemory || app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CampusDbContext>();
    // Ensure database is created (for InMemory this is a no-op but keeps parity)
    db.Database.EnsureCreated();

    if (!db.MenuItems.Any())
    {
        db.MenuItems.AddRange(new[]
        {
            new MenuItem { Id = Guid.NewGuid(), Name = "Cheeseburger", Description = "Beef patty with cheese", Price = 6.99M, IsAvailable = true, CreatedAt = DateTimeOffset.UtcNow },
            new MenuItem { Id = Guid.NewGuid(), Name = "Veggie Wrap", Description = "Fresh veggies and hummus", Price = 5.49M, IsAvailable = true, CreatedAt = DateTimeOffset.UtcNow }
        });
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseCors("AllowClientApp");
app.UseHttpsRedirection();

// --- MenuItems endpoints (GET, GET by id, PUT, DELETE) ---
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

    // update allowed fields
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

app.Run();

