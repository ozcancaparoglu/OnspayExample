using Domain.Orders.Helpers;
using Onspay.SharedKernel;

namespace Domain.Orders.ValueObjects;

public record Currency()
{
    internal static readonly Currency None = new("");
    public static readonly Currency USD = new Currency("USD");
    public static readonly Currency EUR = new Currency("EUR");
    public static readonly Currency GBP = new Currency("GBP");
    public static readonly Currency TRY = new Currency("TRY");
    
    private Currency(string code) : this() => Code = code;
    
    public string Code { get; init; }
    
    public static Currency FromCode(string code)
    {
        return All.FirstOrDefault(x => x.Code == code) 
               ?? throw new ArgumentException($"Invalid currency code: {code}");
    }
    
    public static IReadOnlyCollection<Currency> All =>
    [
        None,
        USD,
        EUR,
        GBP,
        TRY
    ];
}