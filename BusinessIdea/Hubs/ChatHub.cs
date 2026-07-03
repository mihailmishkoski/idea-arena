using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BusinessIdea.Web.Hubs;

/// <summary>
/// The realtime endpoint the SPA connects to. All pushes go server → client
/// ("notification", "chatMessage"); clients act via the regular HTTP API, so the
/// hub itself stays empty. SignalR's default user-id provider maps connections
/// to the NameIdentifier claim, which lets handlers target Clients.User(userId).
/// </summary>
[Authorize]
public class ChatHub : Hub
{
}
