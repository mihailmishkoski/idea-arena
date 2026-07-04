using BusinessIdea.Application.Common.Models;
using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Common.Interfaces;

/// <summary>
/// The persistence abstraction the Application layer depends on. It exposes only
/// the aggregates the handlers need — never the concrete DbContext — so the
/// Application layer stays free of any specific database technology (DIP).
/// </summary>
public interface IApplicationDbContext
{
    DbSet<BusinessIdeaPost> BusinessIdeas { get; }
    DbSet<PostVote> PostVotes { get; }
    DbSet<Comment> Comments { get; }
    DbSet<CommentVote> CommentVotes { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<WeeklyWinner> WeeklyWinners { get; }

    /// <summary>Read-only, Identity-free view of users for name projections.</summary>
    IQueryable<AuthorInfo> Authors { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
