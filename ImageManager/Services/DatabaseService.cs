using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Services;

public interface IDatabaseService
{
    public Task<ICollection<Character>> GetCharacters(ICollection<string> names);
    public Task<ICollection<Tag>> GetTags(ICollection<string> tags);
    public Task<bool> CanAccessImage(User? user, Image image, Guid? token);
    public IQueryable<UserOwnedImage> AccessibleImages(User? user, Guid? token);
    public Task<Image?> GetImageById(Guid id);
    public Task<bool> ImageExists(Guid id);
    public Task SavePlatformToken(PlatformToken token);
    public IQueryable<PlatformToken> UserPlatformTokens(string userId);
    public Task<PlatformToken?> FindPlatformToken(int id);
    public Task DeletePlatformToken(int id);
    public Task SaveShareToken(ShareToken token);
}

public class DatabaseService(ApplicationDbContext dbContext) : IDatabaseService
{
    public async Task<Image?> GetImageById(Guid id)
    {
        return await dbContext.Images.Include(i => i.UserOwnedImages)
            .Include(i => i.Tags)
            .Include(i => i.Characters)
            .FirstOrDefaultAsync(i => i.Id == id);
    }
    public async Task SavePlatformToken(PlatformToken token)
    {
        dbContext.PlatformTokens.Add(token);
        await dbContext.SaveChangesAsync();
    }
    public async Task<bool> ImageExists(Guid id)
    {
        return await dbContext.Images.AnyAsync(i => i.Id == id);
    }
    public async Task<ICollection<Character>> GetCharacters(ICollection<string> names)
    {
        var processedNames = names.Select(x => x.Trim().ToLower()).Distinct().ToArray();
        var existingNames = await dbContext.Characters
            .Where(t => processedNames.Contains(t.Name))
            .ToDictionaryAsync(t => t.Name, t => t);

        return processedNames
            .Select(name =>
                existingNames.TryGetValue(name, out var existing)
                    ? existing
                    : new Character { Name = name })
            .ToArray();
    }
    public async Task<ICollection<Tag>> GetTags(ICollection<string> tags)
    {
        var processedTags = tags.Select(x => x.Trim().ToLower()).Distinct().ToArray();
        var existingTags = await dbContext.Tags
            .Where(t => processedTags.Contains(t.Name))
            .ToDictionaryAsync(t => t.Name, t => t);

        return processedTags
            .Select(name =>
                existingTags.TryGetValue(name, out var existing)
                    ? existing
                    : new Tag() { Name = name })
            .ToArray();
    }

    public async Task<bool> CanAccessImage(User? user, Image image, Guid? token)
    {
        return await AccessibleImages(user, token).AnyAsync(x => x.ImageId == image.Id);
    }

    public IQueryable<UserOwnedImage> AccessibleImages(User? user, Guid? token)
    {
        var baseQuery = dbContext.UserOwnedImages.Where(uoid =>
                (user != null && uoid.UserId == user.Id) || 
                (uoid.Publicity == Publicity.Open && uoid.Image.AgeRating == AgeRating.General) || 
                (uoid.Publicity == Publicity.Open &&
                 (uoid.Image.AgeRating == AgeRating.Sensitive ||
                  uoid.Image.AgeRating == AgeRating.Explicit ||
                  uoid.Image.AgeRating == AgeRating.Questionable) &&
                 user != null) || 
                (uoid.Publicity == Publicity.Restricted && user != null)
        );
        
        if (token != null)
        {
            var tokenQuery = dbContext.UserOwnedImages
                .Where(uoid => uoid.ShareTokens.Any(stk => stk.Id == token && stk.Expires > DateTime.UtcNow));
            baseQuery = baseQuery.Union(tokenQuery);
        }

        return baseQuery;
    }


    public IQueryable<PlatformToken> UserPlatformTokens(string userId)
    {
        return dbContext.PlatformTokens.Where(t => t.UserId == userId);
    }

    public async Task<PlatformToken?> FindPlatformToken(int id)
    {
        return await dbContext.PlatformTokens.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task DeletePlatformToken(int id)
    {
        var token = await dbContext.PlatformTokens.FirstOrDefaultAsync(t => t.Id == id);
        if (token != null)
        {
            dbContext.PlatformTokens.Remove(token);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task SaveShareToken(ShareToken token)
    {
        dbContext.ShareTokens.Add(token);
        await dbContext.SaveChangesAsync();
    }
}