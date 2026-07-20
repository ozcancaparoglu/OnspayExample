using Domain.Orders.Events;
using Domain.Orders.Helpers;
using Domain.Orders.ValueObjects;
using Onspay.SharedKernel;

namespace Domain.Orders;

public sealed class Order : AuditableEntity<Guid>
{
    //For Ef Migrations
    private Order() : base(Guid.CreateVersion7())
    {
    }
    
    private Order(Guid id, 
        int number, 
        OrderStatus status, 
        Description description, 
        IReadOnlyList<OrderItem> orderItems) : base(id)
    {
        Number = number;
        Status = status;
        Description = description;
        OrderItems = orderItems;
        TotalAmount = CalculateTotalAmount();
    }
    
    public int Number { get; private set; }
    public OrderStatus Status { get; private set; }
    public Description Description { get; private set; }
    public Money TotalAmount { get; private set; }
    private IReadOnlyList<OrderItem> OrderItems { get; }

    public static Order Create(int number, 
        OrderStatus status, 
        Description description, 
        IReadOnlyList<OrderItem> orderItems)
    {
        var order = new Order(Guid.CreateVersion7(),
            number, 
            status, 
            description, 
            orderItems);
        
        order.Raise(new OrderCreatedDomainEvent(order.Id));
        
        return order;
    }

    private Money CalculateTotalAmount()
    {
        var totalAmount = OrderItems.Sum(item => item.Amount.Amount * item.Quantity);
        return new Money(totalAmount, Currency.TRY);
    }
}