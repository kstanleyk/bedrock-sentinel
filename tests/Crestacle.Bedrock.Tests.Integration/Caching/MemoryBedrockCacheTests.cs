using Crestacle.Bedrock.EntityFramework.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Crestacle.Bedrock.Tests.Integration.Caching;

public sealed class MemoryBedrockCacheTests : IDisposable
{
    private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
    private readonly MemoryBedrockCache _cache;

    public MemoryBedrockCacheTests()
    {
        _cache = new MemoryBedrockCache(_memoryCache);
    }

    [Fact]
    public async Task SetAsync_ThenGetAsync_ReturnsStoredValue()
    {
        await _cache.SetAsync("key1", "value1", TimeSpan.FromMinutes(5));

        var result = await _cache.GetAsync("key1");

        result.Should().Be("value1");
    }

    [Fact]
    public async Task GetAsync_UnknownKey_ReturnsNull()
    {
        var result = await _cache.GetAsync("no-such-key");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_AfterSet_ReturnsTrue()
    {
        await _cache.SetAsync("exists-key", "v", TimeSpan.FromMinutes(1));

        var exists = await _cache.ExistsAsync("exists-key");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_UnknownKey_ReturnsFalse()
    {
        var exists = await _cache.ExistsAsync("ghost");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ExistingKey_KeyNoLongerExists()
    {
        await _cache.SetAsync("remove-me", "val", TimeSpan.FromMinutes(1));

        await _cache.RemoveAsync("remove-me");

        var exists = await _cache.ExistsAsync("remove-me");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task SetAsync_Expired_ReturnsNull()
    {
        await _cache.SetAsync("expiring", "v", TimeSpan.FromMilliseconds(1));
        await Task.Delay(50);

        var result = await _cache.GetAsync("expiring");

        result.Should().BeNull();
    }

    public void Dispose() => _memoryCache.Dispose();
}
