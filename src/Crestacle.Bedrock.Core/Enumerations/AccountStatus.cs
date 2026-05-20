namespace Crestacle.Bedrock.Core.Enumerations;

/// <summary>Lifecycle status of a user credential record.</summary>
public enum AccountStatus
{
    /// <summary>Registered but email address not yet confirmed.</summary>
    PendingVerification,

    /// <summary>Fully active; authentication is permitted.</summary>
    Active,

    /// <summary>Temporarily locked due to repeated failed login attempts.</summary>
    Locked,

    /// <summary>Administratively suspended; authentication is blocked.</summary>
    Suspended
}
