using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Controllers;

[ApiController]
[Route("/api/characters")]
public class CharacterController(UserManager<User> userManager, ILogger<CharacterController> logger, IDatabaseService databaseService) : Controller
{
    public record GetCharacterResponse(int TagId, string CharacterName, int Count);
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<GetCharacterResponse>>> GetCharacters([FromQuery] Guid? token, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 200;

        var user = await userManager.GetUserAsync(HttpContext.User);

        var baseQuery = databaseService.AccessibleImages(user, token);

        var totalCount = await baseQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)(pageSize));

        var tags = await baseQuery
            .SelectMany(i => i.Image.Characters)
            .GroupBy(t => new { t.Id, t.Name })
            .Select(g => new
            {
                TagId = g.Key.Id,
                CharacterName = g.Key.Name,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = new PaginatedResponse<GetCharacterResponse>()
        {
            Data = tags.Select(t => new GetCharacterResponse(t.TagId, t.CharacterName, t.Count)).ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        };

        return Ok(response);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedResponse<GetCharacterResponse>>> SearchCharacters(
        [FromQuery] string q = "",
        [FromQuery] Guid? token = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 200;

        var user = await userManager.GetUserAsync(HttpContext.User);
        var baseQuery = databaseService.AccessibleImages(user, token);

        var tagsQuery = baseQuery.SelectMany(i => i.Image.Characters);

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.ToLower();
            tagsQuery = tagsQuery.Where(t => EF.Functions.Like(t.Name.ToLower(), $"%{q}%"));
        }

        var grouped = tagsQuery
            .GroupBy(t => new { t.Id, t.Name })
            .Select(g => new
            {
                TagId = g.Key.Id,
                CharacterName = g.Key.Name,
                Count = g.Count()
            });

        var totalCount = await grouped.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)(pageSize));

        var tags = await grouped
            .OrderByDescending(x => x.Count)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = new PaginatedResponse<GetCharacterResponse>()
        {
            Data = tags.Select(t => new GetCharacterResponse(t.TagId, t.CharacterName, t.Count)).ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        };

        return Ok(response);
    }
}