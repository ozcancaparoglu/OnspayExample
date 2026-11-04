using Domain.Orders;
using Domain.Orders.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(o => o.Number).IsRequired();
        builder.HasIndex(o => o.Number).IsUnique();

        builder.Property(o => o.Status).IsRequired();

        builder.Property(o => o.Description)
            .HasConversion(description => description.Value, value => Description.From(value))
            .HasMaxLength(1000)
            .IsRequired();

        // Stored as total_amount_amount / total_amount_currency
        builder.OwnsOne(order => order.TotalAmount, amountBuilder =>
        {
            amountBuilder.Property(m => m.Amount).HasPrecision(18, 2);
            amountBuilder.Property(m => m.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .HasMaxLength(3);
        });

        // Items are only reachable through the aggregate; EF writes the _orderItems backing field.
        builder.HasMany(o => o.OrderItems)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.OrderItems).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
