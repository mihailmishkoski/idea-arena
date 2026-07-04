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
}
