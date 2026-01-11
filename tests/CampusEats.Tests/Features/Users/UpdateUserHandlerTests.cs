using System.Security.Claims;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Users.Update;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Shouldly;

namespace CampusEats.Tests.Features.Users;

public class UpdateUserHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateUser_WhenUserIsOwner()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var validator = CreateMockValidator(isValid: true);
        var httpContextAccessor = CreateMockHttpContextAccessor("test-user-id", "test-user-id", false);

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
        userManager.GetUserId(Arg.Any<ClaimsPrincipal>())
            .Returns("test-user-id");
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>())
            .Returns(user);
        userManager.IsInRoleAsync(user, "Admin")
            .Returns(false);
        userManager.UpdateAsync(user)
            .Returns(IdentityResult.Success);

        var handler = new UpdateUserHandler(userManager, validator, httpContextAccessor);
        var request = new UpdateUserRequest(
            "NewFirst",
            "NewLast",
            "newusername",
            null,
            true,
            null,
            null
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).UpdateAsync(user);
        user.FirstName.ShouldBe("NewFirst");
        user.LastName.ShouldBe("NewLast");
    }

    [Fact]
    public async Task Handle_ShouldUpdateUser_WhenUserIsAdmin()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var validator = CreateMockValidator(isValid: true);
        var httpContextAccessor = CreateMockHttpContextAccessor("admin-user-id", "test-user-id", true);

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

        userManager.FindByIdAsync("test-user-id")
            .Returns(targetUser);
        userManager.GetUserId(Arg.Any<ClaimsPrincipal>())
            .Returns("admin-user-id");
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>())
            .Returns(adminUser);
        userManager.IsInRoleAsync(adminUser, "Admin")
            .Returns(true);
        userManager.UpdateAsync(targetUser)
            .Returns(IdentityResult.Success);

        var handler = new UpdateUserHandler(userManager, validator, httpContextAccessor);
        var request = new UpdateUserRequest(
            "NewFirst",
            "NewLast",
            "newusername",
            null,
            true,
            null,
            null
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).UpdateAsync(targetUser);
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var validator = CreateMockValidator(isValid: false);
        var httpContextAccessor = CreateMockHttpContextAccessor("test-user-id", "test-user-id", false);

        var handler = new UpdateUserHandler(userManager, validator, httpContextAccessor);
        var request = new UpdateUserRequest(
            "",
            "",
            "",
            null,
            true,
            null,
            null
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.DidNotReceive().UpdateAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenUserNotFound()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var validator = CreateMockValidator(isValid: true);
        var httpContextAccessor = CreateMockHttpContextAccessor("admin-user-id", "nonexistent-id", true);

        var adminUser = new ApplicationUser
        {
            Id = "admin-user-id",
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            UserName = "admin",
            IsActive = true
        };

        userManager.FindByIdAsync("nonexistent-id")
            .Returns((ApplicationUser?)null);
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>())
            .Returns(adminUser);

        var handler = new UpdateUserHandler(userManager, validator, httpContextAccessor);
        var request = new UpdateUserRequest(
            "NewFirst",
            "NewLast",
            "newusername",
            null,
            true,
            null,
            null
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.DidNotReceive().UpdateAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var validator = CreateMockValidator(isValid: true);
        var httpContextAccessor = CreateMockHttpContextAccessor(null, "test-user-id", false, isAuthenticated: false);

        var handler = new UpdateUserHandler(userManager, validator, httpContextAccessor);
        var request = new UpdateUserRequest(
            "NewFirst",
            "NewLast",
            "newusername",
            null,
            true,
            null,
            null
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.DidNotReceive().UpdateAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task Handle_ShouldReturnForbid_WhenUserIsNotOwnerOrAdmin()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var validator = CreateMockValidator(isValid: true);
        var httpContextAccessor = CreateMockHttpContextAccessor("other-user-id", "test-user-id", false);

        var currentUser = new ApplicationUser
        {
            Id = "other-user-id",
            Email = "other@example.com",
            FirstName = "Other",
            LastName = "User",
            UserName = "otheruser",
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

        userManager.FindByIdAsync("test-user-id")
            .Returns(targetUser);
        userManager.GetUserId(Arg.Any<ClaimsPrincipal>())
            .Returns("other-user-id");
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>())
            .Returns(currentUser);
        userManager.IsInRoleAsync(currentUser, "Admin")
            .Returns(false);

        var handler = new UpdateUserHandler(userManager, validator, httpContextAccessor);
        var request = new UpdateUserRequest(
            "NewFirst",
            "NewLast",
            "newusername",
            null,
            true,
            null,
            null
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.DidNotReceive().UpdateAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateRole_WhenUserIsAdmin()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var validator = CreateMockValidator(isValid: true);
        var httpContextAccessor = CreateMockHttpContextAccessor("admin-user-id", "test-user-id", true);

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

        userManager.FindByIdAsync("test-user-id")
            .Returns(targetUser);
        userManager.GetUserId(Arg.Any<ClaimsPrincipal>())
            .Returns("admin-user-id");
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>())
            .Returns(adminUser);
        userManager.IsInRoleAsync(adminUser, "Admin")
            .Returns(true);
        userManager.GetRolesAsync(targetUser)
            .Returns(new List<string> { "Customer" });
        userManager.RemoveFromRolesAsync(targetUser, Arg.Any<IEnumerable<string>>())
            .Returns(IdentityResult.Success);
        userManager.AddToRoleAsync(targetUser, "Admin")
            .Returns(IdentityResult.Success);
        userManager.UpdateAsync(targetUser)
            .Returns(IdentityResult.Success);

        var handler = new UpdateUserHandler(userManager, validator, httpContextAccessor);
        var request = new UpdateUserRequest(
            "NewFirst",
            "NewLast",
            "newusername",
            "Admin",
            true,
            null,
            null
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).RemoveFromRolesAsync(targetUser, Arg.Any<IEnumerable<string>>());
        await userManager.Received(1).AddToRoleAsync(targetUser, "Admin");
    }

    [Fact]
    public async Task Handle_ShouldReturnForbid_WhenNonAdminTriesToChangeRole()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var validator = CreateMockValidator(isValid: true);
        var httpContextAccessor = CreateMockHttpContextAccessor("test-user-id", "test-user-id", false);

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
        userManager.GetUserId(Arg.Any<ClaimsPrincipal>())
            .Returns("test-user-id");
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>())
            .Returns(user);
        userManager.IsInRoleAsync(user, "Admin")
            .Returns(false);

        var handler = new UpdateUserHandler(userManager, validator, httpContextAccessor);
        var request = new UpdateUserRequest(
            "NewFirst",
            "NewLast",
            "newusername",
            "Admin",
            true,
            null,
            null
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.DidNotReceive().UpdateAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task Handle_ShouldChangePassword_WhenOwnerProvidesCurrentPassword()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var validator = CreateMockValidator(isValid: true);
        var httpContextAccessor = CreateMockHttpContextAccessor("test-user-id", "test-user-id", false);

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
        userManager.GetUserId(Arg.Any<ClaimsPrincipal>())
            .Returns("test-user-id");
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>())
            .Returns(user);
        userManager.IsInRoleAsync(user, "Admin")
            .Returns(false);
        userManager.ChangePasswordAsync(user, "OldPassword123!", "NewPassword123!")
            .Returns(IdentityResult.Success);
        userManager.UpdateAsync(user)
            .Returns(IdentityResult.Success);

        var handler = new UpdateUserHandler(userManager, validator, httpContextAccessor);
        var request = new UpdateUserRequest(
            "NewFirst",
            "NewLast",
            "newusername",
            null,
            true,
            "OldPassword123!",
            "NewPassword123!"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).ChangePasswordAsync(user, "OldPassword123!", "NewPassword123!");
    }

    [Fact]
    public async Task Handle_ShouldResetPassword_WhenAdminChangesPassword()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var validator = CreateMockValidator(isValid: true);
        var httpContextAccessor = CreateMockHttpContextAccessor("admin-user-id", "test-user-id", true);

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

        userManager.FindByIdAsync("test-user-id")
            .Returns(targetUser);
        userManager.GetUserId(Arg.Any<ClaimsPrincipal>())
            .Returns("admin-user-id");
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>())
            .Returns(adminUser);
        userManager.IsInRoleAsync(adminUser, "Admin")
            .Returns(true);
        userManager.RemovePasswordAsync(targetUser)
            .Returns(IdentityResult.Success);
        userManager.AddPasswordAsync(targetUser, "NewPassword123!")
            .Returns(IdentityResult.Success);
        userManager.UpdateAsync(targetUser)
            .Returns(IdentityResult.Success);

        var handler = new UpdateUserHandler(userManager, validator, httpContextAccessor);
        var request = new UpdateUserRequest(
            "NewFirst",
            "NewLast",
            "newusername",
            null,
            true,
            "OldPassword123!",
            "NewPassword123!"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).RemovePasswordAsync(targetUser);
        await userManager.Received(1).AddPasswordAsync(targetUser, "NewPassword123!");
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenOwnerChangesPasswordWithoutCurrentPassword()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var validator = CreateMockValidator(isValid: true);
        var httpContextAccessor = CreateMockHttpContextAccessor("test-user-id", "test-user-id", false);

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
        userManager.GetUserId(Arg.Any<ClaimsPrincipal>())
            .Returns("test-user-id");
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>())
            .Returns(user);
        userManager.IsInRoleAsync(user, "Admin")
            .Returns(false);

        var handler = new UpdateUserHandler(userManager, validator, httpContextAccessor);
        var request = new UpdateUserRequest(
            "NewFirst",
            "NewLast",
            "newusername",
            null,
            true,
            null,
            "NewPassword123!"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.DidNotReceive().ChangePasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>());
    }

    private static UserManager<ApplicationUser> CreateMockUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var userManager = Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null
        );
        return userManager;
    }

    private static IHttpContextAccessor CreateMockHttpContextAccessor(
        string? currentUserId, 
        string routeId, 
        bool isAdmin, 
        bool isAuthenticated = true
    )
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var request = Substitute.For<HttpRequest>();
        var routeValues = new RouteValueDictionary { { "id", routeId } };
        
        request.RouteValues.Returns(routeValues);
        httpContext.Request.Returns(request);
        
        var claimsPrincipal = new ClaimsPrincipal();
        
        if (isAuthenticated && currentUserId != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, currentUserId)
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

    private static IValidator<UpdateUserRequest> CreateMockValidator(bool isValid = true)
    {
        var validator = Substitute.For<IValidator<UpdateUserRequest>>();
        var validationResult = new ValidationResult();
        
        if (!isValid)
        {
            validationResult.Errors.Add(new ValidationFailure("FirstName", "First name is required"));
        }
        
        validator.ValidateAsync(Arg.Any<UpdateUserRequest>(), Arg.Any<CancellationToken>())
            .Returns(validationResult);
        
        return validator;
    }
}
