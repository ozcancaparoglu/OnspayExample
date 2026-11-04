namespace Application.Features.Orders.IntegrationEvents;

/// <summary>
/// Public contract published to RabbitMQ when an order is created. Unlike the domain event, this is what
/// other services see, so it carries the data they need and must stay backwards compatible.
/// </summary>
public sealed record OrderCreatedIntegrationEvent(
    Guid OrderId,
    int OrderNumber,
    decimal TotalAmount,
    string Currency,
    DateTime OccurredOnUtc)
{
    // Travels as the AMQP "type" property and the inbox "message_type" column; inbox handlers match on it.
    public const string MessageType = "onspay-example.order-created";
}
