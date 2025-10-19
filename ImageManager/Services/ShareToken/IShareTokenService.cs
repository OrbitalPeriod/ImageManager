#region Usings
using System;
using System.Threading.Tasks;
using ImageManager.Data.Models;
#endregion

namespace ImageManager.Services.ShareToken;

#region Records

/// <summary>
/// Payload used when creating a platform share token.
/// </summary>
public record AddPlatformTokenRequest(DateTime? Expiration);

#endregion

#region Interface

/// <summary>
/// Contract for generating and managing share tokens that grant temporary
/// access to images owned by the calling user.
/// </summary>
public interface IShareTokenService
{
    /// <summary>
    /// Creates a share token for an image that the current user owns.
    /// Returns the new token Id or <c>null</c> if the image is not owned by the user.
    /// </summary>
    /// <param name="imageId">The ID of the image to create a token for.</param>
    /// <param name="expiration">
    /// Optional expiration date/time for the share token.  If omitted, the service
    /// applies its default expiry logic.
    /// </param>
    /// <param name="user">The user who owns the image.</param>
    Task<Guid?> AddPlatformTokenAsync(Guid imageId, DateTime? expiration, User user);
}

#endregion