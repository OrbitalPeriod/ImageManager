// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Repositories.Abstract_Interfaces;

namespace ImageManager.Repositories.Repository_Interfaces;

/// <summary>
/// Repository interface for images with advanced access control logic.
/// </summary>
public interface IImageRepository : IRepository<Image, Guid>, IExistsRepository<Image, Guid>,
    IReadableRepository<Image, Guid>,
    IAddableEntityRepository<Image, Guid>
{
    Task<bool> CanAccessImageAsync(User? user, Image image, Guid? token);

    // The `public` modifier is removed – interface members are implicitly public.
    IQueryable<UserOwnedImage> AccessibleImages(User? user, Guid? token);
    Task<Option<Image>> GetByHashAsync(ulong hash);
    Task<Option<Image>> GetByIdFullAsync(Guid id);
}
