using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Crestacle.Bedrock.Core.Options;
using Microsoft.Extensions.Options;

namespace Crestacle.Bedrock.AspNetCore.Services;

/// <summary>
/// Default <see cref="IBedrockTokenIssuer"/> implementation that signs access tokens with
/// Bedrock's own key via <see cref="ITokenService"/>. Used in standalone mode.
/// </summary>
internal sealed class JwtBedrockTokenIssuer : IBedrockTokenIssuer
{
    private readonly ITokenService _tokenService;
    private readonly BedrockOptions _options;

    public JwtBedrockTokenIssuer(ITokenService tokenService, IOptions<BedrockOptions> options)
    {
        _tokenService = tokenService;
        _options = options.Value;
    }

    public Task<AccessTokenDescriptor> IssueAccessTokenAsync(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        string? tenantId,
        IDictionary<string, string>? extraClaims,
        CancellationToken ct = default)
    {
        var token = _tokenService.GenerateAccessToken(userId, email, roles, tenantId, extraClaims);
        var jti = _tokenService.ExtractJti(token);
        var expiresAt = DateTime.UtcNow.Add(_options.Jwt.AccessTokenExpiry);
        return Task.FromResult(new AccessTokenDescriptor(token, jti, expiresAt));
    }
}
