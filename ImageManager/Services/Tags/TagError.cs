namespace ImageManager.Services.Tags;

/// <summary>
/// Error types specific to tag service operations.
/// </summary>
public enum TagError
{
    /// <summary>Invalid pagination parameters were provided.</summary>
    InvalidPagination,
    
    /// <summary>An internal error occurred while processing tags.</summary>
    InternalError
}
