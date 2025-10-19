#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageManager.Controllers;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Repositories;
using Microsoft.EntityFrameworkCore;
#endregion

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
    Task<PaginatedResponse<ImageController.GetImagesResponse>> GetImagesAsync(
        User? user,
        Guid? token,
        int page,
        int pageSize);

    /// <summary>
    /// Searches for images that match the supplied filter criteria.
    /// The returned data contains only the image Id and age rating.
    /// </summary>
    Task<PaginatedResponse<ImageController.GetSearchImagesResponse>> SearchImagesAsync(
        User? user,
        ImageController.GetSearchImagesRequest request,
        int page,
        int pageSize);
}

#endregion

#region Implementation

/// <summary>
/// EF Core implementation of <see cref="IImageQueryService"/>.
/// Uses an <see cref="IUserOwnedImageRepository"/> to obtain the
/// set of images that a user (or anonymous) can view and then applies
/// pagination or filtering as requested by the caller.
/// </summary>
public class ImageQueryService(IUserOwnedImageRepository userOwnedImageRepository) : IImageQueryService
{
    /// <inheritdoc />
    public async Task<PaginatedResponse<ImageController.GetImagesResponse>> GetImagesAsync(
        User? user,
        Guid? token,
        int page,
        int pageSize)
    {
        // Normalise pagination parameters.
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 200;

        // Base query: distinct images that the caller can access.
        var baseQuery = userOwnedImageRepository.AccessibleImages(user, token)
            .Select(uoi => uoi.Image)
            .Distinct();

        var totalCount = await baseQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var images = await baseQuery
            .OrderByDescending(i => i.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        var imageData = images
            .Select(i => new ImageController.GetImagesResponse(i.Id, i.AgeRating))
            .ToArray();

        return new PaginatedResponse<ImageController.GetImagesResponse>
        {
            Data       = imageData,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        };
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<ImageController.GetSearchImagesResponse>> SearchImagesAsync(
        User? user,
        ImageController.GetSearchImagesRequest request,
        int page,
        int pageSize)
    {
        // Normalise pagination parameters.
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 200;

        // Start from all images the caller can access; we do not expose a token filter here
        // because SearchImagesAsync already receives a dedicated request that may contain its own filters.
        var query = userOwnedImageRepository.AccessibleImages(user, null).AsQueryable();

        // Apply tag filters if any.
        if (request.Tags != null && request.Tags.Any())
            query = query.Where(i => i.Image.Tags.Any(t => request.Tags.Contains(t.Name)));

        // Apply character filters if any.
        if (request.Characters != null && request.Characters.Any())
            query = query.Where(i => i.Image.Characters.Any(c => request.Characters.Contains(c.Name)));

        // Apply age‑rating filter if any.
        if (request.Rating != null && request.Rating.Any())
            query = query.Where(i => request.Rating.Contains(i.Image.AgeRating));

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Retrieve the requested page of data, including the related Image entity so we can read AgeRating.
        var imagesData = await query
            .OrderByDescending(i => i.Id)
            .Include(i => i.Image)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        var images = imagesData
            .Select(i => new ImageController.GetSearchImagesResponse(i.ImageId, i.Image.AgeRating))
            .ToArray();

        return new PaginatedResponse<ImageController.GetSearchImagesResponse>
        {
            Data       = images,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        };
    }
}

#endregion
