#nullable enable
namespace BusinessIdea.Application.Tests.Features.Notifications.Commands.MarkNotificationsReadCommandTests;

using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Features.Notifications.Commands.MarkNotificationsRead;
using BusinessIdea.Application.Tests.Common;
using BusinessIdea.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class MarkNotificationsReadCommandTests
{
    [Fact]
    public async Task WhenNotSignedIn_ShouldThrowForbiddenAccessException()
    {
        //Arrange
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create();
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns((string?)null);

        MarkNotificationsReadCommand command = new MarkNotificationsReadCommand();
        MarkNotificationsReadCommandHandler handler = new MarkNotificationsReadCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        Func<Task> function = new Func<Task>(async () => await handler.Handle(command, default));

        //Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(function);
    }

    [Fact]
    public async Task WhenNothingIsUnread_ShouldNotSave()
    {
        //Arrange
        List<Notification> notifications = new List<Notification>
        {
            MarkNotificationsReadCommandTestsHelper.GetNotification(MarkNotificationsReadCommandTestsHelper.UserId, isRead: true),
        };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(notifications: notifications);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(MarkNotificationsReadCommandTestsHelper.UserId);

        MarkNotificationsReadCommand command = new MarkNotificationsReadCommand();
        MarkNotificationsReadCommandHandler handler = new MarkNotificationsReadCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        await handler.Handle(command, default);

        //Assert
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenUnreadNotificationsExist_ShouldMarkOnlyCurrentUsersAsRead()
    {
        //Arrange
        Notification mine = MarkNotificationsReadCommandTestsHelper.GetNotification(MarkNotificationsReadCommandTestsHelper.UserId, isRead: false);
        Notification someoneElses = MarkNotificationsReadCommandTestsHelper.GetNotification(MarkNotificationsReadCommandTestsHelper.OtherUserId, isRead: false);
        List<Notification> notifications = new List<Notification> { mine, someoneElses };
        Mock<IApplicationDbContext> contextStub = ApplicationDbContextMock.Create(notifications: notifications);
        Mock<ICurrentUserService> currentUserStub = new Mock<ICurrentUserService>();
        currentUserStub.Setup(x => x.UserId).Returns(MarkNotificationsReadCommandTestsHelper.UserId);

        MarkNotificationsReadCommand command = new MarkNotificationsReadCommand();
        MarkNotificationsReadCommandHandler handler = new MarkNotificationsReadCommandHandler(contextStub.Object, currentUserStub.Object);

        //Act
        await handler.Handle(command, default);

        //Assert
        Assert.True(mine.IsRead);
        Assert.False(someoneElses.IsRead);
        contextStub.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
