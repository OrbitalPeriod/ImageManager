using ImageManager.Data.Models;
using ImageManager.Repositories;

namespace ImageManager.Services;

public class RemoteSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RemoteSyncService> _logger;

    /// <summary>
    /// Creates a new instance of <see cref="RemoteSyncService"/>.
    /// </summary>
    /// <param name="scopeFactory">Factory for creating scoped service providers.</param>
    /// <param name="logger">Logger used by the service.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="scopeFactory"/> or <paramref name="logger"/> is null.
    /// </exception>
    public RemoteSyncService(IServiceScopeFactory scopeFactory, ILogger<RemoteSyncService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// How often the sync should run.
    /// </summary>
    private static readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Main execution loop. Runs until the application is stopped.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token that signals when the host is shutting down.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Remote sync started – running every {Interval}.", SyncInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1️⃣ Create a scoped service provider for the import operation.
                await using var scope = _scopeFactory.CreateAsyncScope();
                var platformTokenRepository = scope.ServiceProvider.GetRequiredService<IPlatformTokenRepository>();
                var pixivImageImportManager = scope.ServiceProvider.GetRequiredService<IPixivImageImportManager>();

                //Run import for all tokens
                var tokens = await platformTokenRepository.GetAllAsync();
                foreach (var platformToken in tokens)
                {
                    switch (platformToken.Platform)
                    {
                        case Platform.Pixiv:
                            await pixivImageImportManager.ImportAsync(platformToken);
                            break;
                    }
                }

                _logger.LogInformation("Image sync completed at {Timestamp}.", DateTime.UtcNow);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                // Log the full exception for diagnostics.
                _logger.LogError(ex, "Error during Pixiv sync");
            }

            // 3️⃣ Wait until the next run or cancellation is requested.
            try
            {
                await Task.Delay(SyncInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Cancellation requested – exit gracefully.
                break;
            }
        }

        _logger.LogInformation("RemoteSyncService stopped.");
    }

}

