// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Repositories.Abstract_Interfaces;

public interface IDeleteableRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Removes the given entity from the context and persists the change.
    /// </summary>
    void Delete(TEntity entity) => DbContext.Set<TEntity>().Remove(entity);

    /// <summary>
    /// Deletes an entity by its key without first loading it.
    /// Requires EF Core7 or later for <c>ExecuteDeleteAsync</c>.
    /// </summary>
    Task Delete(TKey id) => DbContext.Set<TEntity>().Where(t => t.Id.Equals(id)).ExecuteDeleteAsync();
}
