#nullable enable
namespace BusinessIdea.Application.Tests.Features.Chat.Commands.RequestChatCommandTests;

using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Application.Features.Chat.Commands.RequestChat;
using BusinessIdea.Application.Tests.Common;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class RequestChatCommandTests
{
    [Fact]
    public async Task WhenNotSignedIn_ShouldThrowForbiddenAccessException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns((string?)null);

        RequestChatCommand command = new RequestChatCommand(RequestChatCommandTestsHelper.RecipientId, null);
        RequestChatCommandHandler handler = new RequestChatCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(function);
    }

    [Fact]
    public async Task WhenRequestingChatWithSelf_ShouldThrowValidationException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(RequestChatCommandTestsHelper.RequesterId);

        RequestChatCommand command = new RequestChatCommand(RequestChatCommandTestsHelper.RequesterId, null);
        RequestChatCommandHandler handler = new RequestChatCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ValidationException>(function);
    }

    [Fact]
    public async Task WhenRecipientDoesNotExist_ShouldThrowNotFoundException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(RequestChatCommandTestsHelper.RequesterId);

        RequestChatCommand command = new RequestChatCommand(RequestChatCommandTestsHelper.RecipientId, null);
        RequestChatCommandHandler handler = new RequestChatCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<NotFoundException>(function);
    }

    [Fact]
    public async Task WhenConversationAlreadyExists_ShouldReturnExistingIdWithoutCreatingAnything()
    {
        //Arrange
        Conversation existing = RequestChatCommandTestsHelper.GetConversation(ChatRequestStatus.Accepted);
        List<Conversation> conversations = new List<Conversation> { existing };
        List<AuthorInfo> authors = new List<AuthorInfo>
        {
            RequestChatCommandTestsHelper.GetRequester(),
            RequestChatCommandTestsHelper.GetRecipient(),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            conversations: conversations, authors: authors);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(RequestChatCommandTestsHelper.RequesterId);

        RequestChatCommand command = new RequestChatCommand(RequestChatCommandTestsHelper.RecipientId, null);
        RequestChatCommandHandler handler = new RequestChatCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Guid result = await handler.Handle(command, default);

        //Assert
        Assert.Equal(existing.Id, result);
        Assert.Single(conversations);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenDailyLimitReached_ShouldThrowValidationException()
    {
        //Arrange
        List<Conversation> conversations = RequestChatCommandTestsHelper.GetConversationsAtDailyLimit();
        List<AuthorInfo> authors = new List<AuthorInfo>
        {
            RequestChatCommandTestsHelper.GetRequester(),
            RequestChatCommandTestsHelper.GetRecipient(),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            conversations: conversations, authors: authors);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(RequestChatCommandTestsHelper.RequesterId);

        RequestChatCommand command = new RequestChatCommand(RequestChatCommandTestsHelper.RecipientId, null);
        RequestChatCommandHandler handler = new RequestChatCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ValidationException>(function);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenValid_ShouldCreateConversationNotificationAndOutboxEvent()
    {
        //Arrange
        List<Conversation> conversations = new List<Conversation>();
        List<Notification> notifications = new List<Notification>();
        List<OutboxMessage> outboxMessages = new List<OutboxMessage>();
        List<AuthorInfo> authors = new List<AuthorInfo>
        {
            RequestChatCommandTestsHelper.GetRequester(),
            RequestChatCommandTestsHelper.GetRecipient(),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            conversations: conversations, notifications: notifications,
            outboxMessages: outboxMessages, authors: authors);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(RequestChatCommandTestsHelper.RequesterId);

        RequestChatCommand command = new RequestChatCommand(RequestChatCommandTestsHelper.RecipientId, null);
        RequestChatCommandHandler handler = new RequestChatCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Guid result = await handler.Handle(command, default);

        //Assert
        Conversation conversation = Assert.Single(conversations);
        Assert.Equal(result, conversation.Id);
        Assert.Equal(ChatRequestStatus.Pending, conversation.Status);

        Notification notification = Assert.Single(notifications);
        Assert.Equal(RequestChatCommandTestsHelper.RecipientId, notification.UserId);
        Assert.Equal(NotificationType.ChatRequest, notification.Type);
        Assert.Contains(RequestChatCommandTestsHelper.RequesterName, notification.Text);

        OutboxMessage outboxMessage = Assert.Single(outboxMessages);
        Assert.Equal(OutboxEventTypes.ChatRequested, outboxMessage.Type);

        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        notifierStub.Verify(
            x => x.SendToUserAsync(RequestChatCommandTestsHelper.RecipientId, "notification", It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
