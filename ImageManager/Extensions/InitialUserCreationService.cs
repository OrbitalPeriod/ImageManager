using System.Security.Cryptography;
using System.Text;
using ImageManager.Data;
using ImageManager.Data.Models;
using ImageManager.Services.FolderImport;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ImageManager.Extensions;

public interface IInitialUserCreationService
{
    public Task AddDefaultUser();
}

public class InitialUserCreationService(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    ApplicationDbContext dbContext,
    IConfiguration configuration,
    IFolderImportService folderImportService,
    ILogger<InitialUserCreationService> logger) : IInitialUserCreationService
{
    private const string AdminPasswordEnvVar = "ADMIN_PASSWORD";
    private const int MinPasswordLength = 16; // Generate a longer random password for security

    public async Task AddDefaultUser()
    {
        const string email = "admin@test";
        const string adminRoleName = "Administrator";

        // Create Administrator role if it doesn't exist
        var roleExists = await roleManager.RoleExistsAsync(adminRoleName);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(adminRoleName));
        }

        var userExists = await dbContext.Users.AnyAsync(u => u.Email == email);

        if (!userExists)
        {
            // Get password from environment variable or generate one
            var password = configuration[AdminPasswordEnvVar];
            var passwordWasGenerated = false;

            if (string.IsNullOrWhiteSpace(password))
            {
                password = GenerateRandomPassword();
                passwordWasGenerated = true;
            }

            var adminUser = new User()
            {
                UserName = "admin@test",
                Email = "admin@test",
                IsApproved = true
            };
            
            var createResult = await userManager.CreateAsync(adminUser, password);
            if (createResult.Succeeded)
            {
                // Retrieve the user after creation to ensure all fields are set
                var createdUser = await userManager.FindByEmailAsync(email);
                if (createdUser != null)
                {
                    // Create import folder for the new user
                    try
                    {
                        folderImportService.CreateUserFolder(createdUser.Id);
                    }
                    catch (Exception ex)
                    {
                        // Log but don't fail user creation if folder creation fails
                        logger.LogWarning(ex, "Failed to create import folder for admin user {UserId}", createdUser.Id);
                    }

                    // Assign the Administrator role to the admin user
                    await userManager.AddToRoleAsync(createdUser, adminRoleName);

                    if (passwordWasGenerated)
                    {
                        logger.LogCritical(
                            "========================================\n" +
                            "ADMIN ACCOUNT CREATED\n" +
                            "Email: {Email}\n" +
                            "Password: {Password}\n" +
                            "========================================\n" +
                            "Please save this password securely and change it after first login!",
                            email,
                            password);
                        
                        // Also print to console for visibility
                        Console.WriteLine("========================================");
                        Console.WriteLine("ADMIN ACCOUNT CREATED");
                        Console.WriteLine($"Email: {email}");
                        Console.WriteLine($"Password: {password}");
                        Console.WriteLine("========================================");
                        Console.WriteLine("Please save this password securely and change it after first login!");
                    }
                    else
                    {
                        logger.LogInformation("Admin account created using password from environment variable {EnvVar}", AdminPasswordEnvVar);
                    }
                }
            }
            else
            {
                logger.LogError("Failed to create admin user. Errors: {Errors}", 
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // If user already exists, ensure they have the Administrator role and are approved
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                existingUser.IsApproved = true;
                await userManager.UpdateAsync(existingUser);
                
                var isInRole = await userManager.IsInRoleAsync(existingUser, adminRoleName);
                if (!isInRole)
                {
                    await userManager.AddToRoleAsync(existingUser, adminRoleName);
                }
            }
        }
    }

    /// <summary>
    /// Generates a random password that meets the Identity password requirements:
    /// - At least 6 characters (we generate 12 for security)
    /// - Contains at least one digit
    /// </summary>
    private static string GenerateRandomPassword()
    {
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";
        const string allChars = lowercase + uppercase + digits + special;

        var password = new StringBuilder(MinPasswordLength);
        var random = RandomNumberGenerator.Create();

        // Ensure at least one character from each required category
        password.Append(GetRandomChar(lowercase, random));
        password.Append(GetRandomChar(uppercase, random));
        password.Append(GetRandomChar(digits, random));
        password.Append(GetRandomChar(special, random));

        // Fill the rest with random characters
        var bytes = new byte[4];
        for (int i = password.Length; i < MinPasswordLength; i++)
        {
            random.GetBytes(bytes);
            var index = BitConverter.ToUInt32(bytes, 0) % (uint)allChars.Length;
            password.Append(allChars[(int)index]);
        }

        // Shuffle the password to avoid predictable pattern
        return new string(password.ToString().OrderBy(c => Guid.NewGuid()).ToArray());
    }

    private static char GetRandomChar(string chars, RandomNumberGenerator random)
    {
        var bytes = new byte[4];
        random.GetBytes(bytes);
        var index = BitConverter.ToUInt32(bytes, 0) % (uint)chars.Length;
        return chars[(int)index];
    }
}
