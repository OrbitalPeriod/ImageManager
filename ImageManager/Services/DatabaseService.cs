using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Services;

public interface IDatabaseService
{
    public Task<ICollection<Character>> GetCharacters(ICollection<string> names);
    public Task<ICollection<Tag>> GetTags(ICollection<string> tags);
    public Task<bool> CanAccessImage(User? user, Image image, Guid? token);
    public IQueryable<Image> AccessibleImages(User? user, Guid? token);
    public Task<Image?> GetImageById(Guid id);
    public Task<bool> ImageExists(Guid id);
}

public class DatabaseService(ApplicationDbContext dbContext) : IDatabaseService
{
    public async Task<Image?> GetImageById(Guid id)
    {
        return await dbContext.Images.Include(i => i.User)
            .Include(i => i.ShareTokens)
            .Include(i => i.Tags)
            .Include(i => i.Characters)
            .FirstOrDefaultAsync(i => i.Id == id);
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
        // Owner always has access
        if (user != null && image.User.Id == user.Id)
        {
            return true;
        }

        // if a token is provided, validate it
        if (token.HasValue)
        {
            bool tokenValid = await dbContext.ShareTokens
                .AnyAsync(stk => stk.Id == token.Value && stk.ImageId == image.Id && !stk.IsExpired);

            if (tokenValid)
            {
                return true;
            }
        }

        // Public images
        if (image.Publicity == Publicity.Open)
        {
            if (image.AgeRating is AgeRating.Sensitive or AgeRating.Explicit or AgeRating.Questionable)
            {
                // Must be logged in to see sensitive content
                return user != null;
            }

            // Safe + public => anyone allowed
            return true;
        }

        // Restricted images => only owner
        if (image.Publicity == Publicity.Private)
        {
            return null != user;
        }

        // ✅ 5. Private images => only owner (already handled at step 1)
        if (image.Publicity == Publicity.Private)
        {
            return false;
        }

        return false;
    }

    public IQueryable<Image> AccessibleImages(User? user, Guid? token)
    {
        return dbContext.Images.Where(image =>
            (user != null && image.User.Id == user.Id) || // Owner always has access
            (image.Publicity == Publicity.Open && image.AgeRating == AgeRating.General) || // Safe + public => anyone allowed
            (image.Publicity == Publicity.Open && (image.AgeRating == AgeRating.Sensitive || image.AgeRating == AgeRating.Explicit || image.AgeRating == AgeRating.Questionable) && user != null) || // Must be logged in to see sensitive content
            (image.Publicity == Publicity.Restricted && user != null) || // Restricted images => only logged-in users
            (token != null && image.ShareTokens.Any(stk => stk.Id == token && !stk.IsExpired)) // if a token is provided, validate it
        );
    }
}