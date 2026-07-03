using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessIdea.Infrastructure.Persistence.Configurations;

public class PostVoteConfiguration : IEntityTypeConfiguration<PostVote>
{
    public void Configure(EntityTypeBuilder<PostVote> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Direction).HasConversion<int>();

        // One vote per user per post — the core "Reddit" constraint, enforced by
        // the database rather than trusted to application code.
        builder.HasIndex(x => new { x.PostId, x.UserId }).IsUnique();
    }
}
