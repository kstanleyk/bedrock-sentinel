using Crestacle.Bedrock.AspNetCore;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Crestacle.Bedrock.Redis;

/// <summary>
/// Extension methods for wiring <see cref="RedisBedrockCache"/> into the Bedrock DI pipeline.
/// </summary>
public static class BedrockRedisExtensions
{
    /// <summary>
    /// Replaces the default <see cref="IBedrockCache"/> registration with a Redis-backed
    /// implementation that distributes cache entries across all pods via StackExchange.Redis.
    /// </summary>
    /// <remarks>
    /// Call this <em>after</em> <c>AddBedrockAspNetCore()</c>. The provided
    /// <see cref="IConnectionMultiplexer"/> is registered as a singleton; pass an already-created
    /// multiplexer to reuse a shared connection.
    /// </remarks>
    /// <param name="builder">The Bedrock builder returned by <c>AddBedrockAspNetCore()</c>.</param>
    /// <param name="multiplexer">
    /// A connected <see cref="IConnectionMultiplexer"/> instance (e.g. from
    /// <c>ConnectionMultiplexer.Connect("localhost")</c>).
    /// </param>
    /// <returns>The same builder instance for chaining.</returns>
    public static IBedrockBuilder WithRedisCache(
        this IBedrockBuilder builder,
        IConnectionMultiplexer multiplexer)
    {
        builder.Services.AddSingleton(multiplexer);

        // Replace the default IBedrockCache (NullBedrockCache or MemoryBedrockCache)
        var descriptor = builder.Services
            .LastOrDefault(d => d.ServiceType == typeof(IBedrockCache));

        if (descriptor is not null)
            builder.Services.Remove(descriptor);

        builder.Services.AddSingleton<IBedrockCache>(
            sp => new RedisBedrockCache(sp.GetRequiredService<IConnectionMultiplexer>()));

        return builder;
    }

    /// <summary>
    /// Replaces the default <see cref="IBedrockCache"/> registration with a Redis-backed
    /// implementation. The connection is established lazily using the provided
    /// <paramref name="configuration"/> string.
    /// </summary>
    /// <param name="builder">The Bedrock builder returned by <c>AddBedrockAspNetCore()</c>.</param>
    /// <param name="configuration">
    /// A StackExchange.Redis connection string (e.g. <c>"localhost:6379"</c> or a full
    /// configuration string with options).
    /// </param>
    /// <returns>The same builder instance for chaining.</returns>
    public static IBedrockBuilder WithRedisCache(
        this IBedrockBuilder builder,
        string configuration)
    {
        builder.Services.AddSingleton<IConnectionMultiplexer>(
            _ => ConnectionMultiplexer.Connect(configuration));

        var descriptor = builder.Services
            .LastOrDefault(d => d.ServiceType == typeof(IBedrockCache));

        if (descriptor is not null)
            builder.Services.Remove(descriptor);

        builder.Services.AddSingleton<IBedrockCache>(
            sp => new RedisBedrockCache(sp.GetRequiredService<IConnectionMultiplexer>()));

        return builder;
    }
}
