using Crestacle.Bedrock.Core.Interfaces.Services;

namespace Crestacle.Bedrock.Core.Defaults;

/// <summary>
/// No-op event publisher. Silently discards all domain events.
/// Replace with a MediatR or message-bus implementation to process events.
/// </summary>
public sealed class NullBedrockEventPublisher : IBedrockEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : class
        => Task.CompletedTask;
}
