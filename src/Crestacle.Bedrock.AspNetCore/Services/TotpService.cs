using System.Security.Cryptography;
using System.Text;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.AspNetCore.DataProtection;
using OtpNet;

namespace Crestacle.Bedrock.AspNetCore.Services;

public sealed class TotpService : IMfaService
{
    private const string ProtectorPurpose = "Bedrock.Totp.Secret";
    private const string TotpUsedKeyPrefix = "Bedrock:totp-used:";
    private static readonly TimeSpan ReplayWindow = TimeSpan.FromSeconds(90);

    private readonly IDataProtector _protector;
    private readonly IBedrockCache _cache;

    public TotpService(IDataProtectionProvider dataProtectionProvider, IBedrockCache cache)
    {
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        _cache = cache;
    }

    public (string secret, string qrUri) GenerateTotpSetup(string email, string issuer)
    {
        var secretBytes = new byte[20];
        RandomNumberGenerator.Fill(secretBytes);
        var base32Secret = Base32Encoding.ToString(secretBytes);

        var qrUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}" +
                    $"?secret={base32Secret}&issuer={Uri.EscapeDataString(issuer)}&digits=6&period=30";

        return (base32Secret, qrUri);
    }

    public string EncryptSecret(string plainSecret)
        => _protector.Protect(plainSecret);

    public string DecryptSecret(string encryptedSecret)
        => _protector.Unprotect(encryptedSecret);

    public async Task<bool> VerifyTotp(
        string encryptedSecret,
        string code,
        Guid? userId = null,
        CancellationToken ct = default)
    {
        string? cacheKey = userId.HasValue
            ? TotpUsedKeyPrefix + userId.Value + ":" + code
            : null;

        if (cacheKey is not null && await _cache.ExistsAsync(cacheKey, ct))
            return false;

        bool valid;
        try
        {
            var plain = _protector.Unprotect(encryptedSecret);
            var secretBytes = Base32Encoding.ToBytes(plain);
            var totp = new Totp(secretBytes);
            valid = totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
        }
        catch
        {
            return false;
        }

        if (valid && cacheKey is not null)
            await _cache.SetAsync(cacheKey, "1", ReplayWindow, ct);

        return valid;
    }

    public IReadOnlyList<string> GenerateRecoveryCodes(int count = 10)
    {
        var codes = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            var bytes = RandomNumberGenerator.GetBytes(20);
            codes.Add(Convert.ToHexString(bytes).ToLowerInvariant());
        }
        return codes.AsReadOnly();
    }

    public bool VerifyRecoveryCode(string code, string hash)
    {
        var inputHash = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        var storedHash = Convert.FromHexString(hash);
        return CryptographicOperations.FixedTimeEquals(inputHash, storedHash);
    }
}
