using Application.Abstractions.Data;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Onspay.Infrastructure.DomainEvents;
using Onspay.Infrastructure.Inbox;
using Onspay.Infrastructure.Outbox;
using Onspay.SharedKernel;

namespace Infrastructure.Database;

/// <summary>
/// DomainEventDbContextBase stamps CreatedAt/UpdatedAt and dispatches domain events after SaveChanges.
/// IInboxDbContext is what the RabbitMQ consumer writes received messages through (IInboxWriter);
/// IOutboxDbContext / OutboxMessages is where command handlers enqueue integration events.
/// </summary>
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventDispatcher dispatcher,
    IDateTimeProvider dateTimeProvider)
    : DomainEventDbContextBase(options, dispatcher, dateTimeProvider),
        IApplicationDbContext, IInboxDbContext, IOutboxDbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Shipped by the Onspay packages; the processing jobs query these tables with raw SQL.
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}
