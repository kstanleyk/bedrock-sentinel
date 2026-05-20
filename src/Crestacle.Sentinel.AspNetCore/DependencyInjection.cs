using Crestacle.Sentinel.AspNetCore.Authorization;
using Crestacle.Sentinel.AspNetCore.BackgroundServices;
using Crestacle.Sentinel.AspNetCore.Controllers;
using Crestacle.Sentinel.AspNetCore.Services;
using Crestacle.Sentinel.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Crestacle.Sentinel.AspNetCore;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Sentinel permission policy provider, authorization handler,
    /// IHttpContextAccessor, and ICurrentActor (used by repositories to stamp audit entries).
    /// Call this alongside AddAuthorization() in Program.cs.
    /// </summary>
    public static IServiceCollection AddSentinelAuthorization(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentActor, CurrentActorService>();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Background service that marks expired PendingAssignments as Expired.
        services.AddHostedService<SentinelExpiryService>();

        return services;
    }

    /// <summary>
    /// Registers the Sentinel built-in controllers (Roles, Users, Permissions).
    /// Call this on the IMvcBuilder returned by AddControllers():
    ///   builder.Services.AddControllers().AddSentinelControllers();
    /// </summary>
    public static IMvcBuilder AddSentinelControllers(this IMvcBuilder builder)
    {
        builder.AddApplicationPart(typeof(RolesController).Assembly);
        return builder;
    }
}
