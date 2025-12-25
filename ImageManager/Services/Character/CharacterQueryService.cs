using ImageManager.Controllers;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Repositories.Repository_Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Services.Character;

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
            .Select(c => new
            {
                Id = c.Id,
                Name = c.Name
            })
            .GroupBy(x => new { x.Id, x.Name })
            .Select(g => new
            {
                g.Key.Id,
                g.Key.Name,
                Count = g.Count(),
            })
            .OrderByDescending(x => x.Count)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CharacterController.GetCharacterResponse(x.Id, x.Name, x.Count))
            .ToArrayAsync();

        return new PaginatedResponse<CharacterController.GetCharacterResponse>
        {
            Data = characters,
            Page = page,
            PageSize = pageSize,
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
                Id = g.Key.Id,
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
            Data = pageData.Select(p => new CharacterController.GetCharacterResponse(
                p.Id,
                p.Name,
                p.Count)).ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        };
    }
}

