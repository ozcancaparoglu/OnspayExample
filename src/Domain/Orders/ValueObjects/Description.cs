using Domain.Orders.Helpers;
using Onspay.SharedKernel;

namespace Domain.Orders.ValueObjects;

public record Description
{
    private Description(string value) => Value = value;

    public string Value { get; }

    public static readonly Description Empty = new(string.Empty);

    private static Result<Description> Create(string value)
    {
        if (value.Length > 1000)
            return Result.Failure<Description>(OrderErrors.DescriptionTooLong);

        return new Description(value);
    }
    
    public static Result<Description> ResolveDescription(string? value) =>
        !string.IsNullOrWhiteSpace(value) ? Create(value) : Result.Success(Empty);

    // For reconstitution from persistence only — skips domain validation
    public static Description From(string value) => new(value);

    public override string ToString() => Value;
}