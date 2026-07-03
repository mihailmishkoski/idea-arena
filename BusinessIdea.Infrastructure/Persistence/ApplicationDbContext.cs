using System.Reflection;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Infrastructure.Persistence;

/// <summary>
/// The EF Core context. It implements <see cref="IApplicationDbContext"/> so the
/// Application layer depends on the abstraction, not this concrete type, and
/// derives from <see cref="IdentityDbContext{TUser}"/> to host the auth tables.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<BusinessIdeaPost> BusinessIdeas => Set<BusinessIdeaPost>();
    public DbSet<PostVote> PostVotes => Set<PostVote>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentVote> CommentVotes => Set<CommentVote>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public IQueryable<AuthorInfo> Authors =>
        Set<ApplicationUser>().Select(u => new AuthorInfo
        {
            Id = u.Id,
            DisplayName = u.DisplayName,
            Email = u.Email,
            AvatarId = u.AvatarId,
        });

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
