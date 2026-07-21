using Application.Features.Orders.GetById;
using Application.Features.Orders.Responses;
using Onspay.AspNetCore.Endpoints;
using Onspay.AspNetCore.Extensions;
using Onspay.Cqrs.Messaging;

namespace Web.Api.Endpoints.Orders;

public sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.Map("orders/{id:guid}", async (
                Guid id,
                IQueryHandler<GetOrderByIdQuery, OrderDetailsResponse> queryHandler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetOrderByIdQuery(id);
                var response = await queryHandler.Handle(query, cancellationToken);
                return response.ToApiResponse();
            })
            .WithTags(Tags.Orders)
            .WithName("GetOrderById")
            .Produces<OrderDetailsResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
