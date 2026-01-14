#region Usings

using ImageManager.Extensions;
using ImageManager.Services.UserInfo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
#endregion

namespace ImageManager.Controllers;

/// <summary>
/// Provides endpoints for retrieving information about the currently authenticated user.
/// </summary>
[ApiController]
[Route("api/users")]
public sealed class UserController(
    IUserInfoService userInfoService) : ControllerBase
{
    #region Public Actions

    /// <summary>
    /// Returns basic profile data for the authenticated user.
    /// </summary>
    /// <returns>A <see cref="GetUserInfoResponse"/> containing user details.</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(GetUserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetUserInfoResponse>> GetUserInfo()
    {
        var response = await userInfoService.GetCurrentUserInfoAsync(User);
        return this.ToActionResult(response);
    }

    #endregion
}
