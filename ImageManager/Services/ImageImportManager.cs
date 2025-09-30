using CoenM.ImageHash.HashAlgorithms;
using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageManager.Services;

public interface IImageImportManager
{
    Task ImportPixivBookmarks(User user, string pixivUserId, string pixivRefreshToken, bool checkPrivate);
}

public class ImageImportManager(IPixivService pixivService, ApplicationDbContext dbContext, IDatabaseService databaseService, ITaggerService taggerService, IFileService fileService) : IImageImportManager
{
    private readonly IPixivService _pixivService = pixivService;
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IDatabaseService _databaseService = databaseService;
    private readonly ITaggerService _taggerService = taggerService;
    private readonly IFileService _fileService = fileService;
    private readonly AverageHash _hash = new();

    public async Task ImportPixivBookmarks(User user, string pixivUserId, string pixivRefreshToken, bool checkPrivate)
    {
        var illustrations = await _pixivService.GetLikedBookmarks(pixivUserId, pixivRefreshToken, checkPrivate);

        var illustIds = illustrations.Select(x => x.Id).ToArray();
        var downloadedIds = await _dbContext.DownloadedImages
            .Where(d => d.User == user)
            .Select(d => d.DownloadedId)
            .ToListAsync();

        var toDownload = illustIds.Except(downloadedIds).ToList();

        Console.WriteLine("Downloading: " + toDownload.Count + " new images");


        foreach (var illustration in toDownload.Select(x => illustrations.First(i => i.Id == x)))
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            var imageBytes = await _pixivService.DownloadImage(illustration);
            var imageData = await _taggerService.GetTags(imageBytes);

            var tagEntities = await _databaseService.GetTags(imageData.GeneralTags);
            var characterEntities = await _databaseService.GetCharacters(imageData.CharacterTags);

            using var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(new MemoryStream(imageBytes));
            var guid = await _fileService.SaveFile(image);
            var imageEntity = new Image()
            {
                Id = guid,
                Characters = characterEntities,
                Tags = tagEntities,
                DownloadedImage = new DownloadedImage()
                { DownloadedId = illustration.Id, User = user, Platform = Platform.Pixiv },
                Hash = _hash.Hash(image),
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


}