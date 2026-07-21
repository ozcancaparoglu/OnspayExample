using Domain.Orders.Helpers;

namespace Application.Features.Orders.Responses;

public record OrderResponse(
    int OrderNumber,
    OrderStatus OrderStatus,
    DateTime OrderDate,
    decimal TotalPrice);