using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Crestacle.Bedrock.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Crestacle.Bedrock.Tests.Integration.Controllers;

/// <summary>
/// G11 — JWT signing key rotation tests.
/// </summary>
public sealed class JwtKeyRotationTests : IDisposable
{
    // Two distinct 32-byte+ HS256 keys for rotation testing.
    private const string OldKey = "Old-Bedrock-Signing-Key-Rotation-32B!";
    private const string NewKey = "New-Bedrock-Signing-Key-Rotation-32B!";

    private readonly BedrockTestServer _server;

    public JwtKeyRotationTests()
    {
        _server = new BedrockTestServer(configureOptions: opts =>
        {
            opts.Jwt.SigningKey = NewKey;
            opts.Jwt.PreviousSigningKey = OldKey;
            opts.Jwt.Issuer = "test";
            opts.Jwt.Audience = "test";
        });
    }

    // -------------------------------------------------------------------------
    // Rotation grace-period tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Token_SignedWithPreviousKey_IsAcceptedDuringGracePeriod()
    {
        var token = IssueTokenWithKey(OldKey, issuer: "test", audience: "test");
        _server.Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _server.Client.GetAsync("/api/bedrock/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "the token was signed with the previous key which is still registered");
    }

    [Fact]
    public async Task Token_SignedWithCurrentKey_IsAccepted()
    {
        var token = IssueTokenWithKey(NewKey, issuer: "test", audience: "test");
        _server.Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _server.Client.GetAsync("/api/bedrock/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "the token was signed with the current primary key");
    }

    [Fact]
    public async Task Token_SignedWithUnknownKey_IsRejected()
    {
        const string unknownKey = "Unknown-Bedrock-Signing-Key-Rotation-32B!";
        var token = IssueTokenWithKey(unknownKey, issuer: "test", audience: "test");
        _server.Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _server.Client.GetAsync("/api/bedrock/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            because: "the key is not registered as either current or previous");
    }

    [Fact]
    public async Task Token_SignedWithOldKey_IsRejectedAfterRotationComplete()
    {
        // Server with ONLY the new key — previous key removed (rotation complete).
        using var serverAfterRotation = new BedrockTestServer(configureOptions: opts =>
        {
            opts.Jwt.SigningKey = NewKey;
            // PreviousSigningKey intentionally absent
            opts.Jwt.Issuer = "test";
            opts.Jwt.Audience = "test";
        });

        var oldToken = IssueTokenWithKey(OldKey, issuer: "test", audience: "test");
        serverAfterRotation.Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", oldToken);

        var response = await serverAfterRotation.Client.GetAsync("/api/bedrock/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            because: "the old key has been retired and is no longer in PreviousSigningKey");
    }

    // -------------------------------------------------------------------------
    // JWKS endpoint tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Jwks_Hs256Configuration_ReturnsEmptyKeySet()
    {
        // The test server uses HS256 (SigningKey only, no certificate).
        var response = await _server.Client.GetAsync("/.well-known/jwks.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("keys").GetArrayLength().Should().Be(0,
            because: "HS256 symmetric keys must not be published in the JWKS");
    }

    [Fact]
    public async Task Jwks_IsAnonymous()
    {
        // No Authorization header — should still return 200.
        _server.Client.DefaultRequestHeaders.Authorization = null;
        var response = await _server.Client.GetAsync("/.well-known/jwks.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private static string IssueTokenWithKey(string key, string issuer, string audience)
    {
        var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
        var userId = Guid.NewGuid();

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("user_id", userId.ToString()),
                new Claim("email", "rotation-test@example.com"),
                new Claim("token_type", "access"),
            },
            notBefore: DateTime.UtcNow.AddSeconds(-5),
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public void Dispose() => _server.Dispose();
}
