namespace Crestacle.Bedrock.Core.Options;

/// <summary>WebAuthn / FIDO2 relying-party configuration.</summary>
public sealed class PasskeyOptions
{
    /// <summary>RP ID — typically the site's effective domain (e.g. <c>example.com</c>).</summary>
    public string ServerDomain { get; set; } = "localhost";

    /// <summary>Human-readable RP display name shown in authenticator dialogs.</summary>
    public string ServerName { get; set; } = "Bedrock";

    /// <summary>Allowed origins. Must include the scheme, e.g. <c>https://example.com</c>.</summary>
    public HashSet<string> Origins { get; set; } = ["https://localhost"];

    /// <summary>Acceptable clock-skew between client and server in milliseconds. Default 5 minutes.</summary>
    public int TimestampDriftToleranceMs { get; set; } = 300_000;
}
