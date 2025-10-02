using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Services;

public interface IDatabaseService
{
    public Task<ICollection<Character>> GetCharacters(ICollection<string> names);
    public Task<ICollection<Tag>> GetTags(ICollection<string> tags);
}

public class DatabaseService(ApplicationDbContext dbContext) : IDatabaseService
{
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
}