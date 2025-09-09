using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.Payment.Infra.Data.MapEntities
{
    public class PaymentMap : IEntityTypeConfiguration<Domain.PaymentAggregate.Payment>
    {
        public void Configure(EntityTypeBuilder<Domain.PaymentAggregate.Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.UserId)
                .IsRequired();

            builder.Property(p => p.GameId)
                .IsRequired();

            builder.OwnsOne(p => p.Price, money =>
            {
                money.Property(m => m.Value)
                    .HasColumnName("Price")
                    .IsRequired();
                money.Property(m => m.Currency)
                    .HasColumnName("Price_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .IsRequired();
        }
    }
}
