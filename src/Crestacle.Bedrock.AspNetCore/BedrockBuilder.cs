using Microsoft.Extensions.DependencyInjection;

namespace Crestacle.Bedrock.AspNetCore;

internal sealed class BedrockBuilder : IBedrockBuilder
{
    public BedrockBuilder(IServiceCollection services) => Services = services;
    public IServiceCollection Services { get; }
}
