using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Kitchen;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? builder.Configuration["POSTGRES_HOST"];
var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? builder.Configuration["POSTGRES_PORT"];
var postgresDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? builder.Configuration["POSTGRES_DB"];
var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? builder.Configuration["POSTGRES_USER"];
var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? builder.Configuration["POSTGRES_PASSWORD"];

var connectionString = string.IsNullOrEmpty(postgresHost)
    ? builder.Configuration.GetConnectionString("DefaultConnection")
    : $"Host={postgresHost};Port={postgresPort};Database={postgresDb};Username={postgresUser};Password={postgresPassword}";

builder.Services.AddDbContext<CampusDbContext>(options =>
    options.UseNpgsql(connectionString));

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseCors("AllowClientApp");
app.UseHttpsRedirection();

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
app.MapGet("/api/kitchen/pending-orders", async (IMediator mediator) =>
    await mediator.Send(new GetPendingOrdersQuery()))
    .WithName("GetPendingOrders")
    .WithTags("Kitchen")
    .WithDescription("Returns all orders that are in Pending or Preparing status")
    .Produces<List<PendingOrderDto>>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest);

app.MapPut("/api/kitchen/orders/{id:guid}/status", async (Guid id, UpdateOrderStatusRequest request, IMediator mediator) =>
{
    var status = Enum.TryParse<OrderStatus>(request.Status, out var orderStatus) 
        ? orderStatus 
        : OrderStatus.Pending;
        
    var command = new UpdateOrderStatusCommand(id, status);
    return await mediator.Send(command);
})
    .WithName("UpdateOrderStatus")
    .WithTags("Kitchen")
    .WithDescription("Updates the status of an order. Valid transitions: Pending → Preparing → Ready → Completed")
    .Produces(StatusCodes.Status204NoContent)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.Run();

public record UpdateOrderStatusRequest(string Status);

