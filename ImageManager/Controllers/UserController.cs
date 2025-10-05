using ImageManager.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ImageManager.Controllers;

[ApiController]
[Route("/api/users")]
public class UserController(UserManager<User> userManager) : Controller
{
    public record GetUserInfoResponse(string Id, string? UserName, string? Email, Publicity DefaultPublicity);
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<GetUserInfoResponse>> GetUserInfo()
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null)
        {
            return Unauthorized();
        }

        var response = new GetUserInfoResponse(user.Id, user.UserName, user.Email, user.DefaultPublicity);

        return Ok(response);
    }
}