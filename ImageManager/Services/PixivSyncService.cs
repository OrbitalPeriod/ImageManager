#region Usings
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#endregion

namespace ImageManager.Services;

/// <summary>
/// Background service that periodically imports all Pixiv user bookmarks.
/// </summary>
public sealed class PixivSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PixivSyncService> _logger;

    /// <summary>
    /// How often the sync should run.
    /// </summary>
    private static readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Creates a new instance of <see cref="PixivSyncService"/>.
    /// </summary>
    /// <param name="scopeFactory">Factory for creating scoped service providers.</param>
    /// <param name="logger">Logger used by the service.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="scopeFactory"/> or <paramref name="logger"/> is null.
    /// </exception>
    public PixivSyncService(IServiceScopeFactory scopeFactory, ILogger<PixivSyncService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger       = logger     ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Main execution loop. Runs until the application is stopped.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token that signals when the host is shutting down.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PixivSyncService started – running every {Interval}.", SyncInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1️⃣ Create a scoped service provider for the import operation.
                await using var scope = _scopeFactory.CreateAsyncScope();
                var importManager = scope.ServiceProvider.GetRequiredService<IPixivImageImportManager>();

                // 2️⃣ Trigger the import of all user bookmarks.
                await importManager.ImportAllUserBookmarks();

                _logger.LogInformation("Pixiv sync completed at {Timestamp}.", DateTime.UtcNow);
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

        _logger.LogInformation("PixivSyncService stopped.");
    }
}
