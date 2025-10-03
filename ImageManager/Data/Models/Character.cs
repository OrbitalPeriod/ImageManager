namespace ImageManager.Data.Models;

public class Character
{
    public int Id { get; set; }
    public ICollection<Image> Image { get; init; } = [];
    public required string Name { get; init; }
}