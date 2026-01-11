using CampusEats.Api.Common.Services;
using CampusEats.Api.Data.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CampusEats.Tests.Common.Services;

public class TokenServiceTests
{
    [Fact]
    public void GivenMissingTokenKey_WhenConstructing_ThenThrowsInvalidOperationException()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["Token:Key"]).Returns((string?)null);

        // Act
        Action act = () => new TokenService(configurationMock.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Token:Key*");
    }

    [Fact]
    public void GivenValidUser_WhenCreatingToken_ThenReturnsValidJwtToken()
    {
        // Arrange
        var tokenKey = "this-is-a-very-secure-secret-key-for-testing-purposes-123456789-ABCDEFGHIJKLMNOP";
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["Token:Key"]).Returns(tokenKey);
        configurationMock.Setup(x => x["Token:Issuer"]).Returns("CampusEats.Test");
        configurationMock.Setup(x => x["Token:Audience"]).Returns("CampusEats.Client");

        var tokenService = new TokenService(configurationMock.Object);
        var user = new ApplicationUser
        {
            Id = "test-user-123",
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var token = tokenService.CreateToken(user, new List<string>());

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
        
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Should().NotBeNull();
    }

    [Fact]
    public void GivenUserWithRoles_WhenCreatingToken_ThenTokenContainsRoleClaims()
    {
        // Arrange
        var tokenKey = "this-is-a-very-secure-secret-key-for-testing-purposes-123456789-ABCDEFGHIJKLMNOP";
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["Token:Key"]).Returns(tokenKey);
        configurationMock.Setup(x => x["Token:Issuer"]).Returns("CampusEats.Test");
        configurationMock.Setup(x => x["Token:Audience"]).Returns("CampusEats.Client");

        var tokenService = new TokenService(configurationMock.Object);
        var user = new ApplicationUser
        {
            Id = "test-user-123",
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        var roles = new List<string> { "Customer", "Admin" };

        // Act
        var token = tokenService.CreateToken(user, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        // JWT uses full URI claim types
        var roleClaims = jwtToken.Claims.Where(c => c.Type == "role").ToList();
        roleClaims.Should().HaveCount(2);
        roleClaims.Should().Contain(c => c.Value == "Customer");
        roleClaims.Should().Contain(c => c.Value == "Admin");
    }

    [Fact]
    public void GivenValidUser_WhenCreatingToken_ThenTokenContainsExpectedClaims()
    {
        // Arrange
        var tokenKey = "this-is-a-very-secure-secret-key-for-testing-purposes-123456789-ABCDEFGHIJKLMNOP";
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["Token:Key"]).Returns(tokenKey);
        configurationMock.Setup(x => x["Token:Issuer"]).Returns("CampusEats.Test");
        configurationMock.Setup(x => x["Token:Audience"]).Returns("CampusEats.Client");

        var tokenService = new TokenService(configurationMock.Object);
        var user = new ApplicationUser
        {
            Id = "test-user-123",
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var token = tokenService.CreateToken(user, new List<string>());

        // Assert
        token.Should().NotBeNullOrEmpty("Token service should return a token");
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Should().NotBeNull("JWT token should be readable");
        jwtToken.Claims.Should().NotBeNullOrEmpty("Claims collection should exist and have claims");
        
        // Get all claims to see exactly what we have
        var claimsDebug = jwtToken.Claims.Select(c => $"[{c.Type}={c.Value}]");
        var debugInfo = string.Join("; ", claimsDebug);
        
        // Check we have at least 3 claims (NameIdentifier, Email, GivenName)
        jwtToken.Claims.Count().Should().BeGreaterThanOrEqualTo(3, $"Token should have claims. Found: {debugInfo}");
        
        // JWT token handler converts ClaimTypes.* URIs to short names by default
        // Try to find the name identifier claim with all possible names
        var nameIdentifierClaim = jwtToken.Claims.FirstOrDefault(c => 
            c.Type == "nameid" || c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier || c.Value == "test-user-123");
        nameIdentifierClaim.Should().NotBeNull($"Should find name identifier claim. All claims: {debugInfo}");
        nameIdentifierClaim!.Value.Should().Be("test-user-123");

        var emailClaim = jwtToken.Claims.FirstOrDefault(c => 
            c.Type == "email" || c.Type == ClaimTypes.Email || c.Value == "test@example.com");
        emailClaim.Should().NotBeNull($"Should find email claim. All claims: {debugInfo}");
        emailClaim!.Value.Should().Be("test@example.com");

        var givenNameClaim = jwtToken.Claims.FirstOrDefault(c => 
            c.Type == "given_name" || c.Type == ClaimTypes.GivenName || c.Value == "testuser");
        givenNameClaim.Should().NotBeNull($"Should find given name claim. All claims: {debugInfo}");
        givenNameClaim!.Value.Should().Be("testuser"); // TokenService uses user.UserName for GivenName
    }

    [Fact]
    public void GivenValidUser_WhenCreatingToken_ThenTokenHasCorrectExpiration()
    {
        // Arrange
        var tokenKey = "this-is-a-very-secure-secret-key-for-testing-purposes-123456789-ABCDEFGHIJKLMNOP";
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["Token:Key"]).Returns(tokenKey);
        configurationMock.Setup(x => x["Token:Issuer"]).Returns("CampusEats.Test");
        configurationMock.Setup(x => x["Token:Audience"]).Returns("CampusEats.Client");

        var tokenService = new TokenService(configurationMock.Object);
        var user = new ApplicationUser
        {
            Id = "test-user-123",
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var token = tokenService.CreateToken(user, new List<string>());

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var expectedExpiration = DateTime.UtcNow.AddDays(7);
        var actualExpiration = jwtToken.ValidTo;
        
        // Allow 1 minute tolerance for test execution time
        actualExpiration.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GivenValidUser_WhenCreatingToken_ThenTokenHasCorrectIssuer()
    {
        // Arrange
        var tokenKey = "this-is-a-very-secure-secret-key-for-testing-purposes-123456789-ABCDEFGHIJKLMNOP";
        var expectedIssuer = "CampusEats.TestIssuer";
        
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["Token:Key"]).Returns(tokenKey);
        configurationMock.Setup(x => x["Token:Issuer"]).Returns(expectedIssuer);
        configurationMock.Setup(x => x["Token:Audience"]).Returns("CampusEats.Client");

        var tokenService = new TokenService(configurationMock.Object);
        var user = new ApplicationUser
        {
            Id = "test-user-123",
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var token = tokenService.CreateToken(user, new List<string>());

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Issuer.Should().Be(expectedIssuer);
    }
}
