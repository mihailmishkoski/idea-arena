#nullable enable
namespace BusinessIdea.Application.Tests.Features.Outbox.ChatAcceptedEvent;

using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Features.Outbox;
using BusinessIdea.Application.Tests.Common;
using BusinessIdea.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class ChatAcceptedEventTests
{
    [Fact]
    public async Task WhenConversationWasDeleted_ShouldNotSendEmail()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<IEmailSender> emailStub = new Mock<IEmailSender>();

        ChatAcceptedProcessor processor = new ChatAcceptedProcessor(
            contextStub.Object, emailStub.Object, ChatAcceptedEventTestsHelper.GetUrls());

        //Act
        await processor.ProcessAsync(ChatAcceptedEventTestsHelper.GetMessage(Guid.NewGuid()), default);

        //Assert
        emailStub.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenRequesterHasNoEmail_ShouldNotSendEmail()
    {
        //Arrange
        Guid conversationId = Guid.NewGuid();
        List<Conversation> conversations = new List<Conversation>
        {
            ChatAcceptedEventTestsHelper.GetConversation(conversationId),
        };
        List<AuthorInfo> authors = new List<AuthorInfo>
        {
            ChatAcceptedEventTestsHelper.GetRequester(email: null),
            ChatAcceptedEventTestsHelper.GetRecipient(),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            conversations: conversations, authors: authors);
        Mock<IEmailSender> emailStub = new Mock<IEmailSender>();

        ChatAcceptedProcessor processor = new ChatAcceptedProcessor(
            contextStub.Object, emailStub.Object, ChatAcceptedEventTestsHelper.GetUrls());

        //Act
        await processor.ProcessAsync(ChatAcceptedEventTestsHelper.GetMessage(conversationId), default);

        //Assert
        emailStub.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenAccepted_ShouldEmailRequesterMentioningAccepter()
    {
        //Arrange
        Guid conversationId = Guid.NewGuid();
        List<Conversation> conversations = new List<Conversation>
        {
            ChatAcceptedEventTestsHelper.GetConversation(conversationId),
        };
        List<AuthorInfo> authors = new List<AuthorInfo>
        {
            ChatAcceptedEventTestsHelper.GetRequester(ChatAcceptedEventTestsHelper.RequesterEmail),
            ChatAcceptedEventTestsHelper.GetRecipient(),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            conversations: conversations, authors: authors);
        Mock<IEmailSender> emailStub = new Mock<IEmailSender>();
        EmailMessage? sent = null;
        emailStub
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((message, _) => sent = message)
            .Returns(Task.CompletedTask);

        ChatAcceptedProcessor processor = new ChatAcceptedProcessor(
            contextStub.Object, emailStub.Object, ChatAcceptedEventTestsHelper.GetUrls());

        //Act
        await processor.ProcessAsync(ChatAcceptedEventTestsHelper.GetMessage(conversationId), default);

        //Assert
        Assert.NotNull(sent);
        Assert.Equal(ChatAcceptedEventTestsHelper.RequesterEmail, sent!.ToEmail);
        Assert.Contains(ChatAcceptedEventTestsHelper.RecipientName, sent.Subject);
        Assert.Contains("accepted", sent.Subject);
        Assert.Contains("/messages", sent.HtmlBody);
    }
}
