using SixLabors.ImageSharp;

namespace ImageManager.Services;

public class PixivSyncService(IServiceScopeFactory scopeFactory, ILoggerService logger) : BackgroundService
{
    private readonly ILoggerService _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var importManager = scope.ServiceProvider.GetRequiredService<IPixivImageImportManager>();

                await importManager.ImportAllUserBookmarks();
                _logger.LogInfo($"Pixiv Sync completed at: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during Pixiv Sync: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}