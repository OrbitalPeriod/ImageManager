using Microsoft.AspNetCore.Identity;

namespace ImageManager.Data.Models;

public class User : IdentityUser
{
    public Publicity DefaultPublicity { get; set; } = Publicity.Open;
    
    public ICollection<Image> Images { get; set; } = [];
    public ICollection<ShareToken> ShareTokens { get; set; } = [];
    public ICollection<DownloadedImage> DownloadedImages { get; set; } = [];
    public ICollection<PlatformToken> PlatformTokens { get; set; } = [];
}