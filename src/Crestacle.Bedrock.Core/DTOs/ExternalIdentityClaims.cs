namespace Crestacle.Bedrock.Core.DTOs;

/// <summary>Verified identity claims returned by an <c>IExternalIdentityValidator</c>.</summary>
public sealed record ExternalIdentityClaims(
    string ProviderUserId,
    string? Email,
    string? DisplayName);
