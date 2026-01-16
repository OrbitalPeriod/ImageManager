using ImageManager.Controllers;
using ImageManager.Data.Helpers;
using ImageManager.Services.Admin;
using ImageManager.Services.Character;
using ImageManager.Services.ImageImport;
using ImageManager.Services.PlatformTokens;
using ImageManager.Services.Query;
using ImageManager.Services.Tags;
using Microsoft.AspNetCore.Mvc;

namespace ImageManager.Extensions;

/// <summary>
/// Extension methods for converting Result and Option types to ActionResult.
/// Provides consistent error handling across all controllers.
/// </summary>
public static class ControllerResultExtensions
{
    /// <summary>
    /// Converts a Result to an ActionResult, mapping error enums to appropriate HTTP status codes.
    /// </summary>
    public static ActionResult<T> ToActionResult<T, E>(this ControllerBase controller, Result<T, E> result)
        where E : Enum
    {
        if (result.IsOk)
            return controller.Ok(result.Unwrap());

        var error = result.UnwrapError();
        return error switch
        {
            // Common error patterns
            _ when error.ToString() == "NotFound" => controller.NotFound(),
            _ when error.ToString() == "Forbidden" => controller.Forbid(),
            _ when error.ToString() == "Unauthorized" => controller.Unauthorized(),
            _ when error.ToString() == "ValidationError" || error.ToString() == "InvalidPagination" => controller.BadRequest(),
            
            // Domain-specific error mappings
            ImageError.NotFound => controller.NotFound(),
            ImageError.Forbidden => controller.Forbid(),
            ImageError.InvalidPagination => controller.BadRequest("Invalid pagination parameters"),
            
            TagError.InvalidPagination => controller.BadRequest("Invalid pagination parameters"),
            TagError.InternalError => controller.StatusCode(500, "Internal server error"),
            
            CharacterError.InvalidPagination => controller.BadRequest("Invalid pagination parameters"),
            CharacterError.InternalError => controller.StatusCode(500, "Internal server error"),
            
            PlatformTokenError.NotFound => controller.NotFound(),
            PlatformTokenError.Forbidden => controller.Forbid(),
            PlatformTokenError.InternalError => controller.StatusCode(500, "Internal server error"),
            
            DeleteError.NotFound => controller.NotFound(),
            DeleteError.Forbidden => controller.Forbid(),
            
            ImportImageError.EmptyImage => controller.BadRequest("Empty image"),
            ImportImageError.FailedToGetTags => controller.Problem("Tag retrieval failure"),
            ImportImageError.ImageParseFailed => controller.BadRequest("Invalid image format"),
            ImportImageError.ImageStoreError => controller.Problem("Image IO failure"),
            ImportImageError.AlreadyOwned => controller.BadRequest("Image is already owned"),
            
            AuthError.InvalidCredentials => controller.Unauthorized("Invalid credentials"),
            AuthError.UserAlreadyExists => controller.BadRequest("User already exists"),
            AuthError.InvalidPassword => controller.BadRequest("Invalid password"),
            AuthError.InvalidEmail => controller.BadRequest("Invalid email"),
            AuthError.InternalError => controller.StatusCode(500, "Internal server error"),
            
            _ => controller.StatusCode(500, "Internal server error")
        };
    }

    /// <summary>
    /// Converts an Option to an ActionResult, returning NotFound if None.
    /// </summary>
    public static ActionResult<T> ToActionResult<T>(this ControllerBase controller, Option<T> option)
    {
        if (option.IsSome)
            return controller.Ok(option.Unwrap());
        
        return controller.NotFound();
    }

