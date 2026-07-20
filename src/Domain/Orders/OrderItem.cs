using Domain.Orders.ValueObjects;
using Onspay.SharedKernel;

namespace Domain.Orders;

public sealed class OrderItem : AuditableEntity<Guid>
{
    //For Ef Migrations
    private OrderItem() : base(Guid.CreateVersion7())
    {
    }
    
    private OrderItem(Guid id,
        Guid orderId,
        int stock,
        Name name,
        int quantity,
        Money amount) : base(id)
    {
        OrderId = orderId;
        Stock = stock;
        Name = name;
        Quantity = quantity;
        Amount = amount;
    }
    public Guid OrderId { get; private set; }
    public int Stock { get; private set; }
    public Name Name { get; private set; }
    public int Quantity { get; private set; }
    public Money Amount { get; private set; }

    public static OrderItem Create(Guid orderId,
        int stock,
        Name name,
        int quantity,
        Money amount)
    {
        var orderItem = new OrderItem(Guid.CreateVersion7(),
            orderId,
            stock,
            name,
            quantity,
            amount);
        
        return orderItem;
    }
}