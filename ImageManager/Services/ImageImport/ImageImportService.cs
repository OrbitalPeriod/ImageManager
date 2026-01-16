using CoenM.ImageHash.HashAlgorithms;
using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Repositories.Repository_Interfaces;
using ImageManager.Services.File;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.PixelFormats;


namespace ImageManager.Services.ImageImport;



/// <summary>
/// EF Core‑based implementation of <see cref="IImageImportService"/>.
/// </summary>
public class ImageImportService(
    ITaggerService taggerService,
    IFileService fileService,
    IImageRepository imageRepository,
    IUserOwnedImageRepository userOwnedImageRepository,
    ITagRepository tagRepository,
    ICharacterRepository characterRepository,
    ILogger<ImageImportService> logger) : IImageImportService
{
    private readonly AverageHash _hash = new AverageHash();

    public async Task<Result<ImportImageSuccess, ImportImageError>> ImportImage(byte[] imageBytes, Publicity publicity, string userId)
    {
        // --------------------------------------------------------------------
        // 0️⃣  Validate inputs
        // --------------------------------------------------------------------
        if (imageBytes == null || imageBytes.Length == 0)
            return Result<ImportImageSuccess, ImportImageError>.Err(ImportImageError.EmptyImage);

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID must be supplied.", nameof(userId));

        // --------------------------------------------------------------------
        // 3️⃣  Compute an average‑hash for the image
        // --------------------------------------------------------------------
        using var ms = new MemoryStream(imageBytes);
        using var img = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(ms);

        if (img == null) return Result<ImportImageSuccess, ImportImageError>.Err(ImportImageError.ImageParseFailed);

        using var hashImg = img.Clone();
        ulong hash = _hash.Hash(hashImg);

        // --------------------------------------------------------------------
        // 4️⃣  Look for an existing image by hash via the repository
        // --------------------------------------------------------------------
        var existingImageOption = await imageRepository.GetByHashAsync(hash);

        Guid imageGuid;

        if (existingImageOption.IsSome)
        {
            // Existing image – use its Id
            var existingImage = existingImageOption.Unwrap();
            imageGuid = existingImage.Id;

            // ---- Ensure the user is listed as an owner ---------------------------------
            bool hasOwnership = await userOwnedImageRepository.UserOwnsImageAsync(userId, imageGuid);

            if (!hasOwnership)
            {
                await userOwnedImageRepository.AddAsync(
                    new UserOwnedImage { ImageId = imageGuid, UserId = userId, Publicity = publicity });
                return Result<ImportImageSuccess, ImportImageError>.Ok(new ImportImageSuccess()
                {
                    Id = imageGuid,
                    NewFile = false,
                    AlreadyOwned = false,
                });
            }
            return Result<ImportImageSuccess, ImportImageError>.Ok(new ImportImageSuccess()
            {
                Id = imageGuid,
                NewFile = false,
                AlreadyOwned = true,
            });
        }

        ImageResponse imageData;
        try
        {
            imageData = await taggerService.GetTags(imageBytes);
        }
        catch (Exception)
        {
            return Result<ImportImageSuccess, ImportImageError>.Err(ImportImageError.FailedToGetTags);
        }


        var tagEntities = await tagRepository.GetByStringsAsync(imageData.GeneralTags);
        var characterEntities = await characterRepository.GetByNamesAsync(imageData.CharacterTags);

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
            return Result<ImportImageSuccess, ImportImageError>.Err(ImportImageError.ImageStoreError);
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
            HasThumbnail = true,
            HasCompressedVersion = true,
        };

        await imageRepository.AddAsync(newImage);

        // ------------------------------------------------------------------------
        // 7️⃣  Add ownership record for the user
        // ------------------------------------------------------------------------
        await userOwnedImageRepository.AddAsync(
            new UserOwnedImage { ImageId = imageGuid, UserId = userId, Publicity = publicity });

        return Result<ImportImageSuccess, ImportImageError>.Ok(new ImportImageSuccess()
        {
            Id = imageGuid,
            NewFile = true,
            AlreadyOwned = false
        });

    }
}
