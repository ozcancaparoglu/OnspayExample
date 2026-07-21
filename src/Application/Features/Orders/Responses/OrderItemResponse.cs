namespace Application.Features.Orders.Responses;

public record OrderItemResponse(
    Guid Id,
    string ProductName,
    int Quantity,
    decimal Price,
    string Currency);
