using ImageManager.Controllers;
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
    public async Task<ImageAccessResult> GetImageAccessAsync(
        Guid imageId,
        User? user,
        Guid? token)
    {
        var image = await imageRepository.GetByIdAsync(imageId);
        if (image == null) return new ImageAccessResult { Found = false };

        var allowed = await imageRepository.CanAccessImageAsync(user, image, token);

        return new ImageAccessResult
        {
            Found = true,
            Allowed = allowed,
            Image = image
        };
    }

    /// <inheritdoc />
    public async Task<ImageDataAccessResult> GetImageDataAccessAsync(
        Guid imageId,
        User? user,
        Guid? token)
    {
        var image = await imageRepository.GetByIdFullAsync(imageId);
        if (image == null) return new ImageDataAccessResult { Found = false };

        var allowed = await imageRepository.CanAccessImageAsync(user, image, token);
        if (!allowed) return new ImageDataAccessResult { Found = true, Allowed = false };

        var data = new ImageController.ImageDataResponse(
            image.Id,
            image.Tags.Select(t => t.Name).ToArray(),
            image.Characters.Select(c => c.Name).ToArray(),
            image.AgeRating,
            image.UserOwnedImages.Select(uoi => uoi.UserId).ToArray());

        return new ImageDataAccessResult
        {
            Found = true,
            Allowed = true,
            Data = data
        };
    }
}
