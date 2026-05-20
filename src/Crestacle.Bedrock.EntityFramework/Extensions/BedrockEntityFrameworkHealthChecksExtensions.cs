using Crestacle.Bedrock.EntityFramework.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Crestacle.Bedrock.EntityFramework.Extensions;

/// <summary>Extension methods for registering the Bedrock database health check.</summary>
public static class BedrockEntityFrameworkHealthChecksExtensions
{
    /// <summary>
    /// Adds the Bedrock database health check tagged <c>bedrock</c> and <c>db</c>.
    /// The check calls <c>CanConnectAsync</c> against the registered <see cref="BedrockContext"/>.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static IHealthChecksBuilder AddBedrockDbHealthCheck(this IHealthChecksBuilder builder)
    {
        builder.AddCheck<BedrockDbHealthCheck>(
            "bedrock-db",
            tags: ["bedrock", "db"]);
        return builder;
    }
}
