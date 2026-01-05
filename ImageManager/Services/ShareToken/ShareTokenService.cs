using ImageManager.Data;
using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Repositories;
using ImageManager.Repositories.Repository_Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Services.ShareToken;

/// <summary>
/// EF Core implementation of <see cref="IShareTokenService"/> that creates share tokens
/// for images owned by the calling user.
/// </summary>
public class ShareTokenService(
    IUserOwnedImageRepository userOwnedImageRepository,
    IShareTokenRepository shareTokenRepository,
    ITransactionService transactionService) : IShareTokenService
{
    /// <inheritdoc />
    public async Task<Option<Guid>> AddPlatformTokenAsync(Guid imageId, DateTime? expiration, User user)
    {
        // Defensive checks – make the contract explicit.
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (imageId == Guid.Empty) throw new ArgumentException("Image id cannot be empty", nameof(imageId));

        // Find *the* image that belongs to this user.
        var userOwnedImage = await userOwnedImageRepository.AccessibleImages(user, null)
            .FirstOrDefaultAsync(uoi => uoi.ImageId == imageId);

        if (userOwnedImage == null) return Option<Guid>.None();   // The user does not own the requested image.

        var shareToken = new Data.Models.ShareToken()
        {
            Created = DateTime.UtcNow,
            Expires = expiration ?? DateTime.UtcNow.AddDays(3),  // Default to 3days if none supplied.
            UserOwnedImageId = userOwnedImage.Id,
            UserId = user.Id
        };

        // Persist the token via the repository layer.
        await shareTokenRepository.AddAsync(shareToken);

        await transactionService.SaveChangesAsync();

        return Option<Guid>.Some(shareToken.Id);
    }
}
