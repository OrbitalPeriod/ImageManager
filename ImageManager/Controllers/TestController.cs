using CoenM.ImageHash.HashAlgorithms;
using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageManager.Controllers;

[ApiController]
public class TestController(IPixivService pixivService, ITaggerService taggerService, IPixivImageImportManager importManager, ApplicationDbContext dbContext) : Controller
{
    private readonly IPixivService _pixivService = pixivService;
    private readonly ITaggerService _taggerService = taggerService;
    private readonly IPixivImageImportManager _importManager = importManager;
    private readonly ApplicationDbContext _dbContext = dbContext;

    public record TestRecord(string PixivUserId, string PixivUserToken);

    [HttpPost("test")]
    public async Task Test([FromForm] TestRecord record)
    {
        var user = _dbContext.Users.Include(u => u.Images).Include(u => u.DownloadedImages).First(x => x.UserName == "test");
        var images = _dbContext.Images.Where(x => x.User == user).ToList();

        PlatformToken token = new PlatformToken()
        {
            Platform = Platform.Pixiv,
            PlatformUserId = record.PixivUserId,
            Token = record.PixivUserToken,
            User = user,
        };
        
        await _dbContext.PlatformTokens.AddAsync(token);
        await _dbContext.SaveChangesAsync();
    }
}