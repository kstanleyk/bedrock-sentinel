using System.IO.Compression;
using Crestacle.Bedrock.AspNetCore.Services;
using Crestacle.Bedrock.Core.Options;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Crestacle.Bedrock.Tests.Unit.Services;

public sealed class CommonPasswordValidatorTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static CommonPasswordValidator Build(string? path)
    {
        var opts = new BedrockOptions
        {
            Password = new PasswordOptions { CommonPasswordDenyListPath = path }
        };
        return new CommonPasswordValidator(Options.Create(opts));
    }

    private static string WritePlainTempFile(IEnumerable<string> passwords)
    {
        var path = Path.GetTempFileName();
        File.WriteAllLines(path, passwords);
        return path;
    }

    private static string WriteGzipTempFile(IEnumerable<string> passwords)
    {
        var path = Path.GetTempFileName() + ".gz";
        using var fileStream = File.Create(path);
        using var gzip = new GZipStream(fileStream, CompressionMode.Compress);
        using var writer = new StreamWriter(gzip);
        foreach (var line in passwords)
            writer.WriteLine(line);
        return path;
    }

    // -------------------------------------------------------------------------
    // Deny-list disabled
    // -------------------------------------------------------------------------

    [Fact]
    public void IsValid_PathNull_PassesEverything()
    {
        var validator = Build(null);
        validator.IsValid("password", out var errors).Should().BeTrue();
        errors.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // Plain-text file
    // -------------------------------------------------------------------------

    [Fact]
    public void IsValid_PasswordInPlainTextFile_ReturnsFalse()
    {
        var path = WritePlainTempFile(["letmein", "monkey", "dragon"]);
        try
        {
            var validator = Build(path);
            var result = validator.IsValid("monkey", out var errors);
            result.Should().BeFalse();
            errors.Should().ContainSingle().Which.Should().Contain("too common");
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void IsValid_PasswordNotInList_ReturnsTrue()
    {
        var path = WritePlainTempFile(["letmein", "monkey"]);
        try
        {
            var validator = Build(path);
            validator.IsValid("Wr@ppedC0met#9x!", out var errors).Should().BeTrue();
            errors.Should().BeEmpty();
        }
        finally { File.Delete(path); }
    }

    // -------------------------------------------------------------------------
    // Gzip file
    // -------------------------------------------------------------------------

    [Fact]
    public void IsValid_PasswordInGzipFile_ReturnsFalse()
    {
        var path = WriteGzipTempFile(["toppassword", "anothercommon"]);
        try
        {
            var validator = Build(path);
            validator.IsValid("toppassword", out var errors).Should().BeFalse();
            errors.Should().ContainSingle().Which.Should().Contain("too common");
        }
        finally { File.Delete(path); }
    }

    // -------------------------------------------------------------------------
    // Case insensitivity
    // -------------------------------------------------------------------------

    [Fact]
    public void IsValid_DenyListMatchIsCaseInsensitive()
    {
        var path = WritePlainTempFile(["password"]);
        try
        {
            var validator = Build(path);
            validator.IsValid("PASSWORD", out _).Should().BeFalse();
            validator.IsValid("Password", out _).Should().BeFalse();
            validator.IsValid("pAsSwOrD", out _).Should().BeFalse();
        }
        finally { File.Delete(path); }
    }

    // -------------------------------------------------------------------------
    // Embedded resource
    // -------------------------------------------------------------------------

    [Fact]
    public void IsValid_EmbeddedSentinel_RejectsCommonPassword()
    {
        var validator = Build("embedded");
        validator.IsValid("password", out var errors).Should().BeFalse();
        errors.Should().ContainSingle().Which.Should().Contain("too common");
    }

    [Fact]
    public void IsValid_EmbeddedSentinel_AcceptsStrongPassword()
    {
        var validator = Build("embedded");
        validator.IsValid("Wr@ppedC0met#9x!", out var errors).Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_EmbeddedSentinel_CaseInsensitiveEmbeddedCheck()
    {
        var validator = Build("embedded");
        // "password" is in the embedded list; "PASSWORD" must also be rejected
        validator.IsValid("PASSWORD", out _).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // IsPreviouslyUsed
    // -------------------------------------------------------------------------

    [Fact]
    public void IsPreviouslyUsed_AlwaysReturnsFalse()
    {
        var validator = Build(null);
        validator.IsPreviouslyUsed("anything", []).Should().BeFalse();
    }
}
