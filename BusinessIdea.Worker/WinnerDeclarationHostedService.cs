using BusinessIdea.Application.Features.Winners;

namespace BusinessIdea.Worker;

/// <summary>
/// Runs the weekly-winner declaration on startup and then once an hour. The
/// underlying service is idempotent and backfills missed weeks, so the exact
/// cadence is uncritical — hourly just keeps Monday declarations timely.
/// </summary>
public class WinnerDeclarationHostedService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WinnerDeclarationHostedService> _logger;

    public WinnerDeclarationHostedService(
        IServiceScopeFactory scopeFactory, ILogger<WinnerDeclarationHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Winner declaration service started; checking every {Hours}h.", Interval.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var declarer = scope.ServiceProvider.GetRequiredService<IWinnerDeclarationService>();
                var declared = await declarer.DeclareDueWinnersAsync(stoppingToken);
                if (declared > 0)
                {
                    _logger.LogInformation("Declared {Count} weekly winner(s).", declared);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Winner declaration failed; retrying next cycle.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
