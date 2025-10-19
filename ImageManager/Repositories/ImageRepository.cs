#region Usings
using System;
using System.Linq;                    
using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;
#endregion

namespace ImageManager.Repositories;

/// <summary>
/// Repository for <see cref="Image"/> entities.
/// Provides helper methods to check access, retrieve images by hash or ID,
/// and query the set of images a user can access.
/// </summary>
public class ImageRepository(ApplicationDbContext dbContext)
    : EfRepository<Image, Guid>(dbContext), IImageRepository
{
    #region Public Methods

    /// <summary>
    /// Determines whether the specified <paramref name="image"/> is accessible to the given
    /// <paramref name="user"/> (or by a valid share <paramref name="token"/>).
    /// </summary>
    /// <param name="user">The user attempting access, or <c>null</c> for anonymous.</param>
    /// <param name="image">The image to check.</param>
    /// <param name="token">Optional share token that may grant access.</param>
    /// <returns><c>true</c> if the image is accessible; otherwise <c>false</c>.</returns>
    public async Task<bool> CanAccessImageAsync(User? user, Image image, Guid? token)
    {
        return await AccessibleImages(user, token).AnyAsync(x => x.ImageId == image.Id);
    }

    /// <summary>
    /// Returns a queryable set of <see cref="UserOwnedImage"/> records that the specified
    /// <paramref name="user"/> (or by a valid share <paramref name="token"/>) can access.
    /// The returned sequence is suitable for further filtering or projection.
    /// </summary>
    /// <param name="user">The user attempting access, or <c>null</c> for anonymous.</param>
    /// <param name="token">Optional share token that may grant access.</param>
    /// <returns>An <see cref="IQueryable{UserOwnedImage}"/> representing the accessible images.</returns>
    public IQueryable<UserOwnedImage> AccessibleImages(User? user, Guid? token)
    {
        // Base conditions – covers ownership and public visibility.
        var baseQuery = dbContext.UserOwnedImages.Where(uoid =>
            (user != null && uoid.UserId == user.Id) ||                                      // owned images
            // Open + General rating – public access for everyone
            (uoid.Publicity == Publicity.Open &&
             uoid.Image.AgeRating == AgeRating.General) ||
            // Open + Sensitive/Explicit/Questionable – requires a logged‑in user
            (uoid.Publicity == Publicity.Open &&
             (uoid.Image.AgeRating == AgeRating.Sensitive ||
              uoid.Image.AgeRating == AgeRating.Explicit ||
              uoid.Image.AgeRating == AgeRating.Questionable) &&
             user != null) ||
            // Restricted – only the owner can see it unless a valid share token is provided
            (user != null && uoid.Publicity == Publicity.Restricted && uoid.UserId == user.Id));

        if (token != null)
        {
            var tokenQuery = dbContext.UserOwnedImages
                .Where(uoid => uoid.ShareTokens.Any(stk =>
                    stk.Id == token &&
                    stk.Expires > DateTime.UtcNow));

            baseQuery = baseQuery.Union(tokenQuery);
        }

        return baseQuery;
    }

    /// <summary>
    /// Retrieves an image by its unique hash value.
    /// </summary>
    /// <param name="hash">The hash to search for.</param>
    /// <returns>The matching <see cref="Image"/> or <c>null</c> if none found.</returns>
    public async Task<Image?> GetByHashAsync(ulong hash)
    {
        return await dbContext.Images.FirstOrDefaultAsync(i => i.Hash == hash);
    }

    /// <summary>
    /// Retrieves an image by its ID, including related tags and characters.
    /// </summary>
    /// <param name="id">The image’s GUID.</param>
    /// <returns>The matching <see cref="Image"/> or <c>null</c> if none found.</returns>
    public async Task<Image?> GetByIdFullAsync(Guid id)
    {
        return await dbContext.Images
            .Include(i => i.Tags)
            .Include(i => i.Characters)
            .Include(i => i.UserOwnedImages)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    /// <summary>
    /// Determines whether an image with the specified ID exists.
    /// </summary>
    /// <param name="id">The image’s GUID.</param>
    /// <returns><c>true</c> if a record exists; otherwise <c>false</c>.</returns>
    public async Task<bool> ImageExistsAsync(Guid id)
    {
        return await dbContext.Images.AnyAsync(i => i.Id == id);
    }

    #endregion
}
