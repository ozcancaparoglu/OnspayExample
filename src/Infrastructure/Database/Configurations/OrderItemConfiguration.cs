using Domain.Orders;
using Domain.Orders.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order-items", Schemas.Default);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(oi => oi.OrderId).IsRequired();

        builder.Property(oi => oi.ProductName)
            .HasColumnName("product_name")
            .HasConversion(name => name.Value, value => Name.From(value))
            .HasMaxLength(500)
            .IsRequired();

        builder.OwnsOne(orderItem => orderItem.Amount, amountBuilder =>
        {
            amountBuilder.Property(c => c.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });

        builder.Property(oi => oi.Quantity).IsRequired();

        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}