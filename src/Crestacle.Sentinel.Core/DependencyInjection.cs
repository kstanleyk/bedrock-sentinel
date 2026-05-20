using Microsoft.Extensions.DependencyInjection;

namespace Crestacle.Sentinel.Core;

public static class DependencyInjection
{
    /// <summary>Registers Sentinel Core services (no-op — interfaces are registered by downstream packages).</summary>
    public static IServiceCollection AddSentinelCore(this IServiceCollection services)
        => services;
}
