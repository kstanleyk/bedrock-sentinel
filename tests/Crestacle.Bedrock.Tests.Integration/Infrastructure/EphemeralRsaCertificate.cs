using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Crestacle.Bedrock.Tests.Integration.Infrastructure;

internal static class EphemeralRsaCertificate
{
    /// <summary>
    /// Generates a self-signed RSA-2048 certificate valid for 1 hour. The returned
    /// <see cref="X509Certificate2"/> includes the private key and is fully self-contained —
    /// it does not share a lifetime with the underlying <see cref="RSA"/> key object.
    /// </summary>
    internal static X509Certificate2 Create()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=Bedrock-Test-RS256",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        using var temp = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddMinutes(-5),
            DateTimeOffset.UtcNow.AddHours(1));

        // Export as PFX and reimport so the certificate lifetime is independent of rsa above.
        // X509Certificate2(byte[]) is obsolete in .NET 9+ (SYSLIB0057); use X509CertificateLoader there.
#if NET9_0_OR_GREATER
        return X509CertificateLoader.LoadPkcs12(temp.Export(X509ContentType.Pfx), password: null);
#else
        return new X509Certificate2(temp.Export(X509ContentType.Pfx));
#endif
    }
}
