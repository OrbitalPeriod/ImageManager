// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Controllers;
using ImageManager.Data.Helpers;
using ImageManager.Data.Models;

namespace ImageManager.Services.Query;

#region Interface

/// <summary>
/// Contract for retrieving detailed information about an image,
/// including access checks and the full data payload.
/// </summary>
public interface IImageDetailService
{
    /// <summary>
    /// Determines whether a user (or anonymous) can view the specified image.
    /// Returns the image entity if found and accessible.
    /// </summary>
    Task<Result<Image, ImageError>> GetImageAccessAsync(Guid imageId, User? user, Guid? token);

    /// <summary>
    /// Retrieves the data payload for an image (tags, characters, age rating, owners)
    /// if the caller has permission.
    /// </summary>
    Task<Result<ImageController.ImageDataResponse, ImageError>> GetImageDataAccessAsync(Guid imageId, User? user, Guid? token);
}

#endregion
