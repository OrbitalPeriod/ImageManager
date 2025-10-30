using System.ComponentModel.DataAnnotations;
using ImageManager.Repositories;

namespace ImageManager.Data.Models;

public class Tag : IEntity<Guid>
{
    [Key] public Guid Id { get; private set; }
    public string Name { get; init; } = null!;
    public ICollection<Image> Image { get; init; } = [];

}