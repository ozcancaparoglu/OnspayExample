using Application.Features.Orders.Responses;
using Onspay.Cqrs.Messaging;

namespace Application.Features.Orders.GetAll;

public sealed record GetAllOrdersQuery() : IQuery<IReadOnlyList<OrderResponse>>
{
    public int PageIndex { get; init; } = 0;
    public int PageSize { get; init; } = 100;
}