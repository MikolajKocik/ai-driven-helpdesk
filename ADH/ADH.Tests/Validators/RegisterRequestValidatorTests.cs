using ADH.Application.DTOs;
using ADH.Application.Validators;
using FluentAssertions;
using Xunit;

namespace ADH.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Username_Is_Empty()
    {
        var request = new RegisterRequest("", "Password123", "Test", "test@test.com");
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        var request = new RegisterRequest("user123", "Short1", "Test", "test@test.com");
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Should_Be_Valid_When_Request_Is_Correct()
    {
        var request = new RegisterRequest("mikołaj", "StrongPass123", "Mikołaj", "mikolaj@test.com");
        var result = _validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }
}
