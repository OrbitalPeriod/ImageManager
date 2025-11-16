#region Usings
using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
#endregion

namespace ImageManager.Controllers;

/// <summary>
/// API endpoints for retrieving character statistics.
/// </summary>
[ApiController]
[Route("api/characters")]
public class CharacterController(
    UserManager<User> userManager,
    ILogger<CharacterController> logger,
    ICharacterQueryService queryService) : ControllerBase
{
    #region DTOs / Records
    /// <summary>
    /// Response model for a single character statistic.
    /// </summary>
    public record GetCharacterResponse(Guid TagId, string CharacterName, int Count);
    #endregion

    #region Actions
    /// <summary>
    /// Retrieves a paginated list of characters optionally filtered by an optional token.
    /// </summary>
    /// <param name="token">Optional filter token.</param>
    /// <param name="page">Page number (1‑based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<GetCharacterResponse>>> GetCharacters(
        [FromQuery] Guid? token,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 200)] int pageSize = 20)
    {
        // Optional clamping – kept for safety if validation is bypassed
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 200;

        var user = await userManager.GetUserAsync(HttpContext.User);

        var result = await queryService.GetCharactersAsync(user, token, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Searches for characters matching the supplied query string.
    /// </summary>
    /// <param name="q">Search term.</param>
    /// <param name="token">Optional filter token.</param>
    /// <param name="page">Page number (1‑based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    [HttpGet("search")]
    public async Task<ActionResult<PaginatedResponse<GetCharacterResponse>>> SearchCharacters(
        [FromQuery] string q = "",
        [FromQuery] Guid? token = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 200)] int pageSize = 20)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var result = await queryService.SearchAsync(user, token, q, page, pageSize);
        return Ok(result);
    }
    #endregion
}
