using System.ComponentModel.DataAnnotations;
using ImageManager.Repositories;

namespace ImageManager.Data.Models;

public class Character : IEntity<Guid>
{
    [Key]
    public Guid Id { get; private set; }
    public required string Name { get; init; }
    public ICollection<Image> Images { get; init; } = [];
}