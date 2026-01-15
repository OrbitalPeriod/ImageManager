using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Repositories.Abstract_Interfaces;
using ImageManager.Repositories.Repository_Interfaces;

namespace ImageManager.Repositories.Implementations;

public class PlatformSyncLogRepository(ApplicationDbContext context) : EfRepository<PlatformSyncLog, Guid>(context), IPlatformSyncLogRepository
{

}
