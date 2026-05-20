using System.Diagnostics;
using Crestacle.Bedrock.Core;
using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Enumerations;
using Crestacle.Bedrock.Core.Exceptions;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Crestacle.Bedrock.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crestacle.Bedrock.AspNetCore.Services;

internal sealed partial class RefreshTokenService : IRefreshTokenService
{
    private const string RevokedCacheKeyPrefix = "Bedrock:revoked:";

    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly ISessionRepository _sessionRepo;
    private readonly ICredentialRepository _credentialRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly IBedrockUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IBedrockTokenIssuer _tokenIssuer;
    private readonly IBedrockClaimsEnricher _enricher;
    private readonly IBedrockCache _cache;
    private readonly BedrockOptions _options;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        IRefreshTokenRepository refreshTokenRepo,
        ISessionRepository sessionRepo,
        ICredentialRepository credentialRepo,
        IAuditRepository auditRepo,
        IBedrockUnitOfWork unitOfWork,
        ITokenService tokenService,
        IBedrockTokenIssuer tokenIssuer,
        IBedrockClaimsEnricher enricher,
        IBedrockCache cache,
        IOptions<BedrockOptions> options,
        ILogger<RefreshTokenService> logger)
    {
        _refreshTokenRepo = refreshTokenRepo;
        _sessionRepo = sessionRepo;
        _credentialRepo = credentialRepo;
        _auditRepo = auditRepo;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _tokenIssuer = tokenIssuer;
        _enricher = enricher;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<TokenPair> IssueAsync(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        string ip,
        string userAgent,
        string fingerprintHash,
        string? tenantId = null,
        CancellationToken ct = default)
    {
        var lockKey = $"Bedrock:session-create-lock:{userId}";
        var lockAcquired = false;
        try
        {
            // Serialize concurrent logins for the same user so that the count-check,
            // eviction, and session creation are one atomic application-level operation.
            for (var attempt = 0; attempt < 20; attempt++)
            {
                if (await _cache.TryAcquireLockAsync(lockKey, TimeSpan.FromSeconds(10), ct))
                {
                    lockAcquired = true;
                    break;
                }
                await Task.Delay(50, ct);
            }

            if (!lockAcquired)
                throw new BedrockValidationException("Login is temporarily busy; please retry.");

            // Evict oldest session when the per-user limit is reached
            var activeCount = await _sessionRepo.CountActiveForUserAsync(userId, ct);
            if (activeCount >= _options.Session.MaxConcurrentSessions)
            {
                var oldest = await _sessionRepo.GetOldestActiveForUserAsync(userId, ct);
                if (oldest is not null)
                {
                    var oldToken = await _refreshTokenRepo.GetByHashAsync(oldest.TokenHash, ct);
                    if (oldToken is not null && oldToken.IsActive)
                    {
                        oldToken.Revoke(ip);
                        await _refreshTokenRepo.UpdateAsync(oldToken, ct);
                    }
                    oldest.Revoke(ip);
                    await _sessionRepo.UpdateAsync(oldest, ct);
                    LogSessionEvicted(_logger, userId, tenantId);
                }
            }

            var extraClaims = await _enricher.EnrichAsync(userId, ct);
            var resolvedRoles = await _enricher.GetRolesAsync(userId, ct);
            var rawRefresh = _tokenService.GenerateRefreshToken();
            var refreshHash = _tokenService.HashToken(rawRefresh);
            var tokenDescriptor = await _tokenIssuer.IssueAccessTokenAsync(
                userId, email, resolvedRoles, tenantId, extraClaims, ct);

            var refreshToken = RefreshToken.Create(
                userId, refreshHash,
                DateTime.UtcNow.Add(_options.Jwt.RefreshTokenExpiry), ip, tenantId);
            await _refreshTokenRepo.AddAsync(refreshToken, ct);

            var session = Session.Create(
                userId, refreshHash, fingerprintHash, ip, userAgent,
                DateTime.UtcNow.Add(_options.Jwt.RefreshTokenExpiry), tenantId,
                accessTokenJti: tokenDescriptor.Jti,
                accessTokenExpiresAt: tokenDescriptor.ExpiresAt);
            await _sessionRepo.AddAsync(session, ct);

            await _unitOfWork.SaveChangesAsync(ct);
            LogSessionCreated(_logger, userId, tenantId);

            return new TokenPair(tokenDescriptor.AccessToken, rawRefresh, tokenDescriptor.ExpiresAt);
        }
        finally
        {
            if (lockAcquired)
                await _cache.ReleaseLockAsync(lockKey, ct);
        }
    }

    public async Task<TokenPair> RefreshAsync(
        string rawRefreshToken,
        string ip,
        string userAgent,
        string fingerprintHash,
        CancellationToken ct = default)
    {
        using var activity = BedrockTelemetry.ActivitySource.StartActivity("bedrock.token.refresh");
        var oldHash = _tokenService.HashToken(rawRefreshToken);
        var existing = await _refreshTokenRepo.GetByHashAsync(oldHash, ct);

        if (existing is null || !existing.IsActive)
            throw new BedrockValidationException("The refresh token is invalid or has expired.", BedrockErrorCodes.InvalidToken);

        activity?.SetTag("bedrock.user_id", existing.UserId.ToString());
        var session = await _sessionRepo.GetByTokenHashAsync(oldHash, ct)
            ?? throw new BedrockValidationException("Session not found.", BedrockErrorCodes.InvalidToken);

        if (_options.Session.AbsoluteRefreshExpiry.HasValue &&
            session.CreatedAt.Add(_options.Session.AbsoluteRefreshExpiry.Value) <= DateTime.UtcNow)
        {
            throw new BedrockValidationException(
                "Session has exceeded its maximum lifetime. Please log in again.",
                BedrockErrorCodes.SessionExpired);
        }

        var credential = await _credentialRepo.GetByUserIdAsync(existing.UserId, ct)
            ?? throw new BedrockValidationException("User not found.", BedrockErrorCodes.AccountNotFound);

        var extraClaims = await _enricher.EnrichAsync(existing.UserId, ct);
        var roles = await _enricher.GetRolesAsync(existing.UserId, ct);
        var newRaw = _tokenService.GenerateRefreshToken();
        var newHash = _tokenService.HashToken(newRaw);
        var tokenDescriptor = await _tokenIssuer.IssueAccessTokenAsync(
            existing.UserId, credential.Email, roles, existing.TenantId, extraClaims, ct);

        existing.Revoke(ip, newHash);
        await _refreshTokenRepo.UpdateAsync(existing, ct);

        var newToken = RefreshToken.Create(
            existing.UserId, newHash,
            DateTime.UtcNow.Add(_options.Jwt.RefreshTokenExpiry), ip, existing.TenantId);
        await _refreshTokenRepo.AddAsync(newToken, ct);

        session.UpdateActivity(newHash, tokenDescriptor.Jti, tokenDescriptor.ExpiresAt);
        await _sessionRepo.UpdateAsync(session, ct);

        await _auditRepo.AddAsync(
            AuditEntry.Create(AuditEventType.TokenRefreshed, ip, userAgent,
                existing.UserId, tenantId: existing.TenantId), ct);

        await _unitOfWork.SaveChangesAsync(ct);
        LogTokenRefreshed(_logger, existing.UserId, existing.TenantId);
        BedrockTelemetry.TokenRefreshes.Add(1);

        return new TokenPair(tokenDescriptor.AccessToken, newRaw, tokenDescriptor.ExpiresAt);
    }

    public async Task RevokeAsync(
        string rawRefreshToken,
        string ip,
        string? accessTokenJti = null,
        TimeSpan? accessTokenRemainingLifetime = null,
        CancellationToken ct = default)
    {
        var tokenHash = _tokenService.HashToken(rawRefreshToken);
        var existing = await _refreshTokenRepo.GetByHashAsync(tokenHash, ct);

        if (existing is null || !existing.IsActive)
            return; // Always OK — no enumeration

        existing.Revoke(ip);
        await _refreshTokenRepo.UpdateAsync(existing, ct);

        var session = await _sessionRepo.GetByTokenHashAsync(tokenHash, ct);
        if (session is not null && session.IsActive)
        {
            session.Revoke(ip);
            await _sessionRepo.UpdateAsync(session, ct);
        }

        await _auditRepo.AddAsync(
            AuditEntry.Create(AuditEventType.TokenRevoked, ip, "unknown",
                existing.UserId, tenantId: existing.TenantId), ct);

        await _unitOfWork.SaveChangesAsync(ct);
        LogTokenRevoked(_logger, existing.UserId, existing.TenantId);

        // Blacklist the access token jti so in-flight requests are rejected immediately
        if (!string.IsNullOrEmpty(accessTokenJti)
            && accessTokenRemainingLifetime.HasValue
            && accessTokenRemainingLifetime.Value > TimeSpan.Zero)
        {
            await _cache.SetAsync(
                RevokedCacheKeyPrefix + accessTokenJti, "1",
                accessTokenRemainingLifetime.Value, ct);
        }
    }

    public async Task RevokeAllAsync(Guid userId, string ip, CancellationToken ct = default)
    {
        var activeSessions = await _sessionRepo.GetActiveByUserAsync(userId, ct);

        await _refreshTokenRepo.RevokeAllForUserAsync(userId, ip, ct);
        await _sessionRepo.RevokeAllForUserAsync(userId, ip, ct);
        await _auditRepo.AddAsync(
            AuditEntry.Create(AuditEventType.SessionRevoked, ip, "unknown", userId), ct);
        await _unitOfWork.SaveChangesAsync(ct);
        LogAllTokensRevoked(_logger, userId);

        foreach (var session in activeSessions)
        {
            if (string.IsNullOrEmpty(session.AccessTokenJti)) continue;

            // Use the per-session remaining lifetime so we don't over-cache tokens
            // that are already close to natural expiry.
            var ttl = session.AccessTokenExpiresAt.HasValue
                ? session.AccessTokenExpiresAt.Value - DateTime.UtcNow
                : _options.Jwt.AccessTokenExpiry;

            if (ttl > TimeSpan.Zero)
                await _cache.SetAsync(RevokedCacheKeyPrefix + session.AccessTokenJti, "1", ttl, ct);
        }
    }

    [LoggerMessage(2001, LogLevel.Debug, "Token refreshed: userId={UserId} tenant={TenantId}")]
    private static partial void LogTokenRefreshed(ILogger logger, Guid userId, string? tenantId);

    [LoggerMessage(2002, LogLevel.Information, "Token revoked: userId={UserId} tenant={TenantId}")]
    private static partial void LogTokenRevoked(ILogger logger, Guid userId, string? tenantId);

    [LoggerMessage(2003, LogLevel.Information, "All tokens revoked: userId={UserId}")]
    private static partial void LogAllTokensRevoked(ILogger logger, Guid userId);

    [LoggerMessage(2004, LogLevel.Debug, "Session created: userId={UserId} tenant={TenantId}")]
    private static partial void LogSessionCreated(ILogger logger, Guid userId, string? tenantId);

    [LoggerMessage(2005, LogLevel.Warning, "Session evicted (concurrent session limit reached): userId={UserId} tenant={TenantId}")]
    private static partial void LogSessionEvicted(ILogger logger, Guid userId, string? tenantId);
}
