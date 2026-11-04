using Application.Abstractions.Data;
using Application.Features.Orders.GetById;
using Domain.Orders.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Onspay.Infrastructure.Caching.Redis;
using Onspay.Infrastructure.Inbox;
using Onspay.SharedKernel;

namespace Application.Features.Orders.IntegrationEvents;

/// <summary>
/// Consumes <see cref="OrderCreatedIntegrationEvent"/> from the inbox. In a real system another service
/// (payments, fulfilment…) would consume it; here the same service does, standing in for that downstream
/// step, so the whole Outbox → RabbitMQ → Inbox loop is visible: the order flips from Pending to Completed a
/// few seconds after it is created.
/// <para>
/// Inbox handlers are registered by <c>AddCqrs</c> (assembly scan) and must be idempotent — the inbox
/// de-duplicates on message id, but a handler can still run again after a failure part-way through.
/// Returning a failed <see cref="Result"/> makes ProcessInboxMessagesJob retry with backoff.
/// </para>
/// </summary>
internal sealed class CompleteOrderOnOrderCreatedHandler(
    IApplicationDbContext context,
    ICacheService cacheService,
    ILogger<CompleteOrderOnOrderCreatedHandler> logger)
    : InboxMessageHandlerBase<OrderCreatedIntegrationEvent>
{
    public override string MessageType => OrderCreatedIntegrationEvent.MessageType;

    public override async Task<Result> HandleAsync(
        OrderCreatedIntegrationEvent message,
        CancellationToken cancellationToken)
    {
        var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == message.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(OrderErrors.NotFound(message.OrderId));

        var result = order.Complete();
        if (result.IsFailure)
            return result;

        await context.SaveChangesAsync(cancellationToken);

        // The read side caches order details; evict so the next GET sees the new status.
        await cacheService.RemoveAsync(GetOrderByIdQuery.CacheKeyFor(order.Id), cancellationToken);

        logger.LogInformation("Order {OrderId} completed from inbox message", order.Id);

        return Result.Success();
    }
}
