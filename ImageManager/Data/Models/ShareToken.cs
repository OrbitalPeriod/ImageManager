namespace ImageManager.Data.Models;

public class ShareToken
{
    public Guid Id { get; set; }
    public User User { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public Image Image { get; set; } = null!;
    public Guid ImageId { get; set; }


    public DateTime Created { get; set; }
    public DateTime Expires { get; set; }
    public string Token { get; set; } = null!;
    public bool IsExpired => DateTime.UtcNow > Expires;
}