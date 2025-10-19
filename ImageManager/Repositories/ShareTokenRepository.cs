using ImageManager.Data;
using ImageManager.Data.Models;

namespace ImageManager.Repositories;

public class ShareTokenRepository(ApplicationDbContext dbContext)
    : EfRepository<ShareToken, Guid>(dbContext), IShareTokenRepository
{

}