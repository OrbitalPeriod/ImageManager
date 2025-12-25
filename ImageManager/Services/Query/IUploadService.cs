// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Services.ImageImport;

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
    Task<Result<ImportImageSuccess, ImportImageError>> UploadAsync(IFormFile file, Publicity? publicity, User user);
}

#endregion
