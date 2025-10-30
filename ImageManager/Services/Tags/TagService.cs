#region Usings

using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Repositories;
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
    #region GetTagsAsync

    /// <inheritdoc />
    public async Task<PaginatedResponse<TagCountDto>> GetTagsAsync(
        User? user,
        Guid? token,
        int page,
        int pageSize)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "Page must be >= 1");
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be > 0");

        var baseQuery = userOwnedImageRepository.AccessibleImages(user, token);

        var totalCount = await baseQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var tags = await baseQuery
            .SelectMany(uoi => uoi.Image.Tags)
            .GroupBy(t => new { t.Id, t.Name })
            .Select(g => new TagCountDto(
                g.Key.Id,
                g.Key.Name,
                g.Count()))
            .OrderByDescending(x => x.Count)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<TagCountDto>
        {
            Data = tags.ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        };
    }

    #endregion

    #region SearchTagsAsync

    /// <inheritdoc />
    public async Task<PaginatedResponse<TagCountDto>> SearchTagsAsync(
        User? user,
        string q,
        Guid? token,
        int page,
        int pageSize)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "Page must be >= 1");
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be > 0");

        var baseQuery = userOwnedImageRepository.AccessibleImages(user, token);

        var tagsQuery = baseQuery.SelectMany(uoi => uoi.Image.Tags);
        if (!string.IsNullOrWhiteSpace(q))
        {
            // Ensure case‑insensitive matching by normalising both sides to lower case.
            var pattern = $"%{q.ToLowerInvariant()}%";
            tagsQuery = tagsQuery.Where(t => EF.Functions.Like(t.Name.ToLower(), pattern));
        }

        var grouped = tagsQuery
            .GroupBy(t => new { t.Id, t.Name })
            .Select(g => new TagCountDto(
                g.Key.Id,
                g.Key.Name,
                g.Count()));

        var totalCount = await grouped.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var tags = await grouped
            .OrderByDescending(x => x.Count)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<TagCountDto>
        {
            Data = tags.ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        };
    }

    #endregion
}
