using Application.Features.Orders.GetAll;
using Application.Features.Orders.Responses;
using Onspay.AspNetCore.Endpoints;
using Onspay.AspNetCore.Extensions;
using Onspay.AspNetCore.Responses;
using Onspay.Cqrs.Messaging;

namespace Web.Api.Endpoints.Orders;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders", async (
                IQueryHandler<GetAllOrdersQuery, IReadOnlyList<OrderResponse>> queryHandler,
                CancellationToken cancellationToken,
                int pageIndex = 0,
                int pageSize = 100) =>
            {
                var query = new GetAllOrdersQuery
                {
                    PageIndex = Math.Max(pageIndex, 0),
                    PageSize = Math.Clamp(pageSize, 1, 500)
                };
                var response = await queryHandler.Handle(query, cancellationToken);
                return response.ToApiResponse();
            })
            .WithTags(Tags.Orders)
            .WithName("GetAllOrders")
            .Produces<ApiResponse<IReadOnlyList<OrderResponse>>>();
    }
}
