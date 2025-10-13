using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        var baseQuery = databaseService.AccessibleImages(user, token).Select(uoi => uoi.Image).Distinct();

        var totalCount = await baseQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)(pageSize));

        var images = await baseQuery
            .OrderByDescending(i => i.Id)
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

    [HttpGet("{imageId:guid}")]
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

    public record ImageDataResponse(Guid Id, ICollection<string> Tags, ICollection<string> Characters, AgeRating Rating, ICollection<string> OwnerIds);
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
            image.Characters.Select(x => x.Name).ToArray(), image.AgeRating, image.UserOwnedImages.Select(x => x.UserId).ToArray());

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

    [HttpDelete("delete/{imageId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteImage(Guid imageId)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null)
            return Unauthorized();

        var userOwnedImage = await dbContext.UserOwnedImages.FirstOrDefaultAsync(uoi => uoi.ImageId == imageId);
        if (userOwnedImage == null)
            return NotFound();

        if (userOwnedImage.UserId == user.Id)
            return Forbid();

        dbContext.UserOwnedImages.Remove(userOwnedImage);
        await dbContext.SaveChangesAsync();
        return Ok();
    }

    public record GetSearchImagesRequest(ICollection<string>? Tags, ICollection<string>? Characters, ICollection<AgeRating>? Rating);
    public record GetSearchImagesResponse(Guid Id, AgeRating Rating);

    [HttpGet("search")]
    public async Task<ActionResult<GetSearchImagesResponse>> GetSearchImages([FromQuery] GetSearchImagesRequest request, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        var baseQuery = databaseService.AccessibleImages(user, null);

        var query = baseQuery.AsQueryable();

        if (request.Tags != null && request.Tags.Any()) query = query.Where(i => i.Image.Tags.Any(t => request.Tags.Contains(t.Name)));
        if (request.Characters != null && request.Characters.Any()) query = query.Where(i => i.Image.Characters.Any(c => request.Characters.Contains(c.Name)));
        if (request.Rating != null && request.Rating.Any()) query = query.Where(i => request.Rating.Contains(i.Image.AgeRating));

        var imagesData = await query.OrderByDescending(i => i.Id)
            .Include(i => i.Image)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();
        var images = imagesData.Select(i => new GetSearchImagesResponse(i.ImageId, i.Image.AgeRating)).ToArray();

        var response = new PaginatedResponse<GetSearchImagesResponse>()
        {
            Data = images,
            Page = page,
            PageSize = pageSize,
            TotalItems = await query.CountAsync(),
            TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize)
        };

        return Ok(response);
    }
}