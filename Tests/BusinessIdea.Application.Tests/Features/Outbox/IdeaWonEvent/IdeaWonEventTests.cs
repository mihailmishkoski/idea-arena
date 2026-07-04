#nullable enable
namespace BusinessIdea.Application.Tests.Features.Outbox.IdeaWonEvent;

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

public class IdeaWonEventTests
{
    [Fact]
    public async Task WhenWinnerRowIsGone_ShouldNotSendEmail()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<IEmailSender> emailStub = new Mock<IEmailSender>();

        IdeaWonProcessor processor = new IdeaWonProcessor(
            contextStub.Object, emailStub.Object, IdeaWonEventTestsHelper.GetUrls());

        //Act
        await processor.ProcessAsync(IdeaWonEventTestsHelper.GetMessage(Guid.NewGuid()), default);

        //Assert
        emailStub.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenAuthorHasNoEmail_ShouldNotSendEmail()
    {
        //Arrange
        Guid winnerId = Guid.NewGuid();
        List<WeeklyWinner> weeklyWinners = new List<WeeklyWinner>
        {
            IdeaWonEventTestsHelper.GetWinner(winnerId, Guid.NewGuid()),
        };
        List<AuthorInfo> authors = new List<AuthorInfo>
        {
            IdeaWonEventTestsHelper.GetAuthor(email: null),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            weeklyWinners: weeklyWinners, authors: authors);
        Mock<IEmailSender> emailStub = new Mock<IEmailSender>();

        IdeaWonProcessor processor = new IdeaWonProcessor(
            contextStub.Object, emailStub.Object, IdeaWonEventTestsHelper.GetUrls());

        //Act
        await processor.ProcessAsync(IdeaWonEventTestsHelper.GetMessage(winnerId), default);

        //Assert
        emailStub.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenIdeaWon_ShouldEmailAuthorWithScoreAndIdeaLink()
    {
        //Arrange
        Guid winnerId = Guid.NewGuid();
        Guid postId = Guid.NewGuid();
        List<WeeklyWinner> weeklyWinners = new List<WeeklyWinner>
        {
            IdeaWonEventTestsHelper.GetWinner(winnerId, postId),
        };
        List<AuthorInfo> authors = new List<AuthorInfo>
        {
            IdeaWonEventTestsHelper.GetAuthor(IdeaWonEventTestsHelper.AuthorEmail),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            weeklyWinners: weeklyWinners, authors: authors);
        Mock<IEmailSender> emailStub = new Mock<IEmailSender>();
        EmailMessage? sent = null;
        emailStub
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((message, _) => sent = message)
            .Returns(Task.CompletedTask);

        IdeaWonProcessor processor = new IdeaWonProcessor(
            contextStub.Object, emailStub.Object, IdeaWonEventTestsHelper.GetUrls());

        //Act
        await processor.ProcessAsync(IdeaWonEventTestsHelper.GetMessage(winnerId), default);

        //Assert
        Assert.NotNull(sent);
        Assert.Equal(IdeaWonEventTestsHelper.AuthorEmail, sent!.ToEmail);
        Assert.Contains(IdeaWonEventTestsHelper.PostName, sent.Subject);
        Assert.Contains("won", sent.Subject);
        Assert.Contains("<strong>5</strong>", sent.HtmlBody);
        Assert.Contains($"/ideas/{postId}", sent.HtmlBody);
    }
}
