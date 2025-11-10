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

Env.Load();

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "campuseats";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

builder.Services.AddDbContext<CampusDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

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

// --- User endpoints ---
app.MapPost("/api/users/register", async (CreateUserRequest request, IMediator mediator) =>
{
    var result = await mediator.Send(request);
    return result.IsSuccess
        ? Results.Created($"/api/users/{result.Value!.Id}", result.Value)
        : Results.BadRequest(result.Error);
})
    .WithName("RegisterUser")
    .WithTags("Users")
    .WithOpenApi();

app.MapPost("/api/users/login", async (LoginRequest request, IMediator mediator) =>
{
    return await mediator.Send(request) is var result && result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.Unauthorized();
})
    .WithName("LoginUser")
    .WithTags("Users")
    .WithOpenApi();

app.MapGet("/api/users", async (IMediator mediator) =>
{
    return await mediator.Send(new GetUsersRequest()) is var result && result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.BadRequest(result.Error);
})
    .WithName("GetUsers")
    .WithTags("Users")
    .WithOpenApi();

app.MapGet("/api/users/{id:guid}", async (Guid id, IMediator mediator) =>
{
    return await mediator.Send(new GetUserRequest(id)) is var result && result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.NotFound(result.Error);
})
    .WithName("GetUser")
    .WithTags("Users")
    .WithOpenApi();

app.MapPut("/api/users/{id:guid}", async (Guid id, UpdateUserRequest request, IMediator mediator) =>
{
    return await mediator.Send(request with { Id = id }) is var result && result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.BadRequest(result.Error);
})
    .WithName("UpdateUser")
    .WithTags("Users")
    .WithOpenApi();

app.MapDelete("/api/users/{id:guid}", async (Guid id, IMediator mediator) =>
{
    return await mediator.Send(new DeleteUserRequest(id)) is var result && result.IsSuccess
        ? Results.NoContent()
        : Results.BadRequest(result.Error);
})
    .WithName("DeleteUser")
    .WithTags("Users")
    .WithOpenApi();

app.Run();
