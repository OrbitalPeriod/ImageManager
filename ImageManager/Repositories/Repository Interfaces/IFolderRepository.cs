// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Data.Models;
using ImageManager.Repositories.Abstract_Interfaces;

namespace ImageManager.Repositories.Repository_Interfaces;

/// <summary>
/// Repository interface for folders with user-specific queries.
/// </summary>
public interface IFolderRepository : IRepository<Folder, Guid>,
    IReadableRepository<Folder, Guid>,
    IAddableEntityRepository<Folder, Guid>,
    IUpdateableRepository<Folder, Guid>,
    IDeleteableRepository<Folder, Guid>
{
    /// <summary>
    /// Gets all folders for a specific user.
    /// </summary>
    Task<IReadOnlyCollection<Folder>> GetUserFoldersAsync(string userId);

    /// <summary>
    /// Gets a folder with its images, ensuring user ownership.
    /// </summary>
    Task<Folder?> GetFolderWithImagesAsync(Guid folderId, string userId);

    /// <summary>
    /// Checks if a user owns a folder.
    /// </summary>
    Task<bool> UserOwnsFolderAsync(string userId, Guid folderId);
}
