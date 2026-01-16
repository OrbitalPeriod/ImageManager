namespace ImageManager.Services.FolderImport;

/// <summary>
/// File system implementation of <see cref="IFolderImportService"/>.
/// Creates and manages import folders for users.
/// </summary>
public class FolderImportService(IConfiguration config, ILogger<FolderImportService> logger) : IFolderImportService
{
    private readonly string _baseDirectory = config.GetValue<string>("IMPORT_FOLDER_PATH", "./import");

    /// <inheritdoc />
    public void EnsureBaseFolderExists()
    {
        try
        {
            if (!Directory.Exists(_baseDirectory))
            {
                Directory.CreateDirectory(_baseDirectory);
                logger.LogInformation("Created base import folder at {Path}", _baseDirectory);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create base import folder at {Path}", _baseDirectory);
            throw;
        }
    }

    /// <inheritdoc />
    public void EnsureUserFolderExists(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        CreateUserFolder(userId);
    }

    /// <inheritdoc />
    public void CreateUserFolder(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        try
        {
            // Ensure base folder exists first
            EnsureBaseFolderExists();

            var userFolderPath = Path.Combine(_baseDirectory, userId);
            if (!Directory.Exists(userFolderPath))
            {
                Directory.CreateDirectory(userFolderPath);
                logger.LogInformation("Created import folder for user {UserId} at {Path}", userId, userFolderPath);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create import folder for user {UserId}", userId);
            throw;
        }
    }
}
