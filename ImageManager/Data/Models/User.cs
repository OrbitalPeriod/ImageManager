using ImageManager.Repositories;
using Microsoft.AspNetCore.Identity;

namespace ImageManager.Data.Models;

public class User : IdentityUser, IEntity<string>
{
    public Publicity DefaultPublicity { get; set; } = Publicity.Open;
    public bool IsApproved { get; set; } = false;

    public ICollection<UserOwnedImage> Images { get; set; } = [];
    public ICollection<ShareToken> ShareTokens { get; set; } = [];
    public ICollection<DownloadedImage> DownloadedImages { get; set; } = [];
    public ICollection<PlatformToken> PlatformTokens { get; set; } = [];
    public ICollection<Folder> Folders { get; set; } = [];
}
