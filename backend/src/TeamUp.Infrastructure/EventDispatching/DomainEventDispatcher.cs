using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeamUp.Application.Common.Events;
using TeamUp.Domain.Common;

namespace TeamUp.Infrastructure.EventDispatching;

public sealed partial class DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    : IDomainEventDispatcher
{
    private delegate Task EventDispatcherDelegate(
        IServiceProvider serviceProvider,
        IDomainEvent domainEvent,
        ILogger<DomainEventDispatcher> logger,
        CancellationToken cancellationToken);

    private static readonly ConcurrentDictionary<Type, EventDispatcherDelegate> Dispatchers = new();
    private static readonly MethodInfo DispatchTypedMethod =
        typeof(DomainEventDispatcher).GetMethod(nameof(DispatchTypedAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    [LoggerMessage(Level = LogLevel.Error, Message = "Error executing domain event handler {Handler} for event {Event}")]
    private static partial void LogHandlerError(ILogger logger, Exception ex, string handler, string @event);

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var @event in domainEvents)
        {
            var invoker = Dispatchers.GetOrAdd(@event.GetType(), static eventType =>
                DispatchTypedMethod.MakeGenericMethod(eventType).CreateDelegate<EventDispatcherDelegate>());
            await invoker(serviceProvider, @event, logger, cancellationToken);
        }
    }

    private static async Task DispatchTypedAsync<TEvent>(
        IServiceProvider serviceProvider,
        IDomainEvent domainEvent,
        ILogger<DomainEventDispatcher> logger,
        CancellationToken cancellationToken) where TEvent : IDomainEvent
    {
        var typedEvent = (TEvent)domainEvent;
        foreach (var handler in serviceProvider.GetServices<IDomainEventHandler<TEvent>>())
        {
            try
            {
                await handler.HandleAsync(typedEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                LogHandlerError(logger, ex, handler.GetType().Name, typeof(TEvent).Name);
                throw;
            }
        }
    }
}
