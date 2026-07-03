namespace BusinessIdea.Application.Common.Exceptions;

/// <summary>
/// Thrown when an authenticated user tries to act on a resource they do not own
/// (e.g. editing someone else's idea). Mapped to HTTP 403.
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "You are not allowed to perform this action.")
        : base(message)
    {
    }
}
