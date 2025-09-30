namespace ImageManager.Data.Models;

public class LinkedPlatform
{
    public Platform _platform { get; set; }
    public required string accountId { get; set; }
    public required User User { get; set; }
}