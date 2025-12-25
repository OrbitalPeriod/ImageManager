using ImageManager.Data;
using ImageManager.Data.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Services.Query;

/// <summary>
/// EF Core implementation of <see cref="IDeleteImageService"/>.
/// Uses the application's <see cref="ApplicationDbContext"/> to locate and delete
/// a <c>UserOwnedImage</c> record that matches the supplied identifiers.
/// </summary>
public class DeleteImageService(ApplicationDbContext dbContext) : IDeleteImageService
{
    public async Task<Result<Unit, DeleteError>> DeleteAsync(Guid imageId, string userId)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));

        var uoi = await dbContext.UserOwnedImages
            .FirstOrDefaultAsync(u => u.ImageId == imageId);

        if (uoi == null) return Result<Unit, DeleteError>.Err(DeleteError.NotFound);

        if (uoi.UserId != userId) return Result<Unit, DeleteError>.Err(DeleteError.Forbidden);

        dbContext.UserOwnedImages.Remove(uoi);
        await dbContext.SaveChangesAsync();

        return Result<Unit, DeleteError>.Ok(Unit.New());
    }
}
