using System.Threading.Channels;
using ImageManager.Data.Models;
using ImageManager.Repositories;
using ImageManager.Repositories.Repository_Interfaces;
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
            var logRepo = scope.ServiceProvider.GetRequiredService<IPlatformSyncLogRepository>();
            var transactionRepo = scope.ServiceProvider.GetRequiredService<ITransactionService>();

            PlatformToken? platformToken = null;
            try
            {
                logger.LogInformation(
                    "Processing sync for token {Token}",
                    request.PlatformTokenId);

                platformToken = await repo.GetByIdAsync(request.PlatformTokenId);
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

                await logRepo.AddAsync(new PlatformSyncLog()
                {
                    Message = $"Sync completed for token {request.PlatformTokenId} at {DateTime.UtcNow}",
                    PlatformTokenId = platformToken.Id,
                    Success = true,
                });
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                logger.LogError(ex, "Error while syncing token {Token}", request.PlatformTokenId);

                if (platformToken != null) await logRepo.AddAsync(new PlatformSyncLog()
                {
                    Message =
                            $"Error while syncing token {request.PlatformTokenId} at {DateTime.UtcNow} due to error: {ex.Message}",
                    PlatformTokenId = platformToken.Id,
                    Success = false,
                });
                await transactionRepo.SaveChangesAsync(stoppingToken);
            }
        }
        logger.LogInformation("Remote sync worker stopped.");
    }
}

