#region Usings

using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Repositories;
using ImageManager.Repositories.Implementations;
using ImageManager.Repositories.Repository_Interfaces;
using ImageManager.Services.ImageImport;
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

        //TODO: Somehow get rid of this exception nonsense
        try
        {
            var user = await userRepository.GetByIdAsync(token.UserId);

            if (user == null) throw new ArgumentException("User not found", nameof(token));

            logger.LogInformation("Starting Pixiv import for user {UserName} ({UserId})",
                user.UserName, token.UserId);

            var bookmarks = await pixivService.GetLikedBookmarks(token.PlatformUserId, token.Token, token.CheckPrivate);

            if (!bookmarks.Any())
            {
                logger.LogInformation("No new bookmarks found for user {UserName} ({UserId})",
                    user.UserName, token.UserId);
                return;
            }

            var illustIds = bookmarks.Select(x => x.Id).ToArray();

            // Check which illustrations are already downloaded
            var existingDownloads = await downloadedImageRepository.ListAsync(
                di => illustIds.Contains(di.PlatformImageId));

            var downloadedIds = existingDownloads.Select(di => di.PlatformImageId).ToHashSet();

            var toDownload = bookmarks.Where(ill => !downloadedIds.Contains(ill.Id)).ToList();

            logger.LogInformation("Found {Count} new illustrations for user {UserName} ({UserId})",
                toDownload.Count, user.UserName, token.UserId);

            int successCount = 0;
            int failCount = 0;

            foreach (var illustration in toDownload)
            {
                var downloadImage = await DownloadImage(user, illustration);
                if (downloadImage.IsSome)
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                    logger.LogError(
                        "Failed to download or import illustration {IllustrationId} ({Title}) for user {UserName} ({UserId})",
                        illustration.Id, illustration.Title, user.UserName, token.UserId);
                }
            }

            logger.LogInformation(
                "Completed downloading Pixiv illustrations for user {UserName} ({UserId}): {SuccessCount} succeeded, {FailCount} failed",
                user.UserName, token.UserId, successCount, failCount);

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
                        Publicity = user.DefaultPublicity
                    });
                }

                if (newUserLinks.Any())
                    foreach (var userLink in newUserLinks)
                    {
                        await userOwnedImageRepository.AddAsync(userLink);
                    }

                await transactionService.SaveChangesAsync();

                logger.LogInformation("Added {Count} existing illustrations for user {UserName} ({UserId})",
                    newUserLinks.Count, user.UserName, token.UserId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to link existing illustrations for user {UserName} ({UserId})",
                    user.UserName, token.UserId);
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "Unexpected error during Pixiv import for user ({UserId})",
                token.UserId);
            throw;
        }
    }


    /// <summary>
    /// Downloads a single illustration and imports it into the system.
    /// </summary>
    private async Task<Option<Unit>> DownloadImage(User user, IllustInfo illustration)
    {
        var result = await transactionService.UseTransactionAsync(async () =>
        {
            var imageBytes = await pixivService.DownloadImage(illustration);

            var imageImportResult = await imageImportService.ImportImage(imageBytes, user.DefaultPublicity, user.Id);

            if (!imageImportResult.IsOk)
            {
                var error = imageImportResult.UnwrapError();
                switch (error)
                {
                    case ImportImageError.AlreadyOwned:
                        logger.LogError("Importing image: {illustrationId} already owned by {UserName} ({UserId})}", illustration.Id, user.UserName, user.Id);
                        break;
                    case ImportImageError.EmptyImage:
                        logger.LogError("Importing image: {illustrationId} empty image", illustration.Id);
                        break;
                    case ImportImageError.FailedToGetTags:
                        logger.LogError("Failed to get tags for image: {illustrationId}", illustration.Id);
                        break;
                    case ImportImageError.ImageParseFailed:
                        logger.LogError("Failed to parse image: {illustrationId}", illustration.Id);
                        break;
                    case ImportImageError.ImageStoreError:
                        logger.LogError("Failed to store image: {illustrationId}", illustration.Id);
                        break;
                }
                return Option<Guid>.None();
            }

            var imageSuccessResult = imageImportResult.Unwrap();

            // Check if image is already in downloaded
            if (imageSuccessResult.NewFile)
            {
                await downloadedImageRepository.AddAsync(new DownloadedImage
                {
                    Platform = Platform.Pixiv,
                    PlatformImageId = illustration.Id,
                    ImageId = imageSuccessResult.Id,
                });
            }

            return Option<Guid>.Some(imageSuccessResult.Id);
        });

        if (result.IsNone)
        {
            logger.LogError("Unexpected error while processing illustration {IllustrationId}", illustration.Id);
        }
        else
        {
            logger.LogInformation("Successfully downloaded and imported illustration {IllustrationId}", illustration.Id);
        }

        return result.Map(_ => Unit.New());
    }
}
#endregion
