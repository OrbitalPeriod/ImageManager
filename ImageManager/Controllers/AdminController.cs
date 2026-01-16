#region Usings

using ImageManager.Extensions;
using ImageManager.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace ImageManager.Controllers;

/// <summary>
/// Handles admin operations including user approval and promotion.
/// Requires Administrator role for all endpoints.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Administrator")]
public sealed class AdminController(IAdminService adminService) : ControllerBase
{
    #region Actions

    /// <summary>
    /// Retrieves a list of all pending (unapproved) users.
    /// </summary>
    /// <returns>A collection of pending users.</returns>
    [HttpGet("users/pending")]
    [ProducesResponseType(typeof(ICollection<PendingUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ICollection<PendingUserResponse>>> GetPendingUsers()
    {
        var result = await adminService.GetPendingUsersAsync();
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Approves a user, allowing them to log in.
    /// </summary>
    /// <param name="userId">The ID of the user to approve.</param>
    /// <returns>Success response if the user was approved.</returns>
    [HttpPost("users/{userId}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ApproveUser([FromRoute] string userId)
    {
        var result = await adminService.ApproveUserAsync(userId);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Promotes a user to administrator role.
    /// </summary>
    /// <param name="userId">The ID of the user to promote.</param>
    /// <returns>Success response if the user was promoted.</returns>
    [HttpPost("users/{userId}/promote")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PromoteToAdmin([FromRoute] string userId)
    {
        var result = await adminService.PromoteToAdminAsync(userId);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Demotes an administrator to a regular user by removing their administrator role.
    /// </summary>
    /// <param name="userId">The ID of the administrator to demote.</param>
    /// <returns>Success response if the user was demoted.</returns>
    [HttpPost("users/{userId}/demote")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DemoteFromAdmin([FromRoute] string userId)
    {
        var result = await adminService.DemoteFromAdminAsync(userId);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Toggles the approval status of a user (enables/disables their account).
    /// If the user is approved, they will be disabled. If disabled, they will be approved.
    /// </summary>
    /// <param name="userId">The ID of the user to toggle approval status for.</param>
    /// <returns>Success response if the approval status was toggled.</returns>
    [HttpPost("users/{userId}/toggle-approval")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ToggleUserApproval([FromRoute] string userId)
    {
        var result = await adminService.ToggleUserApprovalAsync(userId);
        return this.ToActionResult(result);
    }

    #endregion
}
