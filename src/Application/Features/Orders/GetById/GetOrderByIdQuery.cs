using Application.Features.Orders.Responses;
using Onspay.Cqrs.Messaging;

namespace Application.Features.Orders.GetById;

public sealed record GetOrderByIdQuery(Guid Id) : IQuery<OrderDetailsResponse>;
