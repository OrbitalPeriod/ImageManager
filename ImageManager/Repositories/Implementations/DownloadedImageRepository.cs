#region Usings

using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Repositories.Abstract_Interfaces;

#endregion

namespace ImageManager.Repositories.Implementations;

/// <summary>
/// Repository for <see cref="DownloadedImage"/> entities.
/// Provides a lightweight existence check used by higher‑level services.
/// </summary>
public class DownloadedImageRepository(ApplicationDbContext dbContext)
    : EfRepository<DownloadedImage, Guid>(dbContext), Repository_Interfaces.IDownloadedImageRepository
{
}
