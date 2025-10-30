#region Usings

using ImageManager.Data.Models;
using ImageManager.Repositories;
#endregion

namespace ImageManager.Services.PlatformTokens;

/// <summary>
/// Concrete implementation of <see cref="IPlatformTokenService"/> that uses an
/// <see cref="IPlatformTokenRepository"/> to persist token data.
/// </summary>
public class PlatformTokenService(IPlatformTokenRepository platformTokenRepository) : IPlatformTokenService
{
    #region Add

    /// <summary>
    /// Creates a new platform token for the specified user.
    /// The request and user are validated for null values; further business rules can be added here.
    /// </summary>
    public async Task AddTokenAsync(AddTokenRequest request, User user)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (user == null) throw new ArgumentNullException(nameof(user));

        var token = new PlatformToken
        {
            PlatformUserId = request.PlatformUserId,
            Expires = request.Expires,
            Token = request.Token,
            Platform = request.Platform,
            CheckPrivate = request.CheckPrivate,
            UserId = user.Id
        };

        await platformTokenRepository.AddAsync(token);
    }

    #endregion

    #region Get

    /// <summary>
    /// Retrieves all tokens that belong to the given user.
    /// </summary>
    public async Task<IReadOnlyCollection<PlatformTokenDto>> GetTokensAsync(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var tokens = await platformTokenRepository.ListAsync(q => q.UserId == user.Id);

        return tokens.Select(t => new PlatformTokenDto(
                t.Id,
                t.Platform,
                t.PlatformUserId,
                t.Expires,
                t.IsExpired,
                t.CheckPrivate))
            .ToArray();
    }

    #endregion

    #region Delete

    /// <summary>
    /// Deletes the token identified by <paramref name="id"/> if it belongs to
    /// the supplied <paramref name="user"/>.
    /// </summary>
    public async Task<DeleteResult> DeleteTokenAsync(Guid id, User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var token = await platformTokenRepository.GetByIdAsync(id);
        if (token == null) return DeleteResult.NotFound;
        if (token.UserId != user.Id) return DeleteResult.Forbidden;

        // The repository’s key‑based delete performs a direct DELETE SQL statement.
        await platformTokenRepository.Delete(token.Id);
        return DeleteResult.Deleted;
    }

    #endregion
}
