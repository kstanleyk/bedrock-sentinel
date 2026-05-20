using Crestacle.Bedrock.AspNetCore.Services;
using Crestacle.Bedrock.Core.Defaults;
using Crestacle.Bedrock.Core.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OtpNet;
using Xunit;

namespace Crestacle.Bedrock.Tests.Unit.Services;

public sealed class TotpServiceTests
{
    private static TotpService CreateService(IBedrockCache? cache = null)
    {
        var provider = new EphemeralDataProtectionProvider(NullLoggerFactory.Instance);
        return new TotpService(provider, cache ?? new NullBedrockCache());
    }

    // -------------------------------------------------------------------------
    // GenerateTotpSetup
    // -------------------------------------------------------------------------

    [Fact]
    public void GenerateTotpSetup_ReturnsOtpauthQrUri()
    {
        var svc = CreateService();
        var (_, qrUri) = svc.GenerateTotpSetup("user@test.com", "TestIssuer");
        qrUri.Should().StartWith("otpauth://totp/");
    }

    [Fact]
    public void GenerateTotpSetup_QrUriContainsIssuerAndEmail()
    {
        var svc = CreateService();
        var (_, qrUri) = svc.GenerateTotpSetup("user@test.com", "MyApp");
        qrUri.Should().Contain("MyApp");
        qrUri.Should().Contain("user%40test.com");
    }

    [Fact]
    public void GenerateTotpSetup_SecretIsBase32()
    {
        var svc = CreateService();
        var (secret, _) = svc.GenerateTotpSetup("u@test.com", "Issuer");
        // Valid Base32 should decode without throwing
        var act = () => Base32Encoding.ToBytes(secret);
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateTotpSetup_SecretIsDifferentEachCall()
    {
        var svc = CreateService();
        var (s1, _) = svc.GenerateTotpSetup("u@test.com", "X");
        var (s2, _) = svc.GenerateTotpSetup("u@test.com", "X");
        s1.Should().NotBe(s2);
    }

    // -------------------------------------------------------------------------
    // EncryptSecret / DecryptSecret
    // -------------------------------------------------------------------------

    [Fact]
    public void EncryptDecrypt_RoundTrip()
    {
        var svc = CreateService();
        var (secret, _) = svc.GenerateTotpSetup("u@test.com", "X");
        var encrypted = svc.EncryptSecret(secret);
        var decrypted = svc.DecryptSecret(encrypted);
        decrypted.Should().Be(secret);
    }

    [Fact]
    public void EncryptSecret_OutputDiffersFromPlaintext()
    {
        var svc = CreateService();
        var (secret, _) = svc.GenerateTotpSetup("u@test.com", "X");
        var encrypted = svc.EncryptSecret(secret);
        encrypted.Should().NotBe(secret);
    }

    // -------------------------------------------------------------------------
    // VerifyTotp
    // -------------------------------------------------------------------------

    [Fact]
    public async Task VerifyTotp_CurrentCode_ReturnsTrue()
    {
        var svc = CreateService();
        var (secret, _) = svc.GenerateTotpSetup("u@test.com", "X");
        var encrypted = svc.EncryptSecret(secret);

        var currentCode = new Totp(Base32Encoding.ToBytes(secret)).ComputeTotp();

        (await svc.VerifyTotp(encrypted, currentCode)).Should().BeTrue();
    }

    [Fact]
    public async Task VerifyTotp_WrongCode_ReturnsFalse()
    {
        var svc = CreateService();
        var (secret, _) = svc.GenerateTotpSetup("u@test.com", "X");
        var encrypted = svc.EncryptSecret(secret);

        (await svc.VerifyTotp(encrypted, "000000")).Should().BeFalse();
    }

    [Fact]
    public async Task VerifyTotp_InvalidEncryptedSecret_ReturnsFalse()
    {
        var svc = CreateService();
        (await svc.VerifyTotp("not-a-valid-protected-payload", "123456")).Should().BeFalse();
    }

    [Fact]
    public async Task VerifyTotp_FirstValidCode_ReturnsTrueAndSetsCache()
    {
        var cache = Substitute.For<IBedrockCache>();
        cache.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(Task.FromResult(false));

        var svc = CreateService(cache);
        var (secret, _) = svc.GenerateTotpSetup("u@test.com", "X");
        var encrypted = svc.EncryptSecret(secret);
        var code = new Totp(Base32Encoding.ToBytes(secret)).ComputeTotp();
        var userId = Guid.NewGuid();

        var result = await svc.VerifyTotp(encrypted, code, userId);

        result.Should().BeTrue();
        await cache.Received(1).SetAsync(
            $"Bedrock:totp-used:{userId}:{code}",
            "1",
            TimeSpan.FromSeconds(90),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyTotp_ReplayedCode_ReturnsFalseWithoutCallingOtpNet()
    {
        var userId = Guid.NewGuid();
        var code = "123456";

        var cache = Substitute.For<IBedrockCache>();
        cache.ExistsAsync($"Bedrock:totp-used:{userId}:{code}", Arg.Any<CancellationToken>())
             .Returns(Task.FromResult(true)); // code already used

        var svc = CreateService(cache);
        var (secret, _) = svc.GenerateTotpSetup("u@test.com", "X");
        var encrypted = svc.EncryptSecret(secret);

        // A valid code is passed but the cache says it was already used
        var result = await svc.VerifyTotp(encrypted, code, userId);

        result.Should().BeFalse();
        // SetAsync must never be called — OtpNet was never reached
        await cache.DidNotReceive().SetAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------
    // GenerateRecoveryCodes
    // -------------------------------------------------------------------------

    [Fact]
    public void GenerateRecoveryCodes_Returns10ByDefault()
    {
        var svc = CreateService();
        svc.GenerateRecoveryCodes().Should().HaveCount(10);
    }

    [Fact]
    public void GenerateRecoveryCodes_CodesAre40CharHexLowercase()
    {
        var svc = CreateService();
        var codes = svc.GenerateRecoveryCodes(5);
        codes.Should().AllSatisfy(c =>
        {
            c.Should().HaveLength(40);
            c.Should().MatchRegex("^[0-9a-f]+$");
        });
    }

    [Fact]
    public void GenerateRecoveryCodes_CodesAreUnique()
    {
        var svc = CreateService();
        var codes = svc.GenerateRecoveryCodes(10);
        codes.Distinct().Should().HaveCount(10);
    }

    // -------------------------------------------------------------------------
    // VerifyRecoveryCode
    // -------------------------------------------------------------------------

    [Fact]
    public void VerifyRecoveryCode_CorrectCode_ReturnsTrue()
    {
        var svc = CreateService();
        var codes = svc.GenerateRecoveryCodes(1);
        var plainCode = codes[0];

        var hashBytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(plainCode));
        var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        svc.VerifyRecoveryCode(plainCode, hash).Should().BeTrue();
    }

    [Fact]
    public void VerifyRecoveryCode_WrongCode_ReturnsFalse()
    {
        var svc = CreateService();
        var hashBytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes("correct-code"));
        var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        svc.VerifyRecoveryCode("wrong-code", hash).Should().BeFalse();
    }
}
