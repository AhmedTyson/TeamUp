using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeamUp.Application.Common.Events;
using TeamUp.Domain.Common;

namespace TeamUp.Infrastructure.EventDispatching;

public sealed class DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var @event in domainEvents)
        {
            var eventType = @event.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            var handlers = serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                if (handler is null) continue;

                try
                {
                    var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync));
                    if (method is not null)
                    {
                        var task = (Task)method.Invoke(handler, [@event, cancellationToken])!;
                        await task;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error executing domain event handler {Handler} for event {Event}",
                        handler.GetType().Name, eventType.Name);
                    throw;
                }
            }
        }
    }
}
