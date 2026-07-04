namespace BusinessIdea.Application.Features.Winners;

/// <summary>
/// Declares the winner of every finished competition week that does not have
/// one yet. Safe to call repeatedly (idempotent) and after downtime (it
/// backfills missed weeks).
/// </summary>
public interface IWinnerDeclarationService
{
    /// <returns>How many new winners were declared.</returns>
    Task<int> DeclareDueWinnersAsync(CancellationToken cancellationToken);
}
