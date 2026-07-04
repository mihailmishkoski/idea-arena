#nullable enable
namespace BusinessIdea.Application.Tests.Features.Outbox.IdeaCreatedEvent;

using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Features.Outbox;
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

public class IdeaCreatedEventTests
{
    [Fact]
    public async Task WhenIdeaWasDeleted_ShouldDoNothing()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<IAiCritic> criticStub = new Mock<IAiCritic>();
        Mock<IAiCriticUserProvider> criticUserStub = new Mock<IAiCriticUserProvider>();

        IdeaCreatedProcessor processor = new IdeaCreatedProcessor(
            contextStub.Object, criticStub.Object, criticUserStub.Object);

        //Act
        await processor.ProcessAsync(IdeaCreatedEventTestsHelper.GetMessage(Guid.NewGuid()), default);

        //Assert
        criticStub.Verify(
            x => x.GenerateQuestionsAsync(It.IsAny<BusinessIdeaPost>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task WhenQuestionsAlreadySeeded_ShouldNotCallCriticAgain()
    {
        //Arrange
        Guid postId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            IdeaCreatedEventTestsHelper.GetIdea(postId),
        };
        List<Comment> comments = new List<Comment>
        {
            new Comment
            {
                PostId = postId,
                AuthorId = IdeaCreatedEventTestsHelper.CriticUserId,
                Content = "Existing critic question",
            },
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, comments: comments);
        Mock<IAiCritic> criticStub = new Mock<IAiCritic>();
        Mock<IAiCriticUserProvider> criticUserStub = new Mock<IAiCriticUserProvider>();
        criticUserStub
            .Setup(x => x.GetAiCriticUserIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdeaCreatedEventTestsHelper.CriticUserId);

        IdeaCreatedProcessor processor = new IdeaCreatedProcessor(
            contextStub.Object, criticStub.Object, criticUserStub.Object);

        //Act
        await processor.ProcessAsync(IdeaCreatedEventTestsHelper.GetMessage(postId), default);

        //Assert
        criticStub.Verify(
            x => x.GenerateQuestionsAsync(It.IsAny<BusinessIdeaPost>(), It.IsAny<CancellationToken>()),
            Times.Never);
        Assert.Single(comments);
    }

    [Fact]
    public async Task WhenQuestionsGenerated_ShouldSeedThemAsCriticComments()
    {
        //Arrange
        Guid postId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            IdeaCreatedEventTestsHelper.GetIdea(postId),
        };
        List<Comment> comments = new List<Comment>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, comments: comments);
        Mock<IAiCritic> criticStub = new Mock<IAiCritic>();
        Mock<IAiCriticUserProvider> criticUserStub = new Mock<IAiCriticUserProvider>();
        criticUserStub
            .Setup(x => x.GetAiCriticUserIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdeaCreatedEventTestsHelper.CriticUserId);
        criticStub
            .Setup(x => x.GenerateQuestionsAsync(It.IsAny<BusinessIdeaPost>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdeaCreatedEventTestsHelper.GetQuestions());

        IdeaCreatedProcessor processor = new IdeaCreatedProcessor(
            contextStub.Object, criticStub.Object, criticUserStub.Object);

        //Act
        await processor.ProcessAsync(IdeaCreatedEventTestsHelper.GetMessage(postId), default);

        //Assert
        Assert.Equal(3, comments.Count);
        Assert.All(comments, c => Assert.Equal(IdeaCreatedEventTestsHelper.CriticUserId, c.AuthorId));
        Assert.All(comments, c => Assert.Equal(postId, c.PostId));
        Assert.Contains(comments, c => c.TargetMetric == IdeaMetric.Competition);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WhenCriticReturnsNoQuestions_ShouldNotSave()
    {
        //Arrange
        Guid postId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            IdeaCreatedEventTestsHelper.GetIdea(postId),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(businessIdeas: businessIdeas);
        Mock<IAiCritic> criticStub = new Mock<IAiCritic>();
        Mock<IAiCriticUserProvider> criticUserStub = new Mock<IAiCriticUserProvider>();
        criticUserStub
            .Setup(x => x.GetAiCriticUserIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdeaCreatedEventTestsHelper.CriticUserId);
        criticStub
            .Setup(x => x.GenerateQuestionsAsync(It.IsAny<BusinessIdeaPost>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BusinessIdea.Application.Common.Models.AiCriticQuestion>());

        IdeaCreatedProcessor processor = new IdeaCreatedProcessor(
            contextStub.Object, criticStub.Object, criticUserStub.Object);

        //Act
        await processor.ProcessAsync(IdeaCreatedEventTestsHelper.GetMessage(postId), default);

        //Assert
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
