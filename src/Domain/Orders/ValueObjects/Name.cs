using Domain.Orders.Helpers;
using Onspay.SharedKernel;

namespace Domain.Orders.ValueObjects;

public record Name
{
    private Name(string value) => Value = value;

    public string Value { get; }

    public static Result<Name> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Name>(OrderErrors.NameNullOrEmpty);

        if (value.Length > 500)
            return Result.Failure<Name>(OrderErrors.NameTooLong);

        return new Name(value);
    }

    // For reconstitution from persistence only — skips domain validation
    public static Name From(string value) => new(value);

    public override string ToString() => Value;
}