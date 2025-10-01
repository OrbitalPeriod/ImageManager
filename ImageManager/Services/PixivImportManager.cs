using CoenM.ImageHash.HashAlgorithms;
using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;
using PixivCS.Models.Illust;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageManager.Services;

public interface IPixivImageImportManager
{
    public Task ImportAllUserBookmarks();
    public Task ImportBookmarks(User user);
}

public class PixivImportManager(IPixivService pixivService, ApplicationDbContext dbContext, IDatabaseService databaseService, ITaggerService taggerService, IFileService fileService, ILoggerService loggerService) : IPixivImageImportManager
{
    private readonly IPixivService _pixivService = pixivService;
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IDatabaseService _databaseService = databaseService;
    private readonly ITaggerService _taggerService = taggerService;
    private readonly IFileService _fileService = fileService;
    private readonly ILoggerService _loggerService = loggerService;
    private readonly AverageHash _hash = new();

    public async Task ImportAllUserBookmarks()
    {
        var users = await _dbContext.Users.Include(u => u.PlatformTokens).Where(u => u.PlatformTokens.Count(pft => pft.Platform == Platform.Pixiv) > 0).ToListAsync();
        foreach (var user in users)
        {
            await ImportBookmarks(user);
        }
    }
    public async Task ImportBookmarks(User user)
    {
        var items = await _dbContext.PlatformTokens.Where(pft => pft.User == user).Where(pt => pt.Platform == Platform.Pixiv)
            .ToListAsync();
        foreach (var item in items)
        {
            await ImportPixivBookmarks(user, item.PlatformUserId, item.Token, item.CheckPrivate);
        }
    }

    public async Task ImportPixivBookmarks(User user, string pixivUserId, string? pixivRefreshToken, bool checkPrivate)
    {
        var illustrations = await _pixivService.GetLikedBookmarks(pixivUserId, pixivRefreshToken, checkPrivate);

        var illustIds = illustrations.Select(x => x.Id).ToArray();
        var downloadedIds = await _dbContext.DownloadedImages
            .Where(d => d.User == user)
            .Select(d => d.DownloadedId)
            .ToListAsync();

        var toDownload = illustIds.Except(downloadedIds).ToList();

        _loggerService.LogInfo("Downloading: " + toDownload.Count + " new images" + " For user: " + user.UserName);
        
        foreach (var illustration in toDownload.Select(x => illustrations.First(i => i.Id == x)))
        {
            await DownloadImage(user, illustration);
        }
    }
    

    private async Task DownloadImage(User user, IllustInfo illustration)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        byte[] imageBytes;
        try
        {
            imageBytes = await _pixivService.DownloadImage(illustration);
        }
        catch (Exception e)
        {
            _loggerService.LogError($"Failed to get tags for {illustration}, due to {e.Message}");
            return;
        }

        ImageResponse imageData;
        try
        {
            imageData = await _taggerService.GetTags(imageBytes);
        }
        catch (Exception e)
        {
            _loggerService.LogError($"Failed to get tags for {illustration}, due to {e.Message}");
            return;
        }

        var tagEntities = await _databaseService.GetTags(imageData.GeneralTags);
        var characterEntities = await _databaseService.GetCharacters(imageData.CharacterTags);


        Guid guid;
        ulong hash;
        try
        {
            using var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(new MemoryStream(imageBytes));
            hash = _hash.Hash(image);
            guid = await _fileService.SaveFile(image);
        }
        catch (Exception e)
        {
            _loggerService.LogError($"Failed to save image tags: {e.Message}");
            return;
        }
        
        var imageEntity = new Image()
        {
            Id = guid,
            Characters = characterEntities,
            Tags = tagEntities,
            DownloadedImage = new DownloadedImage()
                { DownloadedId = illustration.Id, User = user, Platform = Platform.Pixiv },
            Hash = hash,
            Publicity = user.DefaultPublicity,
            Rating = (Data.Models.Rating)imageData.Rating,
            ShareTokens = [],
            User = user
        };

        await _dbContext.Images.AddAsync(imageEntity);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}