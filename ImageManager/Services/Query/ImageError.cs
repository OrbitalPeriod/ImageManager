namespace ImageManager.Services.Query;

/// <summary>
/// Error types specific to image query and access operations.
/// </summary>
public enum ImageError
{
    /// <summary>The requested image was not found.</summary>
    NotFound,
    
    /// <summary>The user does not have permission to access this image.</summary>
    Forbidden,
    
    /// <summary>Invalid pagination parameters were provided.</summary>
    InvalidPagination
}
