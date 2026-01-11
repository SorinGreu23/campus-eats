using CampusEats.Api.Features.Users.Update;
using FluentAssertions;
using Xunit;

namespace CampusEats.Tests.Features.Users;

public class UpdateUserValidatorTests
{
    private readonly UpdateUserValidator _validator = new();

    [Fact]
    public void GivenValidData_ShouldPass()
    {
        var request = new UpdateUserRequest(
            "John",
            "Doe",
            "john.doe",
            "Admin",
            true,
            "CurrentP@ss1",
            "NewP@ssw0rd!"
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GivenUsernameTooShort_ShouldFail()
    {
        var request = new UpdateUserRequest(
            null,
            null,
            "ab",
            null,
            null,
            null,
            null
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserRequest.Username));
    }

    [Fact]
    public void GivenUsernameWithInvalidCharacters_ShouldFail()
    {
        var request = new UpdateUserRequest(
            null,
            null,
            "bad username",
            null,
            null,
            null,
            null
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserRequest.Username));
    }

    [Fact]
    public void GivenNewPasswordWithoutCurrent_ShouldFail()
    {
        var request = new UpdateUserRequest(
            null,
            null,
            null,
            null,
            null,
            null,
            "NewP@ssw0rd!"
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserRequest.CurrentPassword));
    }

    [Fact]
    public void GivenWeakNewPassword_ShouldFail()
    {
        var request = new UpdateUserRequest(
            null,
            null,
            null,
            null,
            null,
            "CurrentP@ss1",
            "lowercase1"
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserRequest.NewPassword));
    }

    [Fact]
    public void GivenEmptyRequest_ShouldPass()
    {
        var request = new UpdateUserRequest(
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
