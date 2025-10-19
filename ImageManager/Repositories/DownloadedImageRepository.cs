#region Usings
using System;
using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;
#endregion

namespace ImageManager.Repositories;

/// <summary>
/// Repository for <see cref="DownloadedImage"/> entities.
/// Provides a lightweight existence check used by higher‑level services.
/// </summary>
public class DownloadedImageRepository(ApplicationDbContext dbContext)
    : EfRepository<DownloadedImage, Guid>(dbContext), IDownloadedImageRepository
{
    #region Public Methods

    /// <summary>
    /// Determines whether an image with the specified <paramref name="imageId"/> has been downloaded.
    /// </summary>
    /// <param name="imageId">The unique identifier of the image.</param>
    /// <returns><c>true</c> if a matching record exists; otherwise <c>false</c>.</returns>
    public async Task<bool> ExistsAsync(Guid imageId)
    {
        return await dbContext.DownloadedImages.AnyAsync(di => di.ImageId == imageId);
    }

    #endregion
}