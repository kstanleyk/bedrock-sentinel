using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Enumerations;
using Crestacle.Bedrock.Core.Exceptions;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Crestacle.Bedrock.AspNetCore.Services;

internal sealed partial class ExternalLoginService : IExternalLoginService
{
    private readonly IEnumerable<IExternalIdentityValidator> _validators;
    private readonly IExternalIdentityRepository _externalIdentityRepo;
    private readonly ICredentialRepository _credentialRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IBedrockUnitOfWork _unitOfWork;
    private readonly ILogger<ExternalLoginService> _logger;

    public ExternalLoginService(
        IEnumerable<IExternalIdentityValidator> validators,
        IExternalIdentityRepository externalIdentityRepo,
        ICredentialRepository credentialRepo,
        IAuditRepository auditRepo,
        IRefreshTokenService refreshTokenService,
        IBedrockUnitOfWork unitOfWork,
        ILogger<ExternalLoginService> logger)
    {
        _validators = validators;
        _externalIdentityRepo = externalIdentityRepo;
        _credentialRepo = credentialRepo;
        _auditRepo = auditRepo;
        _refreshTokenService = refreshTokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // -------------------------------------------------------------------------
    // ExternalLoginAsync / LoginWithClaimsAsync
    // -------------------------------------------------------------------------

    public async Task<LoginResult> ExternalLoginAsync(
        string provider,
        string providerToken,
        string ipAddress,
        string userAgent,
        CancellationToken ct = default)
    {
        var claims = await ValidateProviderTokenAsync(provider, providerToken, ct);
        return await ResolveAndIssueAsync(provider, claims, ipAddress, userAgent, ct);
    }

    public Task<LoginResult> LoginWithClaimsAsync(
        string provider,
        ExternalIdentityClaims claims,
        string ipAddress,
        string userAgent,
        CancellationToken ct = default)
        => ResolveAndIssueAsync(provider, claims, ipAddress, userAgent, ct);

    private async Task<LoginResult> ResolveAndIssueAsync(
        string provider,
        ExternalIdentityClaims claims,
        string ipAddress,
        string userAgent,
        CancellationToken ct)
    {
        var externalId = await _externalIdentityRepo.GetByProviderAsync(
            provider, claims.ProviderUserId, ct);

        UserCredential? credential;

        if (externalId is not null)
        {
            credential = await _credentialRepo.GetByUserIdAsync(externalId.UserId, ct)
                ?? throw new BedrockNotFoundException("User account not found.");
        }
        else if (claims.Email is not null)
        {
            // Auto-link when the provider supplies an email matching an existing account.
            credential = await _credentialRepo.GetByEmailAsync(claims.Email, ct)
                ?? throw new BedrockNotFoundException(
                    "No account is linked to this external identity. " +
                    "Please log in and link this provider from your account settings.");

            var link = ExternalIdentity.Create(
                credential.UserId, provider, claims.ProviderUserId, credential.TenantId);
            await _externalIdentityRepo.AddAsync(link, ct);
        }
        else
        {
            throw new BedrockNotFoundException(
                "No account is linked to this external identity. " +
                "Please log in and link this provider from your account settings.");
        }

        if (credential.IsLockedOut())
            throw new BedrockValidationException("Account is locked.");

        await _auditRepo.AddAsync(
            AuditEntry.Create(AuditEventType.ExternalLoginSucceeded,
                ipAddress, userAgent, credential.UserId, tenantId: credential.TenantId), ct);

        await _unitOfWork.SaveChangesAsync(ct);

        var tokens = await _refreshTokenService.IssueAsync(
            credential.UserId,
            credential.Email,
            roles: [],
            ip: ipAddress,
            userAgent: userAgent,
            fingerprintHash: userAgent,
            tenantId: credential.TenantId,
            ct: ct);

        LogExternalLoginSucceeded(_logger, provider, credential.UserId);
        return new LoginResult { Tokens = tokens };
    }

    // -------------------------------------------------------------------------
    // LinkExternalIdentityAsync / LinkWithClaimsAsync
    // -------------------------------------------------------------------------

    public async Task LinkExternalIdentityAsync(
        Guid userId,
        string provider,
        string providerToken,
        CancellationToken ct = default)
    {
        var claims = await ValidateProviderTokenAsync(provider, providerToken, ct);
        await PersistLinkAsync(userId, provider, claims, ct);
    }

    public Task LinkWithClaimsAsync(
        Guid userId,
        string provider,
        ExternalIdentityClaims claims,
        CancellationToken ct = default)
        => PersistLinkAsync(userId, provider, claims, ct);

    private async Task PersistLinkAsync(
        Guid userId,
        string provider,
        ExternalIdentityClaims claims,
        CancellationToken ct)
    {
        var existing = await _externalIdentityRepo.GetByProviderAsync(
            provider, claims.ProviderUserId, ct);
        if (existing is not null)
            throw new BedrockValidationException(
                "This external identity is already linked to an account.");

        var credential = await _credentialRepo.GetByUserIdAsync(userId, ct)
            ?? throw new BedrockNotFoundException("User account not found.");

        var identity = ExternalIdentity.Create(
            userId, provider, claims.ProviderUserId, credential.TenantId);

        await _externalIdentityRepo.AddAsync(identity, ct);
        await _auditRepo.AddAsync(
            AuditEntry.Create(AuditEventType.ExternalIdentityLinked,
                "unknown", "unknown", userId, tenantId: credential.TenantId), ct);

        await _unitOfWork.SaveChangesAsync(ct);
        LogExternalIdentityLinked(_logger, provider, userId);
    }

    // -------------------------------------------------------------------------
    // UnlinkExternalIdentityAsync
    // -------------------------------------------------------------------------

    public async Task UnlinkExternalIdentityAsync(
        Guid userId,
        string provider,
        CancellationToken ct = default)
    {
        var linked = await _externalIdentityRepo.GetForUserAsync(userId, ct);
        var target = linked.FirstOrDefault(e => e.Provider == provider)
            ?? throw new BedrockNotFoundException(
                $"No linked identity found for provider '{provider}'.");

        var credential = await _credentialRepo.GetByUserIdAsync(userId, ct);
        var hasPassword = !string.IsNullOrEmpty(credential?.PasswordHash);
        var otherIdentities = linked.Count(e => e.Provider != provider);

        if (!hasPassword && otherIdentities == 0)
            throw new BedrockValidationException(
                "Cannot unlink: this is the only credential on the account. " +
                "Set a password first or link another provider.");

        await _externalIdentityRepo.DeleteAsync(target, ct);
        await _auditRepo.AddAsync(
            AuditEntry.Create(AuditEventType.ExternalIdentityUnlinked,
                "unknown", "unknown", userId, tenantId: credential?.TenantId), ct);

        await _unitOfWork.SaveChangesAsync(ct);
        LogExternalIdentityUnlinked(_logger, provider, userId);
    }

    // -------------------------------------------------------------------------
    // GetLinkedIdentitiesAsync
    // -------------------------------------------------------------------------

    public Task<IReadOnlyList<ExternalIdentity>> GetLinkedIdentitiesAsync(
        Guid userId,
        CancellationToken ct = default)
        => _externalIdentityRepo.GetForUserAsync(userId, ct);

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<Core.DTOs.ExternalIdentityClaims> ValidateProviderTokenAsync(
        string provider,
        string providerToken,
        CancellationToken ct)
    {
        var validator = _validators.FirstOrDefault(
            v => string.Equals(v.ProviderName, provider, StringComparison.OrdinalIgnoreCase))
            ?? throw new BedrockValidationException(
                $"Unknown or unsupported provider '{provider}'.");

        var claims = await validator.ValidateAsync(providerToken, ct);
        if (claims is null)
            throw new BedrockValidationException(
                $"The token for provider '{provider}' could not be validated.");

        return claims;
    }

    // -------------------------------------------------------------------------
    // Logging
    // -------------------------------------------------------------------------

    [LoggerMessage(5001, LogLevel.Information,
        "External login succeeded: provider={Provider} userId={UserId}")]
    private static partial void LogExternalLoginSucceeded(
        ILogger logger, string provider, Guid userId);

    [LoggerMessage(5002, LogLevel.Information,
        "External identity linked: provider={Provider} userId={UserId}")]
    private static partial void LogExternalIdentityLinked(
        ILogger logger, string provider, Guid userId);

    [LoggerMessage(5003, LogLevel.Information,
        "External identity unlinked: provider={Provider} userId={UserId}")]
    private static partial void LogExternalIdentityUnlinked(
        ILogger logger, string provider, Guid userId);
}
