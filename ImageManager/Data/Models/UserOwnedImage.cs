namespace ImageManager.Data.Models;

public class UserOwnedImage
{
    public Guid Id { get; set; }

    public string UserId { get; set; }
    public User User { get; set; } = null!;

    public Publicity Publicity { get; set; }
    public ICollection<ShareToken> ShareTokens { get; set; } = [];
    
    public Guid ImageId { get; set; }
    public Image Image { get; set; } = null!;

}