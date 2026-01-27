#region Usings

using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Repositories.Abstract_Interfaces;
using ImageManager.Repositories.Repository_Interfaces;
using Microsoft.EntityFrameworkCore;

#endregion

namespace ImageManager.Repositories.Implementations;

/// <summary>
/// Repository for <see cref="FolderImage"/> entities.
/// Provides methods for managing images in folders.
/// </summary>
public class FolderImageRepository(ApplicationDbContext dbContext)
    : EfRepository<FolderImage, Guid>(dbContext), IFolderImageRepository
{
    /// <inheritdoc />
    public async Task<PaginatedResponse<UserOwnedImage>> GetImagesInFolderAsync(Guid folderId, string userId, int page, int pageSize)
    {
        // First verify the folder belongs to the user
        var folderExists = await DbContext.Folders
            .AnyAsync(f => f.Id == folderId && f.UserId == userId);

        if (!folderExists)
        {
            return new PaginatedResponse<UserOwnedImage>
            {
                Data = [],
                Page = page,
                PageSize = pageSize,
                TotalPages = 0,
                TotalItems = 0
            };
        }

        var query = DbContext.FolderImages
            .Where(fi => fi.FolderId == folderId)
            .Include(fi => fi.UserOwnedImage)
                .ThenInclude(uoi => uoi.Image)
            .Select(fi => fi.UserOwnedImage)
            .OrderByDescending(uoi => uoi.Image.StoredAt);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var images = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToArrayAsync();

        return new PaginatedResponse<UserOwnedImage>
        {
            Data = images,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        };
    }

    /// <inheritdoc />
    public async Task<bool> ImageExistsInFolderAsync(Guid folderId, Guid userOwnedImageId)
    {
        return await DbContext.FolderImages
            .AnyAsync(fi => fi.FolderId == folderId && fi.UserOwnedImageId == userOwnedImageId);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveImageFromFolderAsync(Guid folderId, Guid userOwnedImageId)
    {
        var folderImage = await DbContext.FolderImages
            .FirstOrDefaultAsync(fi => fi.FolderId == folderId && fi.UserOwnedImageId == userOwnedImageId);

        if (folderImage == null)
            return false;

        DbContext.FolderImages.Remove(folderImage);
        return true;
    }
}
