using CoenM.ImageHash.HashAlgorithms;
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

    public async Task ImportPixivBookmarks(User user, string pixivUserId, string? pixivRefreshToken, bool checkPrivate)
    {
        var illustrations = await pixivService.GetLikedBookmarks(pixivUserId, pixivRefreshToken, checkPrivate);

        var illustIds = illustrations.Select(x => x.Id).ToArray();
        var downloadedIds = await dbContext.DownloadedImages
            .Where(d => d.User == user)
            .Select(d => d.Id)
            .ToListAsync();

        var toDownload = illustIds.Except(downloadedIds).ToList();

        logger.LogInformation("Downloading: " + toDownload.Count + " new images" + " For user: " + user.UserName);

        foreach (var illustration in toDownload.Select(x => illustrations.First(i => i.Id == x)))
        {
            await DownloadImage(user, illustration);
        }
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
        var imageGuid = await imageImportService.ImportImage(imageBytes, user.DefaultPublicity, transaction, user.Id);

        if (imageGuid == null)
        {
            logger.LogError($"Failed to import image for {illustration}");
            return;
        }

        dbContext.Add(new DownloadedImage()
        {
            User = user,
            ImageId = imageGuid,
            Platform = Platform.Pixiv,
        });

        await transaction.CommitAsync();
    }

}