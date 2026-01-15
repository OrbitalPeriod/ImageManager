namespace ImageManager.Controllers;

/// <summary>
/// Error types specific to authentication operations.
/// </summary>
public enum AuthError
{
    /// <summary>Invalid credentials were provided.</summary>
    InvalidCredentials,
    
    /// <summary>The username or email is already taken.</summary>
    UserAlreadyExists,
    
    /// <summary>Password validation failed.</summary>
    InvalidPassword,
    
    /// <summary>Email validation failed.</summary>
    InvalidEmail,
    
    /// <summary>An internal error occurred during authentication.</summary>
    InternalError
}
