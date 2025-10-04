namespace ImageManager.Data.Models;

public class PlatformToken
{
    public int Id { get; set; }
    public required string PlatformUserId { get; set; }
    public DateTime? Expires { get; set; }
    public bool IsExpired => DateTime.UtcNow > Expires;
    public required string Token { get; set; }
    public Platform Platform { get; set; }
    public bool CheckPrivate { get; set; } = false;

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;
}