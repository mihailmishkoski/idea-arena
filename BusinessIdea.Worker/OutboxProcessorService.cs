using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Features.Outbox;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Worker;

/// <summary>
/// The heart of the microservice: polls the transactional outbox, dispatches
/// each pending message to its processor, and records success or failure.
/// Failed messages are retried until <see cref="MaxAttempts"/>; afterwards they
/// stay in the table with <c>LastError</c> set for manual inspection.
/// </summary>
public class OutboxProcessorService : BackgroundService
{
    private const int MaxAttempts = 5;
    private const int BatchSize = 20;
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan BusyDelay = TimeSpan.FromSeconds(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorService> _logger;

    public OutboxProcessorService(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox worker started; polling every {Delay}s.", IdleDelay.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            var processedAnything = false;
            try
            {
                processedAnything = await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Batch-level failure (e.g. database unreachable) — log and keep polling.
                _logger.LogError(ex, "Outbox batch failed; retrying after delay.");
            }

            try
            {
                await Task.Delay(processedAnything ? BusyDelay : IdleDelay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Outbox worker stopped.");
    }

    private async Task<bool> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var batch = await context.OutboxMessages
            .Where(m => m.ProcessedAtUtc == null && m.Attempts < MaxAttempts)
            .OrderBy(m => m.CreatedAtUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (batch.Count == 0)
        {
            return false;
        }

        var processors = scope.ServiceProvider
            .GetServices<IOutboxProcessor>()
            .ToDictionary(p => p.Type, StringComparer.OrdinalIgnoreCase);

        foreach (var message in batch)
        {
            message.Attempts++;
            try
            {
                if (!processors.TryGetValue(message.Type, out var processor))
                {
                    throw new InvalidOperationException($"No processor registered for '{message.Type}'.");
                }

                await processor.ProcessAsync(message, cancellationToken);

                message.ProcessedAtUtc = DateTimeOffset.UtcNow;
                message.LastError = null;
                _logger.LogInformation("Processed outbox message {Id} ({Type}).", message.Id, message.Type);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                message.Attempts--; // Shutdown, not a real failure — don't burn an attempt.
                throw;
            }
            catch (Exception ex)
            {
                message.LastError = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                _logger.LogWarning(ex,
                    "Outbox message {Id} ({Type}) failed attempt {Attempt}/{Max}.",
                    message.Id, message.Type, message.Attempts, MaxAttempts);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
