#region Usings
using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;
#endregion

namespace ImageManager.Repositories;

/// <summary>
/// Repository for <see cref="Character"/> entities.
/// Extends the generic <c>EfRepository{TEntity,TKey}</c> and adds a
/// convenience method that retrieves or creates characters by name.
/// </summary>
public class CharacterRepository(ApplicationDbContext dbContext)
    : EfRepository<Character, Guid>(dbContext), ICharacterRepository
{
    #region Public Methods

    /// <summary>
    /// Retrieves all characters whose names match any of the supplied values.
    /// If a name does **not** exist in the database, a new <see cref="Character"/>
    /// instance is created and tracked as <c>Added</c>.
    ///
    /// The returned collection contains *both* existing (Unchanged) and newly
    /// added (Added) entities.  Callers should call <c>SaveChangesAsync()</c>
    /// on the context to persist any new rows.
    /// </summary>
    /// <param name="names">A list of character names to look up.</param>
    /// <returns>A read‑only collection containing the matching or newly created
    /// characters.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="names"/> is <c>null</c>.
    /// </exception>
    public async Task<IReadOnlyCollection<Character>> GetByNamesAsync(IEnumerable<string> names)
    {
        if (names == null) throw new ArgumentNullException(nameof(names));

        var processed = names
            .Where(n => !string.IsNullOrWhiteSpace(n))      
            .Select(n => n.Trim().ToLowerInvariant())     
            .Distinct()
            .ToArray();

        if (processed.Length == 0) return [];
        
        var existing = await dbContext.Characters
            .Where(c => processed.Contains(c.Name))
            .ToDictionaryAsync(c => c.Name, c => c);
        
        var result = new List<Character>(processed.Length);
        foreach (var name in processed)
        {
            if (!existing.TryGetValue(name, out var character))
            {
                character = new Character { Name = name };
                dbContext.Characters.Add(character);   
            }

            result.Add(character);
        }

        return result;     
    }

    #endregion
}
