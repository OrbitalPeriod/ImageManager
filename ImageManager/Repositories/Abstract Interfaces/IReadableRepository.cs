// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq.Expressions;
using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Repositories.Abstract_Interfaces;

public interface IReadableRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Retrieves an entity by its primary key.
    /// </summary>
    async Task<TEntity?> GetByIdAsync(TKey id) => await DbContext.Set<TEntity>().FindAsync(id);

    /// <summary>
    /// Returns a list of entities that match the optional filter,
    /// optionally ordered and limited in size.
    /// </summary>
    async Task<IReadOnlyCollection<TEntity>> ListAsync(
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
}