    /// <summary>
    /// Converts a Result to an ActionResult without a return value.
    /// </summary>
    public static IActionResult ToActionResult<E>(this ControllerBase controller, Result<Unit, E> result)
        where E : Enum
    {
        if (result.IsOk)
            return controller.Ok();

        var error = result.UnwrapError();
        return error switch
        {
            // Common error patterns
            _ when error.ToString() == "NotFound" => controller.NotFound(),
            _ when error.ToString() == "Forbidden" => controller.Forbid(),
            _ when error.ToString() == "Unauthorized" => controller.Unauthorized(),
            _ when error.ToString() == "ValidationError" => controller.BadRequest(),
            
            // Domain-specific error mappings
            ImageError.NotFound => controller.NotFound(),
            ImageError.Forbidden => controller.Forbid(),
            ImageError.InvalidPagination => controller.BadRequest("Invalid pagination parameters"),
            
            PlatformTokenError.NotFound => controller.NotFound(),
            PlatformTokenError.Forbidden => controller.Forbid(),
            PlatformTokenError.InternalError => controller.StatusCode(500, "Internal server error"),
            
            DeleteError.NotFound => controller.NotFound(),
            DeleteError.Forbidden => controller.Forbid(),
            
            AdminError.NotFound => controller.NotFound(),
            AdminError.AlreadyApproved => controller.BadRequest("User is already approved"),
            AdminError.AlreadyAdministrator => controller.BadRequest("User is already an administrator"),
            AdminError.NotAdministrator => controller.BadRequest("User is not an administrator"),
            AdminError.InternalError => controller.StatusCode(500, "Internal server error"),
            
            _ => controller.StatusCode(500, "Internal server error")
        };
    }

    /// <summary>
    /// Converts only the error case of a Result to an IActionResult.
    /// Throws if the result is Ok - use this when you need to handle success differently (e.g., return a file stream).
    /// </summary>
    public static IActionResult ToActionResultForError<T, E>(this ControllerBase controller, Result<T, E> result)
        where E : Enum
    {
        if (result.IsOk)
            throw new InvalidOperationException("Result is Ok, cannot convert error. Use ToActionResult or handle success separately.");

        var error = result.UnwrapError();
        return error switch
        {
            // Common error patterns
            _ when error.ToString() == "NotFound" => controller.NotFound(),
            _ when error.ToString() == "Forbidden" => controller.Forbid(),
            _ when error.ToString() == "Unauthorized" => controller.Unauthorized(),
            _ when error.ToString() == "ValidationError" || error.ToString() == "InvalidPagination" => controller.BadRequest(),
            
            // Domain-specific error mappings
            ImageError.NotFound => controller.NotFound(),
            ImageError.Forbidden => controller.Forbid(),
            ImageError.InvalidPagination => controller.BadRequest("Invalid pagination parameters"),
            
            TagError.InvalidPagination => controller.BadRequest("Invalid pagination parameters"),
            TagError.InternalError => controller.StatusCode(500, "Internal server error"),
            
            CharacterError.InvalidPagination => controller.BadRequest("Invalid pagination parameters"),
            CharacterError.InternalError => controller.StatusCode(500, "Internal server error"),
            
            PlatformTokenError.NotFound => controller.NotFound(),
            PlatformTokenError.Forbidden => controller.Forbid(),
            PlatformTokenError.InternalError => controller.StatusCode(500, "Internal server error"),
            
            DeleteError.NotFound => controller.NotFound(),
            DeleteError.Forbidden => controller.Forbid(),
            
            ImportImageError.EmptyImage => controller.BadRequest("Empty image"),
            ImportImageError.FailedToGetTags => controller.Problem("Tag retrieval failure"),
            ImportImageError.ImageParseFailed => controller.BadRequest("Invalid image format"),
            ImportImageError.ImageStoreError => controller.Problem("Image IO failure"),
            ImportImageError.AlreadyOwned => controller.BadRequest("Image is already owned"),
            
            AuthError.InvalidCredentials => controller.Unauthorized("Invalid credentials"),
            AuthError.UserAlreadyExists => controller.BadRequest("User already exists"),
            AuthError.InvalidPassword => controller.BadRequest("Invalid password"),
            AuthError.InvalidEmail => controller.BadRequest("Invalid email"),
            AuthError.InternalError => controller.StatusCode(500, "Internal server error"),
            
            AdminError.NotFound => controller.NotFound(),
            AdminError.AlreadyApproved => controller.BadRequest("User is already approved"),
            AdminError.AlreadyAdministrator => controller.BadRequest("User is already an administrator"),
            AdminError.NotAdministrator => controller.BadRequest("User is not an administrator"),
            AdminError.InternalError => controller.StatusCode(500, "Internal server error"),
            
            _ => controller.StatusCode(500, "Internal server error")
        };
    }
}
