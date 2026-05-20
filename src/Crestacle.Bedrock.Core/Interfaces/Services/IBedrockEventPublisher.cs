namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>
/// Dispatches domain events after a mutation is successfully persisted.
/// Events are always published <em>after</em> <c>SaveChangesAsync</c> succeeds,
/// never inside the same transaction.
/// The default implementation is a no-op; consumers may wire MediatR or a message bus.
/// </summary>
public interface IBedrockEventPublisher
{
    /// <summary>Publishes a domain event to any registered handlers or message-bus subscribers.</summary>
    /// <typeparam name="TEvent">The type of the domain event.</typeparam>
    /// <param name="domainEvent">The event instance to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : class;
}
