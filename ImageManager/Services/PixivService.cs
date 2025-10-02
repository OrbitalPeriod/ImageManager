using PixivCS.Api;
using PixivCS.Models.Common;
using PixivCS.Models.Illust;
using PixivCS.Network;

namespace ImageManager.Services;

public interface IPixivService
{
    public Task<IllustInfo[]> GetLikedBookmarks(string userId, string? refreshToken, bool checkPrivate);
    public Task<byte[]> DownloadImage(IllustInfo illustration);
}

public class PixivService(string downloadRefreshToken) : IPixivService
{
    private readonly PixivAppApi _api = new(new ConnectionConfig
    {
        MaxRetries = 3,
        EnableRetry = true,
        TimeoutMs = 3000
    });
    private readonly string _downloadRefreshToken = downloadRefreshToken;
    private DateTime _nextRefresh = DateTime.MinValue;

    private async Task Authenticate()
    {
        var result = await _api.AuthAsync(_downloadRefreshToken);
        if (result.HasError) throw new PixivAuthException("Pixiv authentication failed" + result.Error);
        this._nextRefresh = result.ExpiresAt;
    }

    private async Task EnsureAuthenticated()
    {
        if (DateTime.Now > _nextRefresh)
        {
            await Authenticate();
        }
    }

    public async Task<IllustInfo[]> GetLikedBookmarks(string userId, string? refreshToken, bool checkPrivate)
    {
        await EnsureAuthenticated();
        var publicBookmarks = await _api.GetUserBookmarksIllustAsync(userId);
        if (publicBookmarks.HasError) throw new PixivApiException("Error fetching bookmarks");

        if (!checkPrivate || string.IsNullOrWhiteSpace(refreshToken)) return publicBookmarks.Illusts?.ToArray() ?? [];

        var userApi = new PixivAppApi();
        AuthResult result;

        result = await userApi.AuthAsync(refreshToken);
        if (result.HasError) throw new PixivAuthException("Pixiv authentication failed" + result.Error);

        var privateBookmarks = await userApi.GetUserBookmarksIllustAsync(userId, RestrictType.Private);
        if (privateBookmarks.HasError) throw new PixivApiException("Error fetching private bookmarks");

        publicBookmarks.Illusts?.AddRange(privateBookmarks.Illusts ?? []);

        return publicBookmarks.Illusts?.ToArray() ?? [];
    }

    public async Task<byte[]> DownloadImage(IllustInfo illustration)
    {
        await EnsureAuthenticated();

        if (illustration.ImageUrls == null) throw new ArgumentNullException(nameof(illustration));
        var image = await _api.DownloadImageAsync(illustration.ImageUrls.Original ?? illustration.ImageUrls.Large);

        return image;
    }
}

public class PixivAuthException(string message) : Exception(message);

public class PixivApiException(string message) : Exception(message);

public class PixivRateLimitException(string message) : Exception(message);