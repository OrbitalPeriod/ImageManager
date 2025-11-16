#region Usings

using ImageManager.Data;
using ImageManager.Data.Models;
#endregion

namespace ImageManager.Services.Query;

#region Interface

/// <summary>
/// Contract for uploading images to the system.
/// The image is processed by an <see cref="IImageImportService"/> and persisted
/// inside a database transaction.
/// </summary>
public interface IUploadImageService
{
    /// <summary>
    /// Uploads the supplied file, imports it via <paramref name="imageImportService"/>,
    /// and stores it under the given user’s ownership.
    /// </summary>
    Task<Guid?> UploadAsync(IFormFile file, Publicity? publicity, User user);
}

#endregion

#region Implementation

/// <summary>
/// EF Core implementation that imports an uploaded image using
/// <see cref="IImageImportService"/> and persists the result in a transaction.
/// </summary>
public class UploadImageService(
    IImageImportService imageImportService,
    ITransactionService transactionService) : IUploadImageService
{
    /// <inheritdoc />
    public async Task<Guid?> UploadAsync(IFormFile file, Publicity? publicity, User user)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));
        if (user == null) throw new ArgumentNullException(nameof(user));
        
        return await transactionService.UseTransactionAsync(async () =>
        {
            byte[] imageBytes;
            await using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            // Resolve the publicity level – fall back to the user’s default if not supplied.
            var resolvedPublicity = publicity ?? user.DefaultPublicity;

            // Delegate the actual import logic to the dedicated service.
            
            var imageId = await imageImportService.ImportImage(
                imageBytes,
                resolvedPublicity,
                user.Id);
            
            if (imageId != null) return imageId;
            
            throw new Exception("Import failed");
            return null;

        });
    }
}

#endregion
