namespace Crestacle.Bedrock.Core.DTOs;

/// <summary>An access token / refresh token pair issued after successful authentication.</summary>
public sealed record TokenPair(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt);
