namespace ImageManager.Data.Models;

public class ShareToken
{
    public int Id { get; set; }
    public User User { get; set; }
    public string UserId { get; set; }
    public Image Image { get; set; }
    public Guid ImageId { get; set; }


    public DateTime Created { get; set; }
    public DateTime Expires { get; set; }
    public string Token { get; set; }
}