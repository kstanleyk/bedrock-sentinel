using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Crestacle.Bedrock.AspNetCore.Models;
using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Enumerations;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Crestacle.Bedrock.Core.Options;
using Crestacle.Bedrock.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Crestacle.Bedrock.Tests.Integration.Controllers;

/// <summary>
/// G10 — OTP send rate limiting tests.
/// Uses MaxSendsPerWindow = 3 to keep test execution fast.
/// </summary>
public sealed class OtpRateLimitTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private const int TestMaxSends = 3;
    private const string ValidPassword = "ValidP@ssword1!";

    private readonly BedrockTestServer _server;
    private readonly HttpClient _client;

    public OtpRateLimitTests()
    {
        _server = new BedrockTestServer(
            configureOptions: opts =>
            {
                opts.Otp.MaxSendsPerWindow = TestMaxSends;
                opts.Otp.SendWindow = TimeSpan.FromMinutes(10);
            },
            configureServices: s => s.AddScoped<IOtpService, FakeOtpService>());
        _client = _server.Client;
    }

    [Fact]
    public async Task StepUpInitiate_WithinLimit_Returns200()
    {
        var tokens = await SetUpUserWithMfaAsync("rl-within@example.com");
        SetBearer(tokens.AccessToken);

        for (var i = 0; i < TestMaxSends; i++)
        {
            var response = await _client.PostAsync("/api/bedrock/step-up/initiate", null);
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                because: $"send #{i + 1} is within the limit of {TestMaxSends}");
        }
    }

    [Fact]
    public async Task StepUpInitiate_ExceedsLimit_Returns429WithRetryAfter()
    {
        var tokens = await SetUpUserWithMfaAsync("rl-exceed@example.com");
        SetBearer(tokens.AccessToken);

        for (var i = 0; i < TestMaxSends; i++)
            await _client.PostAsync("/api/bedrock/step-up/initiate", null);

        var response = await _client.PostAsync("/api/bedrock/step-up/initiate", null);

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        response.Headers.Contains("Retry-After").Should().BeTrue();
        var retryAfter = int.Parse(response.Headers.GetValues("Retry-After").First());
        retryAfter.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_ExceedsOtpSendLimit_Returns429()
    {
        var email = "rl-login@example.com";
        await RegisterAndActivateAsync(email);

        // Enable EmailOtp MFA directly on the credential entity
        var credential = _server.DbContext.UserCredentials.First(c => c.Email == email);
        credential.EnableMfa(MfaMethod.EmailOtp);
        await _server.DbContext.SaveChangesAsync();

        // Each login attempt with correct password triggers an OTP send (Login purpose)
        for (var i = 0; i < TestMaxSends; i++)
        {
            var r = await _client.PostAsJsonAsync(
                "/api/bedrock/auth/login",
                new LoginRequest(email, ValidPassword));
            r.StatusCode.Should().Be(HttpStatusCode.OK,
                because: $"login #{i + 1} should succeed within the send limit");
        }

        var response = await _client.PostAsJsonAsync(
            "/api/bedrock/auth/login",
            new LoginRequest(email, ValidPassword));

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task RegisterAndActivateAsync(string email)
    {
        await _client.PostAsJsonAsync(
            "/api/bedrock/auth/register",
            new RegisterRequest(email, ValidPassword));

        var userId = _server.DbContext.UserCredentials.First(c => c.Email == email).UserId;
        var tokenHash = _server.DbContext.EmailVerificationTokens.First(t => t.UserId == userId).TokenHash;

        await _client.PostAsJsonAsync(
            "/api/bedrock/auth/confirm-email",
            new ConfirmEmailRequest(tokenHash));
    }

    private async Task<LoginResponse> SetUpUserWithMfaAsync(string email)
    {
        await RegisterAndActivateAsync(email);

        var credential = _server.DbContext.UserCredentials.First(c => c.Email == email);
        credential.EnableMfa(MfaMethod.EmailOtp);
        await _server.DbContext.SaveChangesAsync();

        var userId = credential.UserId;
        _server.DbContext.RecoveryCodes.Add(
            Crestacle.Bedrock.Core.Entities.RecoveryCode.Create(userId, "fakehash1"));
        _server.DbContext.RecoveryCodes.Add(
            Crestacle.Bedrock.Core.Entities.RecoveryCode.Create(userId, "fakehash2"));
        await _server.DbContext.SaveChangesAsync();

        var loginResp = await _client.PostAsJsonAsync(
            "/api/bedrock/auth/login",
            new LoginRequest(email, ValidPassword));
        var loginBody = await ReadAsync<BedrockResponse<LoginResponse>>(loginResp);
        var challengeToken = loginBody.Data!.ChallengeToken!;

        var mfaResp = await _client.PostAsJsonAsync(
            "/api/bedrock/auth/verify-2fa",
            new VerifyMfaRequest(challengeToken, FakeOtpService.FixedCode));
        var mfaBody = await ReadAsync<BedrockResponse<LoginResponse>>(mfaResp);
        return mfaBody.Data!;
    }

    private void SetBearer(string? token)
        => _client.DefaultRequestHeaders.Authorization =
            token is null ? null : new AuthenticationHeaderValue("Bearer", token);

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
    }

    public void Dispose() => _server.Dispose();
}
