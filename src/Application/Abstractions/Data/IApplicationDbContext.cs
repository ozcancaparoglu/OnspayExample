using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Onspay.Infrastructure.Outbox;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }

    // Integration events are written here in the same SaveChanges as the aggregate (transactional outbox);
    // Onspay.Infrastructure.Outbox's ProcessOutboxMessagesJob relays them to RabbitMQ.
    DbSet<OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
