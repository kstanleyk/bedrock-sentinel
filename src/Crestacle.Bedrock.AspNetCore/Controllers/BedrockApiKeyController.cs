using System.Security.Claims;
using Crestacle.Bedrock.AspNetCore.Authorization;
using Crestacle.Bedrock.AspNetCore.Models;
using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crestacle.Bedrock.AspNetCore.Controllers;

[ApiController]
[Route("account/api-keys")]
[Authorize(Policy = BedrockPolicyNames.Default)]
public sealed class BedrockApiKeyController : ControllerBase
{
    private readonly IApiKeyService _apiKeys;

    public BedrockApiKeyController(IApiKeyService apiKeys) => _apiKeys = apiKeys;

    [HttpPost("")]
    public async Task<ActionResult<BedrockResponse<CreateApiKeyResponse>>> Create(
        [FromBody] CreateApiKeyRequest request,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var result = await _apiKeys.CreateAsync(userId, request.Name, ip, ct);

        return Ok(BedrockResponse<CreateApiKeyResponse>.Ok(
            new CreateApiKeyResponse(result.RawKey, result.Id, result.Prefix, result.Name, result.CreatedAt, result.ExpiresAt)));
    }

    [HttpGet("")]
    public async Task<ActionResult<BedrockResponse<IReadOnlyList<ApiKeyResponse>>>> List(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var keys = await _apiKeys.ListAsync(userId, ct);
        var response = keys
            .Select(k => new ApiKeyResponse(k.Id, k.Prefix, k.Name, k.CreatedAt, k.LastUsedAt, k.ExpiresAt, k.IsActive))
            .ToList();

        return Ok(BedrockResponse<IReadOnlyList<ApiKeyResponse>>.Ok(response));
    }

    [HttpDelete("{keyId:guid}")]
    public async Task<ActionResult<BedrockResponse>> Revoke(Guid keyId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        await _apiKeys.RevokeAsync(keyId, userId, ip, ct);
        return Ok(BedrockResponse.Ok());
    }
}
