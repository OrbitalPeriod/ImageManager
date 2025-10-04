namespace ImageManager.Data.Models;

public class Character
{
    public int Id { get; set; }
    public required string Name { get; init; }
    public ICollection<Image> Images { get; init; } = [];
}