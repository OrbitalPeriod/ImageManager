namespace ImageManager.Services.FolderImport;

/// <summary>
/// Service responsible for creating and managing import folders for users.
/// </summary>
public interface IFolderImportService
{
    /// <summary>
    /// Creates the base import folder if it doesn't exist.
    /// </summary>
    void EnsureBaseFolderExists();

    /// <summary>
    /// Creates a user's import folder if it doesn't exist.
    /// </summary>
    /// <param name="userId">The user ID to create a folder for.</param>
    void EnsureUserFolderExists(string userId);

    /// <summary>
    /// Creates a specific user's import folder.
    /// This will create the folder even if it already exists (idempotent operation).
    /// </summary>
    /// <param name="userId">The user ID to create a folder for.</param>
    void CreateUserFolder(string userId);
}
