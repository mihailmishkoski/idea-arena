#nullable enable
namespace BusinessIdea.Application.Tests.Features.Winners;

using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Application.Features.Winners;
using BusinessIdea.Application.Tests.Common;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class WinnerDeclarationServiceTests
{
    [Fact]
    public async Task WhenNoIdeasExist_ShouldDeclareNothing()
    {
        //Arrange
        List<WeeklyWinner> weeklyWinners = new List<WeeklyWinner>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(weeklyWinners: weeklyWinners);

        WinnerDeclarationService service = new WinnerDeclarationService(contextStub.Object);

        //Act
        int declared = await service.DeclareDueWinnersAsync(default);

        //Assert
        Assert.Equal(0, declared);
        Assert.Empty(weeklyWinners);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenAWeekFinished_ShouldDeclareTopScoredIdeaAsWinner()
    {
        //Arrange
        DateTimeOffset weekStart = WinnerDeclarationServiceTestsHelper.LastCompletedWeekStart;
        BusinessIdeaPost runnerUp = WinnerDeclarationServiceTestsHelper.GetExpiredIdea(weekStart, "Runner-up", upVotes: 2);
        BusinessIdeaPost best = WinnerDeclarationServiceTestsHelper.GetExpiredIdea(weekStart, "Best idea", upVotes: 5);
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost> { runnerUp, best };
        List<WeeklyWinner> weeklyWinners = new List<WeeklyWinner>();
        List<Notification> notifications = new List<Notification>();
        List<OutboxMessage> outboxMessages = new List<OutboxMessage>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, weeklyWinners: weeklyWinners,
            notifications: notifications, outboxMessages: outboxMessages);

        WinnerDeclarationService service = new WinnerDeclarationService(contextStub.Object);

        //Act
        int declared = await service.DeclareDueWinnersAsync(default);

        //Assert
        Assert.Equal(1, declared);
        WeeklyWinner winner = Assert.Single(weeklyWinners);
        Assert.Equal(best.Id, winner.PostId);
        Assert.Equal("Best idea", winner.PostName);
        Assert.Equal(5, winner.Score);
        Assert.Equal(weekStart, winner.PeriodStartUtc);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WhenWeekEndedRecently_ShouldNotifyAuthorAndEnqueueIdeaWonEvent()
    {
        //Arrange
        DateTimeOffset weekStart = WinnerDeclarationServiceTestsHelper.LastCompletedWeekStart;
        BusinessIdeaPost best = WinnerDeclarationServiceTestsHelper.GetExpiredIdea(weekStart, "Best idea", upVotes: 3);
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost> { best };
        List<WeeklyWinner> weeklyWinners = new List<WeeklyWinner>();
        List<Notification> notifications = new List<Notification>();
        List<OutboxMessage> outboxMessages = new List<OutboxMessage>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, weeklyWinners: weeklyWinners,
            notifications: notifications, outboxMessages: outboxMessages);

        WinnerDeclarationService service = new WinnerDeclarationService(contextStub.Object);

        //Act
        await service.DeclareDueWinnersAsync(default);

        //Assert
        Notification notification = Assert.Single(notifications);
        Assert.Equal(WinnerDeclarationServiceTestsHelper.AuthorId, notification.UserId);
        Assert.Equal(NotificationType.IdeaWon, notification.Type);
        Assert.Contains("Best idea", notification.Text);

        OutboxMessage outboxMessage = Assert.Single(outboxMessages);
        Assert.Equal(OutboxEventTypes.IdeaWon, outboxMessage.Type);
        IdeaWonPayload payload = OutboxMessageFactory.Deserialize<IdeaWonPayload>(outboxMessage);
        Assert.Equal(weeklyWinners.Single().Id, payload.WinnerId);
    }

    [Fact]
    public async Task WhenWeekEndedLongAgo_ShouldBackfillWinnerSilently()
    {
        //Arrange
        DateTimeOffset oldWeekStart = WinnerDeclarationServiceTestsHelper.LastCompletedWeekStart.AddDays(-28);
        BusinessIdeaPost oldIdea = WinnerDeclarationServiceTestsHelper.GetExpiredIdea(oldWeekStart, "Ancient winner", upVotes: 4);
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost> { oldIdea };
        List<WeeklyWinner> weeklyWinners = new List<WeeklyWinner>();
        List<Notification> notifications = new List<Notification>();
        List<OutboxMessage> outboxMessages = new List<OutboxMessage>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, weeklyWinners: weeklyWinners,
            notifications: notifications, outboxMessages: outboxMessages);

        WinnerDeclarationService service = new WinnerDeclarationService(contextStub.Object);

        //Act
        int declared = await service.DeclareDueWinnersAsync(default);

        //Assert
        Assert.Equal(1, declared);
        Assert.Single(weeklyWinners);
        Assert.Empty(notifications);
        Assert.Empty(outboxMessages);
    }

    [Fact]
    public async Task WhenWinnerAlreadyDeclared_ShouldBeIdempotent()
    {
        //Arrange
        DateTimeOffset weekStart = WinnerDeclarationServiceTestsHelper.LastCompletedWeekStart;
        BusinessIdeaPost best = WinnerDeclarationServiceTestsHelper.GetExpiredIdea(weekStart, "Best idea", upVotes: 5);
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost> { best };
        List<WeeklyWinner> weeklyWinners = new List<WeeklyWinner>
        {
            WinnerDeclarationServiceTestsHelper.GetExistingWinner(weekStart),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, weeklyWinners: weeklyWinners);

        WinnerDeclarationService service = new WinnerDeclarationService(contextStub.Object);

        //Act
        int declared = await service.DeclareDueWinnersAsync(default);

        //Assert
        Assert.Equal(0, declared);
        Assert.Single(weeklyWinners);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenIdeaIsStillActive_ShouldNotBeDeclared()
    {
        //Arrange
        BusinessIdeaPost active = new BusinessIdeaPost
        {
            Name = "Still competing",
            UniqueValueProposition = "UVP",
            Problem = "Problem",
            Solution = "Solution",
            AuthorId = WinnerDeclarationServiceTestsHelper.AuthorId,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
        };
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost> { active };
        List<WeeklyWinner> weeklyWinners = new List<WeeklyWinner>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, weeklyWinners: weeklyWinners);

        WinnerDeclarationService service = new WinnerDeclarationService(contextStub.Object);

        //Act
        int declared = await service.DeclareDueWinnersAsync(default);

        //Assert
        Assert.Equal(0, declared);
        Assert.Empty(weeklyWinners);
    }
}
