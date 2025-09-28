using ImageManager.Data;

namespace ImageManager.Services;

public interface IDatabaseService
{
}

public class DatabaseService(ApplicationDbContext dbContext) : IDatabaseService
{
    private readonly ApplicationDbContext _databaseService = dbContext;
    
    
}