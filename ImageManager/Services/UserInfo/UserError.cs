namespace ImageManager.Services.UserInfo;

/// <summary>
/// Error types specific to user info operations.
/// </summary>
public enum UserError
{
    /// <summary>The requested user was not found.</summary>
    NotFound,
    
    /// <summary>The user is not authenticated.</summary>
    Unauthorized,
    
    /// <summary>An internal error occurred while processing user information.</summary>
    InternalError
}
