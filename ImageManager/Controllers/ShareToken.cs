using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ImageManager.Controllers;

[ApiController]
public class ShareToken(UserManager<User> userManager, IDatabaseService databaseService, ApplicationDbContext dbContext) : Controller
{
    public record AddPlatformTokenRequest(DateTime? Expiration);
    [Authorize]
    [HttpPost("add/{imageId:guid}")]
    public async Task<IActionResult> AddPlatformToken(Guid imageId, [FromBody] AddPlatformTokenRequest request)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null)
        {
            return Unauthorized();
        }

        var userOwnedImage = await dbContext.UserOwnedImages.Where(i => i.ImageId == imageId && i.UserId == user.Id).FirstOrDefaultAsync();
        if (userOwnedImage == null)
        {
            return NotFound();
        }

        var shareToken = new Data.Models.ShareToken()
        {
            Created = DateTime.UtcNow,
            Expires = request.Expiration ?? DateTime.UtcNow + TimeSpan.FromDays(3),
            UserOwnedImageId = userOwnedImage.Id,
            Id = Guid.NewGuid(),
            UserId = user.Id
        };

        await databaseService.SaveShareToken(shareToken);
        return Ok(shareToken.Id);
    }
}