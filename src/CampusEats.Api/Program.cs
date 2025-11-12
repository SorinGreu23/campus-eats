using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Data.Extensions;
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
app.MapPost("/api/users/register", async (RegisterRequest request, IMediator mediator) =>
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
    var result = await mediator.Send(request);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
}); // NO .RequireAuthorization() here!

app.MapGet("/api/users", async (IMediator mediator) =>
{
    var result = await mediator.Send(new GetUsersRequest());
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
}).RequireAuthorization(); // This one should have authorization

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
