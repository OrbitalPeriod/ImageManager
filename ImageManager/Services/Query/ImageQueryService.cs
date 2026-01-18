using ImageManager.Controllers;
using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Repositories;
using ImageManager.Repositories.Repository_Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Services.Query;

/// <summary>
/// EF Core implementation of <see cref="IImageQueryService"/>.
/// Uses an <see cref="IUserOwnedImageRepository"/> to obtain the
/// set of images that a user (or anonymous) can view and then applies
/// pagination or filtering as requested by the caller.
/// </summary>
public class ImageQueryService(IUserOwnedImageRepository userOwnedImageRepository) : IImageQueryService
{
    /// <inheritdoc />
    public async Task<Result<PaginatedResponse<ImageController.GetImagesResponse>, ImageError>> GetImagesAsync(
        User? user,
        Guid? token,
        int page,
        int pageSize)
    {
        // Normalise pagination parameters.
        if (page < 1 || pageSize < 1 || pageSize > 200)
            return Result<PaginatedResponse<ImageController.GetImagesResponse>, ImageError>.Err(ImageError.InvalidPagination);

        // Base query: distinct images that the caller can access.
        var baseQuery = userOwnedImageRepository.AccessibleImages(user, token)
            .Select(uoi => uoi.Image)
            .Distinct();

        var totalCount = await baseQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var images = await baseQuery
            .OrderByDescending(i => i.StoredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        var imageData = images
            .Select(i => new ImageController.GetImagesResponse(i.Id, i.AgeRating, i.StoredAt))
            .ToArray();

        return Result<PaginatedResponse<ImageController.GetImagesResponse>, ImageError>.Ok(new PaginatedResponse<ImageController.GetImagesResponse>
        {
            Data = imageData,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        });
    }

    /// <inheritdoc />
    public async Task<Result<PaginatedResponse<ImageController.GetSearchImagesResponse>, ImageError>> SearchImagesAsync(
        User? user,
        ImageController.GetSearchImagesRequest request,
        Guid? token,
        int page,
        int pageSize)
    {
        // Normalise pagination parameters.
        if (page < 1 || pageSize < 1 || pageSize > 200)
            return Result<PaginatedResponse<ImageController.GetSearchImagesResponse>, ImageError>.Err(ImageError.InvalidPagination);

        // Start from all images the caller can access, using the provided token if available.
        var query = userOwnedImageRepository.AccessibleImages(user, token).AsQueryable();

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
            .OrderByDescending(i => i.Image.StoredAt)
            .Include(i => i.Image)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        var images = imagesData
            .Select(i => new ImageController.GetSearchImagesResponse(i.ImageId, i.Image.AgeRating, i.Image.StoredAt))
            .ToArray();

        return Result<PaginatedResponse<ImageController.GetSearchImagesResponse>, ImageError>.Ok(new PaginatedResponse<ImageController.GetSearchImagesResponse>
        {
            Data = images,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        });
    }
}
