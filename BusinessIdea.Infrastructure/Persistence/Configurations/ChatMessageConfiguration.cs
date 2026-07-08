using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessIdea.Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : BaseEntityConfiguration<ChatMessage>
{
    public override void Configure(EntityTypeBuilder<ChatMessage> builder)
    {   base.Configure(builder);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SenderId).IsRequired();
        builder.Property(x => x.Content).IsRequired().HasMaxLength(2000);

        builder.HasIndex(x => x.ConversationId);
    }
}
