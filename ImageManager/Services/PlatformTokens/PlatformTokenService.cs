#region Usings

using System.Threading.Channels;
using ImageManager.Data;
using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Repositories;
using ImageManager.Repositories.Repository_Interfaces;
using ImageManager.Workers;

#endregion

namespace ImageManager.Services.PlatformTokens;

/// <summary>
/// Concrete implementation of <see cref="IPlatformTokenService"/> that uses an
/// <see cref="IPlatformTokenRepository"/> to persist token data.
/// </summary>
public class PlatformTokenService(IPlatformTokenRepository platformTokenRepository, IPlatformSyncLogRepository platformSyncLogRepository, Channel<PlatformSyncRequest> platformSyncRequestChannel, ITransactionService transactionService) : IPlatformTokenService
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

        await transactionService.SaveChangesAsync();

        await platformSyncRequestChannel.Writer.WriteAsync(new PlatformSyncRequest(token.Id));
    }

    public async Task<IReadOnlyCollection<PlatformTokenSyncLog>?> GetLogAsync(Guid id, User user)
    {
        if (!await platformTokenRepository.CanAccessAsync(id, user)) return null;

        return (await platformSyncLogRepository.ListAsync(psl => psl.PlatformTokenId == id)).Select(psl => new PlatformTokenSyncLog(psl.Id, psl.Success, psl.Message, psl.CreatedAt)).ToArray();
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
    public async Task<Result<Unit, PlatformTokenError>> DeleteTokenAsync(Guid id, User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var tokenOption = await platformTokenRepository.GetByIdAsync(id);
        if (tokenOption.IsNone) return Result<Unit, PlatformTokenError>.Err(PlatformTokenError.NotFound);
        
        var token = tokenOption.Unwrap();
        if (token.UserId != user.Id) return Result<Unit, PlatformTokenError>.Err(PlatformTokenError.Forbidden);

        // The repository's key‑based delete performs a direct DELETE SQL statement.
        await platformTokenRepository.Delete(token.Id);
        await transactionService.SaveChangesAsync();
        return Result<Unit, PlatformTokenError>.Ok(Unit.New());
    }



    #endregion

    #region Queue

    /// <inheritdoc/>
    public async Task<Result<Unit, PlatformTokenError>> QueueAsync(Guid id, User user)
    {
        var platformTokenOption = await platformTokenRepository.GetByIdAsync(id);
        if (platformTokenOption.IsNone) return Result<Unit, PlatformTokenError>.Err(PlatformTokenError.NotFound);
        
        var platformToken = platformTokenOption.Unwrap();
        if (platformToken.UserId != user.Id) return Result<Unit, PlatformTokenError>.Err(PlatformTokenError.Forbidden);

        await platformSyncRequestChannel.Writer.WriteAsync(new PlatformSyncRequest(platformToken.Id));
        return Result<Unit, PlatformTokenError>.Ok(Unit.New());
    }

    #endregion

}
