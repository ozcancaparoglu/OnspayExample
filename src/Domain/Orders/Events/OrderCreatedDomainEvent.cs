using Onspay.SharedKernel;

namespace Domain.Orders.Events;

public sealed record OrderCreatedDomainEvent(Guid OrderId) : IDomainEvent;