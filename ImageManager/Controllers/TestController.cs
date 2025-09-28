using CoenM.ImageHash.HashAlgorithms;
using ImageManager.Services;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageManager.Controllers;

public class TestRequest
{
    public string ImageId { get; set; }
    public string UserToken { get; set; }
    public IFormFile File { get; set; }
}

[ApiController]
public class TestController(IPixivService pixivService, ITaggerService taggerService) : Controller
{
    private readonly IPixivService _pixivService = pixivService;
    private readonly ITaggerService _taggerService = taggerService;

    public record TestRecord(string ImageId, [FromBody] string UserToken, IFormFile File);

    [HttpPost("test")]
    [Consumes("multipart/form-data")]
    public async Task<ImageResponse> Test([FromForm] TestRequest record)
    {
        //var bookmarks = await _pixivService.GetLikedBookmarks(record.ImageId, record.UserToken, true);
        //var bookmark = bookmarks[0];
        //var image = await _pixivService.DownloadImage(bookmark);
        
        byte[] image_data;
        
        using (var memoryStream = new MemoryStream())
        {
            await record.File.CopyToAsync(memoryStream);
            image_data = memoryStream.ToArray();
        }

        var hashAlgo = new AverageHash();
        using var image = Image.Load<Rgba32>(image_data);
        ulong imageHash = hashAlgo.Hash(image);
        Console.WriteLine(imageHash);
        
        var tags = await _taggerService.GetTags(image_data);

        return tags;
    }
}