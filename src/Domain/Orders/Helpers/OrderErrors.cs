using Onspay.SharedKernel;

namespace Domain.Orders.Helpers;

public static class OrderErrors
{
    public static Error NotFound(Guid id) => new(
        "2001",
        $"Order with id: {id} not found.",
        ErrorType.NotFound);

    public static readonly Error NameNullOrEmpty = new(
        "2002",
        "Name cannot be null or empty.",
        ErrorType.Validation);

    public static readonly Error NameTooLong = new(
        "2003",
        "Name cannot be longer than 500 characters.",
        ErrorType.Validation);

    public static readonly Error DescriptionTooLong = new(
        "2004",
        "Description cannot be longer than 1000 characters.",
        ErrorType.Validation);

    public static readonly Error CannotAddDifferentCurrency = new(
        "2005",
        "Cannot add different currency to the order.",
        ErrorType.Validation);

    public static readonly Error CannotSubtractDifferentCurrency = new(
        "2006",
        "Cannot subtract different currency from the order.",
        ErrorType.Validation);

    public static readonly Error InvalidCurrency = new(
        "2007",
        "Invalid currency value.",
        ErrorType.Validation);

    public static readonly Error InvalidQuantity = new(
        "2008",
        "Quantity must be greater than zero.",
        ErrorType.Validation);

    public static Error NotPending(Guid id) => new(
        "2009",
        $"Order with id: {id} is not pending.",
        ErrorType.Conflict);

    public static Error DuplicateNumber(int number) => new(
        "2010",
        $"An order with number {number} already exists.",
        ErrorType.Conflict);
}
