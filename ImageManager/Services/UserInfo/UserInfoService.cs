#region Usings
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ImageManager.Data.Models;
using ImageManager.Repositories;
using Microsoft.AspNetCore.Identity;
#endregion

namespace ImageManager.Services.UserInfo;

/// <summary>
/// Implementation of <see cref="IUserInfoService"/> that retrieves the current user’s
/// profile data from ASP.NET Core Identity and the underlying user repository.
/// </summary>
public class UserInfoService(
    UserManager<User> userManager,
    IUserRepository userRepository) : IUserInfoService
{
    /// <inheritdoc />
    public async Task<GetUserInfoResponse?> GetCurrentUserInfoAsync(ClaimsPrincipal principal)
    {
        if (principal == null) throw new ArgumentNullException(nameof(principal));
        
        var userId = userManager.GetUserId(principal);
        if (string.IsNullOrEmpty(userId)) return null;

        // Load the full user entity to expose all required properties.
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        return new GetUserInfoResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.DefaultPublicity);
    }
}