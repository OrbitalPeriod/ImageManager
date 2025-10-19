#region Usings
using System.Threading.Tasks;
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
    public async Task<ActionResult<GetUserInfoResponse>> GetUserInfo()
    {
        var response = await userInfoService.GetCurrentUserInfoAsync(User);
        
        if (response is null) return Unauthorized();

        return Ok(response);
    }

    #endregion
}