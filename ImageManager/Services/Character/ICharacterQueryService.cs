// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ImageManager.Controllers;
using ImageManager.Data.Models;
using ImageManager.Data.Responses;

namespace ImageManager.Services.Character;

#region Interface

/// <summary>
/// Service that queries characters associated with images the current user can access.
/// </summary>
public interface ICharacterQueryService
{
    /// <summary>
    /// Retrieves a paginated list of all characters present in images accessible to the caller.
    /// </summary>
    /// <param name="user">The authenticated user; may be <c>null</c> for anonymous.</param>
    /// <param name="token">Optional share token granting access to private images.</param>
    /// <param name="page">1‑based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A paginated response containing character counts.</returns>
    Task<PaginatedResponse<CharacterController.GetCharacterResponse>> GetCharactersAsync(
        User? user,
        Guid? token,
        int page,
        int pageSize);

    /// <summary>
    /// Searches for characters whose names contain the supplied search term.
    /// The result is paginated and sorted by descending usage count.
    /// </summary>
    /// <param name="user">The authenticated user; may be <c>null</c> for anonymous.</param>
    /// <param name="token">Optional share token granting access to private images.</param>
    /// <param name="searchTerm">Substring to match against character names. If empty, no filtering is applied.</param>
    /// <param name="page">1‑based page number.</param>
    /// <param name="pageSize">Number of items per page (max 200).</param>
    /// <returns>A paginated response containing matching characters.</returns>
    Task<PaginatedResponse<CharacterController.GetCharacterResponse>> SearchAsync(
        User? user,
        Guid? token,
        string searchTerm,
        int page,
        int pageSize);
}

#endregion
