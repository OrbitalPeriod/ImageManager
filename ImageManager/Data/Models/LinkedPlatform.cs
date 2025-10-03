namespace ImageManager.Data.Models;

public class LinkedPlatform
{
    public Platform Platform { get; set; }
    public required string AccountId { get; set; }
    public required User User { get; set; }
}