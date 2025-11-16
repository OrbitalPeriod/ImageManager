#region Usings

using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Repositories;
using Microsoft.EntityFrameworkCore;
#endregion

namespace ImageManager.Services.ShareToken;

#region Implementation

/// <summary>
/// EF Core implementation of <see cref="IShareTokenService"/> that creates share tokens
/// for images owned by the calling user.
/// </summary>
public class ShareTokenService(
    IUserOwnedImageRepository userOwnedImageRepository,
    IShareTokenRepository shareTokenRepository) : IShareTokenService
{
    /// <inheritdoc />
    public async Task<Guid?> AddPlatformTokenAsync(Guid imageId, DateTime? expiration, User user)
    {
        // Defensive checks – make the contract explicit.
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (imageId == Guid.Empty) throw new ArgumentException("Image id cannot be empty", nameof(imageId));

        // Find *the* image that belongs to this user.
        var userOwnedImage = await userOwnedImageRepository.AccessibleImages(user, null)
            .FirstOrDefaultAsync(uoi => uoi.ImageId == imageId);

        if (userOwnedImage == null) return null;   // The user does not own the requested image.

        var shareToken = new Data.Models.ShareToken()
        {
            Created = DateTime.UtcNow,
            Expires = expiration ?? DateTime.UtcNow.AddDays(3),  // Default to 3 days if none supplied.
            UserOwnedImageId = userOwnedImage.Id,
            UserId = user.Id
        };

        // Persist the token via the repository layer.
        await shareTokenRepository.AddAsync(shareToken);

        return shareToken.Id;
    }
}

#endregion