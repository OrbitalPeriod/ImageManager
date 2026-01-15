using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Repositories.Abstract_Interfaces;
using ImageManager.Repositories.Repository_Interfaces;

namespace ImageManager.Repositories.Implementations;

public class ShareTokenRepository(ApplicationDbContext dbContext)
    : EfRepository<ShareToken, Guid>(dbContext), IShareTokenRepository
{

}
