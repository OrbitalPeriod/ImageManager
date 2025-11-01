#region Usings

using ImageManager.Data.Models;
using ImageManager.Repositories;
using PixivCS.Models.Illust;

#endregion

namespace ImageManager.Services;

/// <summary>
/// Manages importing of Pixiv bookmarks for users.
/// </summary>
public interface IPixivImageImportManager : IImageImportManager
{

}

#region Implementation

/// <summary>
/// EF Core‑based implementation of <see cref="IPixivImageImportManager"/>.
/// It coordinates Pixiv API calls, image downloading, and storage via the existing import service.
/// </summary>
public class PixivImportManager(
    IPixivService pixivService,
    IImageImportService imageImportService,
    ILogger<PixivImportManager> logger,
    IUserRepository userRepository,
    IPlatformTokenRepository platformTokenRepository,
    IDownloadedImageRepository downloadedImageRepository,
    IUserOwnedImageRepository userOwnedImageRepository,
    ITransactionService transactionService) : IPixivImageImportManager
{
    /// <inheritdoc />
public async Task ImportAsync(PlatformToken token)
{
    if (token.Platform != Platform.Pixiv)
        throw new ArgumentException("Token must be Pixiv", nameof(token));

    try
    {
        logger.LogInformation("Starting Pixiv import for user {UserName} ({UserId})", 
            token.User.UserName, token.UserId);

        var bookmarks = await pixivService.GetLikedBookmarks(token.PlatformUserId, token.Token, token.CheckPrivate);

        if (!bookmarks.Any())
        {
            logger.LogInformation("No new bookmarks found for user {UserName} ({UserId})",
                token.User.UserName, token.UserId);
            return;
        }

        var illustIds = bookmarks.Select(x => x.Id).ToArray();

        // Check which illustrations are already downloaded
        var existingDownloads = await downloadedImageRepository.ListAsync(
            di => illustIds.Contains(di.PlatformImageId));

        var downloadedIds = existingDownloads.Select(di => di.PlatformImageId).ToHashSet();

        var toDownload = bookmarks.Where(ill => !downloadedIds.Contains(ill.Id)).ToList();

        logger.LogInformation("Found {Count} new illustrations for user {UserName} ({UserId})",
            toDownload.Count, token.User.UserName, token.UserId);

        int successCount = 0;
        int failCount = 0;

        foreach (var illustration in toDownload)
        {
            try
            {
                await DownloadImage(token.User, illustration);
                successCount++;
            }
            catch (Exception ex)
            {
                failCount++;
                logger.LogError(ex,
                    "Failed to download or import illustration {IllustrationId} ({Title}) for user {UserName} ({UserId})",
                    illustration.Id, illustration.Title, token.User.UserName, token.UserId);
            }
        }

        logger.LogInformation(
            "Completed downloading Pixiv illustrations for user {UserName} ({UserId}): {SuccessCount} succeeded, {FailCount} failed",
            token.User.UserName, token.UserId, successCount, failCount);

        // Handle existing illustrations not yet linked to this user
        var existingIds = illustIds.Except(downloadedIds).ToArray();

        List<UserOwnedImage> newUserLinks = new();

        try
        {
            var notAdded = await downloadedImageRepository.ListAsync(
                di => existingIds.Contains(di.PlatformImageId) &&
                      di.Image.UserOwnedImages.All(uoi => uoi.UserId != token.UserId));

            foreach (var existingImage in notAdded)
            {
                newUserLinks.Add(new UserOwnedImage
                {
                    UserId = token.UserId,
                    ImageId = existingImage.ImageId,
                    Publicity = token.User.DefaultPublicity
                });
            }

            if (newUserLinks.Any())
                foreach (var userLink in newUserLinks)
                {
                    await userOwnedImageRepository.AddAsync(userLink);
                }
            
            logger.LogInformation("Added {Count} existing illustrations for user {UserName} ({UserId})",
                newUserLinks.Count, token.User.UserName, token.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to link existing illustrations for user {UserName} ({UserId})",
                token.User.UserName, token.UserId);
        }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex,
            "Unexpected error during Pixiv import for user {UserName} ({UserId})",
            token.User.UserName, token.UserId);
        throw; 
    }
}


    /// <summary>
    /// Downloads a single illustration and imports it into the system.
    /// </summary>
    private async Task DownloadImage(User user, IllustInfo illustration)
    {
        await using var transaction = await transactionService.BeginTransactionAsync();

        try
        {
            byte[] imageBytes;
            try
            {
                imageBytes = await pixivService.DownloadImage(illustration);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to download image for illustration {IllustrationId} ({Title})",
                    illustration.Id, illustration.Title);
                await transaction.RollbackAsync();
                return;
            }

            var imageId = await imageImportService.ImportImage(imageBytes, user.DefaultPublicity, user.Id);
            if (imageId == null)
            {
                logger.LogError("Failed to import image for illustration {IllustrationId} ({Title})",
                    illustration.Id, illustration.Title);
                await transaction.RollbackAsync();
                return;
            }

            await downloadedImageRepository.AddAsync(new DownloadedImage
            {
                Platform = Platform.Pixiv,
                PlatformImageId = illustration.Id,
                ImageId = imageId.Value
            });

            await transaction.CommitAsync();
            logger.LogInformation("Successfully downloaded and imported illustration {IllustrationId}", illustration.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while processing illustration {IllustrationId}", illustration.Id);
            await transaction.RollbackAsync();
        }
    }
}
#endregion
