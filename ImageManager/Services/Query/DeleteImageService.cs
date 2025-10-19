#region Usings
using System;
using System.Threading.Tasks;
using ImageManager.Data;
using Microsoft.EntityFrameworkCore;
#endregion

namespace ImageManager.Services.Query;

#region Enums

/// <summary>
/// Result codes returned by the delete operation.
/// </summary>
public enum DeleteResult { NotFound, Forbidden, Deleted }

#endregion

#region Interface

/// <summary>
/// Contract for deleting a user‑owned image.
/// The service checks that the image exists and that the caller owns it
/// before performing the deletion.
/// </summary>
public interface IDeleteImageService
{
    /// <summary>
    /// Deletes the image identified by <paramref name="imageId"/> if it belongs to the supplied <paramref name="userId"/>.
    /// </summary>
    Task<DeleteResult> DeleteAsync(Guid imageId, string userId);
}

#endregion

#region Implementation

/// <summary>
/// EF Core implementation of <see cref="IDeleteImageService"/>.
/// Uses the application's <see cref="ApplicationDbContext"/> to locate and delete
/// a <c>UserOwnedImage</c> record that matches the supplied identifiers.
/// </summary>
public class DeleteImageService(ApplicationDbContext dbContext) : IDeleteImageService
{
    public async Task<DeleteResult> DeleteAsync(Guid imageId, string userId)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        
        var uoi = await dbContext.UserOwnedImages
            .FirstOrDefaultAsync(u => u.ImageId == imageId);
        
        if (uoi == null) return DeleteResult.NotFound;
        
        if (uoi.UserId != userId) return DeleteResult.Forbidden;
        
        dbContext.UserOwnedImages.Remove(uoi);
        await dbContext.SaveChangesAsync();

        return DeleteResult.Deleted;
    }
}

#endregion
