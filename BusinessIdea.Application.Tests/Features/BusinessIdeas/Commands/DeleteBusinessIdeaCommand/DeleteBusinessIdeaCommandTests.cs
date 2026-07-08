#nullable enable
namespace BusinessIdea.Application.Tests.Features.BusinessIdeas.Commands.DeleteBusinessIdeaCommandTests;

using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Features.BusinessIdeas.Commands.DeleteBusinessIdea;
using BusinessIdea.Application.Tests.Common;
using BusinessIdea.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class DeleteBusinessIdeaCommandTests
{
    [Fact]
    public async Task WhenIdeaNotFound_ShouldThrowNotFoundException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(DeleteBusinessIdeaCommandTestsHelper.AuthorId);

        DeleteBusinessIdeaCommand command = new DeleteBusinessIdeaCommand(Guid.NewGuid());
        DeleteBusinessIdeaCommandHandler handler = new DeleteBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);

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
            DeleteBusinessIdeaCommandTestsHelper.GetIdea(ideaId),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(businessIdeas: businessIdeas);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(DeleteBusinessIdeaCommandTestsHelper.OtherUserId);

        DeleteBusinessIdeaCommand command = new DeleteBusinessIdeaCommand(ideaId);
        DeleteBusinessIdeaCommandHandler handler = new DeleteBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(function);
        Assert.Single(businessIdeas);
    }

    [Fact]
    public async Task WhenUserIsAuthor_ShouldRemoveIdeaAndSave()
    {
        //Arrange
        Guid ideaId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            DeleteBusinessIdeaCommandTestsHelper.GetIdea(ideaId),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(businessIdeas: businessIdeas);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(DeleteBusinessIdeaCommandTestsHelper.AuthorId);

        DeleteBusinessIdeaCommand command = new DeleteBusinessIdeaCommand(ideaId);
        DeleteBusinessIdeaCommandHandler handler = new DeleteBusinessIdeaCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        await handler.Handle(command, default);

        //Assert
         Assert.Single(businessIdeas);
         Assert.NotNull(businessIdeas[0].DeletedOn);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
