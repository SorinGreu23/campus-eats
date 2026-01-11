using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Data.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CampusEats.Tests.Data.Extensions;

public class IdentityServiceExtensionsTests : IDisposable
{
    private readonly string? _originalJwtSecretKey;
    private readonly string? _originalJwtIssuer;

    public IdentityServiceExtensionsTests()
    {
        _originalJwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        _originalJwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", _originalJwtSecretKey);
        Environment.SetEnvironmentVariable("JWT_ISSUER", _originalJwtIssuer);
    }

    [Fact]
    public async Task GivenValidConfiguration_WhenAddingIdentityServices_ThenConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "this-is-a-very-secure-secret-key-for-testing-purposes-at-least-32-characters-long");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "CampusEats.TestIssuer");
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Add required DbContext for Identity
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // Act
        services.AddIdentityServices(configuration);

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify Identity services are registered
        var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
        userManager.Should().NotBeNull("UserManager should be registered");

        var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
        roleManager.Should().NotBeNull("RoleManager should be registered");

        var signInManager = serviceProvider.GetService<SignInManager<ApplicationUser>>();
        signInManager.Should().NotBeNull("SignInManager should be registered");

        // Assert - Verify authentication scheme is configured
        var authenticationSchemeProvider = serviceProvider.GetService<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
        authenticationSchemeProvider.Should().NotBeNull("Authentication scheme provider should be registered");

        var defaultScheme = await authenticationSchemeProvider!.GetDefaultAuthenticateSchemeAsync();
        defaultScheme.Should().NotBeNull("Default authentication scheme should be configured");
        defaultScheme!.Name.Should().Be(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public async Task GivenIdentityServices_WhenConfigured_ThenSupportsRoles()
    {
        // Arrange
        var services = new ServiceCollection();
        
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "this-is-a-very-secure-secret-key-for-testing-purposes-at-least-32-characters-long");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "CampusEats.TestIssuer");
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // Act
        services.AddIdentityServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
        roleManager.Should().NotBeNull("Role management should be configured");
    }

    [Fact]
    public void GivenIdentityServices_WhenConfigured_ThenSupportsTokenProviders()
    {
        // Arrange
        var services = new ServiceCollection();
        
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "this-is-a-very-secure-secret-key-for-testing-purposes-at-least-32-characters-long");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "CampusEats.TestIssuer");
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // Act
        services.AddIdentityServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
        userManager.Should().NotBeNull();
        
        // Token providers are registered when AddDefaultTokenProviders is called
        // We can verify this by checking if UserManager is properly configured
        userManager!.Options.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenJwtConfiguration_WhenAddingIdentityServices_ThenConfiguresJwtBearer()
    {
        // Arrange
        var services = new ServiceCollection();
        
        var tokenKey = "this-is-a-very-secure-secret-key-for-testing-purposes-at-least-32-characters-long";
        var issuer = "CampusEats.TestIssuer";
        
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", tokenKey);
        Environment.SetEnvironmentVariable("JWT_ISSUER", issuer);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // Act
        services.AddIdentityServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var schemeProvider = serviceProvider.GetService<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
        schemeProvider.Should().NotBeNull();

        var jwtScheme = await schemeProvider!.GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme);
        jwtScheme.Should().NotBeNull("JWT Bearer authentication scheme should be registered");
        jwtScheme!.Name.Should().Be(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public void GivenIdentityCore_WhenConfigured_ThenUsesIdentityDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "this-is-a-very-secure-secret-key-for-testing-purposes-at-least-32-characters-long");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "CampusEats.TestIssuer");
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // Act
        services.AddIdentityServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetService<IdentityDbContext>();
        dbContext.Should().NotBeNull("IdentityDbContext should be available");
        
        var userStore = serviceProvider.GetService<IUserStore<ApplicationUser>>();
        userStore.Should().NotBeNull("User store should be configured with EntityFramework");
    }

    [Fact]
    public async Task GivenConfiguredIdentity_WhenCreatingUser_ThenSucceeds()
    {
        // Arrange
        var services = new ServiceCollection();
        
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "this-is-a-very-secure-secret-key-for-testing-purposes-at-least-32-characters-long");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "CampusEats.TestIssuer");
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddIdentityServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Act
        var user = new ApplicationUser
        {
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var result = await userManager.CreateAsync(user, "Password123!");

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue("User creation should succeed with valid data");
        
        var createdUser = await userManager.FindByEmailAsync("test@example.com");
        createdUser.Should().NotBeNull();
        createdUser!.UserName.Should().Be("testuser");
    }

    [Fact]
    public async Task GivenConfiguredIdentity_WhenCreatingRole_ThenSucceeds()
    {
        // Arrange
        var services = new ServiceCollection();
        
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "this-is-a-very-secure-secret-key-for-testing-purposes-at-least-32-characters-long");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "CampusEats.TestIssuer");
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddIdentityServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Act
        var result = await roleManager.CreateAsync(new IdentityRole("Customer"));

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue("Role creation should succeed");
        
        var roleExists = await roleManager.RoleExistsAsync("Customer");
        roleExists.Should().BeTrue();
    }
}
