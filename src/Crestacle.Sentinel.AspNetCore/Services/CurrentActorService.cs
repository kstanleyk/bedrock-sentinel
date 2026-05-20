using Crestacle.Sentinel.AspNetCore.Authorization;
using Crestacle.Sentinel.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Crestacle.Sentinel.AspNetCore.Services;

/// <summary>
/// Reads actor context from the active HTTP request.
/// Returns nulls when called outside an HTTP context (e.g. background jobs or seeders).
/// </summary>
internal sealed class CurrentActorService(IHttpContextAccessor httpContextAccessor) : ICurrentActor
{
    public string? IdentityId
        => httpContextAccessor.HttpContext?.User.GetSubject();

    public string? IpAddress
        => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent
        => httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
}
