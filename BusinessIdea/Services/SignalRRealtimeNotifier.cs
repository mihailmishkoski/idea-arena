using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BusinessIdea.Web.Services;

/// <summary>
/// SignalR-backed implementation of <see cref="IRealtimeNotifier"/>. Sends to
/// every connection the user currently has open; if they are offline the call
/// is a no-op and they'll see the change on their next fetch.
/// </summary>
public class SignalRRealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<ChatHub> _hubContext;

    public SignalRRealtimeNotifier(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task SendToUserAsync(string userId, string method, object payload, CancellationToken cancellationToken)
        => _hubContext.Clients.User(userId).SendAsync(method, payload, cancellationToken);
}
