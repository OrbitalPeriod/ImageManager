using System.Security.Claims;
using ImageManager.Data;
using ImageManager.Data.Models;
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

    [HttpGet("{imageId}")]
    public async Task<IActionResult> Test(Guid imageId)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var image = await dbContext.Images.Include(x => x.User).Include(x => x.ShareTokens).FirstOrDefaultAsync(i => i.Id == imageId);
        if (image == null)
        {
            return NotFound();
        }

        if ((user == null || image.User.Id != user.Id) && image.Publicity is Publicity.Restricted or Publicity.Private && (user == null || await dbContext.ShareTokens.Where(stk => stk.ImageId == image.Id).Where(tk => !tk.IsExpired)
                .AllAsync(stk => stk.UserId != user.Id)))
        {
            return Forbid();
        }


        var mimeType = "image/png";

        var fileBytes = await fileService.LoadFile(image.Id);

        return File(fileBytes, contentType: mimeType);
    }

    public record UploadImageRequest(IFormFile File, Publicity? Publicity);

    [HttpPost("upload")]
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
            // optionally log ex here
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while importing the image.");
        }
    }

}