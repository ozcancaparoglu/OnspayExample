using Domain.Orders.Helpers;

namespace Application.Features.Orders.Responses;

public sealed record OrderResponse(
    Guid Id,
    int OrderNumber,
    OrderStatus OrderStatus,
    DateTime OrderDate,
    decimal TotalPrice);