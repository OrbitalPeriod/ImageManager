namespace ImageManager.Data.Models;

public class DownloadedImage
{
    public required User User { get; set; }
    public required Platform Platform { get; set; }
    public required int id  { get; set; }
}