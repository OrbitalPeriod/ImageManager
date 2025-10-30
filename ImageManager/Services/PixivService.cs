#region Usings

using PixivCS.Api;
using PixivCS.Models.Common;
using PixivCS.Models.Illust;
using PixivCS.Network;
#endregion

namespace ImageManager.Services;

/// <summary>
/// Service for interacting with the Pixiv API.
/// </summary>
public interface IPixivService
{
    /// <summary>
    /// Retrieves all illustrations that the specified user has liked.
    /// If <paramref name="checkPrivate"/> is true and a non‑empty <paramref name="refreshToken"/>
    /// is supplied, private bookmarks are also included.
    /// </summary>
    /// <param name="userId">Pixiv user identifier.</param>
    /// <param name="refreshToken">Refresh token for the user (optional).</param>
    /// <param name="checkPrivate"><c>true</c> to include private bookmarks.</param>
    /// <returns>An array of <see cref="IllustInfo"/> objects.</returns>
    Task<IllustInfo[]> GetLikedBookmarks(string userId, string? refreshToken, bool checkPrivate);

    /// <summary>
    /// Downloads the image data for a given illustration.
    /// </summary>
    /// <param name="illustration">Illustration metadata.</param>
    /// <returns>Byte array containing the image.</returns>
    Task<byte[]> DownloadImage(IllustInfo illustration);
}

#region Implementation

/// <summary>
/// Concrete implementation of <see cref="IPixivService"/> that uses the PixivCS library.
/// </summary>
public sealed class PixivService : IPixivService
{
    private readonly PixivAppApi _api;
    private readonly string _downloadRefreshToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="PixivService"/> class.
    /// </summary>
    /// <param name="downloadRefreshToken">Refresh token required to download public illustrations.</param>
    /// <exception cref="ArgumentException"><paramref name="downloadRefreshToken"/> is null, empty, or whitespace.</exception>
    public PixivService(string downloadRefreshToken)
    {
        if (string.IsNullOrWhiteSpace(downloadRefreshToken))
            throw new ArgumentException("Download refresh token cannot be null or empty.", nameof(downloadRefreshToken));

        _downloadRefreshToken = downloadRefreshToken;
        _api = new PixivAppApi(new ConnectionConfig
        {
            MaxRetries = 3,
            EnableRetry = true,
            TimeoutMs = 3000
        });
    }

    private DateTime _nextRefresh = DateTime.MinValue;

    /// <summary>
    /// Authenticates the service using the download refresh token.
    /// </summary>
    /// <exception cref="PixivAuthException">Thrown when authentication fails.</exception>
    private async Task AuthenticateAsync()
    {
        var result = await _api.AuthAsync(_downloadRefreshToken);
        if (result.HasError)
            throw new PixivAuthException($"Pixiv authentication failed: {result.Error}");

        _nextRefresh = result.ExpiresAt;
    }

    /// <summary>
    /// Ensures that the service is authenticated before making API calls.
    /// </summary>
    private async Task EnsureAuthenticatedAsync()
    {
        if (DateTime.UtcNow > _nextRefresh)
            await AuthenticateAsync();
    }

    public async Task<IllustInfo[]> GetLikedBookmarks(string userId, string? refreshToken, bool checkPrivate)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        await EnsureAuthenticatedAsync();

        var publicResult = await _api.GetUserBookmarksIllustAsync(userId);
        if (publicResult.HasError)
            throw new PixivApiException($"Error fetching public bookmarks for user {userId}");

        // If private bookmarks are not requested, return the public list immediately.
        if (!checkPrivate || string.IsNullOrWhiteSpace(refreshToken))
            return publicResult.Illusts?.ToArray() ?? [];

        var userApi = new PixivAppApi();
        var authResult = await userApi.AuthAsync(refreshToken);
        if (authResult.HasError)
            throw new PixivAuthException($"Pixiv authentication failed: {authResult.Error}");

        var privateResult = await userApi.GetUserBookmarksIllustAsync(userId, RestrictType.Private);
        if (privateResult.HasError)
            throw new PixivApiException($"Error fetching private bookmarks for user {userId}");

        publicResult.Illusts?.AddRange(privateResult.Illusts ?? []);
        return publicResult.Illusts?.ToArray() ?? [];
    }

    public async Task<byte[]> DownloadImage(IllustInfo illustration)
    {
        if (illustration == null)
            throw new ArgumentNullException(nameof(illustration));

        await EnsureAuthenticatedAsync();

        var imageUrls = illustration.ImageUrls;
        if (imageUrls == null)
            throw new ArgumentNullException(nameof(imageUrls), "Illustration does not contain any image URLs.");

        // Prefer the original URL, fall back to large.
        var url = imageUrls.Original ?? imageUrls.Large;
        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException("No valid image URL available for download.");

        return await _api.DownloadImageAsync(url);
    }
}

#endregion

#region Exceptions

/// <summary>
/// Thrown when Pixiv authentication fails.
/// </summary>
public sealed class PixivAuthException(string message) : Exception(message);

/// <summary>
/// Thrown when a Pixiv API call returns an error.
/// </summary>
public sealed class PixivApiException(string message) : Exception(message);

/// <summary>
/// Thrown when the client has hit the Pixiv rate limit.
/// </summary>
public sealed class PixivRateLimitException(string message) : Exception(message);

#endregion
