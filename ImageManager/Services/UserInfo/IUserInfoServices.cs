#region Usings
using System;
using System.Security.Claims;
using System.Threading.Tasks;
#endregion

namespace ImageManager.Services.UserInfo;

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
    Task<GetUserInfoResponse?> GetCurrentUserInfoAsync(ClaimsPrincipal principal);
}