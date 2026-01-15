// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Repositories.Abstract_Interfaces;

public interface IGetAllInterface<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    async Task<ICollection<TEntity>> GetAllAsync() => await DbContext.Set<TEntity>().Where(t => true).ToArrayAsync();
}
