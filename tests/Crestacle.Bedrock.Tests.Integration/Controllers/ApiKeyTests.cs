using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Crestacle.Bedrock.AspNetCore.Models;
using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Tests.Integration.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Crestacle.Bedrock.Tests.Integration.Controllers;

/// <summary>F10 — API key management integration tests.</summary>
public sealed class ApiKeyTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly BedrockTestServer _server;
    private readonly HttpClient _client;

    private const string Password = "ApiKeyP@ssword1!";

    public ApiKeyTests()
    {
        _server = new BedrockTestServer();
        _client = _server.Client;
    }

    // -------------------------------------------------------------------------
    // Create
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateApiKey_WithBearerAuth_ReturnsRawKeyAndPrefix()
    {
        var email = "apikey-create@example.com";
        var token = await RegisterActivateAndLoginAsync(email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync(
            "/api/bedrock/account/api-keys",
            new CreateApiKeyRequest("test key"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadBedrockResponseAsync<CreateApiKeyResponse>(response);
        body.Data.Should().NotBeNull();
        body.Data!.RawKey.Should().StartWith("bdrk_");
        body.Data!.Prefix.Should().HaveLength(8);
        body.Data!.Prefix.Should().StartWith("bdrk_");
        body.Data!.Name.Should().Be("test key");

        _client.DefaultRequestHeaders.Authorization = null;
    }

    // -------------------------------------------------------------------------
    // Use X-Api-Key to authenticate
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UseApiKey_OnProtectedEndpoint_Returns200()
    {
        var email = "apikey-use@example.com";
        var token = await RegisterActivateAndLoginAsync(email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/bedrock/account/api-keys",
            new CreateApiKeyRequest());

        var created = await ReadBedrockResponseAsync<CreateApiKeyResponse>(createResponse);
        var rawKey = created.Data!.RawKey;

        _client.DefaultRequestHeaders.Authorization = null;

        // Use the raw API key to call a protected endpoint
        _client.DefaultRequestHeaders.Add("X-Api-Key", rawKey);

        var listResponse = await _client.GetAsync("/api/bedrock/account/api-keys");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _client.DefaultRequestHeaders.Remove("X-Api-Key");
    }

    // -------------------------------------------------------------------------
    // List
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListApiKeys_AfterCreate_ReturnsKey()
    {
        var email = "apikey-list@example.com";
        var token = await RegisterActivateAndLoginAsync(email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/bedrock/account/api-keys", new CreateApiKeyRequest("list-test"));

        var listResponse = await _client.GetAsync("/api/bedrock/account/api-keys");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await ReadBedrockResponseAsync<IReadOnlyList<ApiKeyResponse>>(listResponse);
        body.Data.Should().NotBeNull();
        body.Data!.Should().HaveCount(1);
        body.Data![0].Name.Should().Be("list-test");
        body.Data![0].IsActive.Should().BeTrue();

        _client.DefaultRequestHeaders.Authorization = null;
    }

    // -------------------------------------------------------------------------
    // Revoke then use — must return 401
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RevokeApiKey_ThenUse_Returns401()
    {
        var email = "apikey-revoke@example.com";
        var token = await RegisterActivateAndLoginAsync(email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create
        var createResponse = await _client.PostAsJsonAsync(
            "/api/bedrock/account/api-keys",
            new CreateApiKeyRequest());

        var created = await ReadBedrockResponseAsync<CreateApiKeyResponse>(createResponse);
        var rawKey = created.Data!.RawKey;
        var keyId = created.Data!.Id;

        // Revoke
        var revokeResponse = await _client.DeleteAsync($"/api/bedrock/account/api-keys/{keyId}");
        revokeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _client.DefaultRequestHeaders.Authorization = null;

        // Use revoked key
        _client.DefaultRequestHeaders.Add("X-Api-Key", rawKey);

        var listResponse = await _client.GetAsync("/api/bedrock/account/api-keys");
        listResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        _client.DefaultRequestHeaders.Remove("X-Api-Key");
    }

    // -------------------------------------------------------------------------
    // Unknown key — returns 401
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UseUnknownApiKey_Returns401()
    {
        _client.DefaultRequestHeaders.Add("X-Api-Key", "bdrk_unknownkeyvalue");

        var response = await _client.GetAsync("/api/bedrock/account/api-keys");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        _client.DefaultRequestHeaders.Remove("X-Api-Key");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<string> RegisterActivateAndLoginAsync(string email)
    {
        await _client.PostAsJsonAsync("/api/bedrock/auth/register", new RegisterRequest(email, Password));

        var userId = _server.DbContext.UserCredentials.First(c => c.Email == email).UserId;
        var tokenHash = _server.DbContext.EmailVerificationTokens.First(t => t.UserId == userId).TokenHash;

        await _client.PostAsJsonAsync("/api/bedrock/auth/confirm-email", new ConfirmEmailRequest(tokenHash));

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/bedrock/auth/login",
            new LoginRequest(email, Password));

        var json = await loginResponse.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<BedrockResponse<LoginResponse>>(json, JsonOptions)!;
        return body.Data!.AccessToken!;
    }

    private static async Task<BedrockResponse<T>> ReadBedrockResponseAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<BedrockResponse<T>>(json, JsonOptions)!;
    }

    public void Dispose() => _server.Dispose();
}
