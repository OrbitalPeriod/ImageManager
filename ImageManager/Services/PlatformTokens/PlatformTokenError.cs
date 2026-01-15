namespace ImageManager.Services.PlatformTokens;

/// <summary>
/// Error types specific to platform token operations.
/// Consolidates DeleteError and QueueError into a single enum.
/// </summary>
public enum PlatformTokenError
{
    /// <summary>The requested platform token was not found.</summary>
    NotFound,
    
    /// <summary>The user does not have permission to perform this operation.</summary>
    Forbidden,
    
    /// <summary>An internal error occurred while processing the token.</summary>
    InternalError
}
