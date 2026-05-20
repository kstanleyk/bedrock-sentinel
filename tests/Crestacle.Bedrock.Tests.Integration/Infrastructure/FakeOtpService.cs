using System.Security.Cryptography;
using System.Text;
using Crestacle.Bedrock.Core.Interfaces.Services;

namespace Crestacle.Bedrock.Tests.Integration.Infrastructure;

/// <summary>Always generates the same predictable code for deterministic test assertions.</summary>
internal sealed class FakeOtpService : IOtpService
{
    public const string FixedCode = "123456";

    public string GenerateCode() => FixedCode;

    public string HashCode(string rawCode)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawCode));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool VerifyCode(string rawCode, string hash)
    {
        var inputHash = SHA256.HashData(Encoding.UTF8.GetBytes(rawCode));
        var storedHash = Convert.FromHexString(hash);
        return CryptographicOperations.FixedTimeEquals(inputHash, storedHash);
    }
}
