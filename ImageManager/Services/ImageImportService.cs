#region Usings

using CoenM.ImageHash.HashAlgorithms;
using ImageManager.Data.Models;
using ImageManager.Repositories;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.PixelFormats;
#endregion

namespace ImageManager.Services;

/// <summary>
/// Service that imports an image, tags it and records ownership/download status.
/// </summary>
public interface IImageImportService
{
    /// <summary>
    /// Imports the given image bytes, generates tags, calculates a hash,
    /// stores the file (if new), creates or re‑uses an <see cref="Image"/> entity,
    /// and ensures that the specified user owns the image.
    /// </summary>
    /// <param name="imageBytes">Raw PNG/JPEG/GIF data.</param>
    /// <param name="publicity">The default publicity level for the new image.</param>
    /// <param name="userId">Identifier of the user performing the import.</param>
    /// <returns>The GUID of the resulting <see cref="Image"/> or <c>null</c> if the import failed.</returns>
    Task<Guid?> ImportImage(byte[] imageBytes, Publicity publicity, string userId);
}

#region Implementation

/// <summary>
/// EF Core‑based implementation of <see cref="IImageImportService"/>.
/// </summary>
public class ImageImportService(
    ITaggerService taggerService,
    IFileService fileService,
    IImageRepository imageRepository,
    IUserOwnedImageRepository userOwnedImageRepository,
    IDownloadedImageRepository downloadedImageRepository, // currently unused but kept for future extensions
    ITagRepository tagRepository,
    ICharacterRepository characterRepository,
    ILogger<ImageImportService> logger) : IImageImportService
{
    private readonly AverageHash _hash = new AverageHash();

    public async Task<Guid?> ImportImage(byte[] imageBytes, Publicity publicity, string userId)
    {
        // --------------------------------------------------------------------
        // 0️⃣  Validate inputs
        // --------------------------------------------------------------------
        if (imageBytes == null || imageBytes.Length == 0)
            throw new ArgumentException("Image data cannot be empty.", nameof(imageBytes));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID must be supplied.", nameof(userId));

        // --------------------------------------------------------------------
        // 1️⃣  Get tags from the external tagging service
        // --------------------------------------------------------------------
        ImageResponse imageData;
        try
        {
            imageData = await taggerService.GetTags(imageBytes);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get tags for import");
            return null;          // cannot continue without tags
        }

        // --------------------------------------------------------------------
        // 2️⃣  Resolve Tag & Character entities from the repositories
        // --------------------------------------------------------------------
        var tagEntities = await tagRepository.GetByStringsAsync(imageData.GeneralTags);
        var characterEntities = await characterRepository.GetByNamesAsync(imageData.CharacterTags);

        // --------------------------------------------------------------------
        // 3️⃣  Compute an average‑hash for the image
        // --------------------------------------------------------------------
        using var ms = new MemoryStream(imageBytes);
        using var img = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(ms);
        ulong hash = _hash.Hash(img.Clone());

        // --------------------------------------------------------------------
        // 4️⃣  Look for an existing image by hash via the repository
        // --------------------------------------------------------------------
        var existingImage = await imageRepository.GetByHashAsync(hash);

        Guid imageGuid;

        if (existingImage != null)
        {
            // Existing image – use its Id
            imageGuid = existingImage.Id;

            // ---- Ensure the user is listed as an owner ---------------------------------
            bool hasOwnership =
                await userOwnedImageRepository.AccessibleImages(new User { Id = userId }, null)
                                              .AnyAsync(i => i.ImageId == imageGuid);

            if (!hasOwnership)
            {
                await userOwnedImageRepository.AddAsync(
                    new UserOwnedImage { ImageId = imageGuid, UserId = userId });
            }
        }
        else
        {
            // ------------------------------------------------------------------------
            // 5️⃣  New image – first persist the file to obtain a Guid
            // ------------------------------------------------------------------------
            try
            {
                imageGuid = await fileService.SaveFile(img);   // returns the new image Guid
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to store image file");
                return null;
            }

            // ------------------------------------------------------------------------
            // 6️⃣  Build a brand‑new Image entity and add it via the repository
            // ------------------------------------------------------------------------
            var newImage = new Image
            {
                Id = imageGuid,
                Hash = hash,
                Tags = tagEntities.ToList(),
                Characters = characterEntities.ToList(),
                AgeRating = (AgeRating)imageData.Rating,
                DownloadedImageId = null,
                HasThumbnail = true,
            };

            await imageRepository.AddAsync(newImage);

            // ------------------------------------------------------------------------
            // 7️⃣  Add ownership record for the user
            // ------------------------------------------------------------------------
            await userOwnedImageRepository.AddAsync(
                new UserOwnedImage { ImageId = imageGuid, UserId = userId });
        }

        return imageGuid;
    }
}

#endregion
