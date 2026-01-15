// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Data.Models;

namespace ImageManager.Repositories.Abstract_Interfaces;

public interface IAddableEntityRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Adds a new entity to the context and persists it.
    /// </summary>
    async Task AddAsync(TEntity entity) => await DbContext.Set<TEntity>().AddAsync(entity);
}
