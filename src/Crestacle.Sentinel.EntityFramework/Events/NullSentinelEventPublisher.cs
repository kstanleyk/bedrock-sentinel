using Crestacle.Sentinel.Core.Interfaces;

namespace Crestacle.Sentinel.EntityFramework.Events;

/// <summary>
/// Default no-op publisher — discards all events silently.
/// Replace by registering your own <see cref="ISentinelEventPublisher"/> after
/// calling <c>AddSentinelRepositories</c>:
/// <code>
///   services.AddSentinelRepositories&lt;MyContext&gt;();
///   services.AddScoped&lt;ISentinelEventPublisher, MyMediatRPublisher&gt;();
/// </code>
/// </summary>
internal sealed class NullSentinelEventPublisher : ISentinelEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent evt, CancellationToken ct = default)
        where TEvent : class
        => Task.CompletedTask;
}
