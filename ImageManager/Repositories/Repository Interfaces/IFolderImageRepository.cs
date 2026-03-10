// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Repositories.Abstract_Interfaces;

namespace ImageManager.Repositories.Repository_Interfaces;

/// <summary>
/// Repository interface for folder-image relationships.
/// </summary>
public interface IFolderImageRepository : IRepository<FolderImage, Guid>,
    IReadableRepository<FolderImage, Guid>,
    IAddableEntityRepository<FolderImage, Guid>,
    IDeleteableRepository<FolderImage, Guid>
{
    /// <summary>
    /// Gets UserOwnedImages in a folder with pagination, ensuring user ownership of the folder.
    /// </summary>
    Task<PaginatedResponse<UserOwnedImage>> GetImagesInFolderAsync(Guid folderId, string userId, int page, int pageSize);

    /// <summary>
    /// Checks if a UserOwnedImage exists in a folder.
    /// </summary>
    Task<bool> ImageExistsInFolderAsync(Guid folderId, Guid userOwnedImageId);

    /// <summary>
    /// Removes a UserOwnedImage from a folder.
    /// </summary>
    Task<bool> RemoveImageFromFolderAsync(Guid folderId, Guid userOwnedImageId);
}
