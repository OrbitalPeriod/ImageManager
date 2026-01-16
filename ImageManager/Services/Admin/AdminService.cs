using ImageManager.Data.Helpers;
using ImageManager.Data.Models;
using ImageManager.Repositories.Repository_Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Services.Admin;

/// <summary>
/// Implementation of <see cref="IAdminService"/> that handles admin operations
/// including user approval and promotion to administrator.
/// </summary>
public class AdminService(
    IUserRepository userRepository,
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager) : IAdminService
{
    private const string AdministratorRoleName = "Administrator";

    /// <inheritdoc />
    public async Task<Result<ICollection<PendingUserResponse>, AdminError>> GetPendingUsersAsync()
    {
        try
        {
            var allUsers = await userRepository.GetAllAsync();
            var pendingUsers = allUsers
                .Where(u => !u.IsApproved)
                .Select(u => new PendingUserResponse(u.Id, u.UserName, u.Email))
                .ToList();

            return Result<ICollection<PendingUserResponse>, AdminError>.Ok(pendingUsers);
        }
        catch (Exception)
        {
            return Result<ICollection<PendingUserResponse>, AdminError>.Err(AdminError.InternalError);
        }
    }

    /// <inheritdoc />
    public async Task<Result<ICollection<UserListResponse>, AdminError>> GetAllUsersAsync()
    {
        try
        {
            var allUsers = await userRepository.GetAllAsync();
            var userList = new List<UserListResponse>();

            foreach (var user in allUsers)
            {
                var roles = await userManager.GetRolesAsync(user);
                userList.Add(new UserListResponse(user.Id, user.UserName, roles.ToList(), user.IsApproved));
            }

            return Result<ICollection<UserListResponse>, AdminError>.Ok(userList);
        }
        catch (Exception)
        {
            return Result<ICollection<UserListResponse>, AdminError>.Err(AdminError.InternalError);
        }
    }

    /// <inheritdoc />
    public async Task<Result<Unit, AdminError>> ApproveUserAsync(string userId)
    {
        try
        {
            var userOption = await userRepository.GetByIdAsync(userId);
            if (userOption.IsNone)
                return Result<Unit, AdminError>.Err(AdminError.NotFound);

            var user = userOption.Unwrap();

            if (user.IsApproved)
                return Result<Unit, AdminError>.Err(AdminError.AlreadyApproved);

            user.IsApproved = true;
            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                return Result<Unit, AdminError>.Err(AdminError.InternalError);

            return Result<Unit, AdminError>.Ok(Unit.New());
        }
        catch (Exception)
        {
            return Result<Unit, AdminError>.Err(AdminError.InternalError);
        }
    }

    /// <inheritdoc />
    public async Task<Result<Unit, AdminError>> PromoteToAdminAsync(string userId)
    {
        try
        {
            var userOption = await userRepository.GetByIdAsync(userId);
            if (userOption.IsNone)
                return Result<Unit, AdminError>.Err(AdminError.NotFound);

            var user = userOption.Unwrap();

            // Check if user is already an administrator
            var isInRole = await userManager.IsInRoleAsync(user, AdministratorRoleName);
            if (isInRole)
                return Result<Unit, AdminError>.Err(AdminError.AlreadyAdministrator);

            // Ensure the Administrator role exists
            var roleExists = await roleManager.RoleExistsAsync(AdministratorRoleName);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new IdentityRole(AdministratorRoleName));
            }

            var addToRoleResult = await userManager.AddToRoleAsync(user, AdministratorRoleName);

            if (!addToRoleResult.Succeeded)
                return Result<Unit, AdminError>.Err(AdminError.InternalError);

            // Update SecurityStamp to invalidate existing cookies and force re-authentication with fresh claims
            await userManager.UpdateSecurityStampAsync(user);

            return Result<Unit, AdminError>.Ok(Unit.New());
        }
        catch (Exception)
        {
            return Result<Unit, AdminError>.Err(AdminError.InternalError);
        }
    }

    /// <inheritdoc />
    public async Task<Result<Unit, AdminError>> DemoteFromAdminAsync(string userId)
    {
        try
        {
            var userOption = await userRepository.GetByIdAsync(userId);
            if (userOption.IsNone)
                return Result<Unit, AdminError>.Err(AdminError.NotFound);

            var user = userOption.Unwrap();

            // Check if user is an administrator
            var isInRole = await userManager.IsInRoleAsync(user, AdministratorRoleName);
            if (!isInRole)
                return Result<Unit, AdminError>.Err(AdminError.NotAdministrator);

            var removeFromRoleResult = await userManager.RemoveFromRoleAsync(user, AdministratorRoleName);

            if (!removeFromRoleResult.Succeeded)
                return Result<Unit, AdminError>.Err(AdminError.InternalError);

            // Update SecurityStamp to invalidate existing cookies and force re-authentication with fresh claims
            await userManager.UpdateSecurityStampAsync(user);

            return Result<Unit, AdminError>.Ok(Unit.New());
        }
        catch (Exception)
        {
            return Result<Unit, AdminError>.Err(AdminError.InternalError);
        }
    }

    /// <inheritdoc />
    public async Task<Result<Unit, AdminError>> ToggleUserApprovalAsync(string userId)
    {
        try
        {
            var userOption = await userRepository.GetByIdAsync(userId);
            if (userOption.IsNone)
                return Result<Unit, AdminError>.Err(AdminError.NotFound);

            var user = userOption.Unwrap();

            // Toggle the approval status
            user.IsApproved = !user.IsApproved;
            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                return Result<Unit, AdminError>.Err(AdminError.InternalError);

            return Result<Unit, AdminError>.Ok(Unit.New());
        }
        catch (Exception)
        {
            return Result<Unit, AdminError>.Err(AdminError.InternalError);
        }
    }
}
