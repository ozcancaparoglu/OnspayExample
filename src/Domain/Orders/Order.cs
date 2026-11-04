using Domain.Orders.Events;
using Domain.Orders.Helpers;
using Domain.Orders.ValueObjects;
using Onspay.SharedKernel;

namespace Domain.Orders;

public sealed class Order : AuditableEntity<Guid>
{
    private readonly List<OrderItem> _orderItems = [];

    // For EF Core materialization
    private Order() : base(Guid.Empty)
    {
    }

    private Order(Guid id, int number, Description description, Currency currency) : base(id)
    {
        Number = number;
        Status = OrderStatus.Pending;
        Description = description;
        TotalAmount = Money.Zero(currency);
    }

    public int Number { get; private set; }
    public OrderStatus Status { get; private set; }
    public Description Description { get; private set; } = Description.Empty;
    public Money TotalAmount { get; private set; } = Money.Zero();
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public static Order Create(int number, Description description, Currency currency)
    {
        var order = new Order(Guid.CreateVersion7(), number, description, currency);

        order.Raise(new OrderCreatedDomainEvent(order.Id));

        return order;
    }

    public Result AddItem(Name productName, int quantity, Money price)
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure(OrderErrors.NotPending(Id));

        if (quantity <= 0)
            return Result.Failure(OrderErrors.InvalidQuantity);

        var lineTotal = price with { Amount = price.Amount * quantity };
        var newTotal = TotalAmount + lineTotal;
        if (newTotal.IsFailure)
            return newTotal;

        _orderItems.Add(OrderItem.Create(Id, productName, quantity, price));
        TotalAmount = newTotal.Value;

        return Result.Success();
    }

    public Result Complete()
    {
        if (Status == OrderStatus.Completed)
            return Result.Success(); // idempotent: redelivered messages must not fail

        if (Status != OrderStatus.Pending)
            return Result.Failure(OrderErrors.NotPending(Id));

        Status = OrderStatus.Completed;
        Raise(new OrderCompletedDomainEvent(Id));

        return Result.Success();
    }
}
