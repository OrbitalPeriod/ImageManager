// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Data.Helpers;
using ImageManager.Data.Models;

namespace ImageManager.Services.ImageImport;

#region DTOs

public enum ImportImageError
{
    EmptyImage,
    FailedToGetTags,
    ImageParseFailed,
    ImageStoreError,
    AlreadyOwned,
    QueuedForProcessing,
}

public class ImportImageSuccess
{
    public required Guid Id { get; init; }
    public required bool NewFile { get; init; }
    public required bool AlreadyOwned { get; init; }
}



#endregion

#region Interface

/// <summary>
/// Service that imports an image, tags it and records ownership/download status.
/// </summary>
public interface IImageImportService
{
    /// <summary>
    /// Imports the given image bytes, generates tags, calculates a hash,
    /// stores the file (if new), creates or re‑uses an <see cref="Image"/> entity,
    /// and ensures that the specified user owns the image.
    /// </summary>
    /// <param name="imageBytes">Raw PNG/JPEG/GIF data.</param>
    /// <param name="publicity">The default publicity level for the new image.</param>
    /// <param name="userId">Identifier of the user performing the import.</param>
    /// <returns>The GUID of the resulting <see cref="Image"/> or <c>null</c> if the import failed.</returns>
    Task<Result<ImportImageSuccess, ImportImageError>> ImportImage(byte[] imageBytes, Publicity publicity, string userId);
}

#endregion
