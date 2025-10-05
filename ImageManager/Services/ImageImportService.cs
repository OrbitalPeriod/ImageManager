using CoenM.ImageHash.HashAlgorithms;
using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageManager.Services;

public interface IImageImportService
{
    public Task<Guid?> ImportImage(byte[] imageBytes, Publicity publicity, IDbContextTransaction transaction, string userId);
}

public class ImageImportService(ITaggerService taggerService, IFileService fileService, ApplicationDbContext dbContext, ILogger<ImageImportService> logger, IDatabaseService databaseService) : IImageImportService
{
    private readonly AverageHash _hash = new AverageHash();

    public async Task<Guid?> ImportImage(byte[] imageBytes, Publicity publicity, IDbContextTransaction transaction, string userId)
    {
        ImageResponse imageData;
        try
        {
            imageData = await taggerService.GetTags(imageBytes);
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to get tags due to {e.Message}");
            return null;
        }

        var tagEntities = await databaseService.GetTags(imageData.GeneralTags);
        var characterEntities = await databaseService.GetCharacters(imageData.CharacterTags);

        Guid userOwnedImageGuid;
        Guid imageGuid;

        using var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(new MemoryStream(imageBytes));
        ulong hash = _hash.Hash(image.Clone());

        var existingImage = await dbContext.Images.Include(i => i.UserOwnedImages).FirstOrDefaultAsync(i => i.Hash == hash);
        if (existingImage != null && existingImage.UserOwnedImages.Any(uoi => uoi.UserId == userId))
        {
            logger.LogInformation("Image already exists and owned in database, skipping");
            return null;
        }
        else if (existingImage == null)
        {
            userOwnedImageGuid = await fileService.SaveFile(image);
            existingImage = new Image()
            {
                AgeRating = (AgeRating)imageData.Rating,
                Characters = characterEntities,
                Hash = hash,
                Id = userOwnedImageGuid,
                Tags = tagEntities
            };
            await dbContext.Images.AddAsync(existingImage);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            userOwnedImageGuid = Guid.NewGuid();
        }

        var userOwnedImage = new UserOwnedImage()
        {
            Image = existingImage,
            Id = userOwnedImageGuid,
            Publicity = publicity,
            UserId = userId,
        };

        await dbContext.UserOwnedImages.AddAsync(userOwnedImage);
        await dbContext.SaveChangesAsync();

        return existingImage.Id;
    }
}