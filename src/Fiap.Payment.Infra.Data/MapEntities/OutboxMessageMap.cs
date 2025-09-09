using Fiap.Payment.Domain.OutboxAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.Payment.Infra.Data.MapEntities
{
    public class OutboxMessageMap : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Type)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(o => o.Content)
                .IsRequired();

            builder.Property(o => o.OccuredOn)
                .IsRequired();

            builder.Property(o => o.ProcessedOn)
                .IsRequired(false);
        }
    }
}