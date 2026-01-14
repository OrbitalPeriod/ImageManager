using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Extensions;
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
    UserManager<User> userManager,
    IPlatformTokenService tokenService) : ControllerBase
{
    #region Add

    /// <summary>
    /// Adds a new platform token for the current user.
    /// </summary>
    [Authorize]
    [HttpPut("add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    [ProducesResponseType(typeof(IReadOnlyCollection<PlatformTokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPlatformTokens()
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var tokens = await tokenService.GetTokensAsync(user);
        return Ok(tokens);
    }

    /// <summary>
    /// Retrieves sync logs for a specific platform token.
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}/logs")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PlatformTokenSyncLog>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<PlatformTokenSyncLog>>> GetPlatformTokenLogs(Guid id)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var logs = await tokenService.GetLogAsync(id, user);
        if (logs == null) return NotFound();
        return Ok(logs);
    }

    #endregion

    #region Delete

    /// <summary>
    /// Deletes a platform token by its GUID.
    /// </summary>
    [Authorize]
    [HttpDelete("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeletePlatformToken(Guid id)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var result = await tokenService.DeleteTokenAsync(id, user);
        return this.ToActionResult(result);
    }

    #endregion

    #region Run

    /// <summary>
    /// Run the sync service on a token
    /// </summary>
    /// <param name="id">Identifier of token</param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("{id:Guid}/run")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RunPlatformToken(Guid id)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();
        var result = await tokenService.QueueAsync(id, user);
        return this.ToActionResult(result);
    }
    #endregion
}
