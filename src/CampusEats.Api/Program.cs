using CampusEats.Api.Common.Interfaces;
using CampusEats.Api.Common.Services;
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

// --- User endpoints ---
app.MapPost("/api/users/register", async (RegisterRequest request, IMediator mediator) =>
        await mediator.Send(request))
    .WithName("RegisterUser")
    .WithTags("Users")
    .WithOpenApi();

app.MapPost("/api/users/login", async (LoginRequest request, IMediator mediator) =>
        await mediator.Send(request))
    .WithName("LoginUser")
    .WithTags("Users")
    .WithOpenApi();

app.MapGet("/api/users", async (IMediator mediator) =>
        await mediator.Send(new GetUsersRequest()))
    .WithName("GetUsers")
    .WithTags("Users")
    .WithOpenApi();

app.MapGet("/api/users/{id}", async (string id, IMediator mediator) =>
        await mediator.Send(new GetUserRequest(id)))
    .WithName("GetUser")
    .WithTags("Users")
    .WithOpenApi();

app.MapPut("/api/users/{id}", async (string id, UpdateUserRequest request, IMediator mediator) =>
        await mediator.Send(request with { Id = id }))
    .WithName("UpdateUser")
    .WithTags("Users")
    .WithOpenApi();

app.MapDelete("/api/users/{id}", async (string id, IMediator mediator) =>
        await mediator.Send(new DeleteUserRequest(id)))
    .WithName("DeleteUser")
    .WithTags("Users")
    .WithOpenApi();

app.Run();
