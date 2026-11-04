using System.Text.Json;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Features.Orders.IntegrationEvents;
using Domain.Orders;
using Domain.Orders.Helpers;
using Domain.Orders.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Onspay.Cqrs.Messaging;
using Onspay.Infrastructure.Correlation;
using Onspay.Infrastructure.Outbox;
using Onspay.SharedKernel;

namespace Application.Features.Orders.Create;

internal sealed class CreateOrderCommandHandler(
    IApplicationDbContext context,
    ICorrelationAccessor correlationAccessor,
    IDateTimeProvider dateTimeProvider,
    ILogger<CreateOrderCommandHandler> logger) : ICommandHandler<CreateOrderCommand, Guid>
{
    private static readonly Currency OrderCurrency = Currency.TRY;

    public async Task<Result<Guid>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        if (await context.Orders.AnyAsync(o => o.Number == command.OrderNumber, cancellationToken))
            return Result.Failure<Guid>(OrderErrors.DuplicateNumber(command.OrderNumber));

        var description = Description.ResolveDescription(command.Description);
        if (description.IsFailure)
            return Result.Failure<Guid>(description.Error);

        var order = Order.Create(command.OrderNumber, description.Value, OrderCurrency);

        foreach (var item in command.OrderItems)
        {
            var name = Name.Create(item.Name);
            if (name.IsFailure)
                return Result.Failure<Guid>(name.Error);

            var added = order.AddItem(name.Value, item.Quantity, new Money(item.Price, OrderCurrency));
            if (added.IsFailure)
                return Result.Failure<Guid>(added.Error);
        }

        context.Orders.Add(order);

        // Transactional outbox: the message row commits in the same SaveChanges as the order, so the event
        // is published if and only if the order exists. The correlation id is stored with it and restored
        // when the outbox job publishes, so the consumer's logs share this request's correlation id.
        var integrationEvent = new OrderCreatedIntegrationEvent(
            order.Id,
            order.Number,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency.Code,
            dateTimeProvider.UtcNow);

        context.OutboxMessages.Add(OutboxMessage.Create(
            OrderCreatedIntegrationEvent.MessageType,
            JsonSerializer.Serialize(integrationEvent),
            target: MessagingTopology.OrderEventsExchange,
            correlationId: correlationAccessor.CorrelationId));

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created order {OrderNumber} ({OrderId}) with {ItemCount} items",
            order.Number, order.Id, order.OrderItems.Count);

        return order.Id;
    }
}
