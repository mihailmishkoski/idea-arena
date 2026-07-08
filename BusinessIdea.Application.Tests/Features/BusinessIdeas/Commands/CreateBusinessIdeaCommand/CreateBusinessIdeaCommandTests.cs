#nullable enable
namespace BusinessIdea.Application.Tests.Features.BusinessIdeas.Commands.CreateBusinessIdeaCommandTests;

using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Application.Features.BusinessIdeas.Commands.CreateBusinessIdea;
using BusinessIdea.Application.Tests.Common;
using BusinessIdea.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using BusinessIdea.Domain.Enums;

public class CreateBusinessIdeaCommandTests
{
    [Fact]
    public async Task WhenNotSignedIn_ShouldThrowForbiddenAccessException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns((string?)null);

        CreateBusinessIdeaCommand command = CreateBusinessIdeaCommandTestsHelper.GetCommand();
        CreateBusinessIdeaCommandHandler handler = new CreateBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(function);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenSignedIn_ShouldAddTrimmedIdeaAndSave()
    {
        //Arrange
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(businessIdeas: businessIdeas);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CreateBusinessIdeaCommandTestsHelper.UserId);

        CreateBusinessIdeaCommand command = CreateBusinessIdeaCommandTestsHelper.GetCommand();
        CreateBusinessIdeaCommandHandler handler = new CreateBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        Guid result = await handler.Handle(command, default);

