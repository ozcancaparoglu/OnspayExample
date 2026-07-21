using Domain.Orders.Helpers;

namespace Application.Features.Orders.Responses;

public record OrderDetailsResponse(
    Guid Id,
    int Number,
    OrderStatus Status,
    string Description,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAt,
    List<OrderItemResponse> Items);
