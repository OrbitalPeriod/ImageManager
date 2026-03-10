#region Usings

using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Repositories.Repository_Interfaces;
using ImageManager.Services;
using Microsoft.EntityFrameworkCore;

#endregion

namespace ImageManager.Services.Folders;

/// <summary>
/// Service for managing user folders and folder-image relationships.
/// </summary>
public class FolderService(
    IFolderRepository folderRepository,
    IFolderImageRepository folderImageRepository,
    IUserOwnedImageRepository userOwnedImageRepository,
    ITransactionService transactionService) : IFolderService
{
    private const string LikedFolderName = "Liked";

    /// <inheritdoc />
    public async Task<Result<Folder, FolderError>> CreateFolderAsync(string userId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Folder, FolderError>.Err(FolderError.NotFound);

        return await transactionService.UseTransactionAsync(async () =>
        {
            // Check if folder with same name already exists for this user
            var existingFolders = await folderRepository.GetUserFoldersAsync(userId);
            if (existingFolders.Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return Result<Folder, FolderError>.Err(FolderError.AlreadyExists);
            }

            var folder = new Folder
            {
                Name = name.Trim(),
                UserId = userId
            };

            await folderRepository.AddAsync(folder);
            return Result<Folder, FolderError>.Ok(folder);
        });
    }

    /// <inheritdoc />
    public async Task<Result<Unit, FolderError>> DeleteFolderAsync(string userId, Guid folderId)
    {
        return await transactionService.UseTransactionAsync(async () =>
        {
            var folderOption = await folderRepository.GetByIdAsync(folderId);
            if (folderOption.IsNone)
                return Result<Unit, FolderError>.Err(FolderError.NotFound);

            var folder = folderOption.Unwrap();
            
            // Check ownership
            if (folder.UserId != userId)
                return Result<Unit, FolderError>.Err(FolderError.Forbidden);

            // Prevent deletion of "Liked" folder
            if (folder.Name.Equals(LikedFolderName, StringComparison.OrdinalIgnoreCase))
                return Result<Unit, FolderError>.Err(FolderError.CannotDeleteLiked);

            folderRepository.Delete(folder);
            return Result<Unit, FolderError>.Ok(Unit.New());
        });
    }

    /// <inheritdoc />
    public async Task<Result<ICollection<FolderDto>, FolderError>> GetUserFoldersAsync(string userId)
    {
        // Ensure "Liked" folder exists (lazy initialization)
        await EnsureLikedFolderExistsAsync(userId);

        var folders = await folderRepository.GetUserFoldersAsync(userId);
        var folderDtos = folders.Select(f => new FolderDto(f.Id, f.Name)).ToList();

        return Result<ICollection<FolderDto>, FolderError>.Ok(folderDtos);
    }

    /// <inheritdoc />
    public async Task<Result<Unit, FolderError>> AddImageToFolderAsync(string userId, Guid folderId, Guid imageId)
    {
        return await transactionService.UseTransactionAsync(async () =>
        {
            // Verify folder ownership
            var ownsFolder = await folderRepository.UserOwnsFolderAsync(userId, folderId);
            if (!ownsFolder)
                return Result<Unit, FolderError>.Err(FolderError.Forbidden);

            // Resolve imageId to userOwnedImageId
            var userOwnedImage = await userOwnedImageRepository.AccessibleImages(userId, null)
                .FirstOrDefaultAsync(uoi => uoi.ImageId == imageId);

            if (userOwnedImage == null)
                return Result<Unit, FolderError>.Err(FolderError.NotFound);

            // Check if already in folder
            var exists = await folderImageRepository.ImageExistsInFolderAsync(folderId, userOwnedImage.Id);
            if (exists)
                return Result<Unit, FolderError>.Err(FolderError.AlreadyExists);

            var folderImage = new FolderImage
            {
                FolderId = folderId,
                UserOwnedImageId = userOwnedImage.Id
            };

            await folderImageRepository.AddAsync(folderImage);
            return Result<Unit, FolderError>.Ok(Unit.New());
        });
    }

    /// <inheritdoc />
    public async Task<Result<Unit, FolderError>> RemoveImageFromFolderAsync(string userId, Guid folderId, Guid imageId)
    {
        return await transactionService.UseTransactionAsync(async () =>
        {
            // Verify folder ownership
            var ownsFolder = await folderRepository.UserOwnsFolderAsync(userId, folderId);
            if (!ownsFolder)
                return Result<Unit, FolderError>.Err(FolderError.Forbidden);

            // Resolve imageId to userOwnedImageId
            var userOwnedImage = await userOwnedImageRepository.AccessibleImages(userId, null)
                .FirstOrDefaultAsync(uoi => uoi.ImageId == imageId);

            if (userOwnedImage == null)
                return Result<Unit, FolderError>.Err(FolderError.NotFound);

            var removed = await folderImageRepository.RemoveImageFromFolderAsync(folderId, userOwnedImage.Id);
            if (!removed)
                return Result<Unit, FolderError>.Err(FolderError.NotFound);

            return Result<Unit, FolderError>.Ok(Unit.New());
        });
    }

    /// <inheritdoc />
    public async Task<Result<PaginatedResponse<ImageDto>, FolderError>> GetFolderImagesAsync(string userId, Guid folderId, int page, int pageSize)
    {
        if (page < 1 || pageSize < 1 || pageSize > 200)
            return Result<PaginatedResponse<ImageDto>, FolderError>.Err(FolderError.InvalidPagination);

        // Verify folder ownership
        var ownsFolder = await folderRepository.UserOwnsFolderAsync(userId, folderId);
        if (!ownsFolder)
            return Result<PaginatedResponse<ImageDto>, FolderError>.Err(FolderError.Forbidden);

        var paginatedResult = await folderImageRepository.GetImagesInFolderAsync(folderId, userId, page, pageSize);
        
        var imageDtos = paginatedResult.Data
            .Select(uoi => new ImageDto(
                uoi.ImageId, 
                uoi.Image?.AgeRating ?? AgeRating.General, 
                uoi.Image?.StoredAt ?? DateTime.UtcNow))
            .ToArray();

        return Result<PaginatedResponse<ImageDto>, FolderError>.Ok(new PaginatedResponse<ImageDto>
        {
            Data = imageDtos,
            Page = paginatedResult.Page,
            PageSize = paginatedResult.PageSize,
            TotalPages = paginatedResult.TotalPages,
            TotalItems = paginatedResult.TotalItems
        });
    }

    /// <inheritdoc />
    public async Task EnsureLikedFolderExistsAsync(string userId)
    {
        var folders = await folderRepository.GetUserFoldersAsync(userId);
        var likedFolder = folders.FirstOrDefault(f => f.Name.Equals(LikedFolderName, StringComparison.OrdinalIgnoreCase));

        if (likedFolder == null)
        {
            var folder = new Folder
            {
                Name = LikedFolderName,
                UserId = userId
            };

            await folderRepository.AddAsync(folder);
            await transactionService.SaveChangesAsync();
        }
    }
}