        //Assert
        BusinessIdeaPost idea = Assert.Single(businessIdeas);
        Assert.Equal(result, idea.Id);
        Assert.Equal(CreateBusinessIdeaCommandTestsHelper.Name.Trim(), idea.Name);
        Assert.Equal(CreateBusinessIdeaCommandTestsHelper.UserId, idea.AuthorId);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WhenSignedIn_ShouldEnqueueIdeaCreatedOutboxEvent()
    {
        //Arrange
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>();
        List<OutboxMessage> outboxMessages = new List<OutboxMessage>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, outboxMessages: outboxMessages);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CreateBusinessIdeaCommandTestsHelper.UserId);

        CreateBusinessIdeaCommand command = CreateBusinessIdeaCommandTestsHelper.GetCommand();
        CreateBusinessIdeaCommandHandler handler = new CreateBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        Guid result = await handler.Handle(command, default);

        //Assert
        OutboxMessage outboxMessage = Assert.Single(outboxMessages);
        Assert.Equal(OutboxEventTypes.IdeaCreated, outboxMessage.Type);
        IdeaCreatedPayload payload = OutboxMessageFactory.Deserialize<IdeaCreatedPayload>(outboxMessage);
        Assert.Equal(result, payload.PostId);
    }

    [Fact]
    public async Task WhenSignedIn_ShouldPersistCategoriesUnchanged()
    {
        //Arrange
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(businessIdeas: businessIdeas);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CreateBusinessIdeaCommandTestsHelper.UserId);
        List<BusinessIdeaCategory> categories = new List<BusinessIdeaCategory>
        {
            BusinessIdeaCategory.Fintech,
            BusinessIdeaCategory.Health,
        };
        CreateBusinessIdeaCommand command = CreateBusinessIdeaCommandTestsHelper.GetCommand(categories: categories);
        CreateBusinessIdeaCommandHandler handler = new CreateBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);
        //Act
        await handler.Handle(command, default);
        //Assert
        BusinessIdeaPost idea = Assert.Single(businessIdeas);
        Assert.Equal(categories, idea.Categories);
    }

    [Fact]
    public async Task WhenOptionalFieldsProvided_ShouldTrimThemAllBeforeSaving()
    {
        //Arrange
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(businessIdeas: businessIdeas);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CreateBusinessIdeaCommandTestsHelper.UserId);
        CreateBusinessIdeaCommand command = CreateBusinessIdeaCommandTestsHelper.GetCommand(
            competition: "  Big Corp  ",
            incomeStrategy: "  Subscriptions  ",
            exitStrategy: "  Acquisition  ",
            videoPitchUrl: "  https://example.com/pitch  ");
        CreateBusinessIdeaCommandHandler handler = new CreateBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);
        //Act
        await handler.Handle(command, default);
        //Assert
        BusinessIdeaPost idea = Assert.Single(businessIdeas);
        Assert.Equal("Big Corp", idea.Competition);
        Assert.Equal("Subscriptions", idea.IncomeStrategy);
        Assert.Equal("Acquisition", idea.ExitStrategy);
        Assert.Equal("https://example.com/pitch", idea.VideoPitchUrl);
    }

    [Fact]
    public async Task WhenOptionalFieldsAreNull_ShouldSaveWithoutThrowing()
    {
        //Arrange
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(businessIdeas: businessIdeas);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CreateBusinessIdeaCommandTestsHelper.UserId);
        CreateBusinessIdeaCommand command = CreateBusinessIdeaCommandTestsHelper.GetCommand();
        CreateBusinessIdeaCommandHandler handler = new CreateBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);
        //Act
        await handler.Handle(command, default);
        //Assert
        BusinessIdeaPost idea = Assert.Single(businessIdeas);
        Assert.Null(idea.Competition);
        Assert.Null(idea.IncomeStrategy);
        Assert.Null(idea.ExitStrategy);
        Assert.Null(idea.VideoPitchUrl);
    }

    [Fact]
    public async Task WhenCategoriesAtMaxAllowed_ShouldPersistAllThree()
    {
        //Arrange
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(businessIdeas: businessIdeas);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CreateBusinessIdeaCommandTestsHelper.UserId);
        List<BusinessIdeaCategory> categories = new List<BusinessIdeaCategory>
        {
            BusinessIdeaCategory.Tech,
            BusinessIdeaCategory.SaaS,
            BusinessIdeaCategory.Fintech,
        };
        CreateBusinessIdeaCommand command = CreateBusinessIdeaCommandTestsHelper.GetCommand(categories: categories);
        CreateBusinessIdeaCommandHandler handler = new CreateBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        await handler.Handle(command, default);

        //Assert
        BusinessIdeaPost idea = Assert.Single(businessIdeas);
        Assert.Equal(3, idea.Categories.Count);
        Assert.Equal(categories, idea.Categories);
    }

    [Fact]
    public async Task WhenSingleCategoryProvided_ShouldPersistExactlyOne()
    {
        //Arrange
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(businessIdeas: businessIdeas);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CreateBusinessIdeaCommandTestsHelper.UserId);
        List<BusinessIdeaCategory> categories = new List<BusinessIdeaCategory> { BusinessIdeaCategory.Sustainability };
        CreateBusinessIdeaCommand command = CreateBusinessIdeaCommandTestsHelper.GetCommand(categories: categories);
        CreateBusinessIdeaCommandHandler handler = new CreateBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        await handler.Handle(command, default);

        //Assert
        BusinessIdeaPost idea = Assert.Single(businessIdeas);
        Assert.Single(idea.Categories);
        Assert.Equal(BusinessIdeaCategory.Sustainability, idea.Categories[0]);
    }

    [Fact]
    public async Task WhenCategoryOrderProvided_ShouldPreserveOrderOnSave()
    {
        //Arrange
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(businessIdeas: businessIdeas);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CreateBusinessIdeaCommandTestsHelper.UserId);
        List<BusinessIdeaCategory> categories = new List<BusinessIdeaCategory>
        {
            BusinessIdeaCategory.Travel,
            BusinessIdeaCategory.Agriculture,
        };
        CreateBusinessIdeaCommand command = CreateBusinessIdeaCommandTestsHelper.GetCommand(categories: categories);
        CreateBusinessIdeaCommandHandler handler = new CreateBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        await handler.Handle(command, default);

        //Assert
        BusinessIdeaPost idea = Assert.Single(businessIdeas);
        Assert.Equal(BusinessIdeaCategory.Travel, idea.Categories[0]);
        Assert.Equal(BusinessIdeaCategory.Agriculture, idea.Categories[1]);
    }
}
