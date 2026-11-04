namespace Application.Abstractions.Messaging;

/// <summary>
/// Exchange names this service publishes to. They must match the "Exchange" values under
/// "RabbitMq:&lt;bus&gt;:Queues" in appsettings.json. A target without "bus:" goes to the bus marked IsDefault.
/// </summary>
public static class MessagingTopology
{
    public const string OrderEventsExchange = "onspay-example.order-events";
}
