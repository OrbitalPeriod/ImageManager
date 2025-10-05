using ImageManager.Data.Models;
using ImageManager.Data.Responses;
using ImageManager.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace ImageManager.Controllers;

[ApiController]
[Route("/api/tags")]
public class TagController(UserManager<User> userManager, ILogger<TagController> logger, IDatabaseService databaseService) : Controller
{
    public record GetTagsResponse(int TagId, string TagName, int Count);
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<GetTagsResponse>>> GetTags([FromQuery] Guid? token, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 200;

        var user = await userManager.GetUserAsync(HttpContext.User);

        var baseQuery = databaseService.AccessibleImages(user, token);

        var totalCount = await baseQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)(pageSize));

        var tags = await baseQuery
            .SelectMany(uoid => uoid.Image.Tags)
            .GroupBy(t => new { t.Id, t.Name })
            .Select(g => new
            {
                TagId = g.Key.Id,
                TagName = g.Key.Name,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = new PaginatedResponse<GetTagsResponse>()
        {
            Data = tags.Select(t => new GetTagsResponse(t.TagId, t.TagName, t.Count)).ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        };

        return Ok(response);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedResponse<GetTagsResponse>>> SearchTags(
        [FromQuery] string q = "",
        [FromQuery] Guid? token = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 200;

        var user = await userManager.GetUserAsync(HttpContext.User);
        var baseQuery = databaseService.AccessibleImages(user, token);

        var tagsQuery = baseQuery.SelectMany(uoi => uoi.Image.Tags);

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
                TagName = g.Key.Name,
                Count = g.Count()
            });

        var totalCount = await grouped.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)(pageSize));

        var tags = await grouped
            .OrderByDescending(x => x.Count)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = new PaginatedResponse<GetTagsResponse>()
        {
            Data = tags.Select(t => new GetTagsResponse(t.TagId, t.TagName, t.Count)).ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        };

        return Ok(response);
    }
}