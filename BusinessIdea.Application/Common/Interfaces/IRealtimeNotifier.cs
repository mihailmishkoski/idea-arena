namespace BusinessIdea.Application.Common.Interfaces;

/// <summary>
/// Pushes real-time events to a connected user. Implemented in the Web layer
/// with SignalR; the Application layer only knows "send this payload to that
/// user", keeping WebSocket details out of the handlers (DIP).
/// </summary>
public interface IRealtimeNotifier
{
    /// <summary>Sends <paramref name="payload"/> to every open connection of the user.</summary>
    Task SendToUserAsync(string userId, string method, object payload, CancellationToken cancellationToken);
}
