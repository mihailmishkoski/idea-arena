#nullable enable
namespace BusinessIdea.Application.Tests.Features.Chat.Commands.SendChatMessageCommandTests;

using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Features.Chat;
using BusinessIdea.Application.Features.Chat.Commands.SendChatMessage;
using BusinessIdea.Application.Tests.Common;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class SendChatMessageCommandTests
{
    [Fact]
    public async Task WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(SendChatMessageCommandTestsHelper.RequesterId);

        SendChatMessageCommand command = new SendChatMessageCommand(Guid.NewGuid(), SendChatMessageCommandTestsHelper.Content);
        SendChatMessageCommandHandler handler = new SendChatMessageCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<NotFoundException>(function);
    }

    [Fact]
    public async Task WhenUserIsNotParticipant_ShouldThrowForbiddenAccessException()
    {
        //Arrange
        Guid conversationId = Guid.NewGuid();
        List<Conversation> conversations = new List<Conversation>
        {
            SendChatMessageCommandTestsHelper.GetConversation(conversationId),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(conversations: conversations);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(SendChatMessageCommandTestsHelper.OutsiderId);

        SendChatMessageCommand command = new SendChatMessageCommand(conversationId, SendChatMessageCommandTestsHelper.Content);
        SendChatMessageCommandHandler handler = new SendChatMessageCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(function);
    }

    [Fact]
    public async Task WhenRequestNotAcceptedYet_ShouldThrowForbiddenAccessException()
    {
        //Arrange
        Guid conversationId = Guid.NewGuid();
        List<Conversation> conversations = new List<Conversation>
        {
            SendChatMessageCommandTestsHelper.GetConversation(conversationId, ChatRequestStatus.Pending),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(conversations: conversations);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(SendChatMessageCommandTestsHelper.RequesterId);

        SendChatMessageCommand command = new SendChatMessageCommand(conversationId, SendChatMessageCommandTestsHelper.Content);
        SendChatMessageCommandHandler handler = new SendChatMessageCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(function);
    }

    [Fact]
    public async Task WhenValid_ShouldAddMessageAndDeliverToOtherParticipant()
    {
        //Arrange
        Guid conversationId = Guid.NewGuid();
        List<Conversation> conversations = new List<Conversation>
        {
            SendChatMessageCommandTestsHelper.GetConversation(conversationId),
        };
        List<ChatMessage> chatMessages = new List<ChatMessage>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            conversations: conversations, chatMessages: chatMessages);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        Mock<IRealtimeNotifier> notifierStub = new Mock<IRealtimeNotifier>();
        currentUserStub.Setup(x => x.UserId).Returns(SendChatMessageCommandTestsHelper.RequesterId);

        SendChatMessageCommand command = new SendChatMessageCommand(conversationId, SendChatMessageCommandTestsHelper.Content);
        SendChatMessageCommandHandler handler = new SendChatMessageCommandHandler(
            contextStub.Object, currentUserStub.Object, notifierStub.Object);

        //Act
        ChatMessageDto result = await handler.Handle(command, default);

        //Assert
        ChatMessage message = Assert.Single(chatMessages);
        Assert.Equal(SendChatMessageCommandTestsHelper.Content.Trim(), message.Content);
        Assert.Equal(SendChatMessageCommandTestsHelper.RequesterId, message.SenderId);
        Assert.Equal(message.Id, result.Id);
        Assert.Equal(conversationId, result.ConversationId);

        // The sender is the requester, so the message goes to the recipient.
        notifierStub.Verify(
            x => x.SendToUserAsync(SendChatMessageCommandTestsHelper.RecipientId, "chatMessage", It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
