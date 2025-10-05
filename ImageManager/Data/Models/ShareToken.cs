namespace ImageManager.Data.Models;

public class ShareToken
{
    public Guid Id { get; set; }
    public User User { get; set; } = null!;
    public string UserId { get; set; } = null!;

    public Guid UserOwnedImageId { get; set; }
    public UserOwnedImage UserOwnedImage { get; set; } = null!;

    public DateTime Created { get; set; }
    public DateTime Expires { get; set; }
    public bool IsExpired => DateTime.UtcNow > Expires;
}