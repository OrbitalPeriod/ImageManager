namespace ImageManager.Data.Models;

public class DownloadedImage
{
    public int Id { get; set; }

    public Guid UserId { get; set; }
    public required User User { get; set; } = null!;

    public required Platform Platform { get; set; }
    public required int PlatformImageId { get; set; }

    public Guid? ImageId { get; set; } = null;
    public Image? Image { get; set; } = null;

}