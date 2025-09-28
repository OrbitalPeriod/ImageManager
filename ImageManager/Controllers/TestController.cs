using ImageManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImageManager.Controllers;

[ApiController]
public class TestController(IPixivService pixivService, ITaggerService taggerService) : Controller
{
    private readonly IPixivService _pixivService = pixivService;
    private readonly ITaggerService _taggerService = taggerService;

    public record TestRecord(string ImageId, [FromBody] string UserToken);

    [HttpPost("test")]
    [HttpGet("Test")]
    public async Task<ImageResponse> Test([FromBody] TestRecord record)
    {
        var bookmarks = await _pixivService.GetLikedBookmarks(record.ImageId, record.UserToken, true);
        var bookmark = bookmarks[0];
        var image = await _pixivService.DownloadImage(bookmark);
        var tags = await _taggerService.GetTags(image);

        return tags;
    }
}