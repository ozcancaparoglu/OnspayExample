using Application.Features.Orders.GetById;
using Application.Features.Orders.Responses;
using Onspay.AspNetCore.Endpoints;
using Onspay.AspNetCore.Extensions;
using Onspay.AspNetCore.Responses;
using Onspay.Cqrs.Messaging;

namespace Web.Api.Endpoints.Orders;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders/{id:guid}", async (
                Guid id,
                IQueryHandler<GetOrderByIdQuery, OrderDetailsResponse> queryHandler,
                CancellationToken cancellationToken) =>
            {
                var response = await queryHandler.Handle(new GetOrderByIdQuery(id), cancellationToken);
                return response.ToApiResponse();
            })
            .WithTags(Tags.Orders)
            .WithName("GetOrderById")
            .Produces<ApiResponse<OrderDetailsResponse>>()
            .Produces<ApiResponse<OrderDetailsResponse>>(StatusCodes.Status404NotFound);
    }
}
