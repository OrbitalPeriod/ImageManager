#region Usings

using ImageManager.Data.Helpers;
using PixivCS.Api;
using PixivCS.Models.Common;
using PixivCS.Models.Illust;
using PixivCS.Network;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
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
    /// <returns>A Result containing an array of <see cref="IllustInfo"/> objects or an error.</returns>
    Task<Result<IllustInfo[], PixivError>> GetLikedBookmarks(string userId, string? refreshToken, bool checkPrivate);

    /// <summary>
    /// Downloads all images from a given illustration (all pages from MetaPages).
    /// </summary>
    /// <param name="illustration">Illustration metadata.</param>
    /// <returns>A Result containing an array of byte arrays with the image data for each page, or an error.</returns>
    Task<Result<byte[][], PixivError>> DownloadAllImages(IllustInfo illustration);
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
            TimeoutMs = 50000
        });
    }

    private DateTime _nextRefresh = DateTime.MinValue;

    /// <summary>
    /// Authenticates the service using the download refresh token.
    /// </summary>
    private async Task<Result<Unit, PixivError>> AuthenticateAsync()
    {
        try
        {
            var result = await _api.AuthAsync(_downloadRefreshToken);
            if (result.HasError)
                return Result<Unit, PixivError>.Err(PixivError.AuthenticationFailed);

            _nextRefresh = result.ExpiresAt;
            return Result<Unit, PixivError>.Ok(Unit.New());
        }
        catch (TaskCanceledException)
        {
            return Result<Unit, PixivError>.Err(PixivError.Timeout);
        }
        catch (TimeoutException)
        {
            return Result<Unit, PixivError>.Err(PixivError.Timeout);
        }
        catch (HttpRequestException)
        {
            return Result<Unit, PixivError>.Err(PixivError.NetworkError);
        }
        catch (SocketException)
        {
            return Result<Unit, PixivError>.Err(PixivError.NetworkError);
        }
        catch (WebException)
        {
            return Result<Unit, PixivError>.Err(PixivError.NetworkError);
        }
        catch (PixivAuthException)
        {
            return Result<Unit, PixivError>.Err(PixivError.AuthenticationFailed);
        }
        catch (PixivRateLimitException)
        {
            return Result<Unit, PixivError>.Err(PixivError.RateLimited);
        }
        catch (Exception)
        {
            return Result<Unit, PixivError>.Err(PixivError.ApiError);
        }
    }

    /// <summary>
    /// Ensures that the service is authenticated before making API calls.
    /// </summary>
    private async Task<Result<Unit, PixivError>> EnsureAuthenticatedAsync()
    {
        if (DateTime.UtcNow > _nextRefresh)
        {
            var authResult = await AuthenticateAsync();
            if (!authResult.IsOk)
                return authResult;
        }
        return Result<Unit, PixivError>.Ok(Unit.New());
    }

    public async Task<Result<IllustInfo[], PixivError>> GetLikedBookmarks(string userId, string? refreshToken, bool checkPrivate)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<IllustInfo[], PixivError>.Err(PixivError.InvalidInput);

        try
        {
            var authResult = await EnsureAuthenticatedAsync();
            if (!authResult.IsOk)
                return Result<IllustInfo[], PixivError>.Err(authResult.UnwrapError());

            var publicResult = await _api.GetUserBookmarksIllustAsync(userId);
            if (publicResult.HasError)
            {
                // Check if it's a rate limit error
                var errorMessage = publicResult.Error?.ToString() ?? "";
                if (errorMessage.Contains("rate", StringComparison.OrdinalIgnoreCase) ||
                    errorMessage.Contains("limit", StringComparison.OrdinalIgnoreCase))
                {
                    return Result<IllustInfo[], PixivError>.Err(PixivError.RateLimited);
                }
                return Result<IllustInfo[], PixivError>.Err(PixivError.ApiError);
            }

            // If private bookmarks are not requested, return the public list immediately.
            if (!checkPrivate || string.IsNullOrWhiteSpace(refreshToken))
                return Result<IllustInfo[], PixivError>.Ok(publicResult.Illusts?.ToArray() ?? []);

            var userApi = new PixivAppApi();
            var userAuthResult = await userApi.AuthAsync(refreshToken);
            if (userAuthResult.HasError)
                return Result<IllustInfo[], PixivError>.Err(PixivError.AuthenticationFailed);

            var privateResult = await userApi.GetUserBookmarksIllustAsync(userId, RestrictType.Private);
            if (privateResult.HasError)
            {
                // Check if it's a rate limit error
                var errorMessage = privateResult.Error?.ToString() ?? "";
                if (errorMessage.Contains("rate", StringComparison.OrdinalIgnoreCase) ||
                    errorMessage.Contains("limit", StringComparison.OrdinalIgnoreCase))
                {
                    return Result<IllustInfo[], PixivError>.Err(PixivError.RateLimited);
                }
                return Result<IllustInfo[], PixivError>.Err(PixivError.ApiError);
            }

            publicResult.Illusts?.AddRange(privateResult.Illusts ?? []);
            return Result<IllustInfo[], PixivError>.Ok(publicResult.Illusts?.ToArray() ?? []);
        }
        catch (TaskCanceledException)
        {
            return Result<IllustInfo[], PixivError>.Err(PixivError.Timeout);
        }
        catch (TimeoutException)
        {
            return Result<IllustInfo[], PixivError>.Err(PixivError.Timeout);
        }
        catch (HttpRequestException)
        {
            return Result<IllustInfo[], PixivError>.Err(PixivError.NetworkError);
        }
        catch (SocketException)
        {
            return Result<IllustInfo[], PixivError>.Err(PixivError.NetworkError);
        }
        catch (WebException)
        {
            return Result<IllustInfo[], PixivError>.Err(PixivError.NetworkError);
        }
        catch (PixivAuthException)
        {
            return Result<IllustInfo[], PixivError>.Err(PixivError.AuthenticationFailed);
        }
        catch (PixivRateLimitException)
        {
            return Result<IllustInfo[], PixivError>.Err(PixivError.RateLimited);
        }
        catch (PixivApiException)
        {
            return Result<IllustInfo[], PixivError>.Err(PixivError.ApiError);
        }
        catch (Exception)
        {
            return Result<IllustInfo[], PixivError>.Err(PixivError.ApiError);
        }
    }

    /// <summary>
    /// Downloads a single image from a URL with quality fallback.
    /// </summary>
    private async Task<Result<byte[], PixivError>> DownloadImageFromUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Result<byte[], PixivError>.Err(PixivError.InvalidInput);

        try
        {
            var imageData = await _api.DownloadImageAsync(url);
            if (imageData == null || imageData.Length == 0)
                return Result<byte[], PixivError>.Err(PixivError.DownloadFailed);

            return Result<byte[], PixivError>.Ok(imageData);
        }
        catch (TaskCanceledException)
        {
            return Result<byte[], PixivError>.Err(PixivError.Timeout);
        }
        catch (TimeoutException)
        {
            return Result<byte[], PixivError>.Err(PixivError.Timeout);
        }
        catch (HttpRequestException)
        {
            return Result<byte[], PixivError>.Err(PixivError.NetworkError);
        }
        catch (SocketException)
        {
            return Result<byte[], PixivError>.Err(PixivError.NetworkError);
        }
        catch (WebException)
        {
            return Result<byte[], PixivError>.Err(PixivError.NetworkError);
        }
        catch (PixivAuthException)
        {
            return Result<byte[], PixivError>.Err(PixivError.AuthenticationFailed);
        }
        catch (PixivRateLimitException)
        {
            return Result<byte[], PixivError>.Err(PixivError.RateLimited);
        }
        catch (PixivApiException)
        {
            return Result<byte[], PixivError>.Err(PixivError.ApiError);
        }
        catch (Exception)
        {
            return Result<byte[], PixivError>.Err(PixivError.DownloadFailed);
        }
    }

    /// <summary>
    /// Gets the best available image URL from a MetaPage with quality fallback.
    /// </summary>
    private static string? GetImageUrlWithFallback(MetaPage? metaPage)
    {
        if (metaPage?.ImageUrls == null)
            return null;

        return metaPage.ImageUrls.Original
            ?? metaPage.ImageUrls.Large
            ?? metaPage.ImageUrls.Medium
            ?? metaPage.ImageUrls.SquareMedium;
    }

    public async Task<Result<byte[][], PixivError>> DownloadAllImages(IllustInfo illustration)
    {
        if (illustration == null)
            return Result<byte[][], PixivError>.Err(PixivError.InvalidInput);

        try
        {
            var authResult = await EnsureAuthenticatedAsync();
            if (!authResult.IsOk)
                return Result<byte[][], PixivError>.Err(authResult.UnwrapError());

            var downloadedImages = new List<byte[]>();

            // Check if this is a multi-page post
            if (illustration.MetaPages != null && illustration.MetaPages.Count > 0)
            {
                // Download all pages from MetaPages
                foreach (var metaPage in illustration.MetaPages)
                {
                    var url = GetImageUrlWithFallback(metaPage);
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        // Try fallback to ImageUrls if MetaPage doesn't have URLs
                        url = illustration.ImageUrls?.Original
                            ?? illustration.ImageUrls?.Large
                            ?? illustration.ImageUrls?.Medium
                            ?? illustration.ImageUrls?.SquareMedium;
                    }

                    if (string.IsNullOrWhiteSpace(url))
                        continue; // Skip this page if no URL available

                    var downloadResult = await DownloadImageFromUrl(url);
                    if (downloadResult.IsOk)
                    {
                        downloadedImages.Add(downloadResult.Unwrap());
                    }
                    // Continue with next page even if this one fails
                }
            }
            else
            {
                // Single-page post - use MetaSinglePage or ImageUrls
                var url =
                    illustration.MetaPages?.FirstOrDefault()?.ImageUrls?.Original ??
                    illustration.MetaPages?.FirstOrDefault()?.ImageUrls?.Large ??
                    illustration.MetaSinglePage?.OriginalImageUrl ??
                    illustration.ImageUrls?.Original ??
                    illustration.ImageUrls?.Large ??
                    illustration.ImageUrls?.Medium;
                if (string.IsNullOrWhiteSpace(url))
                    return Result<byte[][], PixivError>.Err(PixivError.InvalidInput);

                var downloadResult = await DownloadImageFromUrl(url);
                if (!downloadResult.IsOk)
                    return Result<byte[][], PixivError>.Err(downloadResult.UnwrapError());

                downloadedImages.Add(downloadResult.Unwrap());
            }

            if (downloadedImages.Count == 0)
                return Result<byte[][], PixivError>.Err(PixivError.DownloadFailed);

            return Result<byte[][], PixivError>.Ok(downloadedImages.ToArray());
        }
        catch (TaskCanceledException)
        {
            return Result<byte[][], PixivError>.Err(PixivError.Timeout);
        }
        catch (TimeoutException)
        {
            return Result<byte[][], PixivError>.Err(PixivError.Timeout);
        }
        catch (HttpRequestException)
        {
            return Result<byte[][], PixivError>.Err(PixivError.NetworkError);
        }
        catch (SocketException)
        {
            return Result<byte[][], PixivError>.Err(PixivError.NetworkError);
        }
        catch (WebException)
        {
            return Result<byte[][], PixivError>.Err(PixivError.NetworkError);
        }
        catch (PixivAuthException)
        {
            return Result<byte[][], PixivError>.Err(PixivError.AuthenticationFailed);
        }
        catch (PixivRateLimitException)
        {
            return Result<byte[][], PixivError>.Err(PixivError.RateLimited);
        }
        catch (PixivApiException)
        {
            return Result<byte[][], PixivError>.Err(PixivError.ApiError);
        }
        catch (Exception)
        {
            return Result<byte[][], PixivError>.Err(PixivError.DownloadFailed);
        }
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
