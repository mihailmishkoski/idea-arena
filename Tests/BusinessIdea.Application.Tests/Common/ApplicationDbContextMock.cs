#nullable enable
namespace BusinessIdea.Application.Tests.Common;

using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

/// <summary>
/// Builds a Mock&lt;IApplicationDbContext&gt; backed by in-memory lists. Add/Remove
/// on the mocked DbSets mutate the same lists, so queries executed after a
/// mutation observe it — mirroring how handlers use the real context.
/// </summary>
public static class ApplicationDbContextMock
{
    public static Mock<IApplicationDbContext> Create(
        List<BusinessIdeaPost>? businessIdeas = null,
        List<PostVote>? postVotes = null,
        List<Comment>? comments = null,
        List<CommentVote>? commentVotes = null,
        List<Conversation>? conversations = null,
        List<ChatMessage>? chatMessages = null,
        List<Notification>? notifications = null,
        List<OutboxMessage>? outboxMessages = null,
        List<AuthorInfo>? authors = null)
    {
        Mock<IApplicationDbContext> context = new Mock<IApplicationDbContext>();

        context.Setup(x => x.BusinessIdeas).Returns(BuildDbSet(businessIdeas ?? new List<BusinessIdeaPost>()).Object);
        context.Setup(x => x.PostVotes).Returns(BuildDbSet(postVotes ?? new List<PostVote>()).Object);
        context.Setup(x => x.Comments).Returns(BuildDbSet(comments ?? new List<Comment>()).Object);
        context.Setup(x => x.CommentVotes).Returns(BuildDbSet(commentVotes ?? new List<CommentVote>()).Object);
        context.Setup(x => x.Conversations).Returns(BuildDbSet(conversations ?? new List<Conversation>()).Object);
        context.Setup(x => x.ChatMessages).Returns(BuildDbSet(chatMessages ?? new List<ChatMessage>()).Object);
        context.Setup(x => x.Notifications).Returns(BuildDbSet(notifications ?? new List<Notification>()).Object);
        context.Setup(x => x.OutboxMessages).Returns(BuildDbSet(outboxMessages ?? new List<OutboxMessage>()).Object);
        context.Setup(x => x.Authors).Returns((authors ?? new List<AuthorInfo>()).AsQueryable().BuildMock());
        context.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return context;
    }

    private static Mock<DbSet<TEntity>> BuildDbSet<TEntity>(List<TEntity> source) where TEntity : class
    {
        Mock<DbSet<TEntity>> dbSet = source.AsQueryable().BuildMockDbSet();
        dbSet.Setup(x => x.Add(It.IsAny<TEntity>())).Callback<TEntity>(source.Add);
        dbSet.Setup(x => x.Remove(It.IsAny<TEntity>())).Callback<TEntity>(entity => source.Remove(entity));
        return dbSet;
    }
}
