#region Usings
using ImageManager.Data.Models;          
using ImageManager.Services.ShareToken;  
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
#endregion

namespace ImageManager.Controllers;

/// <summary>
/// Exposes API endpoints that allow an authenticated user to generate a
/// platform‑token for a specific image.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ShareTokenController(
    UserManager<User> userManager,
    IShareTokenService tokenService) : ControllerBase
{
    #region Public Actions
    /// <summary>
    /// Creates a new platform‑token for the image identified by <paramref name="imageId"/>.
    /// </summary>
    /// <param name="imageId">The GUID of the target image.</param>
    /// <param name="request">
    /// Request body containing the desired token expiration date/time.
    /// </param>
    /// <returns>
    /// 200 OK with the new token’s GUID on success;  
    /// 401 Unauthorized if the caller isn’t authenticated;  
    /// 404 Not Found if the image does not exist. 
    /// </returns>
    [Authorize]
    [HttpPost("add/{imageId:guid}")]
    public async Task<IActionResult> AddPlatformToken(
        Guid imageId,
        [FromBody] AddPlatformTokenRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        
        Guid? tokenId;
        try
        {
            tokenId = await tokenService.AddPlatformTokenAsync(
                imageId,
                request.Expiration,
                user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal error: {ex.Message}");
        }
        
        if (!tokenId.HasValue) return NotFound();

        return Ok(tokenId.Value);
    }
    #endregion
}
