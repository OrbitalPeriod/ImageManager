using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Controllers;

[ApiController]
[Route("/api/images")]
public class ImageController(IFileService fileService, IDatabaseService databaseService, ApplicationDbContext dbContext, UserManager<User> userManager, IImageImportService imageImportService) : Controller
{
    public record GetImagesResponse(Guid Id, AgeRating Rating);
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<GetImagesResponse>>> GetImages([FromQuery] Guid? token, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 200;

        var user = await userManager.GetUserAsync(HttpContext.User);

        var baseQuery = databaseService.AccessibleImages(user, token);

        var totalCount = await baseQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)(pageSize));

        var images = await baseQuery.OrderByDescending(i => i.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        var imageData = images.Select(i => new GetImagesResponse(i.Id, i.AgeRating)).ToArray();
        
        var response = new PaginatedResponse<GetImagesResponse>()
        {
            Data = imageData,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        };

        return Ok(response);
    }
    
    [HttpGet("{imageId}")]
    public async Task<IActionResult> GetImage(Guid imageId, [FromQuery] Guid? token)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var image = await databaseService.GetImageById(imageId);

        if (image == null)
        {
            return NotFound();
        }

        if (!await databaseService.CanAccessImage(user, image, token))
        {
            return Forbid();
        }

        return await ReturnImage(image);
    }
    
    private async Task<IActionResult> ReturnImage(Image image)
    {
        var mimeType = "image/png";
        var fileBytes = await fileService.LoadFile(image.Id);
        return File(fileBytes, mimeType);
    }

    public record ImageDataResponse(Guid Id, ICollection<string> Tags, ICollection<string> Characters, AgeRating Rating, Publicity Publicity ,string OwnerId, string? OwnerName);
    [HttpGet("{imageId}/data")]
    public async Task<ActionResult<ImageDataResponse>> Data(Guid imageId, [FromQuery] Guid? token)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var image = await databaseService.GetImageById(imageId);

        if (image == null)
        {
            return NotFound();
        }

        if (!await databaseService.CanAccessImage(user, image, token))
        {
            return Forbid();
        }

        var response = new ImageDataResponse(image.Id, image.Tags.Select(x => x.Name).ToArray(),
            image.Characters.Select(x => x.Name).ToArray(), image.AgeRating, image.Publicity, image.User.Id,
            image.User.UserName);

        return Ok(response);
    }

    public record UploadImageRequest(IFormFile File, Publicity? Publicity);

    [HttpPut("upload")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImageRequests([FromForm] UploadImageRequest request)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null)
            return Unauthorized();

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            byte[] image;
            await using (var memoryStream = new MemoryStream())
            {
                await request.File.CopyToAsync(memoryStream);
                image = memoryStream.ToArray();
            }

            var publicity = request.Publicity ?? user.DefaultPublicity;
            var imageId = await imageImportService.ImportImage(image, publicity, transaction, user.Id);

            if (imageId == null)
            {
                await transaction.RollbackAsync();
                return BadRequest("Failed to import image");
            }

            await transaction.CommitAsync();
            return Ok(imageId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while importing the image." + ex.Message);
        }
    }

    [HttpDelete("delete/{imageId}")]
    [Authorize]
    public async Task<IActionResult> DeleteImage(Guid imageId)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null)
            return Unauthorized();

        var image = await databaseService.GetImageById(imageId);
        if (image == null)
            return NotFound();

        if (image.User.Id != user.Id)
            return Forbid();

        dbContext.Images.Remove(image);
        return Ok();
    }
}