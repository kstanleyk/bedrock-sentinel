using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Crestacle.Bedrock.AspNetCore.Middleware;

public sealed class ApiKeyAuthenticationMiddleware
{
    private const string HeaderName = "X-Api-Key";

    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        IServiceScopeFactory scopeFactory,
        ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip if already authenticated (e.g. by JWT bearer)
        if (context.User.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var headerValues) ||
            string.IsNullOrWhiteSpace(headerValues))
        {
            await _next(context);
            return;
        }

        var rawKey = headerValues.ToString().Trim();
        var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)))
            .ToLowerInvariant();

        await using var scope = _scopeFactory.CreateAsyncScope();
        var apiKeyRepo = scope.ServiceProvider.GetRequiredService<IApiKeyRepository>();
        var credentialRepo = scope.ServiceProvider.GetRequiredService<ICredentialRepository>();
        var enricher = scope.ServiceProvider.GetRequiredService<IBedrockClaimsEnricher>();

        var apiKey = await apiKeyRepo.GetByHashAsync(keyHash, context.RequestAborted);

        if (apiKey is null || !apiKey.IsActive)
        {
            await _next(context);
            return;
        }

        var credential = await credentialRepo.GetByUserIdAsync(apiKey.UserId, context.RequestAborted);
        var email = credential?.Email ?? string.Empty;

        var extraClaims = await enricher.EnrichAsync(apiKey.UserId, context.RequestAborted);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, apiKey.UserId.ToString()),
            new("user_id", apiKey.UserId.ToString()),
            new("email", email),
            new("token_type", "api_key"),
        };

        if (!string.IsNullOrEmpty(apiKey.TenantId))
            claims.Add(new Claim("tenant_id", apiKey.TenantId));

        foreach (var (key, value) in extraClaims)
            claims.Add(new Claim(key, value));

        var identity = new ClaimsIdentity(claims, authenticationType: "ApiKey");
        context.User = new ClaimsPrincipal(identity);

        // Fire-and-forget: update LastUsedAt without blocking the request
        _ = Task.Run(async () =>
        {
            try
            {
                await using var updateScope = _scopeFactory.CreateAsyncScope();
                var repo = updateScope.ServiceProvider.GetRequiredService<IApiKeyRepository>();
                await repo.UpdateLastUsedAsync(apiKey.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update LastUsedAt for API key {KeyId}", apiKey.Id);
            }
        });

        await _next(context);
    }
}
