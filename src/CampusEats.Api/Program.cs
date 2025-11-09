using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Users.Create;
using CampusEats.Api.Features.Users.Delete;
using CampusEats.Api.Features.Users.Get;
using CampusEats.Api.Features.Users.Login;
using CampusEats.Api.Features.Users.Overview;
using CampusEats.Api.Features.Users.Update;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// Configure DbContext with PostgreSQL
var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "campuseats";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

builder.Services.AddDbContext<CampusDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure Identity with ApplicationUser wrapper
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<CampusDbContext>()
.AddDefaultTokenProviders();

// Add Authorization services
builder.Services.AddAuthorization();

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

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CampusDbContext>();
    await db.Database.MigrateAsync();
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

app.UseAuthentication();
app.UseAuthorization();

// --- User endpoints ---
app.MapPost("/api/users/register", async (CreateUserRequest request, IMediator mediator) =>
{
    try
    {
        var response = await mediator.Send(request);
        return Results.Created($"/api/users/{response.Id}", response);
    }
    catch (ValidationException ex)
    {
        return Results.ValidationProblem(ex.Errors.ToDictionary(
            e => e.PropertyName,
            e => new[] { e.ErrorMessage }));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
    .WithName("RegisterUser")
    .WithTags("Users")
    .WithOpenApi();

app.MapPost("/api/users/login", async (LoginRequest request, IMediator mediator) =>
{
    try
    {
        var response = await mediator.Send(request);
        return Results.Ok(response);
    }
    catch (ValidationException ex)
    {
        return Results.ValidationProblem(ex.Errors.ToDictionary(
            e => e.PropertyName,
            e => new[] { e.ErrorMessage }));
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
})
    .WithName("LoginUser")
    .WithTags("Users")
    .WithOpenApi();

app.MapGet("/api/users", async (IMediator mediator) =>
{
    var response = await mediator.Send(new GetUsersRequest());
    return Results.Ok(response);
})
    .WithName("GetUsers")
    .WithTags("Users")
    .WithOpenApi();

app.MapGet("/api/users/{id:guid}", async (Guid id, IMediator mediator) =>
{
    try
    {
        var response = await mediator.Send(new GetUserRequest(id));
        return Results.Ok(response);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
    .WithName("GetUser")
    .WithTags("Users")
    .WithOpenApi();

app.MapPut("/api/users/{id:guid}", async (Guid id, UpdateUserRequest request, IMediator mediator) =>
{
    try
    {
        if (id != request.Id)
            return Results.BadRequest(new { error = "ID mismatch" });

        var response = await mediator.Send(request);
        return Results.Ok(response);
    }
    catch (ValidationException ex)
    {
        return Results.ValidationProblem(ex.Errors.ToDictionary(
            e => e.PropertyName,
            e => new[] { e.ErrorMessage }));
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
    .WithName("UpdateUser")
    .WithTags("Users")
    .WithOpenApi();

app.MapDelete("/api/users/{id:guid}", async (Guid id, IMediator mediator) =>
{
    try
    {
        await mediator.Send(new DeleteUserRequest(id));
        return Results.NoContent();
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
    .WithName("DeleteUser")
    .WithTags("Users")
    .WithOpenApi();

// --- MenuItems endpoints ---
app.MapGet("/api/menuitems", async (CampusDbContext db) =>
    await db.MenuItems.ToListAsync())
    .WithName("GetMenuItems")
    .WithTags("MenuItems")
    .WithOpenApi();

app.MapGet("/api/menuitems/{id:guid}", async (CampusDbContext db, Guid id) =>
    await db.MenuItems.FindAsync(id) is MenuItem item ? Results.Ok(item) : Results.NotFound())
    .WithName("GetMenuItemById")
    .WithTags("MenuItems")
    .WithOpenApi();

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
    .WithTags("MenuItems")
    .WithOpenApi();

app.MapDelete("/api/menuitems/{id:guid}", async (CampusDbContext db, Guid id) =>
{
    var item = await db.MenuItems.FindAsync(id);
    if (item == null) return Results.NotFound();
    db.MenuItems.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
    .WithName("DeleteMenuItem")
    .WithTags("MenuItems")
    .WithOpenApi();

app.Run();