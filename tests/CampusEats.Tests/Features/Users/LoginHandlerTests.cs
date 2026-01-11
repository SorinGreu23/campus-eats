using CampusEats.Api.Common.Interfaces;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Users.Login;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CampusEats.Tests.Features.Users;

public class LoginHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var validator = CreateMockValidator(isValid: true);
        var tokenService = Substitute.For<ITokenService>();

        var user = new ApplicationUser
        {
            Id = "test-user-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser",
            IsActive = true
        };

        userManager.FindByEmailAsync("test@example.com")
            .Returns(user);
        userManager.GetRolesAsync(user)
            .Returns(new List<string> { "Customer" });
        signInManager.CheckPasswordSignInAsync(user, "Password123!", false)
            .Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);
        tokenService.CreateToken(user, Arg.Any<IList<string>>())
            .Returns("mock-token");

        var handler = new LoginHandler(userManager, signInManager, validator, tokenService);
        var request = new LoginRequest("test@example.com", "Password123!");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        tokenService.Received(1).CreateToken(user, Arg.Any<IList<string>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var validator = CreateMockValidator(isValid: false);
        var tokenService = Substitute.For<ITokenService>();

        var handler = new LoginHandler(userManager, signInManager, validator, tokenService);
        var request = new LoginRequest("invalid-email", "weak");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await userManager.DidNotReceive().FindByEmailAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenUserNotFound()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var validator = CreateMockValidator(isValid: true);
        var tokenService = Substitute.For<ITokenService>();

        userManager.FindByEmailAsync(Arg.Any<string>())
            .Returns((ApplicationUser?)null);

        var handler = new LoginHandler(userManager, signInManager, validator, tokenService);
        var request = new LoginRequest("notfound@example.com", "Password123!");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        tokenService.DidNotReceive().CreateToken(Arg.Any<ApplicationUser>(), Arg.Any<IList<string>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenAccountIsInactive()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var validator = CreateMockValidator(isValid: true);
        var tokenService = Substitute.For<ITokenService>();

        var user = new ApplicationUser
        {
            Id = "test-user-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser",
            IsActive = false
        };

        userManager.FindByEmailAsync("test@example.com")
            .Returns(user);

        var handler = new LoginHandler(userManager, signInManager, validator, tokenService);
        var request = new LoginRequest("test@example.com", "Password123!");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await signInManager.DidNotReceive().CheckPasswordSignInAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenPasswordIsIncorrect()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var validator = CreateMockValidator(isValid: true);
        var tokenService = Substitute.For<ITokenService>();

        var user = new ApplicationUser
        {
            Id = "test-user-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser",
            IsActive = true
        };

        userManager.FindByEmailAsync("test@example.com")
            .Returns(user);
        userManager.GetRolesAsync(user)
            .Returns(new List<string> { "Customer" });
        signInManager.CheckPasswordSignInAsync(user, "WrongPassword", false)
            .Returns(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var handler = new LoginHandler(userManager, signInManager, validator, tokenService);
        var request = new LoginRequest("test@example.com", "WrongPassword");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        tokenService.DidNotReceive().CreateToken(Arg.Any<ApplicationUser>(), Arg.Any<IList<string>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenUserHasNoRole()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var validator = CreateMockValidator(isValid: true);
        var tokenService = Substitute.For<ITokenService>();

        var user = new ApplicationUser
        {
            Id = "test-user-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser",
            IsActive = true
        };

        userManager.FindByEmailAsync("test@example.com")
            .Returns(user);
        userManager.GetRolesAsync(user)
            .Returns(new List<string>());

        var handler = new LoginHandler(userManager, signInManager, validator, tokenService);
        var request = new LoginRequest("test@example.com", "Password123!");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await signInManager.DidNotReceive().CheckPasswordSignInAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<bool>());
    }

    private static UserManager<ApplicationUser> CreateMockUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var userManager = Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null
        );
        return userManager;
    }

    private static SignInManager<ApplicationUser> CreateMockSignInManager(UserManager<ApplicationUser> userManager)
    {
        var contextAccessor = Substitute.For<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var signInManager = Substitute.For<SignInManager<ApplicationUser>>(
            userManager, contextAccessor, claimsFactory, null, null, null, null
        );
        return signInManager;
    }

    private static IValidator<LoginRequest> CreateMockValidator(bool isValid = true)
    {
        var validator = Substitute.For<IValidator<LoginRequest>>();
        var validationResult = new ValidationResult();
        
        if (!isValid)
        {
            validationResult.Errors.Add(new ValidationFailure("Email", "Invalid email"));
        }
        
        validator.ValidateAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(validationResult);
        
        return validator;
    }
}
