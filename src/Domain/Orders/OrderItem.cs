using Domain.Orders.ValueObjects;
using Onspay.SharedKernel;

namespace Domain.Orders;

public sealed class OrderItem : AuditableEntity<Guid>
{
    // For EF Core materialization
    private OrderItem() : base(Guid.Empty)
    {
    }

    private OrderItem(Guid id, Guid orderId, Name productName, int quantity, Money price) : base(id)
    {
        OrderId = orderId;
        ProductName = productName;
        Quantity = quantity;
        Price = price;
    }

    public Guid OrderId { get; private set; }
    public Name ProductName { get; private set; } = null!;
    public int Quantity { get; private set; }
    public Money Price { get; private set; } = null!;

    // Only the Order aggregate creates items, so the OrderId is always the owning order's.
    internal static OrderItem Create(Guid orderId, Name productName, int quantity, Money price) =>
        new(Guid.CreateVersion7(), orderId, productName, quantity, price);
}
