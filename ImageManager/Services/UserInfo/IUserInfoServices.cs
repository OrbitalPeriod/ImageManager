using System.Security.Claims;
using ImageManager.Data.Helpers;
using ImageManager.Data.Models;

namespace ImageManager.Services.UserInfo;

#region DTOs

/// <summary>
/// DTO returned by the user‑info service.
/// It contains the authenticated user's basic profile data, roles, and the default publicity level
/// used when that user uploads a new image.
/// </summary>
public record GetUserInfoResponse(
    string Id,

    string? UserName,

    string? Email,

    ICollection<string> Roles,

    Publicity DefaultPublicity);

#endregion

#region interface

/// <summary>
/// Contract for retrieving information about the currently authenticated user.
/// </summary>
public interface IUserInfoService
{
    /// <summary>
    /// Retrieves a DTO with the current user's profile data.
    /// Returns <c>null</c> if the caller is not authenticated or the user cannot be resolved.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the current request.</param>
    /// <returns>A <see cref="GetUserInfoResponse"/> containing the user's data, or <c>null</c> if unavailable.</returns>
    Task<Option<GetUserInfoResponse>> GetCurrentUserInfoAsync(ClaimsPrincipal principal);
}

#endregion
