using Onspay.SharedKernel;

namespace Domain.Orders.Events;

public sealed record OrderCompletedDomainEvent(Guid OrderId) : IDomainEvent;
