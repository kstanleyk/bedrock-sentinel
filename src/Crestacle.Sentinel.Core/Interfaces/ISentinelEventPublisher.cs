namespace Crestacle.Sentinel.Core.Interfaces;

/// <summary>
/// Publishes Sentinel domain events to the host application.
/// Register your own implementation (MediatR, MassTransit, custom) to react to role changes.
/// The default registration is a no-op that silently discards all events.
/// </summary>
public interface ISentinelEventPublisher
{
    Task PublishAsync<TEvent>(TEvent evt, CancellationToken ct = default) where TEvent : class;
}
