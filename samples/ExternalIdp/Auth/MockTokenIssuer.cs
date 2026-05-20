using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExternalIdp.Auth;

/// <summary>
/// Simulates an external IDP by issuing JWTs signed with a separate "external" key.
///
/// In a real integration this class would call your IDP's token endpoint
/// (Auth0, Keycloak, Azure AD B2C, OpenIddict, etc.) and return the result.
/// The important contract is the return value: AccessTokenDescriptor contains
/// the token string, a unique JTI that Bedrock uses for revocation tracking,
/// and the expiry time.
/// </summary>
public class MockTokenIssuer(IConfiguration config) : IBedrockTokenIssuer
{
    public Task<AccessTokenDescriptor> IssueAccessTokenAsync(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        string? tenantId,
        IDictionary<string, string>? extraClaims,
        CancellationToken ct = default)
    {
        var signingKey = config["ExternalIdp:SigningKey"]!;
        var issuer     = config["ExternalIdp:Issuer"]!;
        var audience   = config["ExternalIdp:Audience"]!;
        var expiry     = DateTime.UtcNow.AddMinutes(15);
        var jti        = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti,   jti),
            // The "idp" claim lets downstream services identify the issuer
            new("idp", "mock-external-idp")
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        if (tenantId is not null)
            claims.Add(new Claim("tenant_id", tenantId));

        if (extraClaims is not null)
            foreach (var (key, value) in extraClaims)
                claims.Add(new Claim(key, value));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:   issuer,
            audience: audience,
            claims:   claims,
            expires:  expiry,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(new AccessTokenDescriptor(tokenString, jti, expiry));
    }
}
