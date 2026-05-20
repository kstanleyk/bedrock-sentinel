using Microsoft.Extensions.DependencyInjection;

namespace Crestacle.Bedrock.AspNetCore;

/// <summary>Fluent builder returned from <c>AddBedrock&lt;TContext&gt;()</c>.</summary>
public interface IBedrockBuilder
{
    IServiceCollection Services { get; }
}
