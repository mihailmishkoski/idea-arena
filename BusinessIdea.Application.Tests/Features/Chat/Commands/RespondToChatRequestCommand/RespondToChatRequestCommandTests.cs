#nullable enable
namespace BusinessIdea.Application.Tests.Features.Chat.Commands.RespondToChatRequestCommandTests;

using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Application.Features.Chat.Commands.RespondToChatRequest;
using BusinessIdea.Application.Tests.Common;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class RespondToChatRequestCommandTests
{
    [Fact]
    public async Task WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(RespondToChatRequestCommandTestsHelper.RecipientId);

        RespondToChatRequestCommand command = new RespondToChatRequestCommand(Guid.NewGuid(), Accept: true);
        RespondToChatRequestCommandHandler handler = new RespondToChatRequestCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<NotFoundException>(function);
    }

    [Fact]
    public async Task WhenUserIsNotRecipient_ShouldThrowForbiddenAccessException()
    {
        //Arrange
        Guid conversationId = Guid.NewGuid();
        List<Conversation> conversations = new List<Conversation>
        {
            RespondToChatRequestCommandTestsHelper.GetConversation(conversationId),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(conversations: conversations);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(RespondToChatRequestCommandTestsHelper.RequesterId);

        RespondToChatRequestCommand command = new RespondToChatRequestCommand(conversationId, Accept: true);
        RespondToChatRequestCommandHandler handler = new RespondToChatRequestCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(function);
    }

    [Fact]
    public async Task WhenAlreadyAnswered_ShouldDoNothing()
    {
        //Arrange
        Guid conversationId = Guid.NewGuid();
        Conversation conversation = RespondToChatRequestCommandTestsHelper.GetConversation(
            conversationId, ChatRequestStatus.Accepted);
        List<Conversation> conversations = new List<Conversation> { conversation };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(conversations: conversations);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(RespondToChatRequestCommandTestsHelper.RecipientId);

        RespondToChatRequestCommand command = new RespondToChatRequestCommand(conversationId, Accept: false);
        RespondToChatRequestCommandHandler handler = new RespondToChatRequestCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        await handler.Handle(command, default);

        //Assert
        Assert.Equal(ChatRequestStatus.Accepted, conversation.Status);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenAccepted_ShouldNotifyRequesterAndEnqueueChatAcceptedEvent()
    {
        //Arrange
        Guid conversationId = Guid.NewGuid();
        Conversation conversation = RespondToChatRequestCommandTestsHelper.GetConversation(conversationId);
        List<Conversation> conversations = new List<Conversation> { conversation };
        List<Notification> notifications = new List<Notification>();
        List<OutboxMessage> outboxMessages = new List<OutboxMessage>();
        List<AuthorInfo> authors = new List<AuthorInfo> { RespondToChatRequestCommandTestsHelper.GetRecipient() };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            conversations: conversations, notifications: notifications,
            outboxMessages: outboxMessages, authors: authors);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(RespondToChatRequestCommandTestsHelper.RecipientId);

        RespondToChatRequestCommand command = new RespondToChatRequestCommand(conversationId, Accept: true);
        RespondToChatRequestCommandHandler handler = new RespondToChatRequestCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        await handler.Handle(command, default);

        //Assert
        Assert.Equal(ChatRequestStatus.Accepted, conversation.Status);

        Notification notification = Assert.Single(notifications);
        Assert.Equal(RespondToChatRequestCommandTestsHelper.RequesterId, notification.UserId);
        Assert.Equal(NotificationType.ChatAccepted, notification.Type);
        Assert.Contains(RespondToChatRequestCommandTestsHelper.RecipientName, notification.Text);

        OutboxMessage outboxMessage = Assert.Single(outboxMessages);
        Assert.Equal(OutboxEventTypes.ChatAccepted, outboxMessage.Type);

        notifierStub.Verify(
            x => x.SendToUserAsync(RespondToChatRequestCommandTestsHelper.RequesterId, "notification", It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WhenDeclined_ShouldSaveWithoutNotificationOrOutboxEvent()
    {
        //Arrange
        Guid conversationId = Guid.NewGuid();
        Conversation conversation = RespondToChatRequestCommandTestsHelper.GetConversation(conversationId);
        List<Conversation> conversations = new List<Conversation> { conversation };
        List<Notification> notifications = new List<Notification>();
        List<OutboxMessage> outboxMessages = new List<OutboxMessage>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            conversations: conversations, notifications: notifications, outboxMessages: outboxMessages);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(RespondToChatRequestCommandTestsHelper.RecipientId);

        RespondToChatRequestCommand command = new RespondToChatRequestCommand(conversationId, Accept: false);
        RespondToChatRequestCommandHandler handler = new RespondToChatRequestCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        await handler.Handle(command, default);

        //Assert
        Assert.Equal(ChatRequestStatus.Declined, conversation.Status);
        Assert.Empty(notifications);
        Assert.Empty(outboxMessages);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        notifierStub.Verify(
            x => x.SendToUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
