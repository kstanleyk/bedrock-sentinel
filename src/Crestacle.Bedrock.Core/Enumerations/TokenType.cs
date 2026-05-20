namespace Crestacle.Bedrock.Core.Enumerations;

/// <summary>Identifies the intended use of a token via the <c>token_type</c> claim.</summary>
public enum TokenType
{
    /// <summary>Standard API access token with full role claims.</summary>
    Access,

    /// <summary>Opaque refresh token (not a JWT).</summary>
    Refresh,

    /// <summary>Short-lived JWT required to present a 2FA response.</summary>
    Challenge,

    /// <summary>Short-lived JWT that authorises one sensitive operation.</summary>
    StepUp,

    /// <summary>Scope-limited JWT that unlocks only MFA enrollment endpoints.</summary>
    Enrollment
}
