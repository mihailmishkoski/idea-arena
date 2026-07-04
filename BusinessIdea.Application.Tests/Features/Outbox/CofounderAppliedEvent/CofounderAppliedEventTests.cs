#nullable enable
namespace BusinessIdea.Application.Tests.Features.Outbox.CofounderAppliedEvent;

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

public class CofounderAppliedEventTests
{
    [Fact]
    public async Task WhenConversationWasDeleted_ShouldNotSendEmail()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<IEmailSender> emailStub = new Mock<IEmailSender>();

        CofounderAppliedProcessor processor = new CofounderAppliedProcessor(
            contextStub.Object, emailStub.Object, CofounderAppliedEventTestsHelper.GetUrls());

        //Act
        await processor.ProcessAsync(CofounderAppliedEventTestsHelper.GetMessage(Guid.NewGuid()), default);

        //Assert
        emailStub.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenAuthorHasNoEmail_ShouldNotSendEmail()
    {
        //Arrange
        Guid conversationId = Guid.NewGuid();
        List<Conversation> conversations = new List<Conversation>
        {
            CofounderAppliedEventTestsHelper.GetConversation(conversationId),
        };
        List<AuthorInfo> authors = new List<AuthorInfo>
        {
            CofounderAppliedEventTestsHelper.GetApplicant(),
            CofounderAppliedEventTestsHelper.GetAuthor(email: null),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            conversations: conversations, authors: authors);
        Mock<IEmailSender> emailStub = new Mock<IEmailSender>();

        CofounderAppliedProcessor processor = new CofounderAppliedProcessor(
            contextStub.Object, emailStub.Object, CofounderAppliedEventTestsHelper.GetUrls());

        //Act
        await processor.ProcessAsync(CofounderAppliedEventTestsHelper.GetMessage(conversationId), default);

        //Assert
        emailStub.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenApplied_ShouldEmailAuthorWithHtmlEscapedApplicationText()
    {
        //Arrange
        Guid conversationId = Guid.NewGuid();
        List<Conversation> conversations = new List<Conversation>
        {
            CofounderAppliedEventTestsHelper.GetConversation(conversationId),
        };
        List<AuthorInfo> authors = new List<AuthorInfo>
        {
            CofounderAppliedEventTestsHelper.GetApplicant(),
            CofounderAppliedEventTestsHelper.GetAuthor(CofounderAppliedEventTestsHelper.AuthorEmail),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            conversations: conversations, authors: authors);
        Mock<IEmailSender> emailStub = new Mock<IEmailSender>();
        EmailMessage? sent = null;
        emailStub
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((message, _) => sent = message)
            .Returns(Task.CompletedTask);

        CofounderAppliedProcessor processor = new CofounderAppliedProcessor(
            contextStub.Object, emailStub.Object, CofounderAppliedEventTestsHelper.GetUrls());

        //Act
        await processor.ProcessAsync(CofounderAppliedEventTestsHelper.GetMessage(conversationId), default);

        //Assert
        Assert.NotNull(sent);
        Assert.Equal(CofounderAppliedEventTestsHelper.AuthorEmail, sent!.ToEmail);
        Assert.Contains(CofounderAppliedEventTestsHelper.ApplicantName, sent.Subject);
        // User-provided text must be escaped: the literal <script> tag from the
        // application must never survive into the HTML body.
        Assert.DoesNotContain("<script>", sent.HtmlBody);
        Assert.Contains("&lt;script&gt;", sent.HtmlBody);
        Assert.Contains("Role I can take: CTO", sent.HtmlBody);
    }
}
