using Crestacle.Bedrock.AspNetCore.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Crestacle.Bedrock.AspNetCore.Extensions;

/// <summary>Extension methods for registering Bedrock health checks.</summary>
public static class BedrockHealthChecksExtensions
{
    /// <summary>
    /// Adds the Bedrock cache health check tagged <c>bedrock</c> and <c>cache</c>.
    /// Pair with <c>AddBedrockDbHealthCheck()</c> from <c>Crestacle.Bedrock.EntityFramework</c>
    /// to also cover database connectivity.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static IHealthChecksBuilder AddBedrockCacheHealthCheck(this IHealthChecksBuilder builder)
    {
        builder.AddCheck<BedrockCacheHealthCheck>(
            "bedrock-cache",
            tags: ["bedrock", "cache"]);
        return builder;
    }

    /// <summary>
    /// Opt-in convenience: calls <c>services.AddHealthChecks().AddBedrockCacheHealthCheck()</c>
    /// on the builder's service collection.
    /// Pair the result with <c>AddBedrockDbHealthCheck()</c> from
    /// <c>Crestacle.Bedrock.EntityFramework</c> to also cover database connectivity.
    /// </summary>
    /// <param name="builder">The Bedrock builder.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static IBedrockBuilder WithHealthChecks(this IBedrockBuilder builder)
    {
        builder.Services.AddHealthChecks().AddBedrockCacheHealthCheck();
        return builder;
    }
}
