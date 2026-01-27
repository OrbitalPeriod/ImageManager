#region Usings

using System.ComponentModel.DataAnnotations;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Extensions;
using ImageManager.Services.Folders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace ImageManager.Controllers;

/// <summary>
/// Handles all folder-related API endpoints (creating, listing, deleting folders, and managing images in folders).
/// </summary>
[ApiController]
[Route("api/folders")]
[Authorize]
public class FolderController(
    UserManager<User> userManager,
    IFolderService folderService) : ControllerBase
{
    #region DTOs

    /// <summary>Request payload for creating a folder.</summary>
    public record CreateFolderRequest([Required] string Name);

    #endregion

    #region Actions

    /// <summary>
    /// Creates a new folder for the current user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FolderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FolderDto>> CreateFolder([FromBody] CreateFolderRequest request)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var result = await folderService.CreateFolderAsync(user.Id, request.Name);
        if (!result.IsOk)
        {
            var error = result.UnwrapError();
            if (error == FolderError.AlreadyExists)
            {
                return BadRequest(new { message = "A folder with this name already exists." });
            }
        }

        return this.ToActionResult(result.Map(f => new FolderDto(f.Id, f.Name)));
    }

    /// <summary>
    /// Deletes a folder (except "Liked" folder).
    /// </summary>
    [HttpDelete("{folderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFolder(Guid folderId)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var result = await folderService.DeleteFolderAsync(user.Id, folderId);
        if (!result.IsOk)
        {
            return result.UnwrapError() switch
            {
                FolderError.CannotDeleteLiked => BadRequest(new { message = "Cannot delete the 'Liked' folder." }),
                _ => this.ToActionResultForError(result)
            };
        }

        return Ok();
    }

    /// <summary>
    /// Lists all folders for the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ICollection<FolderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ICollection<FolderDto>>> GetFolders()
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var result = await folderService.GetUserFoldersAsync(user.Id);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Adds an image to a folder.
    /// </summary>
    [HttpPost("{folderId:guid}/images/{imageId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddImageToFolder(Guid folderId, Guid imageId)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var result = await folderService.AddImageToFolderAsync(user.Id, folderId, imageId);
        if (!result.IsOk)
        {
            return result.UnwrapError() switch
            {
                FolderError.AlreadyExists => BadRequest(new { message = "Image is already in this folder." }),
                _ => this.ToActionResultForError(result)
            };
        }

        return Ok();
    }

    /// <summary>
    /// Removes an image from a folder.
    /// </summary>
    [HttpDelete("{folderId:guid}/images/{imageId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveImageFromFolder(Guid folderId, Guid imageId)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var result = await folderService.RemoveImageFromFolderAsync(user.Id, folderId, imageId);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Lists images in a folder with pagination.
    /// </summary>
    [HttpGet("{folderId:guid}/images")]
    [ProducesResponseType(typeof(PaginatedResponse<ImageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResponse<ImageDto>>> GetFolderImages(
        Guid folderId,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 200)] int pageSize = 20)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var result = await folderService.GetFolderImagesAsync(user.Id, folderId, page, pageSize);
        return this.ToActionResult(result);
    }

    #endregion
}
