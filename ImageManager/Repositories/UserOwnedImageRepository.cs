#region Usings

using ImageManager.Data;
using ImageManager.Data.Models;
#endregion

namespace ImageManager.Repositories;

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
        var baseQuery = dbContext.UserOwnedImages.Where(uoid =>
            (id != null && uoid.UserId == id) ||

            (uoid.Publicity == Publicity.Open &&
             uoid.Image.AgeRating == AgeRating.General) ||

            (uoid.Publicity == Publicity.Open &&
             (uoid.Image.AgeRating == AgeRating.Sensitive ||
              uoid.Image.AgeRating == AgeRating.Explicit ||
              uoid.Image.AgeRating == AgeRating.Questionable) &&
             id != null) ||

            (uoid.Publicity == Publicity.Restricted && id != null));

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

    #endregion
}
