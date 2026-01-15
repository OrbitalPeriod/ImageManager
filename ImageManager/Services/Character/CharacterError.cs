namespace ImageManager.Services.Character;

/// <summary>
/// Error types specific to character query operations.
/// </summary>
public enum CharacterError
{
    /// <summary>Invalid pagination parameters were provided.</summary>
    InvalidPagination,
    
    /// <summary>An internal error occurred while processing characters.</summary>
    InternalError
}
