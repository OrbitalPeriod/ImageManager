using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace ImageManager.Services;

public interface IAuthService
{

}

public class AuthService(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext dbContext) : IAuthService
{

}