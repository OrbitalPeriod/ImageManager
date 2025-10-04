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

        Guid guid;
        ulong hash;
        try
        {
            using var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(new MemoryStream(imageBytes));
            hash = _hash.Hash(image.Clone());
            guid = await fileService.SaveFile(image);
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to save image tags: {e.Message}");
            return null;
        }

        if (await dbContext.Images.AnyAsync(i => i.Hash == hash && i.UserId == userId))
        {
            logger.LogInformation("Image already exists in database, skipping");
            return null;
        }
        
        var imageEntity = new Image()
        {
            Id = guid,
            Characters = characterEntities,
            Tags = tagEntities,
            Hash = hash,
            Publicity = publicity,
            AgeRating = (AgeRating)imageData.Rating,
            ShareTokens = [],
            UserId = userId
        };

        await dbContext.Images.AddAsync(imageEntity);
        await dbContext.SaveChangesAsync();

        return guid;
    }
}