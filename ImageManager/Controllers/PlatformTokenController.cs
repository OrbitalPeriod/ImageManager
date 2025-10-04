using ImageManager.Data.Models;
using ImageManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Controllers;

[ApiController]
[Route("/api/platform-tokens")]
public class PlatformTokenController(UserManager<User> userManager, IDatabaseService databaseService) : Controller
{
    public record AddTokenRequest(string Token, string PlatFormUserId, DateTime? Expires, Platform Platform, bool CheckPrivate);
    [Authorize]
    [HttpPut("add")]
    public async Task<IActionResult> AddToken([FromBody]  AddTokenRequest request)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        
        if (user == null)
        {
            return Unauthorized();
        }
        
        var platformToken = new PlatformToken
        {
            PlatformUserId = request.PlatFormUserId,
            Expires = request.Expires,
            Token = request.Token,
            Platform = request.Platform,
            CheckPrivate = request.CheckPrivate,
            UserId = user.Id
        };
        
        await databaseService.SavePlatformToken(platformToken);
        return Ok();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetPlatformTokens()
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null)
        {
            return Unauthorized();
        }
        
        var tokens = await databaseService.UserPlatformTokens(user.Id).ToListAsync();
        
        return Ok(tokens.Select(t => new
        {
            t.Id,
            t.Platform,
            t.PlatformUserId,
            t.Expires,
            t.IsExpired,
            t.CheckPrivate
        }));
    }
}