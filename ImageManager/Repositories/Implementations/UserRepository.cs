using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Repositories.Abstract_Interfaces;
using ImageManager.Repositories.Repository_Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Repositories.Implementations;

public class UserRepository(ApplicationDbContext dbContext) : EfRepository<User, String>(dbContext), IUserRepository
{
    public async Task<ICollection<User>> GetAllAsync()
    {
        return await dbContext.Users.ToListAsync();
    }
}
