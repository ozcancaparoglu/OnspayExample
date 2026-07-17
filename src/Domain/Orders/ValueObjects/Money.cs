using Domain.Orders.Helpers;
using Onspay.SharedKernel;

namespace Domain.Orders.ValueObjects;

public record Money(decimal Amount, Currency Currency)
{
    public static Result<Money> operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            return Result.Failure<Money>(OrderErrors.CannotAddDifferentCurrency);

        return a with { Amount = a.Amount + b.Amount };
    }

    public static Result<Money> operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            return Result.Failure<Money>(OrderErrors.CannotSubtractDifferentCurrency);

        return a with { Amount = a.Amount - b.Amount };
    }
    
    public static Money Zero() => new(0, Currency.None);
    public static Money Zero(Currency currency) => new(0, currency);
}