using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Repositories.Repository_Interfaces;
using ImageManager.Services;
using ImageManager.Services.FolderImport;
using ImageManager.Services.ImageImport;

namespace ImageManager.Workers;

/// <summary>
/// Background service that monitors the import folder for new images and automatically imports them.
/// Watches for files in user-specific subfolders and imports them using the ImageImportService.
/// </summary>
public class FolderImageImportService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    IFolderImportService folderImportService,
    ILogger<FolderImageImportService> logger)
    : BackgroundService
{
    private readonly string _importDirectory = configuration.GetValue<string>("IMPORT_FOLDER_PATH", "./import");
    private FileSystemWatcher? _fileWatcher;
    
    // Supported image file extensions
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Folder image import service started.");

        try
        {
            // Initialize base folder
            folderImportService.EnsureBaseFolderExists();

            // Optionally sync existing user folders at startup
            await SyncExistingUserFoldersAsync(stoppingToken);

            // Process existing image files in the import folder
            await ProcessExistingFilesAsync(stoppingToken);

            // Setup FileSystemWatcher (after processing existing files to avoid double-processing)
            _fileWatcher = new FileSystemWatcher(_importDirectory)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            _fileWatcher.Created += async (sender, e) => await OnFileCreated(e, stoppingToken);
            _fileWatcher.Error += OnFileWatcherError;

            logger.LogInformation("Monitoring import folder: {Path}", _importDirectory);

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            logger.LogError(ex, "Unexpected error in folder image import service");
        }
        finally
        {
            _fileWatcher?.Dispose();
            logger.LogInformation("Folder image import service stopped.");
        }
    }

    /// <summary>
    /// Synchronizes existing user folders at startup to ensure all users have folders.
    /// </summary>
    private async Task SyncExistingUserFoldersAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var allUsers = await userRepository.GetAllAsync();
            var syncFolderService = scope.ServiceProvider.GetRequiredService<IFolderImportService>();

            foreach (var user in allUsers)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    syncFolderService.EnsureUserFolderExists(user.Id);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to create folder for existing user {UserId}", user.Id);
                }
            }

            logger.LogInformation("Synchronized folders for {Count} existing users", allUsers.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync existing user folders");
        }
    }

    /// <summary>
    /// Scans and processes all existing image files in the import folder at startup.
    /// </summary>
    private async Task ProcessExistingFilesAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (!Directory.Exists(_importDirectory))
            {
                logger.LogInformation("Import directory does not exist, skipping existing file processing");
                return;
            }

            logger.LogInformation("Scanning import directory for existing image files: {Path}", _importDirectory);

            var imageFiles = GetImageFilesRecursively(_importDirectory);
            var fileCount = imageFiles.Count();

            logger.LogInformation("Found {Count} existing image file(s) to process", fileCount);

            if (fileCount == 0)
                return;

            // Process files sequentially to avoid overwhelming the system
            foreach (var filePath in imageFiles)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    logger.LogInformation("Processing existing image file: {FilePath}", filePath);
                    await ProcessImageFileAsync(filePath, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process existing file: {FilePath}", filePath);
                    // Continue processing other files even if one fails
                }

                // Small delay between files to avoid overwhelming the system
                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
            }

            logger.LogInformation("Finished processing existing image files");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process existing files");
        }
    }

    /// <summary>
    /// Recursively finds all image files in the import directory.
    /// </summary>
    private IEnumerable<string> GetImageFilesRecursively(string directory)
    {
        var imageFiles = new List<string>();

        try
        {
            // Get all image files in the current directory
            foreach (var extension in ImageExtensions)
            {
                var files = Directory.EnumerateFiles(directory, $"*{extension}", SearchOption.TopDirectoryOnly);
                imageFiles.AddRange(files);
            }

            // Recursively process subdirectories
            var subdirectories = Directory.EnumerateDirectories(directory);
            foreach (var subdirectory in subdirectories)
            {
                try
                {
                    imageFiles.AddRange(GetImageFilesRecursively(subdirectory));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to scan subdirectory: {Path}", subdirectory);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to enumerate files in directory: {Path}", directory);
        }

        return imageFiles;
    }

    /// <summary>
    /// Handles file creation events from FileSystemWatcher.
    /// </summary>
    private async Task OnFileCreated(FileSystemEventArgs e, CancellationToken stoppingToken)
    {
        // Only process files, not directories
        if (Directory.Exists(e.FullPath))
            return;

        var filePath = e.FullPath;
        var fileName = Path.GetFileName(filePath);
        var extension = Path.GetExtension(filePath);

        // Check if it's an image file
        if (!ImageExtensions.Contains(extension))
        {
            logger.LogDebug("Skipping non-image file: {FileName}", fileName);
            return;
        }

        logger.LogInformation("Detected new image file: {FilePath}", filePath);

        // Wait a bit for file to be fully written (in case of large files)
        await Task.Delay(TimeSpan.FromMilliseconds(2000), stoppingToken);

        await ProcessImageFileAsync(filePath, stoppingToken);
    }

    /// <summary>
    /// Processes an image file: validates user, imports image, and deletes file on success.
    /// </summary>
    private async Task ProcessImageFileAsync(string filePath, CancellationToken stoppingToken)
    {
        try
        {
            // Extract user ID from the folder structure
            // Expected structure: import/{userId}/.../file.ext
            var userFolder = ExtractUserIdFromPath(filePath);
            if (string.IsNullOrEmpty(userFolder))
            {
                logger.LogWarning("Could not extract user ID from file path: {FilePath}", filePath);
                return;
            }

            // Create scope for scoped services
            using var scope = scopeFactory.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var imageImportService = scope.ServiceProvider.GetRequiredService<IImageImportService>();
            var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

            // Validate user exists
            var userOption = await userRepository.GetByIdAsync(userFolder);
            if (userOption.IsNone)
            {
                logger.LogWarning("User not found for ID: {UserId}. Skipping file: {FilePath}", userFolder, filePath);
                return;
            }

            var user = userOption.Unwrap();

            // Read image file
            byte[] imageBytes;
            try
            {
                // Retry reading file in case it's still being written
                var retries = 3;
                imageBytes = Array.Empty<byte>();
                for (int i = 0; i < retries; i++)
                {
                    try
                    {
                        imageBytes = await File.ReadAllBytesAsync(filePath, stoppingToken);
                        break;
                    }
                    catch (IOException) when (i < retries - 1)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000 * (i + 1)), stoppingToken);
                    }
                }

                if (imageBytes.Length == 0)
                {
                    logger.LogError("Failed to read image file after {Retries} retries: {FilePath}", retries, filePath);
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to read image file: {FilePath}", filePath);
                return;
            }

            // Import the image within a transaction (handles SaveChangesAsync automatically)
            var importResult = await transactionService.UseTransactionAsync(async () =>
                await imageImportService.ImportImage(
                    imageBytes,
                    user.DefaultPublicity,
                    user.Id));

            if (importResult.IsOk)
            {
                // Delete file after successful import
                try
                {
                    File.Delete(filePath);
                    logger.LogInformation(
                        "Successfully imported and deleted image: {FilePath} for user {UserId}",
                        filePath,
                        user.Id);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to delete file after successful import: {FilePath}", filePath);
                }
            }
            else
            {
                var error = importResult.UnwrapError();
                logger.LogError(
                    "Failed to import image {FilePath} for user {UserId}: {Error}",
                    filePath,
                    user.Id,
                    error);
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            logger.LogError(ex, "Error processing image file: {FilePath}", filePath);
        }
    }

    /// <summary>
    /// Extracts user ID from file path by finding the first subfolder under the import directory.
    /// </summary>
    private string? ExtractUserIdFromPath(string filePath)
    {
        try
        {
            var normalizedImportDir = Path.GetFullPath(_importDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var normalizedFilePath = Path.GetFullPath(filePath);

            if (!normalizedFilePath.StartsWith(normalizedImportDir, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var relativePath = normalizedFilePath.Substring(normalizedImportDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // The first part after the import directory should be the user ID
            if (pathParts.Length > 0 && !string.IsNullOrWhiteSpace(pathParts[0]))
            {
                return pathParts[0];
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to extract user ID from path: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// Handles FileSystemWatcher errors.
    /// </summary>
    private void OnFileWatcherError(object sender, ErrorEventArgs e)
    {
        logger.LogError(e.GetException(), "FileSystemWatcher error occurred");
    }

    public override void Dispose()
    {
        _fileWatcher?.Dispose();
        base.Dispose();
    }
}
