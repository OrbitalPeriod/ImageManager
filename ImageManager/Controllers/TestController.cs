using CoenM.ImageHash.HashAlgorithms;
using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageManager.Controllers;

[ApiController]
public class TestController(IPixivService pixivService, ITaggerService taggerService, IPixivImageImportManager importManager, ApplicationDbContext dbContext, UserManager<User> userManager) : Controller
{


    public record TestRecord(string PixivUserId, string PixivUserToken);

    [HttpPost("test")]
    [Authorize]
    public async Task Test([FromForm] TestRecord record)
    {
        var user = (User)(await userManager.GetUserAsync(HttpContext.User)!)!;

        var images = dbContext.Images.Where(x => x.User == user).ToList();

        var token = new PlatformToken()
        {
            Platform = Platform.Pixiv,
            PlatformUserId = record.PixivUserId,
            Token = record.PixivUserToken,
            User = user,
        };

        await dbContext.PlatformTokens.AddAsync(token);
        await dbContext.SaveChangesAsync();
    }
}