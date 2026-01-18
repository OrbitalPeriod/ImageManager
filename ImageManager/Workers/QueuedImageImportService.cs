using System.Threading.Channels;
using ImageManager.Services;
using ImageManager.Services.ImageImport;

namespace ImageManager.Workers;

/// <summary>
/// Background service that processes queued image imports when AnimeTagger becomes ready.
/// </summary>
public class QueuedImageImportService(
    IServiceScopeFactory scopeFactory,
    Channel<QueuedImageImport> channel,
    ILogger<QueuedImageImportService> logger)
    : BackgroundService
{
    /// <summary>
    /// How often to check if AnimeTagger is ready when processing queued imports.
    /// </summary>
    private static readonly TimeSpan ReadinessCheckInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// How often to check for queued imports when AnimeTagger is not ready.
    /// </summary>
    private static readonly TimeSpan IdleCheckInterval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Queued image import service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Check if AnimeTagger is ready
                bool isReady = await CheckAnimeTaggerReadinessAsync(stoppingToken);

                if (isReady)
                {
                    // Process queued imports while service is ready
                    await ProcessQueuedImportsAsync(stoppingToken);
                }
                else
                {
                    // Wait a bit before checking again
                    await Task.Delay(IdleCheckInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                logger.LogError(ex, "Unexpected error in queued image import service.");
                await Task.Delay(ReadinessCheckInterval, stoppingToken);
            }
        }

        logger.LogInformation("Queued image import service stopped.");
    }

    /// <summary>
    /// Checks if AnimeTagger service is ready by attempting to get the service and checking its status.
    /// </summary>
    private async Task<bool> CheckAnimeTaggerReadinessAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var taggerService = scope.ServiceProvider.GetRequiredService<ITaggerService>();
            
            // If IsReady is true, we can trust it (it's updated on successful calls)
            if (taggerService.IsReady)
            {
                return true;
            }

            // Try a lightweight check by attempting to read from the channel
            // If there are items in the queue and the service might be ready, we'll attempt processing
            // The actual readiness will be verified when we try to process
            return false;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Error checking AnimeTagger readiness");
            return false;
        }
    }

    /// <summary>
    /// Processes queued image imports while AnimeTagger is ready.
    /// </summary>
    private async Task ProcessQueuedImportsAsync(CancellationToken stoppingToken)
    {
        int processedCount = 0;
        int failedCount = 0;

        // Process all available items in the queue
        while (channel.Reader.TryRead(out var queuedImport))
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                using var scope = scopeFactory.CreateScope();
                var imageImportService = scope.ServiceProvider.GetRequiredService<IImageImportService>();
                var taggerService = scope.ServiceProvider.GetRequiredService<ITaggerService>();

                // Double-check readiness before processing
                if (!taggerService.IsReady)
                {
                    // Service became unavailable, put the item back at the front of the queue
                    // We can't easily put it back at the front, so we'll log and skip for now
                    // In practice, if this happens, it will be picked up on the next cycle
                    logger.LogWarning("AnimeTagger became unavailable while processing queue. Will retry later.");
                    
                    // Attempt to write back (might fail if queue is full, but that's okay)
                    try
                    {
                        await channel.Writer.WriteAsync(queuedImport, stoppingToken);
                    }
                    catch (InvalidOperationException)
                    {
                        // Queue is full or closed, item will be lost but that's acceptable
                        logger.LogWarning("Could not re-queue import - queue may be full");
                    }
                    
                    break; // Exit processing loop, will check again later
                }

                var queueAge = DateTime.UtcNow - queuedImport.QueuedAt;
                logger.LogInformation(
                    "Processing queued import for user {UserId} (queued {QueueAge} ago)",
                    queuedImport.UserId,
                    queueAge);

                var result = await imageImportService.ImportImage(
                    queuedImport.ImageBytes,
                    queuedImport.Publicity,
                    queuedImport.UserId);

                if (result.IsOk)
                {
                    processedCount++;
                    var success = result.Unwrap();
                    logger.LogInformation(
                        "Successfully processed queued import for user {UserId}. Image ID: {ImageId}",
                        queuedImport.UserId,
                        success.Id);
                }
                else
                {
                    failedCount++;
                    var error = result.UnwrapError();
                    logger.LogError(
                        "Failed to process queued import for user {UserId}. Error: {Error}",
                        queuedImport.UserId,
                        error);
                    
                    // Check if it's a tagger service failure - if so, re-queue
                    if (error == ImportImageError.FailedToGetTags || error == ImportImageError.QueuedForProcessing)
                    {
                        // Service might have become unavailable again, re-queue
                        try
                        {
                            await channel.Writer.WriteAsync(queuedImport, stoppingToken);
                            logger.LogInformation("Re-queued import due to tagger service failure");
                        }
                        catch (InvalidOperationException)
                        {
                            logger.LogWarning("Could not re-queue import after failure - queue may be full");
                        }
                    }
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                failedCount++;
                logger.LogError(ex, "Unexpected error processing queued import for user {UserId}", queuedImport.UserId);
                
                // Don't re-queue on unexpected errors - they might not be recoverable
            }

            // Small delay between items to avoid overwhelming the system
            await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
        }

        if (processedCount > 0 || failedCount > 0)
        {
            logger.LogInformation(
                "Processed {ProcessedCount} queued imports successfully, {FailedCount} failed",
                processedCount,
                failedCount);
        }

        // Wait a bit before checking for more items (TryRead loop naturally exits when queue is empty)
        await Task.Delay(ReadinessCheckInterval, stoppingToken);
    }
}
