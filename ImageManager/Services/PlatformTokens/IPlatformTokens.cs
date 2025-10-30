#region Usings

using ImageManager.Data.Models;
#endregion

namespace ImageManager.Services.PlatformTokens
{
    #region DTOs

    /// <summary>
    /// Payload used when creating a new platform token.
    /// </summary>
    public record AddTokenRequest(
        string Token,
        string PlatformUserId,
        DateTime? Expires,
        Platform Platform,
        bool CheckPrivate);

    /// <summary>
    /// Data transfer object returned by the service for a stored token.
    /// Includes derived properties such as <see cref="IsExpired"/>.
    /// </summary>
    public record PlatformTokenDto(
        Guid Id,
        Platform Platform,
        string PlatformUserId,
        DateTime? Expires,
        bool IsExpired,
        bool CheckPrivate);

    #endregion

    #region Result enum

    /// <summary>
    /// Indicates the outcome of a delete operation.
    /// </summary>
    public enum DeleteResult { NotFound, Forbidden, Deleted }

    #endregion

    #region Service contract

    /// <summary>
    /// Contract for managing platform tokens that are associated with a user account.
    /// </summary>
    public interface IPlatformTokenService
    {
        /// <summary>
        /// Creates a new token for the specified user.
        /// The service must validate the request and persist the token.
        /// </summary>
        Task AddTokenAsync(AddTokenRequest request, User user);

        /// <summary>
        /// Retrieves all tokens that belong to the given user.
        /// </summary>
        Task<IReadOnlyCollection<PlatformTokenDto>> GetTokensAsync(User user);

        /// <summary>
        /// Deletes a token identified by <paramref name="id"/> if it belongs to <paramref name="user"/>.
        /// </summary>
        /// <returns>A <see cref="DeleteResult"/> indicating whether the operation succeeded.</returns>
        Task<DeleteResult> DeleteTokenAsync(Guid id, User user);
    }

    #endregion
}
