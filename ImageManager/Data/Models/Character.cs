namespace ImageManager.Data.Models;

public class Character
{
    public required int Id { get; set; }
    public required ICollection<Image> Image { get; set; } = [];
    public required string Name { get; set; }
}