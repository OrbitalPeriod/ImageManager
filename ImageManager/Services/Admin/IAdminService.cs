using ImageManager.Data.Helpers;
using ImageManager.Data.Models;

namespace ImageManager.Services.Admin;

#region DTOs

/// <summary>
/// DTO representing a pending (unapproved) user.
/// </summary>
public record PendingUserResponse(
    string Id,
    string? UserName,
    string? Email);

#endregion

#region Interface

/// <summary>
/// Contract for admin operations including user approval and promotion.
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Retrieves a list of all pending (unapproved) users.
    /// </summary>
    /// <returns>A collection of pending users.</returns>
    Task<Result<ICollection<PendingUserResponse>, AdminError>> GetPendingUsersAsync();

    /// <summary>
    /// Approves a user, allowing them to log in.
    /// </summary>
    /// <param name="userId">The ID of the user to approve.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result<Unit, AdminError>> ApproveUserAsync(string userId);

    /// <summary>
    /// Promotes a user to administrator role.
    /// </summary>
    /// <param name="userId">The ID of the user to promote.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result<Unit, AdminError>> PromoteToAdminAsync(string userId);

    /// <summary>
    /// Demotes an administrator to a regular user by removing their administrator role.
    /// </summary>
    /// <param name="userId">The ID of the administrator to demote.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result<Unit, AdminError>> DemoteFromAdminAsync(string userId);

    /// <summary>
    /// Toggles the approval status of a user (enables/disables their account).
    /// If the user is approved, they will be disabled. If disabled, they will be approved.
    /// </summary>
    /// <param name="userId">The ID of the user to toggle approval status for.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result<Unit, AdminError>> ToggleUserApprovalAsync(string userId);
}

#endregion
