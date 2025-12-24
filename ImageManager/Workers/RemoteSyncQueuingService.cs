using System.Threading.Channels;
using ImageManager.Data.Models;
using ImageManager.Repositories;

namespace ImageManager.Workers;

/// <summary>
/// Periodically scans all platform tokens and enqueues a sync request for each one.
/// </summary>
public class RemoteSyncQueuingService(
    IServiceScopeFactory scopeFactory,
    Channel<PlatformSyncRequest> channel,
    ILogger<RemoteSyncQueuingService> logger)
    : BackgroundService
{
    /// <summary>
    /// How often the worker should refresh the queue.
    /// </summary>
    private static readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Remote sync queuing service started – running every {Interval}.",
            SyncInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await QueueAllTokensAsync(stoppingToken);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                logger.LogError(ex, "Unexpected error while queuing sync requests.");
            }

            try
            {
                await Task.Delay(SyncInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("RemoteSyncQueuingService stopped.");
    }

    /// <summary>
    /// Creates a new scope, pulls all tokens and pushes them onto the channel.
    /// </summary>
    private async Task QueueAllTokensAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPlatformTokenRepository>();

        IEnumerable<PlatformToken> tokens;
        try
        {
            tokens = await repo.GetAllAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve platform tokens.");
            return;
        }

        foreach (var token in tokens)
        {
            try
            {
                await channel.Writer.WriteAsync(
                    new PlatformSyncRequest(token.Id), ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to enqueue sync request for token {TokenId}.", token.Id);
            }
        }
    }
}
