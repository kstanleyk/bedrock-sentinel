using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Crestacle.Bedrock.AspNetCore.Models;
using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Enumerations;
using Crestacle.Bedrock.Tests.Integration.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Crestacle.Bedrock.Tests.Integration.Controllers;

public sealed class AccountControllerTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly BedrockTestServer _server;
    private readonly HttpClient _client;

    private const string ValidPassword = "ValidP@ssword1!";

    public AccountControllerTests()
    {
        _server = new BedrockTestServer();
        _client = _server.Client;
    }

    // -------------------------------------------------------------------------
    // DELETE /api/bedrock/account
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAccount_Authenticated_AnonymizesCredentialAndSubsequentLoginFails()
    {
        var email = "account-delete@example.com";
        await RegisterAndActivateAsync(email);
        var tokens = await LoginAsync(email);

        var userId = _server.DbContext.UserCredentials.First(c => c.Email == email).UserId;

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var deleteResponse = await _client.DeleteAsync("/api/bedrock/account");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _client.DefaultRequestHeaders.Authorization = null;

        // Subsequent login must fail — credential is anonymized
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/bedrock/auth/login",
            new LoginRequest(email, ValidPassword));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Audit rows must survive credential anonymization (no FK cascade)
        _server.DbContext.AuditEntries
            .Any(a => a.UserId == userId && a.EventType == AuditEventType.AccountAnonymized)
            .Should().BeTrue(because: "the AccountAnonymized audit entry must persist after the credential is scrubbed");

        _server.DbContext.AuditEntries
            .Count(a => a.UserId == userId)
            .Should().BeGreaterThan(1,
                because: "login and anonymization events must both survive the anonymization");
    }

    [Fact]
    public async Task DeleteAccount_Unauthenticated_Returns401()
    {
        var response = await _client.DeleteAsync("/api/bedrock/account");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -------------------------------------------------------------------------
    // DELETE /api/bedrock/admin/users/{userId}/anonymize
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AdminAnonymize_WithoutAdminClaim_Returns403()
    {
        var email = "admin-anon-noadmin@example.com";
        await RegisterAndActivateAsync(email);
        var tokens = await LoginAsync(email);

        var userId = _server.DbContext.UserCredentials.First(c => c.Email == email).UserId;

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await _client.DeleteAsync($"/api/bedrock/admin/users/{userId}/anonymize");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    // -------------------------------------------------------------------------
    // POST /api/bedrock/account/consent  &  GET /api/bedrock/account/consent
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RecordConsent_ThenGetHistory_ContainsExpectedEntry()
    {
        var email = "consent-record@example.com";
        await RegisterAndActivateAsync(email);
        var tokens = await LoginAsync(email);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var postResponse = await _client.PostAsJsonAsync(
            "/api/bedrock/account/consent",
            new RecordConsentRequest("TermsOfService", "2.0"));
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync("/api/bedrock/account/consent");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await ReadBedrockResponseAsync<IReadOnlyList<ConsentRecordResponse>>(getResponse);
        body.Data.Should().NotBeNull();
        body.Data!.Should().HaveCount(1);
        body.Data![0].PolicyType.Should().Be("TermsOfService");
        body.Data![0].PolicyVersion.Should().Be("2.0");
        body.Data![0].AcceptedAt.Should().NotBe(default);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task RecordConsent_Unauthenticated_Returns401()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/bedrock/account/consent",
            new RecordConsentRequest("TermsOfService", "1.0"));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetConsentHistory_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/bedrock/account/consent");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task RegisterAndActivateAsync(string email)
    {
        await _client.PostAsJsonAsync("/api/bedrock/auth/register", new RegisterRequest(email, ValidPassword));

        var userId = _server.DbContext.UserCredentials.First(c => c.Email == email).UserId;
        var tokenHash = _server.DbContext.EmailVerificationTokens.First(t => t.UserId == userId).TokenHash;

        await _client.PostAsJsonAsync("/api/bedrock/auth/confirm-email", new ConfirmEmailRequest(tokenHash));
    }

    private async Task<LoginResponse> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/bedrock/auth/login",
            new LoginRequest(email, ValidPassword));

        var body = await ReadBedrockResponseAsync<LoginResponse>(response);
        return body.Data!;
    }

    private static async Task<BedrockResponse<T>> ReadBedrockResponseAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<BedrockResponse<T>>(json, JsonOptions)!;
    }

    public void Dispose() => _server.Dispose();
}
