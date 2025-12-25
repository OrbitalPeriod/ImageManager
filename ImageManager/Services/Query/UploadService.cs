using ImageManager.Data;
using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Services.ImageImport;


namespace ImageManager.Services.Query;

/// <summary>
/// EF Core implementation that imports an uploaded image using
/// <see cref="IImageImportService"/> and persists the result in a transaction.
/// </summary>
public class UploadImageService(
    IImageImportService imageImportService,
    ITransactionService transactionService) : IUploadImageService
{
    /// <inheritdoc />
    public async Task<Result<ImportImageSuccess, ImportImageError>> UploadAsync(IFormFile file, Publicity? publicity, User user)
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

            return await imageImportService.ImportImage(
                imageBytes,
                resolvedPublicity,
                user.Id);
        });
    }
}
