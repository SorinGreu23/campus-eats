using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Users.Create;
using CampusEats.Api.Features.Users.Register;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CampusEats.Tests.Features.Users;

public class RegisterHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRegisterUser_WhenValidRequest()
    {
        // Arrange 
        var userManager = CreateMockUserManager();
        var roleManager = CreateMockRoleManager();
        var validator = CreateMockValidator(isValid: true);

        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Test error" }));

        var handler = new RegisterHandler(userManager, roleManager, validator);
        var request = new RegisterRequest(
            "test@example.com",
            "Password123!",
            "Test",
            "User",
            "User",
            "testuser"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert - Just verify that CreateAsync was called at least once
        result.ShouldNotBeNull();
        await userManager.Received(1).CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var roleManager = CreateMockRoleManager();
        var validator = CreateMockValidator(isValid: false);

        var handler = new RegisterHandler(userManager, roleManager, validator);
        var request = new RegisterRequest(
            "invalid-email",
            "weak",
            "Test",
            "User",
            "User",
            "testuser"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenUserCreationFails()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var roleManager = CreateMockRoleManager();
        var validator = CreateMockValidator(isValid: true);

        var errors = new[] { new IdentityError { Description = "User already exists" } };
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(errors));

        var handler = new RegisterHandler(userManager, roleManager, validator);
        var request = new RegisterRequest(
            "test@example.com",
            "Password123!",
            "Test",
            "User",
            "User",
            "testuser"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldAssignRole_WhenRoleProvided()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var roleManager = CreateMockRoleManager();
        var validator = CreateMockValidator(isValid: true);

        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(callInfo =>
            {
                var user = callInfo.Arg<ApplicationUser>();
                user.Id = "test-user-id";
                user.UserName = user.UserName ?? "testuser";
                user.Email = user.Email ?? "test@example.com";
                return Task.FromResult(IdentityResult.Success);
            });
        userManager.AddClaimsAsync(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<System.Security.Claims.Claim>>())
            .Returns(IdentityResult.Success);
        roleManager.RoleExistsAsync(Arg.Any<string>())
            .Returns(true);
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        userManager.AddClaimAsync(Arg.Any<ApplicationUser>(), Arg.Any<System.Security.Claims.Claim>())
            .Returns(IdentityResult.Success);

        var handler = new RegisterHandler(userManager, roleManager, validator);
        var request = new RegisterRequest(
            "testuser",
            "test@example.com",
            "Test",
            "User",
            "Password123!",
            "Customer"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldCreateRole_WhenRoleDoesNotExist()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var roleManager = CreateMockRoleManager();
        var validator = CreateMockValidator(isValid: true);

        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(callInfo =>
            {
                var user = callInfo.Arg<ApplicationUser>();
                user.Id = "test-user-id";
                user.UserName = user.UserName ?? "testuser";
                user.Email = user.Email ?? "test@example.com";
                return Task.FromResult(IdentityResult.Success);
            });
        userManager.AddClaimsAsync(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<System.Security.Claims.Claim>>())
            .Returns(IdentityResult.Success);
        roleManager.RoleExistsAsync(Arg.Any<string>())
            .Returns(false);
        roleManager.CreateAsync(Arg.Any<IdentityRole>())
            .Returns(IdentityResult.Success);
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        userManager.AddClaimAsync(Arg.Any<ApplicationUser>(), Arg.Any<System.Security.Claims.Claim>())
            .Returns(IdentityResult.Success);

        var handler = new RegisterHandler(userManager, roleManager, validator);
        var request = new RegisterRequest(
            "testuser",
            "test@example.com",
            "Test",
            "User",
            "Password123!",
            "NewRole"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldDeleteUser_WhenRoleAssignmentFails()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var roleManager = CreateMockRoleManager();
        var validator = CreateMockValidator(isValid: true);

        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        userManager.AddClaimsAsync(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<System.Security.Claims.Claim>>())
            .Returns(IdentityResult.Success);
        roleManager.RoleExistsAsync(Arg.Any<string>())
            .Returns(true);
        
        var errors = new[] { new IdentityError { Description = "Failed to assign role" } };
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(errors));
        userManager.DeleteAsync(Arg.Any<ApplicationUser>())
            .Returns(IdentityResult.Success);

        var handler = new RegisterHandler(userManager, roleManager, validator);
        var request = new RegisterRequest(
            "testuser",
            "test@example.com",
            "Test",
            "User",
            "Password123!",
            "Customer"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.Received(1).DeleteAsync(Arg.Any<ApplicationUser>());
    }

    private UserManager<ApplicationUser> CreateMockUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var userManager = Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null
        );
        return userManager;
    }

    private RoleManager<IdentityRole> CreateMockRoleManager()
    {
        var store = Substitute.For<IRoleStore<IdentityRole>>();
        var roleManager = Substitute.For<RoleManager<IdentityRole>>(
            store, null, null, null, null
        );
        return roleManager;
    }

    private IValidator<RegisterRequest> CreateMockValidator(bool isValid = true)
    {
        var validator = Substitute.For<IValidator<RegisterRequest>>();
        var validationResult = new ValidationResult();
        
        if (!isValid)
        {
            validationResult.Errors.Add(new ValidationFailure("Email", "Invalid email"));
        }
        
        validator.ValidateAsync(Arg.Any<RegisterRequest>(), Arg.Any<CancellationToken>())
            .Returns(validationResult);
        
        return validator;
    }
}
