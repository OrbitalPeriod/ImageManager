namespace ImageManager.Data.Models;

public class Image
{
    public required int Id { get; set; }
    public required ICollection<Tag> Tags { get; set; } = [];
    public required ICollection<Character> Characters { get; set; } = [];
    public required ICollection<ShareToken>  ShareTokens { get; set; } = [];
    public required Rating Rating { get; set; }
    public required User User { get; set; }
}