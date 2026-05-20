using System.Security.Cryptography.X509Certificates;
using Crestacle.Bedrock.Core.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// Intentionally outside the Crestacle.Bedrock.AspNetCore.Controllers namespace so that
// BedrockRouteConvention does not prepend the configurable base path (e.g. "api/bedrock")
// to this well-known endpoint.
namespace Crestacle.Bedrock.AspNetCore.Discovery;

[ApiController]
[Route(".well-known/jwks.json")]
[AllowAnonymous]
public sealed class BedrockDiscoveryController : ControllerBase
{
    private readonly JwtOptions _jwt;

    public BedrockDiscoveryController(IOptions<BedrockOptions> options)
        => _jwt = options.Value.Jwt;

    /// <summary>
    /// Returns the public JSON Web Key Set (JWKS) for RS256 configurations so that
    /// relying parties can verify tokens without a shared secret.
    /// HS256 configurations return an empty keys array — symmetric keys must not be
    /// published.
    /// </summary>
    [HttpGet]
    public IActionResult GetJwks()
    {
        // HS256 — symmetric key is secret; return an empty key set.
        if (_jwt.SigningCertificate is null && _jwt.PreviousSigningCertificate is null)
            return Ok(new { keys = Array.Empty<object>() });

        var keys = new List<object>();

        if (_jwt.SigningCertificate is not null)
            keys.Add(BuildJwk(_jwt.SigningCertificate));

        if (_jwt.PreviousSigningCertificate is not null)
            keys.Add(BuildJwk(_jwt.PreviousSigningCertificate));

        return Ok(new { keys });
    }

    private static object BuildJwk(X509Certificate2 cert)
    {
        var rsa = cert.GetRSAPublicKey()
            ?? throw new InvalidOperationException("Certificate does not contain an RSA public key.");

        var p = rsa.ExportParameters(includePrivateParameters: false);
        return new
        {
            kty = "RSA",
            use = "sig",
            alg = "RS256",
            kid = cert.Thumbprint,
            n = ToBase64Url(p.Modulus!),
            e = ToBase64Url(p.Exponent!),
        };
    }

    private static string ToBase64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
