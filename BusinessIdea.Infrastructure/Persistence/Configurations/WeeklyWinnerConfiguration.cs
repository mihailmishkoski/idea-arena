using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessIdea.Infrastructure.Persistence.Configurations;

public class WeeklyWinnerConfiguration : IEntityTypeConfiguration<WeeklyWinner>
{
    public void Configure(EntityTypeBuilder<WeeklyWinner> builder)
    {
        builder.HasKey(w => w.Id);

        // One winner per competition week.
        builder.HasIndex(w => w.PeriodStartUtc).IsUnique();

        builder.Property(w => w.PostName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(w => w.AuthorId)
            .HasMaxLength(450)
            .IsRequired();

        // Keep hall-of-fame rows even when the winning post is deleted; the
        // snapshot columns carry what the page needs.
        builder.HasOne(w => w.Post)
            .WithMany()
            .HasForeignKey(w => w.PostId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
