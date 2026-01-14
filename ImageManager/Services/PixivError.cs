namespace ImageManager.Services;

/// <summary>
/// Error types specific to Pixiv service operations.
/// </summary>
public enum PixivError
{
    /// <summary>The request timed out.</summary>
    Timeout,
    
    /// <summary>A network connectivity error occurred.</summary>
    NetworkError,
    
    /// <summary>Authentication with Pixiv API failed.</summary>
    AuthenticationFailed,
    
    /// <summary>The rate limit has been exceeded.</summary>
    RateLimited,
    
    /// <summary>The Pixiv API returned an error.</summary>
    ApiError,
    
    /// <summary>The API response was invalid or unexpected.</summary>
    InvalidResponse,
    
    /// <summary>Image download failed.</summary>
    DownloadFailed,
    
    /// <summary>Invalid input parameters were provided.</summary>
    InvalidInput
}
