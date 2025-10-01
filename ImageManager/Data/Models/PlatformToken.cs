namespace ImageManager.Data.Models;

public class PlatformToken
{
    public int Id { get; set; }
    public Platform Platform { get; set; }
    public required string Token { get; set; }
    public required string PlatformUserId { get; set; }
    public DateTime? Expires { get; set; }
    public required User User { get; set; }
    public bool IsExpired => DateTime.UtcNow > Expires;
    public bool CheckPrivate { get; set; } = false;
}