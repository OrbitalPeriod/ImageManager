using System.ComponentModel.DataAnnotations;
using ImageManager.Repositories;

namespace ImageManager.Data.Models;

public class Folder : IEntity<Guid>, IUserOwnedEntity
{
    [Key]
    public Guid Id { get; private set; }

    public required string Name { get; set; }

    public required string UserId { get; init; }
    public User User { get; private set; } = null!;

    public ICollection<FolderImage> FolderImages { get; set; } = [];
}
