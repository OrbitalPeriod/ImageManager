namespace ImageManager.Data.Helpers;

/// <summary>
/// Common error types that can be reused across different service domains.
/// These represent generic failure modes that are applicable to multiple contexts.
/// </summary>
public enum ServiceError
{
    /// <summary>The requested resource was not found.</summary>
    NotFound,
    
    /// <summary>The operation is forbidden for the current user.</summary>
    Forbidden,
    
    /// <summary>The request contains validation errors.</summary>
    ValidationError,
    
    /// <summary>A database operation failed.</summary>
    DatabaseError,
    
    /// <summary>The user is not authorized to perform this operation.</summary>
    Unauthorized,
    
    /// <summary>An internal server error occurred.</summary>
    InternalError
}
