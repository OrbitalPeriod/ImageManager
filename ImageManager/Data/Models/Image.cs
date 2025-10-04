using Microsoft.EntityFrameworkCore.Metadata;

namespace ImageManager.Data.Models;

public class Image
{
    public Guid Id { get; set; }
    public ulong Hash { get; set; }
    public AgeRating AgeRating { get; set; }
    public Publicity Publicity { get; set; }
    
    public int DownloadedImageId { get; set; }
    public DownloadedImage? DownloadedImage { get; set; }
    
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;
    
    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<Character> Characters { get; set; } = [];
    public ICollection<ShareToken> ShareTokens { get; set; } = [];
}