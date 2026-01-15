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
    IDownloadedImageRepository downloadedImageRepository,
    IUserOwnedImageRepository userOwnedImageRepository,
    IImageRepository imageRepository,
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
            var userOption = await userRepository.GetByIdAsync(token.UserId);
            if (userOption.IsNone) throw new ArgumentException("User not found", nameof(token));
            var user = userOption.Unwrap();

            logger.LogInformation("Starting Pixiv import for user {UserName} ({UserId})",
                user.UserName, token.UserId);

            var bookmarksResult = await pixivService.GetLikedBookmarks(token.PlatformUserId, token.Token, token.CheckPrivate);
            
            if (!bookmarksResult.IsOk)
            {
                var error = bookmarksResult.UnwrapError();
                var errorMessage = error switch
                {
                    PixivError.Timeout => "Request timed out",
                    PixivError.NetworkError => "Network connectivity error",
                    PixivError.AuthenticationFailed => "Authentication failed",
                    PixivError.RateLimited => "Rate limit exceeded",
                    PixivError.ApiError => "API error",
                    PixivError.InvalidInput => "Invalid input parameters",
                    _ => "Unknown error"
                };
                
                logger.LogError(
                    "Failed to fetch bookmarks for user {UserName} ({UserId}): {Error}",
                    user.UserName, token.UserId, errorMessage);
                return;
            }

            var bookmarks = bookmarksResult.Unwrap();

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
                    logger.LogWarning(
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
                var existingDownloadsForLinking = await downloadedImageRepository.ListAsync(
                    di => existingIds.Contains(di.PlatformImageId));

                foreach (var downloadedImage in existingDownloadsForLinking)
                {
                    // Get all images linked to this DownloadedImage
                    var images = await imageRepository.ListAsync(
                        img => img.DownloadedImageId == downloadedImage.Id);

                    // For each image, check if user already owns it
                    foreach (var image in images)
                    {
                        var alreadyOwned = image.UserOwnedImages.Any(uoi => uoi.UserId == token.UserId);
                        if (!alreadyOwned)
                        {
                            newUserLinks.Add(new UserOwnedImage
                            {
                                UserId = token.UserId,
                                ImageId = image.Id,
                                Publicity = user.DefaultPublicity
                            });
                        }
                    }
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
    /// Downloads all images from an illustration and imports them into the system.
    /// All images are linked to the same DownloadedImage entity to track the source post.
    /// </summary>
    private async Task<Option<Unit>> DownloadImage(User user, IllustInfo illustration)
    {
        Exception? caughtException = null;
        Option<Unit> result;
        
        try
        {
            result = await transactionService.UseTransactionAsync(async () =>
            {
            try
            {
            var downloadResult = await pixivService.DownloadAllImages(illustration);
            
            if (!downloadResult.IsOk)
            {
                var error = downloadResult.UnwrapError();
                var errorMessage = error switch
                {
                    PixivError.Timeout => "Download timed out",
                    PixivError.NetworkError => "Network connectivity error during download",
                    PixivError.AuthenticationFailed => "Authentication failed during download",
                    PixivError.RateLimited => "Rate limit exceeded during download",
                    PixivError.DownloadFailed => "Download failed",
                    PixivError.InvalidInput => "Invalid illustration data",
                    _ => "Unknown download error"
                };
                
                logger.LogWarning(
                    "Failed to download illustration {IllustrationId} ({Title}): {Error}",
                    illustration.Id, illustration.Title, errorMessage);
                return Option<Unit>.None();
            }

            var allImageBytes = downloadResult.Unwrap();
            
            if (allImageBytes.Length == 0)
            {
                logger.LogWarning(
                    "No images downloaded for illustration {IllustrationId} ({Title})",
                    illustration.Id, illustration.Title);
                return Option<Unit>.None();
            }

            // Get or create DownloadedImage entity for this post
            var existingDownloadedImages = await downloadedImageRepository.ListAsync(
                di => di.Platform == Platform.Pixiv && di.PlatformImageId == illustration.Id);
            
            DownloadedImage downloadedImage;
            if (existingDownloadedImages.Any())
            {
                downloadedImage = existingDownloadedImages.First();
            }
            else
            {
                downloadedImage = new DownloadedImage
                {
                    Platform = Platform.Pixiv,
                    PlatformImageId = illustration.Id,
                };
                await downloadedImageRepository.AddAsync(downloadedImage);
                // Save the DownloadedImage to get its database-generated ID
                // This is within the transaction, so it won't commit separately
                await transactionService.SaveChangesAsync();
            }

            int successCount = 0;
            int failCount = 0;

            // Import each image and link it to the DownloadedImage
            foreach (var imageBytes in allImageBytes)
            {
                var imageImportResult = await imageImportService.ImportImage(imageBytes, user.DefaultPublicity, user.Id);

                if (!imageImportResult.IsOk)
                {
                    failCount++;
                    var error = imageImportResult.UnwrapError();
                    switch (error)
                    {
                        case ImportImageError.AlreadyOwned:
                            logger.LogWarning("Image from illustration {IllustrationId} already owned by {UserName} ({UserId})", 
                                illustration.Id, user.UserName, user.Id);
                            break;
                        case ImportImageError.EmptyImage:
                            logger.LogWarning("Empty image from illustration {IllustrationId}", illustration.Id);
                            break;
                        case ImportImageError.FailedToGetTags:
                            logger.LogWarning("Failed to get tags for image from illustration {IllustrationId}", illustration.Id);
                            break;
                        case ImportImageError.ImageParseFailed:
                            logger.LogWarning("Failed to parse image from illustration {IllustrationId}", illustration.Id);
                            break;
                        case ImportImageError.ImageStoreError:
                            logger.LogWarning("Failed to store image from illustration {IllustrationId}", illustration.Id);
                            break;
                    }
                    continue;
                }

                var imageSuccessResult = imageImportResult.Unwrap();

                // Link the Image to the DownloadedImage by setting DownloadedImageId
                // The Image was just added to the context by ImageImportService but not yet saved
                // GetByIdAsync uses FindAsync which checks the change tracker first before querying the database
                var imageOption = await imageRepository.GetByIdAsync(imageSuccessResult.Id);
                if (imageOption.IsSome)
                {
                    var image = imageOption.Unwrap();
                    // Set DownloadedImageId - EF Core will track this change since the entity is tracked
                    image.DownloadedImageId = downloadedImage.Id;
                }
                else
                {
                    logger.LogWarning("Image {ImageId} not found after import for illustration {IllustrationId}", 
                        imageSuccessResult.Id, illustration.Id);
                }

                successCount++;
            }

            if (successCount == 0)
            {
                logger.LogWarning(
                    "Failed to import any images for illustration {IllustrationId} ({Title})",
                    illustration.Id, illustration.Title);
                return Option<Unit>.None();
            }

            logger.LogInformation(
                "Successfully imported {SuccessCount}/{TotalCount} images for illustration {IllustrationId} ({Title})",
                successCount, allImageBytes.Length, illustration.Id, illustration.Title);

            return Option<Unit>.Some(Unit.New());
            }
            catch (Exception ex)
            {
                caughtException = ex;
                logger.LogError(ex, "Exception during image import for illustration {IllustrationId} ({Title})", illustration.Id, illustration.Title);
                throw; // Re-throw to let transaction service handle rollback
            }
            });
        }
        catch (Exception ex)
        {
            caughtException = ex;
            result = Option<Unit>.None();
        }

        if (result.IsNone)
        {
            if (caughtException != null)
            {
                logger.LogError(caughtException, "Unexpected error while processing illustration {IllustrationId} ({Title}): {ExceptionType} - {ExceptionMessage}", 
                    illustration.Id, illustration.Title, caughtException.GetType().Name, caughtException.Message);
            }
            else
            {
                logger.LogError("Unexpected error while processing illustration {IllustrationId} ({Title}) - transaction returned None without exception", 
                    illustration.Id, illustration.Title);
            }
        }
        else
        {
            logger.LogInformation("Successfully downloaded and imported illustration {IllustrationId}", illustration.Id);
        }

        return result;
    }
}
#endregion
