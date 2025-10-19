#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageManager.Controllers;
using ImageManager.Data.Models;
using ImageManager.Repositories;
#endregion

namespace ImageManager.Services.Query;

#region DTOs / Result types

/// <summary>
/// Result of an image‑access check.
/// </summary>
public record ImageAccessResult
{
    /// <summary>Whether the image exists.</summary>
    public bool Found { get; init; }

    /// <summary>Whether the caller is allowed to view the image.</summary>
    public bool Allowed { get; init; }

    /// <summary>The image entity, if found.  May be <c>null</c> when <see cref="Found"/> is <c>false</c>.</summary>
    public Image? Image { get; init; }
}

/// <summary>
/// Result of a request for an image’s data (tags, characters, etc.).
/// </summary>
public record ImageDataAccessResult
{
    /// <summary>Whether the image exists.</summary>
    public bool Found { get; init; }

    /// <summary>Whether the caller is allowed to access the image’s data.</summary>
    public bool Allowed { get; init; }

    /// <summary>The serialized image data.  Null when <see cref="Allowed"/> is <c>false</c>.</summary>
    public ImageController.ImageDataResponse? Data { get; init; }
}

#endregion

#region Interface

/// <summary>
/// Contract for retrieving detailed information about an image,
/// including access checks and the full data payload.
/// </summary>
public interface IImageDetailService
{
    /// <summary>
    /// Determines whether a user (or anonymous) can view the specified image.
    /// Returns both existence and permission flags, plus the image entity if found.
    /// </summary>
    Task<ImageAccessResult> GetImageAccessAsync(Guid imageId, User? user, Guid? token);

    /// <summary>
    /// Retrieves the data payload for an image (tags, characters, age rating, owners)
    /// if the caller has permission.  The result contains flags indicating
    /// whether the image was found and whether access is allowed.
    /// </summary>
    Task<ImageDataAccessResult> GetImageDataAccessAsync(Guid imageId, User? user, Guid? token);
}

#endregion

#region Implementation

/// <summary>
/// EF Core implementation of <see cref="IImageDetailService"/>.
/// Uses an <see cref="IImageRepository"/> to fetch images and perform access checks.
/// </summary>
public class ImageDetailService(IImageRepository userOwnedImageRepository) : IImageDetailService
{
    /// <inheritdoc />
    public async Task<ImageAccessResult> GetImageAccessAsync(
        Guid imageId,
        User? user,
        Guid? token)
    {
        var image = await userOwnedImageRepository.GetByIdAsync(imageId);
        if (image == null) return new ImageAccessResult { Found = false };

        var allowed = await userOwnedImageRepository.CanAccessImageAsync(user, image, token);

        return new ImageAccessResult
        {
            Found   = true,
            Allowed = allowed,
            Image   = image
        };
    }

    /// <inheritdoc />
    public async Task<ImageDataAccessResult> GetImageDataAccessAsync(
        Guid imageId,
        User? user,
        Guid? token)
    {
        var image = await userOwnedImageRepository.GetByIdFullAsync(imageId);
        if (image == null) return new ImageDataAccessResult { Found = false };

        var allowed = await userOwnedImageRepository.CanAccessImageAsync(user, image, token);
        if (!allowed) return new ImageDataAccessResult { Found = true, Allowed = false };

        var data = new ImageController.ImageDataResponse(
            image.Id,
            image.Tags.Select(t => t.Name).ToArray(),
            image.Characters.Select(c => c.Name).ToArray(),
            image.AgeRating,
            image.UserOwnedImages.Select(uoi => uoi.UserId).ToArray());

        return new ImageDataAccessResult
        {
            Found   = true,
            Allowed = true,
            Data    = data
        };
    }
}

#endregion
