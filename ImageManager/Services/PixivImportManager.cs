using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;
using PixivCS.Models.Illust;

namespace ImageManager.Services;

public interface IPixivImageImportManager
{
    public Task ImportAllUserBookmarks();
    public Task ImportBookmarks(User user);
}

public class PixivImportManager(IPixivService pixivService, ApplicationDbContext dbContext, IImageImportService imageImportService, ILogger<PixivImportManager> logger) : IPixivImageImportManager
{
    public async Task ImportAllUserBookmarks()
    {
        var users = await dbContext.Users.Include(u => u.PlatformTokens).Where(u => u.PlatformTokens.Count(pft => pft.Platform == Platform.Pixiv) > 0).ToListAsync();
        foreach (var user in users)
        {
            await ImportBookmarks(user);
        }
    }
    public async Task ImportBookmarks(User user)
    {
        var items = await dbContext.PlatformTokens.Where(pft => pft.User == user).Where(pt => pt.Platform == Platform.Pixiv)
            .ToListAsync();
        foreach (var item in items)
        {
            await ImportPixivBookmarks(user, item.PlatformUserId, item.Token, item.CheckPrivate);
        }
    }

    private async Task ImportPixivBookmarks(User user, string pixivUserId, string? pixivRefreshToken, bool checkPrivate)
    {
        var illustrations = await pixivService.GetLikedBookmarks(pixivUserId, pixivRefreshToken, checkPrivate);
        var illustIds = illustrations.Select(x => x.Id).ToArray();

        var downloadedIds = await dbContext.DownloadedImages
            .Where(di => illustIds.Contains(di.PlatformImageId))
            .Select(di => di.PlatformImageId)
            .ToListAsync();
        
        var toDowload = illustrations.Where(ill => !downloadedIds.Contains(ill.Id)).ToList();
        logger.LogInformation($"Found {toDowload.Count} new illustrations for user {user.UserName} ({user.Id})");
        foreach (var illustration in toDowload)
        {
            await DownloadImage(user, illustration);
        }
        logger.LogInformation($"Finished importing {toDowload.Count} illustrations for user {user.UserName} ({user.Id})");

        var exisiting = illustIds.Except(downloadedIds).ToArray();

        var notAdded = await dbContext.DownloadedImages.Where(d => exisiting.Contains(d.Id))
            .Where(di => di.Image.UserOwnedImages.Any(uoi => uoi.UserId != user.Id)).ToListAsync();

        foreach (var existingImage in notAdded)
        {
            dbContext.Add(new UserOwnedImage()
            {
                UserId = user.Id,
                ImageId = existingImage.ImageId,
                Publicity = user.DefaultPublicity,
            });
        }
        await dbContext.SaveChangesAsync();
        logger.LogInformation($"Added {notAdded.Count} existing illustrations for user {user.UserName} ({user.Id})");
    }


    private async Task DownloadImage(User user, IllustInfo illustration)
    {
        byte[] imageBytes;
        try
        {
            imageBytes = await pixivService.DownloadImage(illustration);
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to get tags for {illustration}, due to {e.Message}");
            return;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var userOwnedId = await imageImportService.ImportImage(imageBytes, user.DefaultPublicity, transaction, user.Id);

        if (userOwnedId == null)
        {
            logger.LogError($"Failed to import image for {illustration}");
            return;
        }

        dbContext.Add(new DownloadedImage()
        {
            Platform = Platform.Pixiv,
            PlatformImageId = illustration.Id,
            ImageId = await dbContext.UserOwnedImages.Where(uoi => uoi.Id == userOwnedId).Select(uoi => uoi.ImageId).FirstAsync()
        });

        await transaction.CommitAsync();
    }

}