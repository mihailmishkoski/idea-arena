#nullable enable
namespace BusinessIdea.Application.Tests.Features.Outbox.ChatRequestedEvent;

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

public class ChatRequestedEventTests
{
    [Fact]
    public async Task WhenConversationWasDeleted_ShouldNotSendEmail()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<IEmailSender> emailStub = new Mock<IEmailSender>();

        ChatRequestedProcessor processor = new ChatRequestedProcessor(
            contextStub.Object, emailStub.Object, ChatRequestedEventTestsHelper.GetUrls());

        //Act
        await processor.ProcessAsync(ChatRequestedEventTestsHelper.GetMessage(Guid.NewGuid()), default);

        //Assert
        emailStub.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenRecipientHasNoEmail_ShouldNotSendEmail()
    {
        //Arrange
        Guid conversationId = Guid.NewGuid();
        List<Conversation> conversations = new List<Conversation>
        {
            ChatRequestedEventTestsHelper.GetConversation(conversationId),
        };
        List<AuthorInfo> authors = new List<AuthorInfo>
        {
            ChatRequestedEventTestsHelper.GetRequester(),
            ChatRequestedEventTestsHelper.GetRecipient(email: null),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            conversations: conversations, authors: authors);
        Mock<IEmailSender> emailStub = new Mock<IEmailSender>();

        ChatRequestedProcessor processor = new ChatRequestedProcessor(
            contextStub.Object, emailStub.Object, ChatRequestedEventTestsHelper.GetUrls());

        //Act
        await processor.ProcessAsync(ChatRequestedEventTestsHelper.GetMessage(conversationId), default);

        //Assert
        emailStub.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenPlainChatRequest_ShouldEmailRecipientWithChatSubject()
    {
        //Arrange
        Guid conversationId = Guid.NewGuid();
        List<Conversation> conversations = new List<Conversation>
        {
            ChatRequestedEventTestsHelper.GetConversation(conversationId),
        };
        List<AuthorInfo> authors = new List<AuthorInfo>
        {
            ChatRequestedEventTestsHelper.GetRequester(),
            ChatRequestedEventTestsHelper.GetRecipient(ChatRequestedEventTestsHelper.RecipientEmail),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            conversations: conversations, authors: authors);
        Mock<IEmailSender> emailStub = new Mock<IEmailSender>();
        EmailMessage? sent = null;
        emailStub
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((message, _) => sent = message)
            .Returns(Task.CompletedTask);

        ChatRequestedProcessor processor = new ChatRequestedProcessor(
            contextStub.Object, emailStub.Object, ChatRequestedEventTestsHelper.GetUrls());

        //Act
        await processor.ProcessAsync(ChatRequestedEventTestsHelper.GetMessage(conversationId), default);

        //Assert
        Assert.NotNull(sent);
        Assert.Equal(ChatRequestedEventTestsHelper.RecipientEmail, sent!.ToEmail);
        Assert.Contains(ChatRequestedEventTestsHelper.RequesterName, sent.Subject);
        Assert.Contains("wants to chat", sent.Subject);
    }

    [Fact]
    public async Task WhenRequestTargetsAnIdea_ShouldMentionIdeaInSubject()
    {
        //Arrange
        Guid conversationId = Guid.NewGuid();
        Guid postId = Guid.NewGuid();
        List<Conversation> conversations = new List<Conversation>
        {
            ChatRequestedEventTestsHelper.GetConversation(conversationId, postId),
        };
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            ChatRequestedEventTestsHelper.GetIdea(postId),
        };
        List<AuthorInfo> authors = new List<AuthorInfo>
        {
            ChatRequestedEventTestsHelper.GetRequester(),
            ChatRequestedEventTestsHelper.GetRecipient(ChatRequestedEventTestsHelper.RecipientEmail),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, conversations: conversations, authors: authors);
        Mock<IEmailSender> emailStub = new Mock<IEmailSender>();
        EmailMessage? sent = null;
        emailStub
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((message, _) => sent = message)
            .Returns(Task.CompletedTask);

        ChatRequestedProcessor processor = new ChatRequestedProcessor(
            contextStub.Object, emailStub.Object, ChatRequestedEventTestsHelper.GetUrls());

        //Act
        await processor.ProcessAsync(ChatRequestedEventTestsHelper.GetMessage(conversationId), default);

        //Assert
        Assert.NotNull(sent);
        Assert.Contains(ChatRequestedEventTestsHelper.PostName, sent!.Subject);
        Assert.Contains($"/ideas/{postId}", sent.HtmlBody);
    }
}
