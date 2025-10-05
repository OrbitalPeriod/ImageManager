namespace ImageManager.Data.Models;

public class DownloadedImage
{
    public int Id { get; set; }

    public required Platform Platform { get; set; }
    public required int PlatformImageId { get; set; }


    public Guid ImageId { get; set; }
    public Image Image { get; set; } = null!;
}