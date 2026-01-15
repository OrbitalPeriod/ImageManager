#region Usings

using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Repositories;
using ImageManager.Repositories.Repository_Interfaces;
using Microsoft.EntityFrameworkCore;
#endregion

namespace ImageManager.Services.Tags;

/// <summary>
/// Service responsible for retrieving tag statistics and performing searches over tags.
/// It uses the <see cref="IUserOwnedImageRepository"/> to determine which images
/// a caller can access, then aggregates the tags of those images.
/// </summary>
public class TagService(IUserOwnedImageRepository userOwnedImageRepository) : ITagService
{
    /// <inheritdoc />
    public async Task<Result<PaginatedResponse<TagCountDto>, TagError>> GetTagsAsync(
        User? user,
        Guid? token,
        int page,
        int pageSize)
    {
        if (page < 1 || pageSize <= 0)
            return Result<PaginatedResponse<TagCountDto>, TagError>.Err(TagError.InvalidPagination);

        var baseQuery = userOwnedImageRepository.AccessibleImages(user, token);

        // First, get the grouped tag data
        var groupedTags = await baseQuery
            .SelectMany(uoi => uoi.Image.Tags)
            .GroupBy(t => new { t.Id, t.Name })
            .Select(g => new
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Count = g.Count()
            })
            .ToListAsync();

        // Then group by name only to merge duplicates (similar to characters fix)
        var mergedTags = groupedTags
            .GroupBy(t => t.Name)
            .Select(g => new TagCountDto(
                g.OrderBy(t => t.Id).First().Id,
                g.Key,
                g.Sum(t => t.Count)))
            .OrderByDescending(x => x.Count)
            .ToList();

        // Total count for paging metadata (count unique tags by name)
        var totalCount = mergedTags.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var tags = mergedTags
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        return Result<PaginatedResponse<TagCountDto>, TagError>.Ok(new PaginatedResponse<TagCountDto>
        {
            Data = tags,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        });
    }

    /// <inheritdoc />
    public async Task<Result<PaginatedResponse<TagCountDto>, TagError>> SearchTagsAsync(
        User? user,
        string q,
        Guid? token,
        int page,
        int pageSize)
    {
        if (page < 1 || pageSize <= 0)
            return Result<PaginatedResponse<TagCountDto>, TagError>.Err(TagError.InvalidPagination);

        var baseQuery = userOwnedImageRepository.AccessibleImages(user, token);

        var tagsQuery = baseQuery.SelectMany(uoi => uoi.Image.Tags);
        if (!string.IsNullOrWhiteSpace(q))
        {
            // Ensure case‑insensitive matching by normalising both sides to lower case.
            var pattern = $"%{q.ToLowerInvariant()}%";
            tagsQuery = tagsQuery.Where(t => EF.Functions.Like(t.Name.ToLower(), pattern));
        }

        // First, get the grouped tag data
        var groupedTags = await tagsQuery
            .GroupBy(t => new { t.Id, t.Name })
            .Select(g => new
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Count = g.Count()
            })
            .ToListAsync();

        // Then group by name only to merge duplicates (similar to characters fix)
        var mergedTags = groupedTags
            .GroupBy(t => t.Name)
            .Select(g => new TagCountDto(
                g.OrderBy(t => t.Id).First().Id,
                g.Key,
                g.Sum(t => t.Count)))
            .OrderByDescending(x => x.Count)
            .ToList();

        var totalCount = mergedTags.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var tags = mergedTags
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        return Result<PaginatedResponse<TagCountDto>, TagError>.Ok(new PaginatedResponse<TagCountDto>
        {
            Data = tags,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        });
    }
}
