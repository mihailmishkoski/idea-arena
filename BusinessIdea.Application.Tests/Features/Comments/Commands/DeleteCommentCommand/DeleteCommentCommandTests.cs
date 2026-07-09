#nullable enable
namespace BusinessIdea.Application.Tests.Features.Comments.Commands.DeleteCommentCommandTests;

using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Features.Comments.Commands.DeleteComment;
using BusinessIdea.Application.Tests.Common;
using BusinessIdea.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class DeleteCommentCommandTests
{
    [Fact]
    public async Task WhenCommentNotFound_ShouldThrowNotFoundException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(DeleteCommentCommandTestsHelper.AuthorId);

        DeleteCommentCommand command = new DeleteCommentCommand(Guid.NewGuid());
        DeleteCommentCommandHandler handler = new DeleteCommentCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<NotFoundException>(function);
    }

    [Fact]
    public async Task WhenUserIsNotAuthor_ShouldThrowForbiddenAccessException()
    {
        //Arrange
        Guid commentId = Guid.NewGuid();
        List<Comment> comments = new List<Comment>
        {
            DeleteCommentCommandTestsHelper.GetComment(commentId),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(comments: comments);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(DeleteCommentCommandTestsHelper.OtherUserId);

        DeleteCommentCommand command = new DeleteCommentCommand(commentId);
        DeleteCommentCommandHandler handler = new DeleteCommentCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(function);
        Assert.Single(comments);
    }

    [Fact]
    public async Task WhenUserIsAuthor_ShouldRemoveCommentAndSave()
    {
        //Arrange
        Guid commentId = Guid.NewGuid();
        List<Comment> comments = new List<Comment>
        {
            DeleteCommentCommandTestsHelper.GetComment(commentId),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(comments: comments);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(DeleteCommentCommandTestsHelper.AuthorId);

        DeleteCommentCommand command = new DeleteCommentCommand(commentId);
        DeleteCommentCommandHandler handler = new DeleteCommentCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        await handler.Handle(command, default);

        //Assert
        Assert.Single(comments);
        Assert.NotNull(comments[0].DeletedOn);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
