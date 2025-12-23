using ImageManager.Data;
using ImageManager.Data.Models;

namespace ImageManager.Repositories;

public class PlatformSyncLogRepository(ApplicationDbContext context) : EfRepository<PlatformSyncLog, Guid>(context), IPlatformSyncLogRepository
{

}