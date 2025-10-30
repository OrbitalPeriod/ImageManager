#region Usings

using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;
#endregion

namespace ImageManager.Repositories;

/// <summary>
/// Repository for <see cref="Tag"/> entities.
/// Provides a helper that returns existing tags or creates new ones
/// when they do not yet exist in the database.
/// </summary>
public class TagRepository(ApplicationDbContext dbContext)
    : EfRepository<Tag, Guid>(dbContext), ITagRepository
{
    #region Public Methods

    /// <summary>
    /// Retrieves all tags whose names match any of the supplied strings.
    /// If a name does not exist in the database, a new <see cref="Tag"/>
    /// entity is created and tracked as Added – it will be persisted when
    /// the caller calls <c>SaveChangesAsync()</c>.
    /// </summary>
    /// <param name="tags">A collection of tag names to look up.</param>
    /// <returns>A read‑only collection containing the matching or newly created tags.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="tags"/> is <c>null</c>.
    /// </exception>
    public async Task<IReadOnlyCollection<Tag>> GetByStringsAsync(IEnumerable<string> tags)
    {
        if (tags == null) throw new ArgumentNullException(nameof(tags));

        var processedTags = tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLowerInvariant())
            .Distinct()
            .ToArray();

        if (!processedTags.Any()) return [];

        var existing = await dbContext.Tags
            .Where(t => processedTags.Contains(t.Name))
            .ToDictionaryAsync(t => t.Name, t => t);

        var result = new List<Tag>(processedTags.Length);
        foreach (var name in processedTags)
        {
            if (!existing.TryGetValue(name, out var tag))
            {

                tag = new Tag { Name = name };
                dbContext.Tags.Add(tag);
            }

            result.Add(tag);
        }

        await dbContext.SaveChangesAsync();
        return result;
    }

    #endregion
}
