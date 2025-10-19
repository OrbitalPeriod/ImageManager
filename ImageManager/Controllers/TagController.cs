#region Usings
using System;
using System.Threading.Tasks;
using ImageManager.Data.Models;         
using ImageManager.Data.Responses;       
using ImageManager.Services.Tags;        
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
#endregion

namespace ImageManager.Controllers;

/// <summary>
/// Handles tag‑related API calls, including listing and searching.
/// </summary>
[ApiController]
[Route("api/tags")]
public sealed class TagController(
    UserManager<User> userManager,
    ITagService tagService) : ControllerBase
{
    #region Public Actions

    /// <summary>
    /// Returns a paginated list of tags that the current user is allowed to see.
    /// </summary>
    /// <param name="token">
    /// Optional share‑token that grants temporary access to private tags.
    /// </param>
    /// <param name="page">Page number (1‑based). Defaults to 1.</param>
    /// <param name="pageSize">Number of items per page. Max 200.</param>
    /// <returns>Paginated response containing <see cref="TagCountDto"/> items.</returns>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<TagCountDto>>> GetTags(
        [FromQuery] Guid? token,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Validate paging parameters
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 200;

        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        PaginatedResponse<TagCountDto> result;
        try
        {
            result = await tagService.GetTagsAsync(user, token, page, pageSize);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal error: {ex.Message}");
        }

        return Ok(result);
    }

    /// <summary>
    /// Searches for tags that match the supplied query string.
    /// </summary>
    /// <param name="q">Search term; an empty string returns all tags.</param>
    /// <param name="token">
    /// Optional share‑token that grants temporary access to private tags.
    /// </param>
    /// <param name="page">Page number (1‑based). Defaults to 1.</param>
    /// <param name="pageSize">Number of items per page. Max 200.</param>
    /// <returns>Paginated response containing <see cref="TagCountDto"/> items.</returns>
    [HttpGet("search")]
    public async Task<ActionResult<PaginatedResponse<TagCountDto>>> SearchTags(
        [FromQuery] string q = "",
        [FromQuery] Guid? token = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 200;

        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        PaginatedResponse<TagCountDto> result;
        try
        {
            result = await tagService.SearchTagsAsync(user, q, token, page, pageSize);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal error: {ex.Message}");
        }

        return Ok(result);
    }

    #endregion
}
