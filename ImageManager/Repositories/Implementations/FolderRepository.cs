#region Usings

using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Repositories.Abstract_Interfaces;
using ImageManager.Repositories.Repository_Interfaces;
using Microsoft.EntityFrameworkCore;

#endregion

namespace ImageManager.Repositories.Implementations;

/// <summary>
/// Repository for <see cref="Folder"/> entities.
/// Provides user-scoped queries for security.
/// </summary>
public class FolderRepository(ApplicationDbContext dbContext)
    : EfRepository<Folder, Guid>(dbContext), IFolderRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Folder>> GetUserFoldersAsync(string userId)
    {
        return await DbContext.Folders
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Folder?> GetFolderWithImagesAsync(Guid folderId, string userId)
    {
        return await DbContext.Folders
            .Where(f => f.Id == folderId && f.UserId == userId)
            .Include(f => f.FolderImages)
                .ThenInclude(fi => fi.UserOwnedImage)
                    .ThenInclude(uoi => uoi.Image)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<bool> UserOwnsFolderAsync(string userId, Guid folderId)
    {
        return await DbContext.Folders
            .AnyAsync(f => f.Id == folderId && f.UserId == userId);
    }
}
