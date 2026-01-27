namespace ImageManager.Services.Folders;

/// <summary>
/// Error types specific to folder service operations.
/// </summary>
public enum FolderError
{
    /// <summary>The requested folder or image was not found.</summary>
    NotFound,
    
    /// <summary>The user does not have permission to access this folder.</summary>
    Forbidden,
    
    /// <summary>The image already exists in the folder, or folder name conflict.</summary>
    AlreadyExists,
    
    /// <summary>Attempt to delete the "Liked" folder.</summary>
    CannotDeleteLiked,
    
    /// <summary>Invalid pagination parameters were provided.</summary>
    InvalidPagination
}
