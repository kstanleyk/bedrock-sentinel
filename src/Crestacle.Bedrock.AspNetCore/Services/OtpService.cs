using System.Security.Cryptography;
using System.Text;
using Crestacle.Bedrock.Core.Interfaces.Services;

namespace Crestacle.Bedrock.AspNetCore.Services;

public sealed class OtpService : IOtpService
{
    public string GenerateCode()
        => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

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
