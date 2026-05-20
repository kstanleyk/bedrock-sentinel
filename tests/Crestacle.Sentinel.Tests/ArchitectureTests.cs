using System.Reflection;
using Crestacle.Sentinel.Core.Authorization;
using Crestacle.Sentinel.AspNetCore.Controllers;
using Crestacle.Sentinel.EntityFramework.Seeding;

namespace Crestacle.Sentinel.Tests;

/// <summary>
/// Verifies the dependency flow Core ← EntityFramework ← AspNetCore is never violated.
/// These tests act as a compile-time safety net enforced at test-run time via reflection.
/// </summary>
public sealed class ArchitectureTests
{
    private static readonly Assembly CoreAssembly = typeof(SentinelFeature).Assembly;

    // Use a public type from each upper layer to resolve its assembly name at runtime,
    // so the check stays correct even if the assembly name is ever changed.
    private static readonly string EfAssemblyName          = typeof(SentinelSeeder).Assembly.GetName().Name!;
    private static readonly string AspNetCoreAssemblyName  = typeof(UsersController).Assembly.GetName().Name!;

    [Fact]
    public void Core_DoesNotReference_SentinelEntityFramework()
    {
        var referenced = CoreAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty);

        referenced.Should().NotContain(
            EfAssemblyName,
            $"Sentinel.Core must not depend on {EfAssemblyName}");
    }

    [Fact]
    public void Core_DoesNotReference_SentinelAspNetCore()
    {
        var referenced = CoreAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty);

        referenced.Should().NotContain(
            AspNetCoreAssemblyName,
            $"Sentinel.Core must not depend on {AspNetCoreAssemblyName}");
    }

    [Fact]
    public void EntityFramework_DoesNotReference_SentinelAspNetCore()
    {
        var referenced = typeof(SentinelSeeder).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty);

        referenced.Should().NotContain(
            AspNetCoreAssemblyName,
            $"Sentinel.EntityFramework must not depend on {AspNetCoreAssemblyName}");
    }
}
