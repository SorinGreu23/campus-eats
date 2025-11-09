using CampusEats.Api.Data;
using CampusEats.Api.Features.Menu;
using FluentValidation;
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

// Map feature endpoints
app.MapMenuEndpoints();

app.Run();

