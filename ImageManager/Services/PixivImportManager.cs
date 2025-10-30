#region Usings

using ImageManager.Data.Models;
using ImageManager.Repositories;
using PixivCS.Models.Illust;

#endregion

namespace ImageManager.Services;

/// <summary>
/// Manages importing of Pixiv bookmarks for users.
/// </summary>
public interface IPixivImageImportManager
{
    /// <summary>
    /// Imports all bookmark data for every user stored in the system.
    /// </summary>
    Task ImportAllUserBookmarks();

    /// <summary>
    /// Imports bookmark data for a single user.
    /// </summary>
    /// <param name="user">The user whose bookmarks should be imported.</param>
    Task ImportBookmarks(User user);
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
    IUserOwnedImageRepository userOwnedImageRepository) : IPixivImageImportManager
{
    /// <inheritdoc />
    public async Task ImportAllUserBookmarks()
    {
        var users = await userRepository.ListAsync();
        foreach (var user in users)
        {
            await ImportBookmarks(user);
        }
    }

    /// <inheritdoc />
    public async Task ImportBookmarks(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var tokens = await platformTokenRepository.ListAsync(pft => pft.UserId == user.Id);
        foreach (var token in tokens)
        {
            await ImportPixivBookmarks(user, token.PlatformUserId, token.Token, token.CheckPrivate);
        }
    }

    /// <summary>
    /// Imports the Pixiv bookmarks for a specific platform account.
    /// </summary>
    private async Task ImportPixivBookmarks(User user, string pixivUserId, string? pixivRefreshToken, bool checkPrivate)
    {
        // Retrieve all liked illustrations from Pixiv
        var illustrations = await pixivService.GetLikedBookmarks(pixivUserId, pixivRefreshToken, checkPrivate);
        if (!illustrations.Any())
            return;

        var illustIds = illustrations.Select(x => x.Id).ToArray();

        // Find which of those illustrations have already been downloaded.
        var existingDownloads = await downloadedImageRepository.ListAsync(di => illustIds.Contains(di.PlatformImageId));
        var downloadedIds = existingDownloads.Select(di => di.PlatformImageId);

        // Determine which illustrations need to be fetched and imported
        var toDownload = illustrations.Where(ill => !downloadedIds.Contains(ill.Id)).ToList();

        logger.LogInformation("Found {Count} new illustrations for user {UserName} ({UserId})",
            toDownload.Count, user.UserName, user.Id);

        foreach (var illustration in toDownload)
        {
            await DownloadImage(user, illustration);
        }

        logger.LogInformation("Finished importing {Count} illustrations for user {UserName} ({UserId})",
            toDownload.Count, user.UserName, user.Id);

        // Find illustrations that were already downloaded but are not yet owned by this user
        var existingIds = illustIds.Except(downloadedIds).ToArray();

        var notAdded = await downloadedImageRepository.ListAsync(
            di => existingIds.Contains(di.PlatformImageId) &&
                  di.Image.UserOwnedImages.Any(uoi => uoi.UserId != user.Id));

        foreach (var existingImage in notAdded)
        {
            await userOwnedImageRepository.AddAsync(new UserOwnedImage
            {
                UserId = user.Id,
                ImageId = existingImage.ImageId,
                Publicity = user.DefaultPublicity
            });
        }

        logger.LogInformation("Added {Count} existing illustrations for user {UserName} ({UserId})",
            notAdded.Count, user.UserName, user.Id);
    }

    /// <summary>
    /// Downloads a single illustration and imports it into the system.
    /// </summary>
    private async Task DownloadImage(User user, IllustInfo illustration)
    {
        byte[] imageBytes;
        try
        {
            imageBytes = await pixivService.DownloadImage(illustration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to download image for illustration {IllustrationId} ({Title})",
                illustration.Id, illustration.Title);
            return;
        }

        var imageId = await imageImportService.ImportImage(imageBytes, user.DefaultPublicity, user.Id);

        if (imageId == null)
        {
            logger.LogError("Failed to import image for illustration {IllustrationId} ({Title})",
                illustration.Id, illustration.Title);
            return;
        }

        await downloadedImageRepository.AddAsync(new DownloadedImage
        {
            Platform = Platform.Pixiv,
            PlatformImageId = illustration.Id,
            ImageId = (Guid)imageId
        });
    }
}

#endregion
