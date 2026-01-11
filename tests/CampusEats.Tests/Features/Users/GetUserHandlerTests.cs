using System.Security.Claims;
using CampusEats.Api.Common.Interfaces;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Users.Get;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CampusEats.Tests.Features.Users;

public class GetUserHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnUser_WhenUserIsOwner()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var httpContextAccessor = CreateMockHttpContextAccessor("test-user-id", false);

        var user = new ApplicationUser
        {
            Id = "test-user-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser",
            IsActive = true
        };

        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>())
            .Returns(user);
        userManager.IsInRoleAsync(user, "Admin")
            .Returns(false);
        userManager.GetUserId(Arg.Any<ClaimsPrincipal>())
            .Returns("test-user-id");
        userManager.FindByIdAsync("test-user-id")
            .Returns(user);
        userManager.GetRolesAsync(user)
            .Returns(new List<string> { "Customer" });

        var handler = new GetUserHandler(userManager, httpContextAccessor);
        var request = new GetUserRequest("test-user-id");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).FindByIdAsync("test-user-id");
    }

    [Fact]
    public async Task Handle_ShouldReturnUser_WhenUserIsAdmin()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var httpContextAccessor = CreateMockHttpContextAccessor("admin-user-id", true);

        var adminUser = new ApplicationUser
        {
            Id = "admin-user-id",
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            UserName = "admin",
            IsActive = true
        };

        var targetUser = new ApplicationUser
        {
            Id = "test-user-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser",
            IsActive = true
        };

        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>())
            .Returns(adminUser);
        userManager.IsInRoleAsync(adminUser, "Admin")
            .Returns(true);
        userManager.GetUserId(Arg.Any<ClaimsPrincipal>())
            .Returns("admin-user-id");
        userManager.FindByIdAsync("test-user-id")
            .Returns(targetUser);
        userManager.GetRolesAsync(targetUser)
            .Returns(new List<string> { "Customer" });

        var handler = new GetUserHandler(userManager, httpContextAccessor);
        var request = new GetUserRequest("test-user-id");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).FindByIdAsync("test-user-id");
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var httpContextAccessor = CreateMockHttpContextAccessor(null, false, isAuthenticated: false);

        var handler = new GetUserHandler(userManager, httpContextAccessor);
        var request = new GetUserRequest("test-user-id");

        // Act & Assert
        var result = await handler.Handle(request, CancellationToken.None);
        result.ShouldNotBeNull();
        await userManager.DidNotReceive().FindByIdAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var httpContextAccessor = CreateMockHttpContextAccessor("admin-user-id", true);

        var adminUser = new ApplicationUser
        {
            Id = "admin-user-id",
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            UserName = "admin",
            IsActive = true
        };

        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>())
            .Returns(adminUser);
        userManager.IsInRoleAsync(adminUser, "Admin")
            .Returns(true);
        userManager.GetUserId(Arg.Any<ClaimsPrincipal>())
            .Returns("admin-user-id");
        userManager.FindByIdAsync("nonexistent-id")
            .Returns((ApplicationUser?)null);

        var handler = new GetUserHandler(userManager, httpContextAccessor);
        var request = new GetUserRequest("nonexistent-id");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).FindByIdAsync("nonexistent-id");
    }

    [Fact]
    public async Task Handle_ShouldReturnProblem_WhenUserEmailIsMissing()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var httpContextAccessor = CreateMockHttpContextAccessor("test-user-id", false);

        var user = new ApplicationUser
        {
            Id = "test-user-id",
            Email = null,
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser",
            IsActive = true
        };

        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>())
            .Returns(user);
        userManager.IsInRoleAsync(user, "Admin")
            .Returns(false);
        userManager.GetUserId(Arg.Any<ClaimsPrincipal>())
            .Returns("test-user-id");
        userManager.FindByIdAsync("test-user-id")
            .Returns(user);

        var handler = new GetUserHandler(userManager, httpContextAccessor);
        var request = new GetUserRequest("test-user-id");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).FindByIdAsync("test-user-id");
    }

    [Fact]
    public async Task Handle_ShouldReturnProblem_WhenUserRoleIsMissing()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var httpContextAccessor = CreateMockHttpContextAccessor("test-user-id", false);

        var user = new ApplicationUser
        {
            Id = "test-user-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser",
            IsActive = true
        };

        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>())
            .Returns(user);
        userManager.IsInRoleAsync(user, "Admin")
            .Returns(false);
        userManager.GetUserId(Arg.Any<ClaimsPrincipal>())
            .Returns("test-user-id");
        userManager.FindByIdAsync("test-user-id")
            .Returns(user);
        userManager.GetRolesAsync(user)
            .Returns(new List<string>());

        var handler = new GetUserHandler(userManager, httpContextAccessor);
        var request = new GetUserRequest("test-user-id");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).FindByIdAsync("test-user-id");
    }

    private static UserManager<ApplicationUser> CreateMockUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var userManager = Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null
        );
        return userManager;
    }

    private static IHttpContextAccessor CreateMockHttpContextAccessor(string? userId, bool isAdmin, bool isAuthenticated = true)
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claimsPrincipal = new ClaimsPrincipal();
        
        if (isAuthenticated && userId != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            
            if (isAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
            
            var identity = new ClaimsIdentity(claims, "TestAuthentication");
            claimsPrincipal = new ClaimsPrincipal(identity);
        }
        else if (isAuthenticated)
        {
            var identity = new ClaimsIdentity(new List<Claim>(), "TestAuthentication");
            claimsPrincipal = new ClaimsPrincipal(identity);
        }

        httpContext.User.Returns(claimsPrincipal);
        httpContextAccessor.HttpContext.Returns(httpContext);
        
        return httpContextAccessor;
    }
}
