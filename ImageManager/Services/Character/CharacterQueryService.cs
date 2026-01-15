using ImageManager.Controllers;
using ImageManager.Data.Helpers;
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
    public async Task<Result<PaginatedResponse<CharacterController.GetCharacterResponse>, CharacterError>> GetCharactersAsync(
        User? user,
        Guid? token,
        int page,
        int pageSize)
    {
        if (page < 1 || pageSize <= 0)
            return Result<PaginatedResponse<CharacterController.GetCharacterResponse>, CharacterError>.Err(CharacterError.InvalidPagination);

        var baseQuery = userOwnedImageRepository.AccessibleImages(user, token);

        // Retrieve the requested page of character counts
        // First group by ID and Name to get counts per character entity
        var characterCounts = await baseQuery
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
            .ToListAsync();

        // Then group by Name only to merge duplicates and pick the minimum ID
        var groupedCharacters = characterCounts
            .GroupBy(c => c.Name)
            .Select(g => new
            {
                Id = g.OrderBy(c => c.Id).First().Id,
                Name = g.Key,
                Count = g.Sum(c => c.Count)
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        // Total count for paging metadata (count unique characters by name)
        var totalCount = groupedCharacters.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var characters = groupedCharacters
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CharacterController.GetCharacterResponse(x.Id, x.Name, x.Count))
            .ToArray();

        return Result<PaginatedResponse<CharacterController.GetCharacterResponse>, CharacterError>.Ok(new PaginatedResponse<CharacterController.GetCharacterResponse>
        {
            Data = characters,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        });
    }

    /// <inheritdoc />
    public async Task<Result<PaginatedResponse<CharacterController.GetCharacterResponse>, CharacterError>> SearchAsync(
        User? user,
        Guid? token,
        string searchTerm,
        int page,
        int pageSize)
    {
        // Normalise paging parameters and guard against unreasonable values
        if (page < 1 || pageSize <= 0 || pageSize > 200)
            return Result<PaginatedResponse<CharacterController.GetCharacterResponse>, CharacterError>.Err(CharacterError.InvalidPagination);

        var baseQuery = userOwnedImageRepository.AccessibleImages(user, token);

        var charactersQuery = baseQuery
            .SelectMany(i => i.Image.Characters);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLowerInvariant();
            charactersQuery = charactersQuery.Where(c =>
                EF.Functions.Like(c.Name.ToLower(), $"%{lowerSearch}%"));
        }

        // Group by character name to count usage and handle duplicates
        // First group by ID and Name to get counts per character entity
        var characterCounts = await charactersQuery
            .GroupBy(c => new { c.Id, c.Name })
            .Select(g => new
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Count = g.Count()
            })
            .ToListAsync();

        // Then group by Name only to merge duplicates and pick the minimum ID
        var grouped = characterCounts
            .GroupBy(c => c.Name)
            .Select(g => new
            {
                Id = g.OrderBy(c => c.Id).First().Id,
                Name = g.Key,
                Count = g.Sum(c => c.Count)
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var totalCount = grouped.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pageData = grouped
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Result<PaginatedResponse<CharacterController.GetCharacterResponse>, CharacterError>.Ok(new PaginatedResponse<CharacterController.GetCharacterResponse>
        {
            Data = pageData.Select(p => new CharacterController.GetCharacterResponse(
                p.Id,
                p.Name,
                p.Count)).ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        });
    }
}

