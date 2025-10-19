#region Usings
using ImageManager.Data.Models;
using ImageManager.Data.Responses;
#endregion

namespace ImageManager.Services.Tags;

#region DTOs

/// <summary>
/// Represents a tag together with the number of times it occurs in the current context.
/// The controller simply forwards this DTO to the client.
/// </summary>
public record TagCountDto(Guid TagId, string TagName, int Count);

#endregion

#region Interface

/// <summary>
/// Contract for querying tags and their usage counts.
/// Implementations typically perform paging and optional filtering by user or token.
/// </summary>
public interface ITagService
{
    /// <summary>
    /// Retrieves a paginated list of all tags that the caller can see.
    /// The result is filtered based on the supplied <paramref name="user"/> and/or
    /// <paramref name="token"/>.
    /// </summary>
    /// <param name="user">The user making the request; may be <c>null</c> for anonymous.</param>
    /// <param name="token">
    /// Optional share token that grants temporary access to restricted images.
    /// </param>
    /// <param name="page">1‑based page number (default 1).</param>
    /// <param name="pageSize">Number of items per page (default 20).</param>
    Task<PaginatedResponse<TagCountDto>> GetTagsAsync(
        User? user,
        Guid? token,
        int page,
        int pageSize);

    /// <summary>
    /// Searches for tags whose names match the supplied query string.
    /// The search is case‑insensitive and supports paging.
    /// </summary>
    /// <param name="user">The user making the request; may be <c>null</c> for anonymous.</param>
    /// <param name="q">Search term (e.g. a prefix of the tag name).</param>
    /// <param name="token">
    /// Optional share token that grants temporary access to restricted images.
    /// </param>
    /// <param name="page">1‑based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    Task<PaginatedResponse<TagCountDto>> SearchTagsAsync(
        User? user,
        string q,
        Guid? token,
        int page,
        int pageSize);
}

#endregion
