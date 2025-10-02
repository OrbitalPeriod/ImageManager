namespace ImageManager.Data.Models;

public class DownloadedImage
{
    public int Id { get; set; }
    public required User User { get; set; }
    public required Platform Platform { get; set; }
    public Guid? ImageId { get; set; }
    public int DownloadedId { get; set; }
    public Image? Image { get; set; }
}