using Application.Features.Orders.Responses;
using Onspay.Infrastructure.Caching.Redis.Cqrs;

namespace Application.Features.Orders.GetById;

/// <summary>
/// <see cref="ICachedQuery{TResponse}"/> makes Onspay.Infrastructure.Caching.Redis's CachingDecorator serve
/// this from Redis (registered by <c>AddQueryCaching()</c>). Writers evict <see cref="CacheKeyFor"/>.
/// </summary>
public sealed record GetOrderByIdQuery(Guid Id) : ICachedQuery<OrderDetailsResponse>
{
    public static string CacheKeyFor(Guid id) => $"orders:{id}";

    public string CacheKey => CacheKeyFor(Id);

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}
