namespace ImageManager.Services.Admin;

/// <summary>
/// Error types specific to admin service operations.
/// </summary>
public enum AdminError
{
    /// <summary>The requested user was not found.</summary>
    NotFound,
    
    /// <summary>The user is already approved.</summary>
    AlreadyApproved,
    
    /// <summary>The user is already an administrator.</summary>
    AlreadyAdministrator,
    
    /// <summary>The user is not an administrator.</summary>
    NotAdministrator,
    
    /// <summary>An internal error occurred while processing the admin operation.</summary>
    InternalError
}
