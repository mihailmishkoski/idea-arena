using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessIdea.Infrastructure.Persistence.Configurations;

public  class OutboxMessageConfiguration : BaseEntityConfiguration<OutboxMessage>
{
    public override void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {       base.Configure(builder);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PayloadJson).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(2000);

        // The worker's polling query: oldest unprocessed first.
        builder.HasIndex(x => new { x.ProcessedAtUtc, x.CreatedAtUtc });
    }
}
