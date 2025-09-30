namespace ImageManager.Data.Models;

public class Image
{
    public Guid Id { get; set; }
    public ulong Hash { get; set; }
    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<Character> Characters { get; set; } = [];
    public ICollection<ShareToken> ShareTokens { get; set; } = [];
    public Rating Rating { get; set; }
    public User User { get; set; }
    public Publicity Publicity { get; set; }
    public DownloadedImage? DownloadedImage { get; set; }
}