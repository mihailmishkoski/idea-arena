using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusinessIdea.Domain.Enums;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BusinessIdea.Infrastructure.Persistence.Configurations;

public class BusinessIdeaPostConfiguration : IEntityTypeConfiguration<BusinessIdeaPost>
{
    public void Configure(EntityTypeBuilder<BusinessIdeaPost> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.UniqueValueProposition).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Problem).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.Solution).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.Competition).HasMaxLength(2000);
        builder.Property(x => x.IncomeStrategy).HasMaxLength(2000);
        builder.Property(x => x.ExitStrategy).HasMaxLength(2000);

        builder.Property(x => x.VideoPitchUrl).HasMaxLength(2048);

        var categoriesComparer = new ValueComparer<List<BusinessIdeaCategory>>(
            (a, b) => a!.SequenceEqual(b!),
            v => v.Aggregate(0, (hash, c) => HashCode.Combine(hash, c)),
            v => v.ToList());

        builder.Property(x => x.Categories)
            .HasColumnType("integer[]")
            .HasConversion(
                v => v.Select(c => (int)c).ToArray(),
                v => v.Select(i => (BusinessIdeaCategory)i).ToList())
            .Metadata.SetValueComparer(categoriesComparer);
        builder.Property(x => x.AuthorId).IsRequired();

        builder.HasIndex(x => x.AuthorId);
        builder.HasIndex(x => x.CreatedAtUtc);

        builder.HasMany(x => x.Votes)
            .WithOne(v => v.Post)
            .HasForeignKey(v => v.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
