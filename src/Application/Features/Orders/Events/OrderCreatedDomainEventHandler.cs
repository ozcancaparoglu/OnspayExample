using Domain.Orders.Events;
using Microsoft.Extensions.Logging;
using Onspay.SharedKernel;

namespace Application.Features.Orders.Events;

internal sealed class OrderCreatedDomainEventHandler(
    ILogger<OrderCreatedDomainEventHandler> logger)
: IDomainEventHandler<OrderCreatedDomainEvent>
{
    public Task Handle(OrderCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Order created with ID: {OrderId}", domainEvent.OrderId);
        
        //TODO: Do what you want here this will work after order.created method.
        
        return Task.CompletedTask;
    }
}