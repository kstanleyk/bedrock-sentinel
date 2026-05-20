using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Enumerations;
using Crestacle.Bedrock.Core.Exceptions;
using Crestacle.Bedrock.Core.Interfaces;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Crestacle.Bedrock.AspNetCore.Services;

internal sealed partial class ApiKeyService : IApiKeyService
{
    private readonly IApiKeyRepository _apiKeyRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly IBedrockUnitOfWork _unitOfWork;
    private readonly ILogger<ApiKeyService> _logger;

    public ApiKeyService(
        IApiKeyRepository apiKeyRepo,
        IAuditRepository auditRepo,
        IBedrockUnitOfWork unitOfWork,
        ILogger<ApiKeyService> logger)
    {
        _apiKeyRepo = apiKeyRepo;
        _auditRepo = auditRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CreateApiKeyResult> CreateAsync(
        Guid userId,
        string? name,
        string ipAddress,
        CancellationToken ct = default)
    {
        var (entity, rawKey) = ApiKey.Create(userId, name);

        await _apiKeyRepo.AddAsync(entity, ct);
        await _auditRepo.AddAsync(
            AuditEntry.Create(AuditEventType.ApiKeyCreated, ipAddress, "api",
                userId, metadata: entity.Prefix), ct);

        await _unitOfWork.SaveChangesAsync(ct);

        LogApiKeyCreated(_logger, userId, entity.Prefix);

        return new CreateApiKeyResult(rawKey, entity.Id, entity.Prefix, entity.Name, entity.CreatedAt, entity.ExpiresAt);
    }

    public async Task<IReadOnlyList<ApiKeySummary>> ListAsync(Guid userId, CancellationToken ct = default)
    {
        var keys = await _apiKeyRepo.GetForUserAsync(userId, ct);
        return keys
            .Select(k => new ApiKeySummary(k.Id, k.Prefix, k.Name, k.CreatedAt, k.LastUsedAt, k.ExpiresAt, k.IsActive))
            .ToList();
    }

    public async Task RevokeAsync(Guid keyId, Guid userId, string ipAddress, CancellationToken ct = default)
    {
        var keys = await _apiKeyRepo.GetForUserAsync(userId, ct);
        var key = keys.FirstOrDefault(k => k.Id == keyId);

        if (key is null)
            throw new BedrockNotFoundException("API key not found.");

        await _apiKeyRepo.RevokeAsync(keyId, userId, ct);
        await _auditRepo.AddAsync(
            AuditEntry.Create(AuditEventType.ApiKeyRevoked, ipAddress, "api",
                userId, metadata: key.Prefix), ct);

        await _unitOfWork.SaveChangesAsync(ct);

        LogApiKeyRevoked(_logger, userId, key.Prefix);
    }

    [LoggerMessage(4001, LogLevel.Information, "API key created for user {UserId} prefix={Prefix}")]
    private static partial void LogApiKeyCreated(ILogger logger, Guid userId, string prefix);

    [LoggerMessage(4002, LogLevel.Information, "API key revoked for user {UserId} prefix={Prefix}")]
    private static partial void LogApiKeyRevoked(ILogger logger, Guid userId, string prefix);
}
