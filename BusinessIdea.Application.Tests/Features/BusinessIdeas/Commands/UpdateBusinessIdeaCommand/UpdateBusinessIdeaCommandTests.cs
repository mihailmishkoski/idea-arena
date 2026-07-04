#nullable enable
namespace BusinessIdea.Application.Tests.Features.BusinessIdeas.Commands.UpdateBusinessIdeaCommandTests;

using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Features.BusinessIdeas.Commands.UpdateBusinessIdea;
using BusinessIdea.Application.Tests.Common;
using BusinessIdea.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class UpdateBusinessIdeaCommandTests
{
    [Fact]
    public async Task WhenIdeaNotFound_ShouldThrowNotFoundException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(UpdateBusinessIdeaCommandTestsHelper.AuthorId);

        UpdateBusinessIdeaCommand command = UpdateBusinessIdeaCommandTestsHelper.GetCommand(Guid.NewGuid());
        UpdateBusinessIdeaCommandHandler handler = new UpdateBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<NotFoundException>(function);
    }

    [Fact]
    public async Task WhenUserIsNotAuthor_ShouldThrowForbiddenAccessException()
    {
        //Arrange
        Guid ideaId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            UpdateBusinessIdeaCommandTestsHelper.GetIdea(ideaId),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(businessIdeas: businessIdeas);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(UpdateBusinessIdeaCommandTestsHelper.OtherUserId);

        UpdateBusinessIdeaCommand command = UpdateBusinessIdeaCommandTestsHelper.GetCommand(ideaId);
        UpdateBusinessIdeaCommandHandler handler = new UpdateBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(function);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenUserIsAuthor_ShouldUpdateTrimmedFieldsAndSave()
    {
        //Arrange
        Guid ideaId = Guid.NewGuid();
        BusinessIdeaPost idea = UpdateBusinessIdeaCommandTestsHelper.GetIdea(ideaId);
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost> { idea };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(businessIdeas: businessIdeas);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(UpdateBusinessIdeaCommandTestsHelper.AuthorId);

        UpdateBusinessIdeaCommand command = UpdateBusinessIdeaCommandTestsHelper.GetCommand(ideaId);
        UpdateBusinessIdeaCommandHandler handler = new UpdateBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        await handler.Handle(command, default);

        //Assert
        Assert.Equal(UpdateBusinessIdeaCommandTestsHelper.UpdatedName.Trim(), idea.Name);
        Assert.Equal("New UVP", idea.UniqueValueProposition);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
