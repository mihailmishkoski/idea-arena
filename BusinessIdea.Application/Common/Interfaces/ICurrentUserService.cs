namespace BusinessIdea.Application.Common.Interfaces;

/// <summary>
/// Provides the identity of the user making the current request. Implemented in
/// the Web layer (which owns the HTTP context) and consumed by handlers so they
/// never touch <c>HttpContext</c> directly.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Identity id of the authenticated user, or null if anonymous.</summary>
    string? UserId { get; }

    bool IsAuthenticated { get; }
}
