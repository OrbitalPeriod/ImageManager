// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Data.Models;
using ImageManager.Repositories.Abstract_Interfaces;

namespace ImageManager.Repositories.Repository_Interfaces;

/// <summary>
/// Repository interface for tags.
/// </summary>
public interface ITagRepository : IRepository<Tag, Guid>
{
    Task<IReadOnlyCollection<Tag>> GetByStringsAsync(IEnumerable<string> tags);
}
