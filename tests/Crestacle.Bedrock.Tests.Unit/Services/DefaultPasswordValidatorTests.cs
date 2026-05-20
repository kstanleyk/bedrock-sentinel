using Crestacle.Bedrock.AspNetCore.Services;
using Crestacle.Bedrock.Core.Options;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using IPasswordHasher = Crestacle.Bedrock.Core.Interfaces.Services.IPasswordHasher;

namespace Crestacle.Bedrock.Tests.Unit.Services;

public sealed class DefaultPasswordValidatorTests
{
    private static DefaultPasswordValidator BuildValidator(PasswordOptions? opts = null)
    {
        var options = new BedrockOptions { Password = opts ?? new PasswordOptions() };
        var wrapped = Options.Create(options);
        var hasher = new Argon2idPasswordHasher();
        var commonValidator = new CommonPasswordValidator(wrapped);
        return new DefaultPasswordValidator(wrapped, hasher, commonValidator);
    }

    [Fact]
    public void IsValid_StrongPassword_ReturnsTrue()
    {
        var validator = BuildValidator();
        var valid = validator.IsValid("StrongP@ssw0rd!", out var errors);
        valid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_TooShort_ReturnsFalse()
    {
        var validator = BuildValidator(new PasswordOptions { MinLength = 12 });
        var valid = validator.IsValid("Short1!", out var errors);
        valid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("12 characters"));
    }

    [Fact]
    public void IsValid_NoUppercase_ReturnsFalse()
    {
        var validator = BuildValidator();
        var valid = validator.IsValid("nouppercase1!", out var errors);
        valid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("uppercase"));
    }

    [Fact]
    public void IsValid_NoLowercase_ReturnsFalse()
    {
        var validator = BuildValidator();
        var valid = validator.IsValid("NOLOWERCASE1!", out var errors);
        valid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("lowercase"));
    }

    [Fact]
    public void IsValid_NoDigit_ReturnsFalse()
    {
        var validator = BuildValidator();
        var valid = validator.IsValid("NoDigitPass!", out var errors);
        valid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("digit"));
    }

    [Fact]
    public void IsValid_NoSpecialChar_ReturnsFalse()
    {
        var validator = BuildValidator();
        var valid = validator.IsValid("NoSpecialChar1", out var errors);
        valid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("special"));
    }

    [Fact]
    public void IsValid_MultipleViolations_ReportsAll()
    {
        var validator = BuildValidator();
        var valid = validator.IsValid("short", out var errors);
        valid.Should().BeFalse();
        errors.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public void IsPreviouslyUsed_MatchingHash_ReturnsTrue()
    {
        var hasher = new Argon2idPasswordHasher();
        var hash = hasher.Hash("Password1!");
        var validator = BuildValidator();
        validator.IsPreviouslyUsed("Password1!", [hash]).Should().BeTrue();
    }

    [Fact]
    public void IsPreviouslyUsed_NoMatch_ReturnsFalse()
    {
        var hasher = new Argon2idPasswordHasher();
        var hash = hasher.Hash("DifferentP@ss1");
        var validator = BuildValidator();
        validator.IsPreviouslyUsed("Password1!", [hash]).Should().BeFalse();
    }

    [Fact]
    public void IsPreviouslyUsed_EmptyHistory_ReturnsFalse()
    {
        var validator = BuildValidator();
        validator.IsPreviouslyUsed("Password1!", []).Should().BeFalse();
    }
}
