using Application.Abstractions.Data;
using Domain.Orders;
using Domain.Orders.Helpers;
using Domain.Orders.ValueObjects;
using Microsoft.Extensions.Logging;
using Onspay.Cqrs.Messaging;
using Onspay.SharedKernel;

namespace Application.Features.Orders.Create;

internal sealed class CreateOrderCommandHandler(
    ILogger<CreateOrderCommandHandler> logger,
    IApplicationDbContext context) : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var description = Description.Empty;
        if (!string.IsNullOrWhiteSpace(command.Description))
            description = Description.ResolveDescription(command.Description).Value;

        var orderItems = new List<OrderItem>();

        foreach (var orderItem in command.OrderItems)
        {
            orderItems.Add(OrderItem.Create(
                    orderId: Guid.NewGuid(),
                    productName: Name.Create(orderItem.Name).Value,
                    quantity: orderItem.Quantity,
                    amount: new Money(orderItem.Price, Currency.FromCode("TR"))
                )
            );
        }
        
        var order = Order.Create(command.OrderNumber, OrderStatus.Pending, description, orderItems);
        
        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Creating order with number: {OrderNumber} and {ItemCount} items.", order.Number, orderItems.Count);
        
        return Result.Success(order.Id);
    }
}