using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessIdea.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RequesterId).IsRequired();
        builder.Property(x => x.RecipientId).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>();

        // Fast lookup of "my conversations" from either side.
        builder.HasIndex(x => x.RequesterId);
        builder.HasIndex(x => x.RecipientId);

        builder.HasMany(x => x.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
