using System.ComponentModel.DataAnnotations;
using ImageManager.Repositories;

namespace ImageManager.Data.Models;

public class FolderImage : IEntity<Guid>
{
    [Key]
    public Guid Id { get; private set; }

    public required Guid FolderId { get; init; }
    public Folder Folder { get; private set; } = null!;

    public required Guid UserOwnedImageId { get; init; }
    public UserOwnedImage UserOwnedImage { get; private set; } = null!;
}
