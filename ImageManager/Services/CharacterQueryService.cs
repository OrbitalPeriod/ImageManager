#region Usings
using System;
using System.Linq;
using System.Threading.Tasks;
using ImageManager.Controllers;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Repositories;
using Microsoft.EntityFrameworkCore;
#endregion

namespace ImageManager.Services;

/// <summary>
/// Service that queries characters associated with images the current user can access.
/// </summary>
public interface ICharacterQueryService
{
    /// <summary>
    /// Retrieves a paginated list of all characters present in images accessible to the caller.
    /// </summary>
    /// <param name="user">The authenticated user; may be <c>null</c> for anonymous.</param>
    /// <param name="token">Optional share token granting access to private images.</param>
    /// <param name="page">1‑based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A paginated response containing character counts.</returns>
    Task<PaginatedResponse<CharacterController.GetCharacterResponse>> GetCharactersAsync(
        User? user,
        Guid? token,
        int page,
        int pageSize);

    /// <summary>
    /// Searches for characters whose names contain the supplied search term.
    /// The result is paginated and sorted by descending usage count.
    /// </summary>
    /// <param name="user">The authenticated user; may be <c>null</c> for anonymous.</param>
    /// <param name="token">Optional share token granting access to private images.</param>
    /// <param name="searchTerm">Substring to match against character names. If empty, no filtering is applied.</param>
    /// <param name="page">1‑based page number.</param>
    /// <param name="pageSize">Number of items per page (max 200).</param>
    /// <returns>A paginated response containing matching characters.</returns>
    Task<PaginatedResponse<CharacterController.GetCharacterResponse>> SearchAsync(
        User? user,
        Guid? token,
        string searchTerm,
        int page,
        int pageSize);
}

#region Implementation

/// <summary>
/// EF Core implementation of <see cref="ICharacterQueryService"/> that queries
/// character usage from images the caller can access.
/// </summary>
public class CharacterQueryService(IUserOwnedImageRepository userOwnedImageRepository) : ICharacterQueryService
{
    /// <inheritdoc />
    public async Task<PaginatedResponse<CharacterController.GetCharacterResponse>> GetCharactersAsync(
        User? user,
        Guid? token,
        int page,
        int pageSize)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "Page must be >= 1");
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be > 0");

        var baseQuery = userOwnedImageRepository.AccessibleImages(user, token);

        // Total count for paging metadata
        var totalCount = await baseQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Retrieve the requested page of character counts
        var characters = await baseQuery
            .SelectMany(i => i.Image.Characters)
            .GroupBy(c => new { c.Id, c.Name })
            .Select(g => new CharacterController.GetCharacterResponse(
                g.Key.Id,
                g.Key.Name,
                g.Count()))
            .OrderByDescending(x => x.Count)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<CharacterController.GetCharacterResponse>
        {
            Data       = characters.ToArray(),
            Page       = page,
            PageSize   = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        };
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<CharacterController.GetCharacterResponse>> SearchAsync(
        User? user,
        Guid? token,
        string searchTerm,
        int page,
        int pageSize)
    {
        // Normalise paging parameters and guard against unreasonable values
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 200;

        var baseQuery = userOwnedImageRepository.AccessibleImages(user, token);

        var charactersQuery = baseQuery
            .SelectMany(i => i.Image.Characters);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLowerInvariant();
            charactersQuery = charactersQuery.Where(c =>
                EF.Functions.Like(c.Name.ToLower(), $"%{lowerSearch}%"));
        }

        // Group by character to count usage
        var grouped = charactersQuery
            .GroupBy(c => new { c.Id, c.Name })
            .Select(g => new
            {
                Id   = g.Key.Id,
                Name = g.Key.Name,
                Count = g.Count()
            });

        var totalCount = await grouped.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pageData = await grouped
            .OrderByDescending(x => x.Count)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<CharacterController.GetCharacterResponse>
        {
            Data       = pageData.Select(p => new CharacterController.GetCharacterResponse(
                p.Id,
                p.Name,
                p.Count)).ToArray(),
            Page       = page,
            PageSize   = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        };
    }
}

#endregion
