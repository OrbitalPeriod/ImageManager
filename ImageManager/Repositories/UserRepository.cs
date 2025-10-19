using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Repositories;

public class UserRepository(ApplicationDbContext dbContext) : EfRepository<User, String>(dbContext), IUserRepository
{
    public async Task<ICollection<User>> GetAllAsync()
    {
        return await dbContext.Users.ToListAsync();
    }
}