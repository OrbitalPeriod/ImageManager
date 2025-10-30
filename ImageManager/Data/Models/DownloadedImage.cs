using System.ComponentModel.DataAnnotations;
using ImageManager.Repositories;

namespace ImageManager.Data.Models;

public class DownloadedImage : IEntity<Guid>
{
    [Key]
    public Guid Id { get; private set; }
    public required Platform Platform { get; init; }
    public required int PlatformImageId { get; init; }


    public required Guid ImageId { get; init; }
    public Image Image { get; private set; } = null!;
}