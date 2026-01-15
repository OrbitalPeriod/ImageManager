// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Repositories.Abstract_Interfaces;

public interface IExistsRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    Task<bool> ExistsAsync(TKey id) => DbContext.Set<TEntity>().AnyAsync(t => t.Id.Equals(id));
}
