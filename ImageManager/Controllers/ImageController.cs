#region Usings
using System.ComponentModel.DataAnnotations;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Extensions;
using ImageManager.Services;
using ImageManager.Services.File;
using ImageManager.Services.ImageImport;
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
    public record GetImagesResponse(Guid Id, AgeRating Rating, DateTime StoredAt);

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
        ICollection<string> OwnerIds,
        DateTime StoredAt);

    /// <summary>Query parameters for searching images.</summary>
    public record GetSearchImagesRequest(
        ICollection<string>? Tags,
        ICollection<string>? Characters,
        ICollection<AgeRating>? Rating);

    /// <summary>Response returned for a paginated list of search results.</summary>
    public record GetSearchImagesResponse(Guid Id, AgeRating Rating, DateTime StoredAt);
    #endregion

    #region Actions
    /// <summary>
    /// Returns a paginated list of images that the user can access.  
    /// If no authentication is required, this endpoint remains open to anonymous users.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<GetImagesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<GetImagesResponse>>> GetImages(
        [FromQuery] Guid? token,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 200)] int pageSize = 20)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var result = await imageQueryService.GetImagesAsync(user, token, page, pageSize);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Uploads a new image and returns its GUID.  
    /// Requires the caller to be authenticated.
    /// </summary>
    [HttpPut()]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<Guid>> Upload([FromForm] UploadImageRequest request)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var uploadResult = await uploadImageService.UploadAsync(
            request.File,
            request.Publicity ?? user.DefaultPublicity,
            user);

        return this.ToActionResult(uploadResult.Map(success => success.Id));
    }

    /// <summary>
    /// Deletes an existing image.  
    /// Only the owner or an administrator can delete; otherwise a 403 is returned.
    /// </summary>
    [HttpDelete("{imageId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid imageId)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Unauthorized();

        var result = await deleteImageService.DeleteAsync(imageId, user);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Retrieves public metadata for an image (tags, characters, rating, owners).  
    /// Access is validated against the supplied token.q
    /// </summary>
    [HttpGet("{imageId:guid}/data")]
    [ProducesResponseType(typeof(ImageDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImageDataResponse>> Data(Guid imageId, [FromQuery] Guid? token)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var result = await imageDetailService.GetImageDataAccessAsync(imageId, user, token);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Streams the raw image file to the caller.  
    /// The MIME type is now inferred from the image record or by inspecting the file header.
    /// </summary>
    [HttpGet("{imageId:guid}")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetImage(Guid imageId, [FromQuery] Guid? token)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var result = await imageDetailService.GetImageAccessAsync(imageId, user, token);
        if (!result.IsOk)
        {
            return this.ToActionResultForError(result);
        }

        var image = result.Unwrap();
        // Return the image file
        return await ReturnCompressed(image);
    }


    /// <summary>
    /// Streams the raw image file to the caller.  
    /// The MIME type is now inferred from the image record or by inspecting the file header.
    /// </summary>
    [HttpGet("{imageId:guid}/original")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOriginalImage(Guid imageId, [FromQuery] Guid? token)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var result = await imageDetailService.GetImageAccessAsync(imageId, user, token);
        if (!result.IsOk)
        {
            return this.ToActionResultForError(result);
        }

        var image = result.Unwrap();
        // Return the image file
        return await ReturnImage(image);
    }

    /// <summary>
    /// Streams the thumbnail image file to the caller.
    /// </summary>
    [HttpGet("{imageId:guid}/thumb")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetThumbImage(Guid imageId, [FromQuery] Guid? token)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var result = await imageDetailService.GetImageAccessAsync(imageId, user, token);
        if (!result.IsOk)
        {
            return this.ToActionResultForError(result);
        }

        var image = result.Unwrap();
        // Return the image file
        return await ReturnThumbnail(image);
    }

    /// <summary>
    /// Searches images by tags, characters or rating.  
    /// Returns a paginated result set.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedResponse<GetSearchImagesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<GetSearchImagesResponse>>> Search(
        [FromQuery] GetSearchImagesRequest request,
        [FromQuery] Guid? token,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 200)] int pageSize = 20)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var result = await imageQueryService.SearchImagesAsync(user, request, token, page, pageSize);
        return this.ToActionResult(result);
    }
    #endregion

    #region Helpers
    private async Task<IActionResult> ReturnImage(Image image)
    {
        return await ReturnImg(image, ImageType.Original);
    }

    private async Task<IActionResult> ReturnCompressed(Image image)
    {
        return await ReturnImg(image, ImageType.Compressed);
    }

    private async Task<IActionResult> ReturnThumbnail(Image image)
    {
        return await ReturnImg(image, ImageType.Thumbnail);
    }
    /// <summary>
    /// Loads an image from the file system and returns it as a FileResult.  
    /// Handles MIME‑type inference and I/O errors gracefully.
    /// </summary>
    private async Task<IActionResult> ReturnImg(Image image, ImageType imageType)
    {
        try
        {
            byte[] fileBytes = imageType switch
            {
                ImageType.Original => await fileService.LoadFullImage(image.Id),
                ImageType.Compressed => await fileService.LoadCompressedImage(image.Id),
                ImageType.Thumbnail => await fileService.LoadThumbnailImage(image.Id),
                _ => throw new ArgumentOutOfRangeException(nameof(imageType), imageType, null)
            };

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
