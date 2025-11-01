using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Repositories;

public class PlatformTokenRepository(ApplicationDbContext dbContext) : EfRepository<PlatformToken, Guid>(dbContext), IPlatformTokenRepository
{
    public async Task<IReadOnlyCollection<PlatformToken>> GetAllAsync()
    {
        return await dbContext.PlatformTokens.Include(i => i.User).AsNoTracking().ToListAsync();
    }
}