namespace ImageManager.Data.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Image> Image { get; set; } = [];

}