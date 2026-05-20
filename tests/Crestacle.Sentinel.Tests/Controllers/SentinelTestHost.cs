using System.Security.Claims;
using System.Text.Encodings.Web;
using Crestacle.Sentinel.AspNetCore;
using Crestacle.Sentinel.Core.DTOs;
using Crestacle.Sentinel.Core.Enums;
using Crestacle.Sentinel.Core.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crestacle.Sentinel.Tests.Controllers;

// ─────────────────────────────────────────────────────────────────────────────
// Fake authentication — reads X-Test-Subject header; absent = unauthenticated.
// ─────────────────────────────────────────────────────────────────────────────
internal sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory                               logger,
    UrlEncoder                                   encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-Subject", out var subject)
            || string.IsNullOrEmpty(subject))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims    = new[] { new Claim(ClaimTypes.NameIdentifier, subject.ToString()) };
        var identity  = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket    = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ICurrentActor that reads identity from the same X-Test-Subject header.
// ─────────────────────────────────────────────────────────────────────────────
internal sealed class TestCurrentActor(IHttpContextAccessor accessor) : ICurrentActor
{
    public string? IdentityId
        => accessor.HttpContext?.Request.Headers["X-Test-Subject"].FirstOrDefault();
    public string? IpAddress => "127.0.0.1";
    public string? UserAgent => "test";
}

// ─────────────────────────────────────────────────────────────────────────────
// Test host — wraps a TestServer with Sentinel controllers and mocked repos.
// ─────────────────────────────────────────────────────────────────────────────
internal sealed class SentinelTestHost : IDisposable
{
    public IUserRepository               UserRepo     { get; }
    public IRoleRepository               RoleRepo     { get; }
    public IUserPermissionRepository     PermRepo     { get; }
    public IPermissionConflictRepository ConflictRepo { get; }
    public IPendingAssignmentRepository  PendingRepo  { get; }
    public IAuditRepository              AuditRepo    { get; }

    private readonly TestServer _server;

    private SentinelTestHost(
        IUserRepository               userRepo,
        IRoleRepository               roleRepo,
        IUserPermissionRepository     permRepo,
        IPermissionConflictRepository conflictRepo,
        IPendingAssignmentRepository  pendingRepo,
        IAuditRepository              auditRepo,
        TestServer                    server)
    {
        UserRepo     = userRepo;
        RoleRepo     = roleRepo;
        PermRepo     = permRepo;
        ConflictRepo = conflictRepo;
        PendingRepo  = pendingRepo;
        AuditRepo    = auditRepo;
        _server      = server;
    }

    public static SentinelTestHost Create()
    {
        var userRepo     = Substitute.For<IUserRepository>();
        var roleRepo     = Substitute.For<IRoleRepository>();
        var permRepo     = Substitute.For<IUserPermissionRepository>();
        var conflictRepo = Substitute.For<IPermissionConflictRepository>();
        var pendingRepo  = Substitute.For<IPendingAssignmentRepository>();
        var auditRepo    = Substitute.For<IAuditRepository>();

        // ── Sensible defaults so tests only configure what they care about ──
        permRepo.GetPermissionsForUserAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new HashSet<string>());

        roleRepo.GetRoleTypeAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((RoleType?)RoleType.Default);
        roleRepo.RequiresDualApprovalAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(false);
        roleRepo.GetAllWithPermissionsAsync(
                    Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(new PagedResult<RoleDto>([], 1, 50, 0));
        roleRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((RoleDto?)null);

        conflictRepo.HasConflictAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(false);

        userRepo.GetIdentityIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((string?)null);
        userRepo.GetAllWithRolesAsync(
                    Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(new PagedResult<UserDto>([], 1, 50, 0));
        userRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((UserDto?)null);

        pendingRepo.GetPendingAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                   .Returns(new PagedResult<PendingAssignmentDto>([], 1, 50, 0));
        pendingRepo.ApproveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        pendingRepo.RejectAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                   .Returns(true);
        pendingRepo.MarkExpiredBatchAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        auditRepo.GetAsync(
                     Arg.Any<string?>(), Arg.Any<AuditAction?>(),
                     Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
                     Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(new PagedResult<AuditEntryDto>([], 1, 50, 0));

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddControllers().AddSentinelControllers();

        // Fake JWT: reads X-Test-Subject header, absent = unauthenticated.
        builder.Services.AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddSentinelAuthorization();

        // Mock repositories (registered after Sentinel DI so they win).
        builder.Services.AddSingleton<IUserRepository>(userRepo);
        builder.Services.AddSingleton<IRoleRepository>(roleRepo);
        builder.Services.AddSingleton<IUserPermissionRepository>(permRepo);
        builder.Services.AddSingleton<IPermissionConflictRepository>(conflictRepo);
        builder.Services.AddSingleton<IPendingAssignmentRepository>(pendingRepo);
        builder.Services.AddSingleton<IAuditRepository>(auditRepo);

        // Test ICurrentActor reads identity from the same header.
        builder.Services.AddScoped<ICurrentActor, TestCurrentActor>();

        var app = builder.Build();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Start();

        var server = app.GetTestServer();

        return new SentinelTestHost(
            userRepo, roleRepo, permRepo, conflictRepo, pendingRepo, auditRepo, server);
    }

    /// <summary>
    /// Creates an HTTP client for the test server.
    /// When <paramref name="subject"/> is provided the client is authenticated as that identity
    /// and the permission repository is configured to return the given permissions for it.
    /// </summary>
    public HttpClient CreateClient(string? subject = null, params string[] permissions)
    {
        if (subject is not null)
            PermRepo.GetPermissionsForUserAsync(subject, Arg.Any<CancellationToken>())
                    .Returns(permissions.ToHashSet());

        var client = _server.CreateClient();
        if (subject is not null)
            client.DefaultRequestHeaders.Add("X-Test-Subject", subject);
        return client;
    }

    public void Dispose() => _server.Dispose();
}
