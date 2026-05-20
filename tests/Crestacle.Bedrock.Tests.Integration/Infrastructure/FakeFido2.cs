using System.Text;
using System.Text.Json;
using Fido2NetLib;
using Fido2NetLib.Objects;

namespace Crestacle.Bedrock.Tests.Integration.Infrastructure;

/// <summary>
/// Stub IFido2 that bypasses all WebAuthn cryptography so integration tests can drive
/// the full passkey registration and authentication flow without a real authenticator.
/// </summary>
internal sealed class FakeFido2 : IFido2
{
    public static readonly byte[] FixedCredentialId = new byte[32];
    public static readonly byte[] FixedPublicKey = new byte[64];

    // All-zeros challenge — predictable for building ClientDataJSON in tests.
    public static readonly byte[] FixedChallenge = new byte[32];

    public static string FixedChallengeBase64Url
        => Convert.ToBase64String(FixedChallenge).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string BuildFakeClientDataJson()
    {
        var obj = new { type = "webauthn.get", challenge = FixedChallengeBase64Url, origin = "https://localhost" };
        return JsonSerializer.Serialize(obj);
    }

    public static string BuildFakeAssertionResponseJson()
    {
        var clientDataBytes = Encoding.UTF8.GetBytes(BuildFakeClientDataJson());
        var response = new AuthenticatorAssertionRawResponse
        {
            RawId = FixedCredentialId,
            Response = new AuthenticatorAssertionRawResponse.AssertionResponse
            {
                ClientDataJson = clientDataBytes,
                AuthenticatorData = new byte[37],
                Signature = new byte[64]
            }
        };
        return JsonSerializer.Serialize(response,
            new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
    }

    public static string BuildFakeAttestationResponseJson()
        => JsonSerializer.Serialize(new AuthenticatorAttestationRawResponse());

    // --- IFido2 ---

    public CredentialCreateOptions RequestNewCredential(RequestNewCredentialParams @params)
        => new CredentialCreateOptions
        {
            Rp = new PublicKeyCredentialRpEntity("Test", "localhost", ""),
            User = @params.User,
            Challenge = FixedChallenge,
            PubKeyCredParams = PubKeyCredParam.Defaults
        };

    public Task<RegisteredPublicKeyCredential> MakeNewCredentialAsync(
        MakeNewCredentialParams @params, CancellationToken cancellationToken = default)
        => Task.FromResult(new RegisteredPublicKeyCredential
        {
            Id = FixedCredentialId,
            PublicKey = FixedPublicKey,
            SignCount = 0,
            IsBackedUp = false
        });

    public Fido2NetLib.AssertionOptions GetAssertionOptions(GetAssertionOptionsParams @params)
        => new Fido2NetLib.AssertionOptions { Challenge = FixedChallenge };

    public Task<VerifyAssertionResult> MakeAssertionAsync(
        MakeAssertionParams @params, CancellationToken cancellationToken = default)
        => Task.FromResult(new VerifyAssertionResult
        {
            CredentialId = FixedCredentialId,
            SignCount = 1
        });
}
