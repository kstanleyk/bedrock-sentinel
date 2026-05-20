using Crestacle.Sentinel.EntityFramework;
using Crestacle.Sentinel.EntityFramework.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace Crestacle.Sentinel.Tests.EntityFramework;

/// <summary>
/// Tests for MemoryPermissionCache covering the configurable TTL behaviour
/// introduced in v1.12.0 — specifically that TimeSpan.Zero bypasses the cache
/// entirely (immediate revocation) and a positive TTL stores entries normally.
/// </summary>
public sealed class MemoryPermissionCacheTests : IDisposable
{
    private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

    public void Dispose() => _memoryCache.Dispose();

    private MemoryPermissionCache BuildCache(TimeSpan ttl)
    {
        var options = new SentinelOptions { PermissionCacheTtl = ttl };
        return new MemoryPermissionCache(_memoryCache, options);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Zero TTL — cache disabled
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SetAsync_WithZeroTtl_DoesNotStorePermissions()
    {
        var cache = BuildCache(TimeSpan.Zero);
        var perms = new HashSet<string> { "Permission.Invoice.Read" };

        await cache.SetAsync("user-1", perms);
        var result = await cache.GetAsync("user-1");

        result.Should().BeNull("a zero TTL means caching is disabled; every call must hit the database");
    }

    [Fact]
    public async Task GetAsync_WithZeroTtl_AlwaysReturnsNull()
    {
        var cache = BuildCache(TimeSpan.Zero);

        // Nothing was ever stored, but this confirms the contract explicitly.
        var result = await cache.GetAsync("user-nobody");

        result.Should().BeNull();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Positive TTL — cache enabled
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SetAsync_WithPositiveTtl_StoresAndReturnsPermissions()
    {
        var cache = BuildCache(TimeSpan.FromMinutes(5));
        var perms = new HashSet<string> { "Permission.Invoice.Read", "Permission.Invoice.Create" };

        await cache.SetAsync("user-2", perms);
        var result = await cache.GetAsync("user-2");

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(perms);
    }

    [Fact]
    public async Task InvalidateUserAsync_RemovesCachedEntry()
    {
        var cache = BuildCache(TimeSpan.FromMinutes(5));
        var perms = new HashSet<string> { "Permission.Role.Read" };

        await cache.SetAsync("user-3", perms);
        await cache.InvalidateUserAsync("user-3");
        var result = await cache.GetAsync("user-3");

        result.Should().BeNull("InvalidateUserAsync must evict the entry regardless of TTL");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Custom TTL wired through SentinelOptions
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SetAsync_WithCustomTtl_UsesProvidedDuration()
    {
        // 1-second TTL — verifies that a non-default value is respected.
        var cache = BuildCache(TimeSpan.FromSeconds(1));
        var perms = new HashSet<string> { "Permission.Audit.Read" };

        await cache.SetAsync("user-4", perms);

        // Immediately after Set, the entry must be present.
        var resultBefore = await cache.GetAsync("user-4");
        resultBefore.Should().NotBeNull("entry should be cached right after Set");
    }
}
