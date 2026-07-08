using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessIdea.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : BaseEntityConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Text).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Type).HasConversion<int>();

        // The bell icon's unread-count query.
        builder.HasIndex(x => new { x.UserId, x.IsRead });
    }
}
