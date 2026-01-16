#region Usings

using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Repositories.Abstract_Interfaces;
using ImageManager.Repositories.Repository_Interfaces;
using Microsoft.EntityFrameworkCore;

#endregion

namespace ImageManager.Repositories.Implementations;

/// <summary>
/// Repository for <see cref="UserOwnedImage"/> entities.
/// Provides helpers that return the set of images a user (or anonymous) can access,
/// optionally filtered by an active share token.
/// </summary>
public class UserOwnedImageRepository(ApplicationDbContext dbContext)
    : EfRepository<UserOwnedImage, Guid>(dbContext), IUserOwnedImageRepository
{
    #region Public Methods

    /// <summary>
    /// Returns the images that are visible to a specific <see cref="User"/> or anonymous.
    /// Internally forwards to the string‑based overload using the user’s Id.
    /// </summary>
    public IQueryable<UserOwnedImage> AccessibleImages(User? user, Guid? token)
    {
        var id = user?.Id;
        return AccessibleImages(id, token);
    }

    /// <summary>
    /// Returns the images that are visible to a user identified by <paramref name="id"/> (or anonymous when null),
    /// optionally including any images that have an active share token.
    /// The query is intentionally read‑only – no tracking is applied unless the caller adds it explicitly.
    /// </summary>
    public IQueryable<UserOwnedImage> AccessibleImages(string? id, Guid? token)
    {
        var baseQuery = DbContext.UserOwnedImages.Where(uoid =>
            // User owns the image (always accessible regardless of publicity)
            (id != null && uoid.UserId == id) ||

            // Public images: Open with General rating (visible to everyone, including anonymous)
            (uoid.Publicity == Publicity.Open &&
             uoid.Image.AgeRating == AgeRating.General) ||

            // Public images: Open with sensitive/explicit/questionable rating (requires authenticated user)
            (uoid.Publicity == Publicity.Open &&
             (uoid.Image.AgeRating == AgeRating.Sensitive ||
              uoid.Image.AgeRating == AgeRating.Explicit ||
              uoid.Image.AgeRating == AgeRating.Questionable) &&
             id != null) ||

            // Restricted images (requires authenticated user)
            (uoid.Publicity == Publicity.Restricted && id != null));
        // Note: Private images are explicitly excluded - they are only accessible through ownership condition above

        if (token != null)
        {
            var tokenQuery = DbContext.UserOwnedImages
                .Where(uoid => uoid.ShareTokens.Any(stk =>
                    stk.Id == token &&
                    stk.Expires > DateTime.UtcNow));
            baseQuery = baseQuery.Union(tokenQuery);
        }

        return baseQuery;
    }

    /// <summary>
    /// Checks if a user directly owns an image, regardless of publicity settings.
    /// This method checks the database directly without any filters.
    /// </summary>
    public async Task<bool> UserOwnsImageAsync(string userId, Guid imageId)
    {
        return await DbContext.UserOwnedImages
            .AnyAsync(uoi => uoi.UserId == userId && uoi.ImageId == imageId);
    }

    #endregion
}
