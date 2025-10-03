namespace ImageManager.Services;

public class PixivSyncService(IServiceScopeFactory scopeFactory, ILogger<PixivSyncService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var importManager = scope.ServiceProvider.GetRequiredService<IPixivImageImportManager>();

                await importManager.ImportAllUserBookmarks();
                logger.LogInformation($"Pixiv Sync completed at: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error during Pixiv Sync: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}