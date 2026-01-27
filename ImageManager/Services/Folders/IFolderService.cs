using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;

namespace ImageManager.Services.Folders;

/// <summary>
/// Service contract for managing user folders and folder-image relationships.
/// </summary>
public interface IFolderService
{
    /// <summary>
    /// Creates a new folder for the user.
    /// </summary>
    Task<Result<Folder, FolderError>> CreateFolderAsync(string userId, string name);

    /// <summary>
    /// Deletes a folder (except "Liked" folder).
    /// </summary>
    Task<Result<Unit, FolderError>> DeleteFolderAsync(string userId, Guid folderId);

    /// <summary>
    /// Gets all folders for a user.
    /// </summary>
    Task<Result<ICollection<FolderDto>, FolderError>> GetUserFoldersAsync(string userId);

    /// <summary>
    /// Adds an image to a folder. Accepts imageId for UX, internally resolves to userOwnedImageId.
    /// </summary>
    Task<Result<Unit, FolderError>> AddImageToFolderAsync(string userId, Guid folderId, Guid imageId);

    /// <summary>
    /// Removes an image from a folder. Accepts imageId for UX, internally resolves to userOwnedImageId.
    /// </summary>
    Task<Result<Unit, FolderError>> RemoveImageFromFolderAsync(string userId, Guid folderId, Guid imageId);

    /// <summary>
    /// Gets paginated list of images in a folder.
    /// </summary>
    Task<Result<PaginatedResponse<ImageDto>, FolderError>> GetFolderImagesAsync(string userId, Guid folderId, int page, int pageSize);

    /// <summary>
    /// Ensures the "Liked" folder exists for a user. Creates it if it doesn't exist.
    /// </summary>
    Task EnsureLikedFolderExistsAsync(string userId);
}

/// <summary>
/// DTO for folder information.
/// </summary>
public record FolderDto(Guid Id, string Name);

/// <summary>
/// DTO for image information in folder responses.
/// </summary>
public record ImageDto(Guid Id, AgeRating Rating, DateTime StoredAt);
