using Domain.Orders;
using Domain.Orders.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders", Schemas.Default);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(o => o.Number).IsRequired();

        builder.Property(o => o.Status).IsRequired();

        builder.Property(o => o.Description)
            .HasColumnName(nameof(Order.Description))
            .HasConversion(description => description.Value, value => Description.From(value))
            .HasMaxLength(1000)
            .IsRequired();

        builder.OwnsOne(order => order.TotalAmount, amountBuilder =>
        {
            amountBuilder.Property(c => c.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });
    }
}