using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Repositories.Abstract_Interfaces;
using ImageManager.Repositories.Repository_Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Repositories.Implementations;

public class PlatformTokenRepository(ApplicationDbContext dbContext) : EfRepository<PlatformToken, Guid>(dbContext), IPlatformTokenRepository
{
    public async Task<IReadOnlyCollection<PlatformToken>> GetAllAsync()
    {
        return await DbContext.PlatformTokens.Include(i => i.User).AsNoTracking().ToListAsync();
    }
}
