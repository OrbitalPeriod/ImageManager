#region Usings
using System.ComponentModel.DataAnnotations;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Services;
using ImageManager.Services.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
#endregion

namespace ImageManager.Controllers;

/// <summary>
/// Handles all image‑related API endpoints (listing, uploading, deleting, and searching).  
/// The controller is intentionally stateless; all dependencies are injected via the constructor.
/// </summary>
[ApiController]
[Route("api/images")]
public class ImageController(
    UserManager<User> userManager,
    IImageQueryService imageQueryService,
    IImageDetailService imageDetailService,
    IUploadImageService uploadImageService,
    IDeleteImageService deleteImageService,
    IFileService fileService,
    ILogger<ImageController> logger) : ControllerBase
{
    
    #region DTOs used by this controller
    /// <summary>Response returned for a paginated list of images.</summary>
    public record GetImagesResponse(Guid Id, AgeRating Rating);

    /// <summary>Request payload for uploading an image.</summary>
    public record UploadImageRequest(
        [Required] IFormFile File,
        Publicity? Publicity);

    /// <summary>Full image data exposed to the caller.</summary>
    public record ImageDataResponse(
        Guid Id,
        ICollection<string> Tags,
        ICollection<string> Characters,
        AgeRating Rating,
        ICollection<string> OwnerIds);

    /// <summary>Query parameters for searching images.</summary>
    public record GetSearchImagesRequest(
        ICollection<string>? Tags,
        ICollection<string>? Characters,
        ICollection<AgeRating>? Rating);

    /// <summary>Response returned for a paginated list of search results.</summary>
    public record GetSearchImagesResponse(Guid Id, AgeRating Rating);
    #endregion

    #region Actions
    /// <summary>
    /// Returns a paginated list of images that the user can access.  
    /// If no authentication is required, this endpoint remains open to anonymous users.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<GetImagesResponse>>> GetImages(
        [FromQuery] Guid? token,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 200)] int pageSize = 20)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var result = await imageQueryService.GetImagesAsync(user, token, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Uploads a new image and returns its GUID.  
    /// Requires the caller to be authenticated.
    /// </summary>
    [HttpPut("upload")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] UploadImageRequest request)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var imageId = await uploadImageService.UploadAsync(
            request.File,
            request.Publicity ?? user.DefaultPublicity,
            user);

        if (imageId == null)
            return BadRequest("Failed to import image");

        return Ok(imageId);
    }

    /// <summary>
    /// Deletes an existing image.  
    /// Only the owner or a privileged user can delete; otherwise a 403 is returned.
    /// </summary>
    [HttpDelete("delete/{imageId:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid imageId)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var result = await deleteImageService.DeleteAsync(imageId, user.Id);

        return result switch
        {
            DeleteResult.NotFound => NotFound(),
            DeleteResult.Forbidden => Forbid(),
            DeleteResult.Deleted  => Ok(),
            _ => BadRequest()
        };
    }

    /// <summary>
    /// Retrieves public metadata for an image (tags, characters, rating, owners).  
    /// Access is validated against the supplied token.
    /// </summary>
    [HttpGet("{imageId:guid}/data")]
    public async Task<ActionResult<ImageDataResponse>> Data(Guid imageId, [FromQuery] Guid? token)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var result = await imageDetailService.GetImageDataAccessAsync(imageId, user, token);
        if (!result.Found)   return NotFound();
        if (!result.Allowed) return Forbid();

        var data = result.Data;
        if (data == null)
            return NotFound("Image data not found.");

        return Ok(data);
    }

    /// <summary>
    /// Streams the raw image file to the caller.  
    /// The MIME type is now inferred from the image record or by inspecting the file header.
    /// </summary>
    [HttpGet("{imageId:guid}")]
    public async Task<IActionResult> GetImage(Guid imageId, [FromQuery] Guid? token)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var result = await imageDetailService.GetImageAccessAsync(imageId, user, token);
        if (!result.Found)   return NotFound();
        if (!result.Allowed) return Forbid();

        var image = result.Image;
        if (image == null)
            return NotFound("Requested image not found.");

        // Return the image file
        return await ReturnImage(image);
    }

    /// <summary>
    /// Searches images by tags, characters or rating.  
    /// Returns a paginated result set.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<PaginatedResponse<GetSearchImagesResponse>>> Search(
        [FromQuery] GetSearchImagesRequest request,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 200)] int pageSize = 20)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var result = await imageQueryService.SearchImagesAsync(user, request, page, pageSize);
        return Ok(result);
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Loads an image from the file system and returns it as a FileResult.  
    /// Handles MIME‑type inference and I/O errors gracefully.
    /// </summary>
    private async Task<IActionResult> ReturnImage(Image image)
    {
        try
        {
            var fileBytes = await fileService.LoadFile(image.Id);
            if (fileBytes == null || fileBytes.Length == 0) return NotFound("Requested image not found.");

            // Determine MIME type – prefer the stored value, otherwise guess from header bytes.
            string mimeType = "image/png";
            if (string.IsNullOrWhiteSpace(mimeType))
            {
                if (fileBytes.Length >= 8 &&
                    fileBytes[0] == 0x89 && fileBytes[1] == 0x50 && fileBytes[2] == 0x4E &&
                    fileBytes[3] == 0x47 && fileBytes[4] == 0x0D && fileBytes[5] == 0x0A &&
                    fileBytes[6] == 0x1A && fileBytes[7] == 0x0A)
                {
                    mimeType = "image/png";
                }
                else if (fileBytes.Length >= 3 &&
                         fileBytes[0] == 0xFF && fileBytes[1] == 0xD8 && fileBytes[2] == 0xFF)
                {
                    mimeType = "image/jpeg";
                }
                else
                {
                    // Fallback: generic binary stream
                    mimeType = "application/octet-stream";
                }
            }

            return File(fileBytes, mimeType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load image {ImageId}", image.Id);
            // Return 500 – the client cannot recover from an I/O failure.
            return StatusCode(500, "Unable to retrieve the requested image.");
        }
    }
    #endregion
}
