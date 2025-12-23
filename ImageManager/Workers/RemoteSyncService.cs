using System.Threading.Channels;
using ImageManager.Data.Models;
using ImageManager.Repositories;
using ImageManager.Services;

namespace ImageManager.Workers;

public record PlatformSyncRequest(Guid PlatformTokenId);

/// <summary>
/// Service that reads from the PlatformSyncRequests Queue, and downlaods images.
/// </summary>
/// <param name="scopeFactory">Required to instantiate the required services.</param>
/// <param name="channel">Channel to read the requests from</param>
/// <param name="logger"></param>
public class RemoteSyncService(
    IServiceScopeFactory scopeFactory,
    Channel<PlatformSyncRequest> channel,
    ILogger<RemoteSyncService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Remote sync worker started.");

        await foreach (var request in channel.Reader.ReadAllAsync(stoppingToken))
        {
            // Create a new scope for every message
            using var scope = scopeFactory.CreateScope();

            // Resolve scoped services *inside* the scope
            var repo = scope.ServiceProvider.GetRequiredService<IPlatformTokenRepository>();
            var importer = scope.ServiceProvider.GetRequiredService<IPixivImageImportManager>();

            try
            {
                logger.LogInformation(
                    "Processing sync for token {Token}",
                    request.PlatformTokenId);

                var platformToken = await repo.GetByIdAsync(request.PlatformTokenId);
                if (platformToken == null)
                {
                    logger.LogWarning("Token not found: {Token}", request.PlatformTokenId);
                    continue;
                }

                switch (platformToken.Platform)
                {
                    case Platform.Pixiv:
                        await importer.ImportAsync(platformToken);
                        break;
                        // … other platforms …
                }

                logger.LogInformation(
                    "Sync completed for token {Token} at {Time}",
                    request.PlatformTokenId,
                    DateTime.UtcNow);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                logger.LogError(ex, "Error while syncing token {Token}", request.PlatformTokenId);
            }
        }

        logger.LogInformation("Remote sync worker stopped.");
    }
}

