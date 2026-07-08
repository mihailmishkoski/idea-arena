using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessIdea.Infrastructure.Persistence.Configurations;

public class CommentConfiguration : BaseEntityConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AuthorId).IsRequired();
        builder.Property(x => x.Content).IsRequired().HasMaxLength(5000);
        builder.Property(x => x.TargetMetric).HasConversion<int>();

        builder.HasIndex(x => x.PostId);
        builder.HasIndex(x => new { x.PostId, x.TargetMetric });
        builder.HasIndex(x => x.ParentCommentId);

        // Self-reference for replies. Deleting a comment removes its replies too
        // (Postgres permits the resulting multiple cascade paths from the post).
        builder.HasMany(x => x.Replies)
            .WithOne(x => x.Parent)
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Votes)
            .WithOne(v => v.Comment)
            .HasForeignKey(v => v.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
