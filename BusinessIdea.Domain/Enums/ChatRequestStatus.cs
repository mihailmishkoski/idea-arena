namespace BusinessIdea.Domain.Enums;

/// <summary>Lifecycle of a chat request between two users.</summary>
public enum ChatRequestStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2
}
