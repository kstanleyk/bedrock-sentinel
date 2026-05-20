using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExternalIdp.Auth;

/// <summary>
/// Simulates an external IDP's token endpoint for development and testing.
///
/// POST /mock-idp/token  →  issues a JWT signed with ExternalIdp:SigningKey
///
/// In a real deployment this controller would not exist — token issuance
/// would be handled by Auth0, Keycloak, Azure AD B2C, or similar. This
/// controller exists purely to make the sample self-contained and runnable
/// without any external dependencies.
/// </summary>
[ApiController]
[Route("mock-idp")]
[AllowAnonymous]
public class MockIdpController(IConfiguration config) : ControllerBase
{
    /// <summary>Issues a JWT for the given subject (any string is accepted in this mock).</summary>
    [HttpPost("token")]
    public IActionResult IssueToken([FromBody] MockTokenRequest request)
    {
        var signingKey = config["ExternalIdp:SigningKey"]!;
        var issuer     = config["ExternalIdp:Issuer"]!;
        var audience   = config["ExternalIdp:Audience"]!;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   request.Subject),
            new Claim(JwtRegisteredClaimNames.Email, request.Email),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim("idp", "mock-external-idp")
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:   issuer,
            audience: audience,
            claims:   claims,
            expires:  DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return Ok(new { access_token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}

public record MockTokenRequest(string Subject, string Email);
