using Application.Features.Orders.Responses;
using Dapper;
using Domain.Orders.Helpers;
using Onspay.Cqrs.Messaging;
using Onspay.SharedKernel;

namespace Application.Features.Orders.GetById;

internal sealed class GetOrderByIdQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetOrderByIdQuery, OrderDetailsResponse>
{
    public async Task<Result<OrderDetailsResponse>> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                o.id AS "Id",
                o.number AS "Number",
                o.status AS "Status",
                o."description" AS "Description",
                o.total_amount_amount AS "TotalAmount",
                o.total_amount_currency AS "Currency",
                o.created_at AS "CreatedAt",
                oi.id AS "Id",
                oi."product_name" AS "ProductName",
                oi.quantity AS "Quantity",
                oi.amount_amount AS "Price",
                oi.amount_currency AS "Currency"
            FROM public.orders o
            LEFT JOIN public."order-items" oi ON oi.order_id = o.id
            WHERE o.id = @Id
            """;

        var orders = new Dictionary<Guid, OrderDetailsResponse>();

        await connection.QueryAsync<OrderDetailsResponse, OrderItemResponse, OrderDetailsResponse>(
            sql,
            (order, item) =>
            {
                if (!orders.TryGetValue(order.Id, out var existingOrder))
                {
                    existingOrder = order with { Items = [] };
                    orders.Add(existingOrder.Id, existingOrder);
                }

                if (item.Id != Guid.Empty)
                    existingOrder.Items.Add(item);

                return existingOrder;
            },
            new { query.Id },
            splitOn: "Id");

        return orders.TryGetValue(query.Id, out var orderResponse)
            ? orderResponse
            : Result.Failure<OrderDetailsResponse>(OrderErrors.NotFound(query.Id));
    }
}
