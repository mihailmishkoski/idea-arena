#nullable enable
namespace BusinessIdea.Application.Tests.Features.Votes.CastCommentVoteCommandTests;

using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Features.Votes;
using BusinessIdea.Application.Features.Votes.CastCommentVote;
using BusinessIdea.Application.Tests.Common;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class CastCommentVoteCommandTests
{
    [Fact]
    public async Task WhenNotSignedIn_ShouldThrowForbiddenAccessException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns((string?)null);

        CastCommentVoteCommand command = new CastCommentVoteCommand(Guid.NewGuid(), VoteDirection.Up);
        CastCommentVoteCommandHandler handler = new CastCommentVoteCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(function);
    }

    [Fact]
    public async Task WhenCommentNotFound_ShouldThrowNotFoundException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CastCommentVoteCommandTestsHelper.UserId);

        CastCommentVoteCommand command = new CastCommentVoteCommand(Guid.NewGuid(), VoteDirection.Up);
        CastCommentVoteCommandHandler handler = new CastCommentVoteCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<NotFoundException>(function);
    }

    [Fact]
    public async Task WhenNoExistingVote_ShouldAddVote()
    {
        //Arrange
        Guid commentId = Guid.NewGuid();
        List<Comment> comments = new List<Comment>
        {
            CastCommentVoteCommandTestsHelper.GetComment(commentId),
        };
        List<CommentVote> commentVotes = new List<CommentVote>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            comments: comments, commentVotes: commentVotes);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CastCommentVoteCommandTestsHelper.UserId);

        CastCommentVoteCommand command = new CastCommentVoteCommand(commentId, VoteDirection.Down);
        CastCommentVoteCommandHandler handler = new CastCommentVoteCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        VoteResultDto result = await handler.Handle(command, default);

        //Assert
        CommentVote vote = Assert.Single(commentVotes);
        Assert.Equal(VoteDirection.Down, vote.Direction);
        Assert.Equal(1, result.DownVotes);
        Assert.Equal(VoteDirection.Down, result.CurrentUserVote);
    }

    [Fact]
    public async Task WhenVotingSameDirectionAgain_ShouldClearVote()
    {
        //Arrange
        Guid commentId = Guid.NewGuid();
        List<Comment> comments = new List<Comment>
        {
            CastCommentVoteCommandTestsHelper.GetComment(commentId),
        };
        List<CommentVote> commentVotes = new List<CommentVote>
        {
            CastCommentVoteCommandTestsHelper.GetVote(commentId, VoteDirection.Down),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            comments: comments, commentVotes: commentVotes);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CastCommentVoteCommandTestsHelper.UserId);

        CastCommentVoteCommand command = new CastCommentVoteCommand(commentId, VoteDirection.Down);
        CastCommentVoteCommandHandler handler = new CastCommentVoteCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        VoteResultDto result = await handler.Handle(command, default);

        //Assert
        Assert.Empty(commentVotes);
        Assert.Null(result.CurrentUserVote);
    }

    [Fact]
    public async Task WhenVotingOppositeDirection_ShouldFlipVote()
    {
        //Arrange
        Guid commentId = Guid.NewGuid();
        List<Comment> comments = new List<Comment>
        {
            CastCommentVoteCommandTestsHelper.GetComment(commentId),
        };
        List<CommentVote> commentVotes = new List<CommentVote>
        {
            CastCommentVoteCommandTestsHelper.GetVote(commentId, VoteDirection.Down),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            comments: comments, commentVotes: commentVotes);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CastCommentVoteCommandTestsHelper.UserId);

        CastCommentVoteCommand command = new CastCommentVoteCommand(commentId, VoteDirection.Up);
        CastCommentVoteCommandHandler handler = new CastCommentVoteCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        VoteResultDto result = await handler.Handle(command, default);

        //Assert
        CommentVote vote = Assert.Single(commentVotes);
        Assert.Equal(VoteDirection.Up, vote.Direction);
        Assert.Equal(1, result.UpVotes);
        Assert.Equal(0, result.DownVotes);
        Assert.Equal(VoteDirection.Up, result.CurrentUserVote);
    }
}
