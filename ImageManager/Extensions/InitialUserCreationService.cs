using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Extensions;

public interface IInitialUserCreationService
{
    public Task AddDefaultUser();
}

public class InitialUserCreationService(UserManager<User>  userManager, ApplicationDbContext dbContext) : IInitialUserCreationService
{
    public async Task AddDefaultUser()
    {
        const string email = "admin@test";

        var userExists = await dbContext.Users.AnyAsync(u => u.Email == email);

        if (!userExists)
        {
            await userManager.CreateAsync(new User()
            {
                UserName = "admin@test",
                Email = "admin@test",
            }, "Test123!");
        }
    }
}