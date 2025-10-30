using ImageManager.Data;
using ImageManager.Data.Models;

namespace ImageManager.Repositories;

public class PlatformTokenRepository(ApplicationDbContext dbContext) : EfRepository<PlatformToken, Guid>(dbContext), IPlatformTokenRepository
{

}