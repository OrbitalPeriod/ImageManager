using ImageManager.Controllers;
using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Repositories;
using ImageManager.Repositories.Repository_Interfaces;

namespace ImageManager.Services.Query;

/// <summary>
/// EF Core implementation of <see cref="IImageDetailService"/>.
/// Uses an <see cref="IImageRepository"/> to fetch images and perform access checks.
/// </summary>
public class ImageDetailService(IImageRepository imageRepository) : IImageDetailService
{
    /// <inheritdoc />
    public async Task<Result<Image, ImageError>> GetImageAccessAsync(
        Guid imageId,
        User? user,
        Guid? token)
    {
        var imageOption = await imageRepository.GetByIdAsync(imageId);
        if (imageOption.IsNone)
            return Result<Image, ImageError>.Err(ImageError.NotFound);

        var image = imageOption.Unwrap();
        var allowed = await imageRepository.CanAccessImageAsync(user, image, token);
        
        if (!allowed)
            return Result<Image, ImageError>.Err(ImageError.Forbidden);

        return Result<Image, ImageError>.Ok(image);
    }

    /// <inheritdoc />
    public async Task<Result<ImageController.ImageDataResponse, ImageError>> GetImageDataAccessAsync(
        Guid imageId,
        User? user,
        Guid? token)
    {
        var imageOption = await imageRepository.GetByIdFullAsync(imageId);
        if (imageOption.IsNone)
            return Result<ImageController.ImageDataResponse, ImageError>.Err(ImageError.NotFound);

        var image = imageOption.Unwrap();
        var allowed = await imageRepository.CanAccessImageAsync(user, image, token);
        
        if (!allowed)
            return Result<ImageController.ImageDataResponse, ImageError>.Err(ImageError.Forbidden);

        var data = new ImageController.ImageDataResponse(
            image.Id,
            image.Tags.Select(t => t.Name).ToArray(),
            image.Characters.Select(c => c.Name).ToArray(),
            image.AgeRating,
            image.UserOwnedImages.Select(uoi => uoi.UserId).ToArray(),
            image.StoredAt);

        return Result<ImageController.ImageDataResponse, ImageError>.Ok(data);
    }
}
