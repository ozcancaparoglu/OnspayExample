using Domain.Orders.Helpers;
using Onspay.Cqrs.Messaging;

namespace Application.Features.Orders.Create;

public record CreateOrderCommand(
    int OrderNumber,
    OrderStatus Status,
    string? Description,
    List<CreateOrderItemCommand> OrderItems) : ICommand<Guid>;


public record CreateOrderItemCommand(string Name, int Quantity, decimal Price);
    
    
