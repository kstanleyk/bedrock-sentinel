using Crestacle.Bedrock.AspNetCore.Services;
using Crestacle.Bedrock.Core.Interfaces.Services;
using FluentAssertions;
using Xunit;

namespace Crestacle.Bedrock.Tests.Unit.Services;

public sealed class OtpServiceTests
{
    private static IOtpService CreateService() => new OtpService();

    [Fact]
    public void GenerateCode_Returns6CharString()
    {
        var svc = CreateService();
        var code = svc.GenerateCode();
        code.Should().HaveLength(6);
    }

    [Fact]
    public void GenerateCode_IsNumericOnly()
    {
        var svc = CreateService();
        var code = svc.GenerateCode();
        code.Should().MatchRegex("^[0-9]+$");
    }

    [Fact]
    public void GenerateCode_IsZeroPadded()
    {
        // Run enough iterations to have a statistical chance of hitting a leading-zero code
        var svc = CreateService();
        var codes = Enumerable.Range(0, 200).Select(_ => svc.GenerateCode()).ToList();
        codes.Should().AllSatisfy(c => c.Should().HaveLength(6));
    }

    [Fact]
    public void HashCode_ReturnsSha256HexLowercase()
    {
        var svc = CreateService();
        var hash = svc.HashCode("123456");
        hash.Should().HaveLength(64);
        hash.Should().MatchRegex("^[0-9a-f]+$");
    }

    [Fact]
    public void HashCode_SameInput_SameOutput()
    {
        var svc = CreateService();
        svc.HashCode("000001").Should().Be(svc.HashCode("000001"));
    }

    [Fact]
    public void HashCode_DifferentInput_DifferentOutput()
    {
        var svc = CreateService();
        svc.HashCode("000001").Should().NotBe(svc.HashCode("000002"));
    }

    [Fact]
    public void VerifyCode_CorrectCode_ReturnsTrue()
    {
        var svc = CreateService();
        var code = svc.GenerateCode();
        var hash = svc.HashCode(code);
        svc.VerifyCode(code, hash).Should().BeTrue();
    }

    [Fact]
    public void VerifyCode_WrongCode_ReturnsFalse()
    {
        var svc = CreateService();
        var hash = svc.HashCode("123456");
        svc.VerifyCode("654321", hash).Should().BeFalse();
    }

    [Fact]
    public void VerifyCode_EmptyCode_ReturnsFalse()
    {
        var svc = CreateService();
        var hash = svc.HashCode("123456");
        svc.VerifyCode("", hash).Should().BeFalse();
    }
}
