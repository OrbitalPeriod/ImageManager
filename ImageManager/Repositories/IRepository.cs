#region Usings

using System.Linq.Expressions;
using ImageManager.Data;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;
#endregion

namespace ImageManager.Repositories;

/// <summary>
/// Generic repository interface that defines basic CRUD operations.
/// </summary>
public interface IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// Retrieves an entity by its primary key.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TKey id);

    /// <summary>
    /// Returns a list of entities that match the optional filter,
    /// optionally ordered and limited in size.
    /// </summary>
    Task<IReadOnlyCollection<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int? take = null);

    /// <summary>
    /// Adds a new entity to the context and persists it.
    /// </summary>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// Marks an entity as modified.  Callers must invoke
    /// <see cref="ApplicationDbContext.SaveChangesAsync"/> to persist.
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Removes the given entity from the context and persists the change.
    /// </summary>
    Task Delete(TEntity entity);

    /// <summary>
    /// Deletes an entity by its key without first loading it.
    /// Requires EF Core 7 or later for <c>ExecuteDeleteAsync</c>.
    /// </summary>
    Task Delete(TKey id);
}

/// <summary>
/// Entity Framework implementation of <see cref="IRepository{TEntity,TKey}"/>.
/// </summary>
public class EfRepository<TEntity, TKey>(ApplicationDbContext ctx) : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    protected readonly ApplicationDbContext DbContext = ctx;

    public async Task<TEntity?> GetByIdAsync(TKey id)
        => await DbContext.Set<TEntity>().FindAsync(id);

    public async Task<IReadOnlyCollection<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int? take = null)
    {
        IQueryable<TEntity> query = DbContext.Set<TEntity>();

        if (filter != null) query = query.Where(filter);
        if (orderBy != null) query = orderBy(query);
        if (take.HasValue) query = query.Take(take.Value);

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task AddAsync(TEntity entity)
    {
        DbContext.Set<TEntity>().Add(entity);
    }

    // Note: Update does not persist changes automatically.
    // The caller must call SaveChangesAsync() when desired.
    public void Update(TEntity entity) => DbContext.Entry(entity).State = EntityState.Modified;

    public async Task Delete(TEntity entity)
    {
        DbContext.Set<TEntity>().Remove(entity);
    }

    public async Task Delete(TKey id)
    {
        // Bulk delete – EF Core 7+ only.
        await DbContext.Set<TEntity>()
            .Where(t => t.Id.Equals(id))
            .ExecuteDeleteAsync();
    }
}

/// <summary>
/// Repository interface for <see cref="Character"/> entities.
/// </summary>
public interface ICharacterRepository : IRepository<Character, Guid>
{
    Task<IReadOnlyCollection<Character>> GetByNamesAsync(IEnumerable<string> names);
}

/// <summary>
/// Repository interface for downloaded image records.
/// </summary>
public interface IDownloadedImageRepository : IRepository<DownloadedImage, Guid>
{
    Task<bool> ExistsAsync(Guid imageId);
}

/// <summary>
/// Repository interface for images with advanced access control logic.
/// </summary>
public interface IImageRepository : IRepository<Image, Guid>
{
    Task<bool> CanAccessImageAsync(User? user, Image image, Guid? token);
    Task<bool> ImageExistsAsync(Guid id);

    // The `public` modifier is removed – interface members are implicitly public.
    IQueryable<UserOwnedImage> AccessibleImages(User? user, Guid? token);
    Task<Image?> GetByHashAsync(ulong hash);
    Task<Image?> GetByIdFullAsync(Guid id);
}

/// <summary>
/// Repository interface for platform‑level tokens.
/// </summary>
public interface IPlatformTokenRepository : IRepository<PlatformToken, Guid>
{
    Task<IReadOnlyCollection<PlatformToken>> GetAllAsync();
}

/// <summary>
/// Repository interface for share tokens.
/// </summary>
public interface IShareTokenRepository : IRepository<ShareToken, Guid>;

/// <summary>
/// Repository interface for tags.
/// </summary>
public interface ITagRepository : IRepository<Tag, Guid>
{
    Task<IReadOnlyCollection<Tag>> GetByStringsAsync(IEnumerable<string> tags);
}

/// <summary>
/// Repository interface for users.
/// </summary>
public interface IUserRepository : IRepository<User, string>
{
    Task<ICollection<User>> GetAllAsync();
}

/// <summary>
/// Repository interface for user‑owned images with access helpers.
/// </summary>
public interface IUserOwnedImageRepository : IRepository<UserOwnedImage, Guid>
{
    IQueryable<UserOwnedImage> AccessibleImages(User? user, Guid? token);
    IQueryable<UserOwnedImage> AccessibleImages(string? user, Guid? token);
}
