// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Controllers;
using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;

namespace ImageManager.Services.Query;

#region Interface

/// <summary>
/// Contract for querying image collections, supporting pagination and filtering.
/// </summary>
public interface IImageQueryService
{
    /// <summary>
    /// Retrieves a page of images that the caller can access.
    /// The returned data contains only the image Id and age rating.
    /// </summary>
    Task<Result<PaginatedResponse<ImageController.GetImagesResponse>, ImageError>> GetImagesAsync(
        User? user,
        Guid? token,
        int page,
        int pageSize);

    /// <summary>
    /// Searches for images that match the supplied filter criteria.
    /// The returned data contains only the image Id and age rating.
    /// </summary>
    Task<Result<PaginatedResponse<ImageController.GetSearchImagesResponse>, ImageError>> SearchImagesAsync(
        User? user,
        ImageController.GetSearchImagesRequest request,
        int page,
        int pageSize);
}

#endregion
