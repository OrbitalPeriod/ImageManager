using ImageManager.Data;
using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Services.Query;

/// <summary>
/// EF Core implementation of <see cref="IDeleteImageService"/>.
/// Uses the application's <see cref="ApplicationDbContext"/> to locate and delete
/// a <c>UserOwnedImage</c> record that matches the supplied identifiers.
/// Administrators can delete any image regardless of ownership.
/// </summary>
public class DeleteImageService(
    ApplicationDbContext dbContext,
    ITransactionService transactionService,
    UserManager<User> userManager) : IDeleteImageService
{
    private const string AdministratorRoleName = "Administrator";

    public async Task<Result<Unit, DeleteError>> DeleteAsync(Guid imageId, User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        return await transactionService.UseTransactionAsync(async () =>
        {
            var uoi = await dbContext.UserOwnedImages
                .FirstOrDefaultAsync(u => u.ImageId == imageId);

            if (uoi == null) return Result<Unit, DeleteError>.Err(DeleteError.NotFound);

            // Check if user is an administrator
            var isAdmin = await userManager.IsInRoleAsync(user, AdministratorRoleName);

            // Allow deletion if user owns the image OR if user is an administrator
            if (uoi.UserId != user.Id && !isAdmin)
                return Result<Unit, DeleteError>.Err(DeleteError.Forbidden);

            dbContext.UserOwnedImages.Remove(uoi);
            return Result<Unit, DeleteError>.Ok(Unit.New());
        });
    }
}
