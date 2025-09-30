namespace ImageManager.Data.Models;

public class Character
{
    public int Id { get; set; }
    public ICollection<Image> Image { get; set; } = [];
    public required string Name { get; set; }
}