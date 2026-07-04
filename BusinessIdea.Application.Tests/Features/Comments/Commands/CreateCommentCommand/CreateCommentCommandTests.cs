#nullable enable
namespace BusinessIdea.Application.Tests.Features.Comments.Commands.CreateCommentCommandTests;

using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Features.Comments.Commands.CreateComment;
using BusinessIdea.Application.Tests.Common;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class CreateCommentCommandTests
{
    [Fact]
    public async Task WhenNotSignedIn_ShouldThrowForbiddenAccessException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns((string?)null);

        CreateCommentCommand command = CreateCommentCommandTestsHelper.GetCommand(Guid.NewGuid());
        CreateCommentCommandHandler handler = new CreateCommentCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(function);
    }

    [Fact]
    public async Task WhenPostNotFound_ShouldThrowNotFoundException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(CreateCommentCommandTestsHelper.CommenterId);

        CreateCommentCommand command = CreateCommentCommandTestsHelper.GetCommand(Guid.NewGuid());
        CreateCommentCommandHandler handler = new CreateCommentCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<NotFoundException>(function);
    }

    [Fact]
    public async Task WhenParentCommentNotFound_ShouldThrowNotFoundException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(CreateCommentCommandTestsHelper.CommenterId);

        CreateCommentCommand command = CreateCommentCommandTestsHelper.GetCommand(Guid.NewGuid(), parentCommentId: Guid.NewGuid());
        CreateCommentCommandHandler handler = new CreateCommentCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<NotFoundException>(function);
    }

    [Fact]
    public async Task WhenTopLevelComment_ShouldAddCommentAndNotifyPostAuthor()
    {
        //Arrange
        Guid postId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            CreateCommentCommandTestsHelper.GetIdea(postId),
        };
        List<Comment> comments = new List<Comment>();
        List<Notification> notifications = new List<Notification>();
        List<AuthorInfo> authors = new List<AuthorInfo> { CreateCommentCommandTestsHelper.GetCommenter() };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, comments: comments, notifications: notifications, authors: authors);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(CreateCommentCommandTestsHelper.CommenterId);

        CreateCommentCommand command = CreateCommentCommandTestsHelper.GetCommand(postId);
        CreateCommentCommandHandler handler = new CreateCommentCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Guid result = await handler.Handle(command, default);

        //Assert
        Comment comment = Assert.Single(comments);
        Assert.Equal(result, comment.Id);
        Assert.Equal(CreateCommentCommandTestsHelper.Content.Trim(), comment.Content);
        Assert.Equal(IdeaMetric.Problem, comment.TargetMetric);
        Assert.Null(comment.ParentCommentId);

        Notification notification = Assert.Single(notifications);
        Assert.Equal(CreateCommentCommandTestsHelper.PostAuthorId, notification.UserId);
        Assert.Equal(NotificationType.PostComment, notification.Type);
        notifierStub.Verify(
            x => x.SendToUserAsync(CreateCommentCommandTestsHelper.PostAuthorId, "notification", It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WhenReply_ShouldInheritParentPostAndMetric()
    {
        //Arrange
        Guid postId = Guid.NewGuid();
        Guid parentId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            CreateCommentCommandTestsHelper.GetIdea(postId),
        };
        List<Comment> comments = new List<Comment>
        {
            CreateCommentCommandTestsHelper.GetParentComment(parentId, postId),
        };
        List<AuthorInfo> authors = new List<AuthorInfo> { CreateCommentCommandTestsHelper.GetCommenter() };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, comments: comments, authors: authors);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(CreateCommentCommandTestsHelper.CommenterId);

        // The command deliberately points at a DIFFERENT post and metric — the
        // handler must take both from the parent instead.
        CreateCommentCommand command = CreateCommentCommandTestsHelper.GetCommand(Guid.NewGuid(), parentCommentId: parentId);
        CreateCommentCommandHandler handler = new CreateCommentCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Guid result = await handler.Handle(command, default);

        //Assert
        Comment reply = Assert.Single(comments, c => c.Id == result);
        Assert.Equal(postId, reply.PostId);
        Assert.Equal(IdeaMetric.ExitStrategy, reply.TargetMetric);
        Assert.Equal(parentId, reply.ParentCommentId);
    }

    [Fact]
    public async Task WhenCommentingOwnPost_ShouldNotCreateNotification()
    {
        //Arrange
        Guid postId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            CreateCommentCommandTestsHelper.GetIdea(postId),
        };
        List<Notification> notifications = new List<Notification>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, notifications: notifications);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(CreateCommentCommandTestsHelper.PostAuthorId);

        CreateCommentCommand command = CreateCommentCommandTestsHelper.GetCommand(postId);
        CreateCommentCommandHandler handler = new CreateCommentCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        await handler.Handle(command, default);

        //Assert
        Assert.Empty(notifications);
        notifierStub.Verify(
            x => x.SendToUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
