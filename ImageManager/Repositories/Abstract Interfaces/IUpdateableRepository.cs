// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Repositories.Abstract_Interfaces;

public interface IUpdateableRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Marks an entity as modified.  Callers must invoke
    /// <see>
    ///     <cref>ApplicationDbContext.SaveChangesAsync</cref>
    /// </see>
    /// to persist.
    /// </summary>
    void Update(TEntity entity) => DbContext.Entry(entity).State = EntityState.Modified;
}
