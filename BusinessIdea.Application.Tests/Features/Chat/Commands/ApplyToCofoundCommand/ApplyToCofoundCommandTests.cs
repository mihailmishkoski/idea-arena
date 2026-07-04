#nullable enable
namespace BusinessIdea.Application.Tests.Features.Chat.Commands.ApplyToCofoundCommandTests;

using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Application.Features.Chat.Commands.ApplyToCofound;
using BusinessIdea.Application.Tests.Common;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class ApplyToCofoundCommandTests
{
    [Fact]
    public async Task WhenNotSignedIn_ShouldThrowForbiddenAccessException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns((string?)null);

        ApplyToCofoundCommand command = ApplyToCofoundCommandTestsHelper.GetCommand(Guid.NewGuid());
        ApplyToCofoundCommandHandler handler = new ApplyToCofoundCommandHandler(
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
        currentUserStub.Setup(x => x.UserId).Returns(ApplyToCofoundCommandTestsHelper.ApplicantId);

        ApplyToCofoundCommand command = ApplyToCofoundCommandTestsHelper.GetCommand(Guid.NewGuid());
        ApplyToCofoundCommandHandler handler = new ApplyToCofoundCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<NotFoundException>(function);
    }

    [Fact]
    public async Task WhenApplyingToOwnIdea_ShouldThrowValidationException()
    {
        //Arrange
        Guid postId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            ApplyToCofoundCommandTestsHelper.GetIdea(postId),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(businessIdeas: businessIdeas);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(ApplyToCofoundCommandTestsHelper.AuthorId);

        ApplyToCofoundCommand command = ApplyToCofoundCommandTestsHelper.GetCommand(postId);
        ApplyToCofoundCommandHandler handler = new ApplyToCofoundCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ValidationException>(function);
    }

    [Theory]
    [InlineData(ChatRequestStatus.Pending)]
    [InlineData(ChatRequestStatus.Accepted)]
    public async Task WhenConversationWithAuthorAlreadyExists_ShouldThrowValidationException(ChatRequestStatus existingStatus)
    {
        //Arrange
        Guid postId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            ApplyToCofoundCommandTestsHelper.GetIdea(postId),
        };
        List<Conversation> conversations = new List<Conversation>
        {
            ApplyToCofoundCommandTestsHelper.GetConversation(existingStatus),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, conversations: conversations);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(ApplyToCofoundCommandTestsHelper.ApplicantId);

        ApplyToCofoundCommand command = ApplyToCofoundCommandTestsHelper.GetCommand(postId);
        ApplyToCofoundCommandHandler handler = new ApplyToCofoundCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ValidationException>(function);
        Assert.Single(conversations);
    }

    [Fact]
    public async Task WhenDailyLimitReached_ShouldThrowValidationException()
    {
        //Arrange
        Guid postId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            ApplyToCofoundCommandTestsHelper.GetIdea(postId),
        };
        List<Conversation> conversations = ApplyToCofoundCommandTestsHelper.GetConversationsAtDailyLimit();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, conversations: conversations);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(ApplyToCofoundCommandTestsHelper.ApplicantId);

        ApplyToCofoundCommand command = ApplyToCofoundCommandTestsHelper.GetCommand(postId);
        ApplyToCofoundCommandHandler handler = new ApplyToCofoundCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ValidationException>(function);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenValid_ShouldCreateConversationNotificationAndOutboxEventWithApplicationText()
    {
        //Arrange
        Guid postId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            ApplyToCofoundCommandTestsHelper.GetIdea(postId),
        };
        List<Conversation> conversations = new List<Conversation>();
        List<Notification> notifications = new List<Notification>();
        List<OutboxMessage> outboxMessages = new List<OutboxMessage>();
        List<AuthorInfo> authors = new List<AuthorInfo> { ApplyToCofoundCommandTestsHelper.GetApplicant() };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, conversations: conversations,
            notifications: notifications, outboxMessages: outboxMessages, authors: authors);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(ApplyToCofoundCommandTestsHelper.ApplicantId);

        ApplyToCofoundCommand command = ApplyToCofoundCommandTestsHelper.GetCommand(postId);
        ApplyToCofoundCommandHandler handler = new ApplyToCofoundCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Guid result = await handler.Handle(command, default);

        //Assert
        Conversation conversation = Assert.Single(conversations);
        Assert.Equal(result, conversation.Id);
        Assert.Equal(ApplyToCofoundCommandTestsHelper.AuthorId, conversation.RecipientId);
        Assert.Equal(postId, conversation.PostId);

        Notification notification = Assert.Single(notifications);
        Assert.Equal(ApplyToCofoundCommandTestsHelper.AuthorId, notification.UserId);
        Assert.Contains(ApplyToCofoundCommandTestsHelper.ApplicantName, notification.Text);

        // The application text travels ONLY inside the outbox payload (worker
        // emails it) — it must never appear as a chat message.
        OutboxMessage outboxMessage = Assert.Single(outboxMessages);
        Assert.Equal(OutboxEventTypes.CofounderApplied, outboxMessage.Type);
        CofounderAppliedPayload payload = OutboxMessageFactory.Deserialize<CofounderAppliedPayload>(outboxMessage);
        Assert.Equal(conversation.Id, payload.ConversationId);
        Assert.Contains(ApplyToCofoundCommandTestsHelper.Role, payload.ApplicationText);
        Assert.Contains(ApplyToCofoundCommandTestsHelper.PostName, payload.ApplicationText);

        notifierStub.Verify(
            x => x.SendToUserAsync(ApplyToCofoundCommandTestsHelper.AuthorId, "notification", It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
