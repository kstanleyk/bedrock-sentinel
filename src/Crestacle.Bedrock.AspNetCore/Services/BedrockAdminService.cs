using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Enumerations;
using Crestacle.Bedrock.Core.Exceptions;
using Crestacle.Bedrock.Core.Interfaces;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Crestacle.Bedrock.AspNetCore.Services;

internal sealed partial class BedrockAdminService : IBedrockAdminService
{
    private readonly ICredentialRepository _credentialRepo;
    private readonly IRecoveryCodeRepository _recoveryCodeRepo;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IAuditRepository _auditRepo;
    private readonly IBedrockUnitOfWork _unitOfWork;
    private readonly ILogger<BedrockAdminService> _logger;

    public BedrockAdminService(
        ICredentialRepository credentialRepo,
        IRecoveryCodeRepository recoveryCodeRepo,
        IRefreshTokenService refreshTokenService,
        IAuditRepository auditRepo,
        IBedrockUnitOfWork unitOfWork,
        ILogger<BedrockAdminService> logger)
    {
        _credentialRepo = credentialRepo;
        _recoveryCodeRepo = recoveryCodeRepo;
        _refreshTokenService = refreshTokenService;
        _auditRepo = auditRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<PagedResult<CredentialSummary>> GetUsersAsync(
        int page, int pageSize, CancellationToken ct = default)
        => _credentialRepo.GetPagedAsync(page, pageSize, ct);

    public async Task<CredentialDetail> GetUserAsync(Guid userId, CancellationToken ct = default)
    {
        var credential = await _credentialRepo.GetByUserIdAsync(userId, ct)
            ?? throw new BedrockNotFoundException($"User {userId} not found.");
        return ToDetail(credential);
    }

    public async Task LockUserAsync(Guid adminId, Guid userId, CancellationToken ct = default)
    {
        var credential = await _credentialRepo.GetByUserIdAsync(userId, ct)
            ?? throw new BedrockNotFoundException($"User {userId} not found.");

        credential.AdminLock();
        await _credentialRepo.UpdateAsync(credential, ct);
        await _auditRepo.AddAsync(
            AuditEntry.Create(AuditEventType.AdminAccountLocked, "admin", "admin",
                userId, metadata: adminId.ToString(), tenantId: credential.TenantId), ct);
        await _unitOfWork.SaveChangesAsync(ct);
        LogAdminLocked(_logger, adminId, userId);
    }

    public async Task UnlockUserAsync(Guid adminId, Guid userId, CancellationToken ct = default)
    {
        var credential = await _credentialRepo.GetByUserIdAsync(userId, ct)
            ?? throw new BedrockNotFoundException($"User {userId} not found.");

        credential.AdminUnlock();
        await _credentialRepo.UpdateAsync(credential, ct);
        await _auditRepo.AddAsync(
            AuditEntry.Create(AuditEventType.AdminAccountUnlocked, "admin", "admin",
                userId, metadata: adminId.ToString(), tenantId: credential.TenantId), ct);
        await _unitOfWork.SaveChangesAsync(ct);
        LogAdminUnlocked(_logger, adminId, userId);
    }

    public async Task ResetMfaAsync(Guid adminId, Guid userId, CancellationToken ct = default)
    {
        var credential = await _credentialRepo.GetByUserIdAsync(userId, ct)
            ?? throw new BedrockNotFoundException($"User {userId} not found.");

        credential.DisableMfa();
        await _credentialRepo.UpdateAsync(credential, ct);
        await _recoveryCodeRepo.InvalidateAllForUserAsync(userId, ct);
        await _auditRepo.AddAsync(
            AuditEntry.Create(AuditEventType.AdminMfaReset, "admin", "admin",
                userId, metadata: adminId.ToString(), tenantId: credential.TenantId), ct);
        await _unitOfWork.SaveChangesAsync(ct);
        LogAdminMfaReset(_logger, adminId, userId);
    }

    public async Task ExpirePasswordAsync(Guid adminId, Guid userId, CancellationToken ct = default)
    {
        var credential = await _credentialRepo.GetByUserIdAsync(userId, ct)
            ?? throw new BedrockNotFoundException($"User {userId} not found.");

        credential.ExpirePassword();
        await _credentialRepo.UpdateAsync(credential, ct);
        await _auditRepo.AddAsync(
            AuditEntry.Create(AuditEventType.AdminPasswordExpired, "admin", "admin",
                userId, metadata: adminId.ToString(), tenantId: credential.TenantId), ct);
        await _unitOfWork.SaveChangesAsync(ct);
        LogAdminPasswordExpired(_logger, adminId, userId);
    }

    public async Task RevokeAllSessionsAsync(Guid adminId, Guid userId, CancellationToken ct = default)
    {
        var credential = await _credentialRepo.GetByUserIdAsync(userId, ct)
            ?? throw new BedrockNotFoundException($"User {userId} not found.");

        await _refreshTokenService.RevokeAllAsync(userId, "admin", ct);
        await _auditRepo.AddAsync(
            AuditEntry.Create(AuditEventType.AdminSessionsRevoked, "admin", "admin",
                userId, metadata: adminId.ToString(), tenantId: credential.TenantId), ct);
        await _unitOfWork.SaveChangesAsync(ct);
        LogAdminSessionsRevoked(_logger, adminId, userId);
    }

    private static CredentialDetail ToDetail(UserCredential c) => new(
        c.UserId, c.Email, c.Status, c.EmailConfirmed, c.MfaEnabled, c.MfaMethod,
        c.IsLockedOut(), c.LockoutEnd, c.FailedLoginAttempts,
        c.PasswordExpiresAt, c.PasswordChangedAt, c.MfaGracePeriodEndsAt,
        c.TenantId, c.CreatedAt, c.UpdatedAt);

    [LoggerMessage(2001, LogLevel.Warning, "Admin {AdminId} locked user {UserId}")]
    private static partial void LogAdminLocked(ILogger logger, Guid adminId, Guid userId);

    [LoggerMessage(2002, LogLevel.Information, "Admin {AdminId} unlocked user {UserId}")]
    private static partial void LogAdminUnlocked(ILogger logger, Guid adminId, Guid userId);

    [LoggerMessage(2003, LogLevel.Warning, "Admin {AdminId} reset MFA for user {UserId}")]
    private static partial void LogAdminMfaReset(ILogger logger, Guid adminId, Guid userId);

    [LoggerMessage(2004, LogLevel.Warning, "Admin {AdminId} expired password for user {UserId}")]
    private static partial void LogAdminPasswordExpired(ILogger logger, Guid adminId, Guid userId);

    [LoggerMessage(2005, LogLevel.Warning, "Admin {AdminId} revoked all sessions for user {UserId}")]
    private static partial void LogAdminSessionsRevoked(ILogger logger, Guid adminId, Guid userId);
}
