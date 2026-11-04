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
                o.id AS Id,
                o.number AS Number,
                o.status AS Status,
                o.description AS Description,
                o.total_amount_amount AS TotalAmount,
                o.total_amount_currency AS Currency,
                o.created_at AS CreatedAt,
                oi.id AS ItemId,
                oi.product_name AS ProductName,
                oi.quantity AS Quantity,
                oi.price_amount AS Price,
                oi.price_currency AS Currency
            FROM public.orders o
            LEFT JOIN public.order_items oi ON oi.order_id = o.id
            WHERE o.id = @Id
            ORDER BY oi.created_at
            """;

        OrderRow? order = null;
        var items = new List<OrderItemResponse>();

        // Dapper maps the flat rows into mutable row types (a response record whose constructor also takes
        // Items can't be materialized); the response is assembled afterwards.
        await connection.QueryAsync<OrderRow, OrderItemRow?, OrderRow>(
            new CommandDefinition(sql, new { query.Id }, cancellationToken: cancellationToken),
            (orderRow, itemRow) =>
            {
                order ??= orderRow;

                if (itemRow is not null)
                    items.Add(new OrderItemResponse(
                        itemRow.ItemId, itemRow.ProductName, itemRow.Quantity, itemRow.Price, itemRow.Currency));

                return orderRow;
            },
            splitOn: "ItemId");

        if (order is null)
            return Result.Failure<OrderDetailsResponse>(OrderErrors.NotFound(query.Id));

        return new OrderDetailsResponse(
            order.Id,
            order.Number,
            (OrderStatus)order.Status,
            order.Description,
            order.TotalAmount,
            order.Currency,
            order.CreatedAt,
            items);
    }

    private sealed class OrderRow
    {
        public Guid Id { get; init; }
        public int Number { get; init; }
        public int Status { get; init; }
        public string Description { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }

    private sealed class OrderItemRow
    {
        public Guid ItemId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal Price { get; init; }
        public string Currency { get; init; } = string.Empty;
    }
}
