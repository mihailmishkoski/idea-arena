#nullable enable
namespace BusinessIdea.Application.Tests.Features.Votes.CastPostVoteCommandTests;

using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Features.Votes;
using BusinessIdea.Application.Features.Votes.CastPostVote;
using BusinessIdea.Application.Tests.Common;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class CastPostVoteCommandTests
{
    [Fact]
    public async Task WhenNotSignedIn_ShouldThrowForbiddenAccessException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns((string?)null);

        CastPostVoteCommand command = new CastPostVoteCommand(Guid.NewGuid(), VoteDirection.Up);
        CastPostVoteCommandHandler handler = new CastPostVoteCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(function);
    }

    [Fact]
    public async Task WhenPostNotFound_ShouldThrowNotFoundException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CastPostVoteCommandTestsHelper.UserId);

        CastPostVoteCommand command = new CastPostVoteCommand(Guid.NewGuid(), VoteDirection.Up);
        CastPostVoteCommandHandler handler = new CastPostVoteCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<NotFoundException>(function);
    }

    [Fact]
    public async Task WhenNoExistingVote_ShouldAddVote()
    {
        //Arrange
        Guid postId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            CastPostVoteCommandTestsHelper.GetIdea(postId),
        };
        List<PostVote> postVotes = new List<PostVote>();
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, postVotes: postVotes);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CastPostVoteCommandTestsHelper.UserId);

        CastPostVoteCommand command = new CastPostVoteCommand(postId, VoteDirection.Up);
        CastPostVoteCommandHandler handler = new CastPostVoteCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        VoteResultDto result = await handler.Handle(command, default);

        //Assert
        PostVote vote = Assert.Single(postVotes);
        Assert.Equal(VoteDirection.Up, vote.Direction);
        Assert.Equal(1, result.UpVotes);
        Assert.Equal(0, result.DownVotes);
        Assert.Equal(VoteDirection.Up, result.CurrentUserVote);
    }

    [Fact]
    public async Task WhenVotingSameDirectionAgain_ShouldClearVote()
    {
        //Arrange
        Guid postId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            CastPostVoteCommandTestsHelper.GetIdea(postId),
        };
        List<PostVote> postVotes = new List<PostVote>
        {
            CastPostVoteCommandTestsHelper.GetVote(postId, VoteDirection.Up),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, postVotes: postVotes);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CastPostVoteCommandTestsHelper.UserId);

        CastPostVoteCommand command = new CastPostVoteCommand(postId, VoteDirection.Up);
        CastPostVoteCommandHandler handler = new CastPostVoteCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        VoteResultDto result = await handler.Handle(command, default);

        //Assert
        Assert.Empty(postVotes);
        Assert.Equal(0, result.UpVotes);
        Assert.Null(result.CurrentUserVote);
    }

    [Fact]
    public async Task WhenVotingOppositeDirection_ShouldFlipVote()
    {
        //Arrange
        Guid postId = Guid.NewGuid();
        List<BusinessIdeaPost> businessIdeas = new List<BusinessIdeaPost>
        {
            CastPostVoteCommandTestsHelper.GetIdea(postId),
        };
        List<PostVote> postVotes = new List<PostVote>
        {
            CastPostVoteCommandTestsHelper.GetVote(postId, VoteDirection.Up),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(
            businessIdeas: businessIdeas, postVotes: postVotes);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(CastPostVoteCommandTestsHelper.UserId);

        CastPostVoteCommand command = new CastPostVoteCommand(postId, VoteDirection.Down);
        CastPostVoteCommandHandler handler = new CastPostVoteCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        VoteResultDto result = await handler.Handle(command, default);

        //Assert
        PostVote vote = Assert.Single(postVotes);
        Assert.Equal(VoteDirection.Down, vote.Direction);
        Assert.Equal(0, result.UpVotes);
        Assert.Equal(1, result.DownVotes);
        Assert.Equal(VoteDirection.Down, result.CurrentUserVote);
    }
}
