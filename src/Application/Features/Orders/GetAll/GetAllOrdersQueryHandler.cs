using Application.Features.Orders.Responses;
using Dapper;
using Onspay.Cqrs.Messaging;
using Onspay.SharedKernel;

namespace Application.Features.Orders.GetAll;

internal sealed class GetAllOrdersQueryHandler(ISqlConnectionFactory sqlConnectionFactory) 
    : IQueryHandler<GetAllOrdersQuery, IReadOnlyList<OrderResponse>>
{
    public async Task<Result<IReadOnlyList<OrderResponse>>> Handle(GetAllOrdersQuery query, CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                o.number AS "OrderNumber",
                o.status AS "OrderStatus",
                o.created_at AS "OrderDate",
                o.total_amount_amount AS "TotalPrice"
            FROM public.orders o
            ORDER BY o.created_at
            LIMIT @PageSize OFFSET @PageIndex * @PageSize
            """;
        
        var orders = await connection.QueryAsync<OrderResponse>(sql, new
        {
            query.PageIndex,
            query.PageSize
        });

        return orders.ToList();
    }
}