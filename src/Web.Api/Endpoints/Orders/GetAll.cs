using Application.Features.Orders.GetAll;
using Application.Features.Orders.Responses;
using Onspay.AspNetCore.Endpoints;
using Onspay.AspNetCore.Extensions;
using Onspay.Cqrs.Messaging;

namespace Web.Api.Endpoints.Orders;

public sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.Map("orders", async (
                IQueryHandler<GetAllOrdersQuery, IReadOnlyList<OrderResponse>> queryHandler,
                int pageIndex, int pageSize, CancellationToken cancellationToken) =>
            {
                var query = new GetAllOrdersQuery()
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
                var response = await queryHandler.Handle(query, cancellationToken);
                return response.ToApiResponse();
            })
            .WithTags(Tags.Orders)
            .WithName("GetAll")
            .Produces<IReadOnlyList<OrderResponse>>();
    }
}