using Onspay.Cqrs.Messaging;

namespace Application.Features.Orders.Create;

public sealed record CreateOrderCommand(
    int OrderNumber,
    string? Description,
    IReadOnlyList<CreateOrderItemCommand> OrderItems) : ICommand<Guid>;

public sealed record CreateOrderItemCommand(string Name, int Quantity, decimal Price);
