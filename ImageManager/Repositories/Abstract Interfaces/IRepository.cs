#region Usings

using ImageManager.Data;
using ImageManager.Data.Models;

#endregion

namespace ImageManager.Repositories.Abstract_Interfaces;

/// <summary>
/// Generic repository interface that defines basic CRUD operations.
/// </summary>
public interface IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    internal ApplicationDbContext DbContext { get; }
}

/// <summary>
/// Entity Framework implementation of <see cref="IRepository{TEntity,TKey}"/>.
/// </summary>
public class EfRepository<TEntity, TKey>(ApplicationDbContext dbContext) : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    public ApplicationDbContext DbContext { get; } = dbContext;
}
