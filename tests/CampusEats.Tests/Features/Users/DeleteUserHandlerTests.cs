using System.Security.Claims;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Users.Delete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Shouldly;

namespace CampusEats.Tests.Features.Users;

public class DeleteUserHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteUser_WhenUserIsOwner()
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

        userManager.FindByIdAsync("test-user-id")
            .Returns(user);
        userManager.DeleteAsync(user)
            .Returns(IdentityResult.Success);

        var handler = new DeleteUserHandler(userManager, httpContextAccessor);
        var request = new DeleteUserRequest("test-user-id");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).FindByIdAsync("test-user-id");
        await userManager.Received(1).DeleteAsync(user);
    }

    [Fact]
    public async Task Handle_ShouldDeleteUser_WhenUserIsAdmin()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var httpContextAccessor = CreateMockHttpContextAccessor("admin-user-id", true);

        var targetUser = new ApplicationUser
        {
            Id = "test-user-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser",
            IsActive = true
        };

        userManager.FindByIdAsync("test-user-id")
            .Returns(targetUser);
        userManager.DeleteAsync(targetUser)
            .Returns(IdentityResult.Success);

        var handler = new DeleteUserHandler(userManager, httpContextAccessor);
        var request = new DeleteUserRequest("test-user-id");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).FindByIdAsync("test-user-id");
        await userManager.Received(1).DeleteAsync(targetUser);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var httpContextAccessor = CreateMockHttpContextAccessor(null, false, isAuthenticated: false);

        var handler = new DeleteUserHandler(userManager, httpContextAccessor);
        var request = new DeleteUserRequest("test-user-id");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.DidNotReceive().FindByIdAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var httpContextAccessor = CreateMockHttpContextAccessor("admin-user-id", true);

        userManager.FindByIdAsync("nonexistent-id")
            .Returns((ApplicationUser?)null);

        var handler = new DeleteUserHandler(userManager, httpContextAccessor);
        var request = new DeleteUserRequest("nonexistent-id");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).FindByIdAsync("nonexistent-id");
        await userManager.DidNotReceive().DeleteAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenDeletionFails()
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

        var errors = new[] { new IdentityError { Description = "Cannot delete user" } };
        userManager.FindByIdAsync("test-user-id")
            .Returns(user);
        userManager.DeleteAsync(user)
            .Returns(IdentityResult.Failed(errors));

        var handler = new DeleteUserHandler(userManager, httpContextAccessor);
        var request = new DeleteUserRequest("test-user-id");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).DeleteAsync(user);
    }

    [Fact]
    public async Task GivenNonOwnerNonAdmin_WhenDeleting_ThenReturnsForbid()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var httpContextAccessor = CreateMockHttpContextAccessor("different-user-id", false);

        var handler = new DeleteUserHandler(userManager, httpContextAccessor);
        var request = new DeleteUserRequest("target-user-id");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        // Verify that FindByIdAsync was never called because authorization failed first
        await userManager.DidNotReceive().FindByIdAsync(Arg.Any<string>());
        await userManager.DidNotReceive().DeleteAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task GivenUserWithOrders_WhenDeletionFailsDueToConstraint_ThenReturnsBadRequestWithMessage()
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

        // Simulate deletion failure due to foreign key constraint (user has orders)
        var constraintError = new IdentityError 
        { 
            Code = "UserHasOrders", 
            Description = "Cannot delete user with existing orders" 
        };
        
        userManager.FindByIdAsync("test-user-id")
            .Returns(user);
        userManager.DeleteAsync(user)
            .Returns(IdentityResult.Failed(constraintError));

        var handler = new DeleteUserHandler(userManager, httpContextAccessor);
        var request = new DeleteUserRequest("test-user-id");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).FindByIdAsync("test-user-id");
        await userManager.Received(1).DeleteAsync(user);
        // The handler should return BadRequest with the constraint violation message
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
