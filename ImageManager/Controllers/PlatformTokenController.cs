using ImageManager.Data.Models;
using ImageManager.Services;
using ImageManager.Services.PlatformTokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ImageManager.Controllers;

/// <summary>
/// Handles CRUD operations for platform tokens belonging to the authenticated user.
/// </summary>
[ApiController]
[Route("api/platform-tokens")]
public class PlatformTokenController(
    UserManager<User>          userManager,
    IPlatformTokenService      tokenService) : ControllerBase
{
    #region Add

    /// <summary>
    /// Adds a new platform token for the current user.
    /// </summary>
    [Authorize]
    [HttpPut("add")]
    public async Task<IActionResult> AddToken([FromBody] AddTokenRequest request)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        await tokenService.AddTokenAsync(request, user);
        return Ok();
    }

    #endregion

    #region Get

    /// <summary>
    /// Retrieves all platform tokens owned by the current user.
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetPlatformTokens()
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var tokens = await tokenService.GetTokensAsync(user);
        return Ok(tokens);
    }

    #endregion

    #region Delete

    /// <summary>
    /// Deletes a platform token by its GUID.
    /// </summary>
    [Authorize]
    [HttpDelete("delete/{id:Guid}")]
    public async Task<IActionResult> DeletePlatformToken(Guid id)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var result = await tokenService.DeleteTokenAsync(id, user);

        return result switch
        {
            DeleteResult.NotFound => NotFound(),
            DeleteResult.Forbidden => Forbid(),
            DeleteResult.Deleted  => Ok(),
            _ => BadRequest()
        };
    }

    #endregion
}
