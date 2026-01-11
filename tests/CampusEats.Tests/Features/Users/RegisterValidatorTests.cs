using CampusEats.Api.Features.Users.Create;
using FluentAssertions;
using Xunit;

namespace CampusEats.Tests.Features.Users;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    [Fact]
    public void GivenValidRequest_ShouldPass()
    {
        var request = new RegisterRequest(
            "user@example.com",
            "StrongP@ssw0rd",
            "John",
            "Doe",
            "Admin",
            "jdoe"
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GivenMissingEmail_ShouldFail()
    {
        var request = new RegisterRequest(
            "",
            "StrongP@ssw0rd",
            "John",
            "Doe",
            "Admin",
            "jdoe"
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Email));
    }

    [Fact]
    public void GivenInvalidEmail_ShouldFail()
    {
        var request = new RegisterRequest(
            "not-an-email",
            "StrongP@ssw0rd",
            "John",
            "Doe",
            "Admin",
            "jdoe"
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GivenWeakPassword_ShouldFail()
    {
        var request = new RegisterRequest(
            "user@example.com",
            "weak",
            "John",
            "Doe",
            "Admin",
            "jdoe"
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void GivenMissingFirstName_ShouldFail()
    {
        var request = new RegisterRequest(
            "user@example.com",
            "StrongP@ssw0rd",
            "",
            "Doe",
            "Admin",
            "jdoe"
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.FirstName));
    }

    [Fact]
    public void GivenMissingLastName_ShouldFail()
    {
        var request = new RegisterRequest(
            "user@example.com",
            "StrongP@ssw0rd",
            "John",
            "",
            "Admin",
            "jdoe"
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.LastName));
    }
}
