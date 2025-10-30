#region Usings

using ImageManager.Data.Models;
#endregion

namespace ImageManager.Services.UserInfo;

/// <summary>
/// DTO returned by the user‑info service.
/// It contains the authenticated user’s basic profile data and the default publicity level
/// used when that user uploads a new image.
/// </summary>
public record GetUserInfoResponse(
    string Id,

    string? UserName,

    string? Email,

    Publicity DefaultPublicity);