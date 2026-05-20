using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Crestacle.Bedrock.AspNetCore.Models;
using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Crestacle.Bedrock.Tests.Integration.Controllers;

/// <summary>
/// G3 — RS256 signing path integration tests.
/// Exercises <c>BedrockOptions.Jwt.SigningCertificate</c> end-to-end via an ephemeral
/// self-signed RSA-2048 certificate.
/// </summary>
public sealed class Rs256Tests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly X509Certificate2 _cert;
    private readonly BedrockTestServer _server;
    private readonly HttpClient _client;

    private const string ValidPassword = "ValidP@ssword1!";

    public Rs256Tests()
    {
        _cert = EphemeralRsaCertificate.Create();
        _server = new BedrockTestServer(configureOptions: opts =>
        {
            opts.Jwt.SigningKey = null;           // disable HS256
            opts.Jwt.SigningCertificate = _cert;  // enable RS256
        });
        _client = _server.Client;
    }

    // -------------------------------------------------------------------------
    // Full end-to-end RS256 flow
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Login_WithSigningCertificate_AccessTokenHasRS256AlgHeader()
    {
        const string email = "rs256-alg@example.com";
        await RegisterAndActivateAsync(email);

        var loginResp = await _client.PostAsJsonAsync(
            "/api/bedrock/auth/login",
            new LoginRequest(email, ValidPassword));

        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadAsync<BedrockResponse<LoginResponse>>(loginResp);
        var accessToken = body.Data!.AccessToken!;

        // Assert the JOSE header carries RS256.
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        parsed.Header.Alg.Should().Be(SecurityAlgorithms.RsaSha256,
            because: "JwtService must use RS256 when SigningCertificate is configured");

        // Assert the token is accepted on a protected endpoint.
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
        var sessionsResp = await _client.GetAsync("/api/bedrock/sessions");
        _client.DefaultRequestHeaders.Authorization = null;

        sessionsResp.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "the JWT bearer middleware must accept tokens signed with the configured RS256 certificate");
    }

    // -------------------------------------------------------------------------
    // Public-key-only validation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Login_WithSigningCertificate_TokenValidatesAgainstPublicKeyOnly()
    {
        const string email = "rs256-pubkey@example.com";
        await RegisterAndActivateAsync(email);

        var loginResp = await _client.PostAsJsonAsync(
            "/api/bedrock/auth/login",
            new LoginRequest(email, ValidPassword));
        var body = await ReadAsync<BedrockResponse<LoginResponse>>(loginResp);
        var accessToken = body.Data!.AccessToken!;

        // _cert.RawData is DER-encoded public certificate only (no private key).
        // This proves the token was signed with the matching private key.
        var publicCert = X509CertificateLoader.LoadCertificate(_cert.RawData);
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new X509SecurityKey(publicCert),
            ValidateIssuer = true,
            ValidIssuer = "test",
            ValidateAudience = true,
            ValidAudience = "test",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "user_id",
            RoleClaimType = ClaimTypes.Role,
        };

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var act = () => handler.ValidateToken(accessToken, validationParams, out _);
        act.Should().NotThrow(
            because: "a token signed with the private key must validate against the corresponding public key");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task RegisterAndActivateAsync(string email)
    {
        await _client.PostAsJsonAsync("/api/bedrock/auth/register",
            new RegisterRequest(email, ValidPassword));
        var userId = _server.DbContext.UserCredentials.First(c => c.Email == email).UserId;
        var tokenHash = _server.DbContext.EmailVerificationTokens
            .First(t => t.UserId == userId).TokenHash;
        await _client.PostAsJsonAsync("/api/bedrock/auth/confirm-email",
            new ConfirmEmailRequest(tokenHash));
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
    }

    public void Dispose()
    {
        _server.Dispose();
        _cert.Dispose();
    }
}
