using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Controllers;

[ApiController]
public class TestController(IPixivService pixivService, ITaggerService taggerService, IPixivImageImportManager importManager, ApplicationDbContext dbContext, UserManager<User> userManager, IDatabaseService databaseService) : Controller
{


    public record TestRecord(ICollection<ImageController.ImageDataResponse> Images);

    [HttpPost("test")]
    public async Task<TestRecord> Test()
    {
        var user = await userManager.GetUserAsync(HttpContext.User);

        var images = await databaseService.AccessibleImages(user, null).Include(i => i.Tags).Include(i => i.Characters).Include(u => u.User).ToArrayAsync();

        return new TestRecord(images.Select(i => new ImageController.ImageDataResponse(i.Id, i.Tags.Select(t => t.Name).ToArray(), i.Characters.Select(i => i.Name).ToArray(), i.AgeRating, i.User.Id, i.User.UserName)).ToArray());
    }

}