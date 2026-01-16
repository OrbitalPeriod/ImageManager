using ImageManager.Data.Helpers;
using ImageManager.Data.Models;

namespace ImageManager.Services.Query;

#region Result enums

public enum DeleteError { NotFound, Forbidden };

#endregion

#region inteface

/// <summary>
/// Contract for deleting a user‑owned image.
/// The service checks that the image exists and that the caller owns it
/// before performing the deletion. Administrators can delete any image.
/// </summary>
public interface IDeleteImageService
{
    /// <summary>
    /// Deletes the image identified by <paramref name="imageId"/> if it belongs to the supplied <paramref name="user"/>.
    /// Administrators can delete any image regardless of ownership.
    /// </summary>
    Task<Result<Unit, DeleteError>> DeleteAsync(Guid imageId, User user);
}

#endregion
