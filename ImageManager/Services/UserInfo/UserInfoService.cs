using System.Security.Claims;
using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Repositories;
using ImageManager.Repositories.Repository_Interfaces;
using Microsoft.AspNetCore.Identity;


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
    public async Task<Option<GetUserInfoResponse>> GetCurrentUserInfoAsync(ClaimsPrincipal principal)
    {
        if (principal == null) throw new ArgumentNullException(nameof(principal));

        var userId = userManager.GetUserId(principal);
        if (string.IsNullOrEmpty(userId)) return Option<GetUserInfoResponse>.None();

        // Load the full user entity to expose all required properties.
        var userOption = await userRepository.GetByIdAsync(userId);
        if (userOption.IsNone) return Option<GetUserInfoResponse>.None();

        var user = userOption.Unwrap();
        var roles = await userManager.GetRolesAsync(user);
        return Option<GetUserInfoResponse>.Some(new GetUserInfoResponse(
            user.Id,
            user.UserName,
            user.Email,
            roles.ToList(),
            user.DefaultPublicity));
    }
}
