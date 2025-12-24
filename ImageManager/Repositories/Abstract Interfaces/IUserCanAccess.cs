// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Repositories.Abstract_Interfaces;

public interface IUserCanAccess<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>, IUserOwnedEntity
    where TKey : IEquatable<TKey>
{
    async Task<bool> CanAccessAsync(TKey id, string userId) => await DbContext.Set<TEntity>().AnyAsync(t => t.Id.Equals(id) && t.UserId == userId);
    async Task<bool> CanAccessAsync(TKey id, User? user) => await DbContext.Set<TEntity>().AnyAsync(t => t.Id.Equals(id) && t.UserId == (user == null ? null : user.Id));
}
