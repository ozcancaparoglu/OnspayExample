using Domain.Orders;
using Domain.Orders.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(oi => oi.OrderId).IsRequired();

        builder.Property(oi => oi.ProductName)
            .HasConversion(name => name.Value, value => Name.From(value))
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(oi => oi.Quantity).IsRequired();

        // Stored as price_amount / price_currency
        builder.OwnsOne(orderItem => orderItem.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount).HasPrecision(18, 2);
            priceBuilder.Property(m => m.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .HasMaxLength(3);
        });
    }
}
