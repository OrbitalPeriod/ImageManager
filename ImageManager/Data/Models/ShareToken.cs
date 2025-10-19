using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImageManager.Repositories;

namespace ImageManager.Data.Models;

public class ShareToken : IEntity<Guid>
{
    [Key]
    public Guid Id { get; private set; }
    public required string UserId { get; init; } = null!;
    public User User { get; private set; } = null!;

    public required Guid UserOwnedImageId { get; init; }
    public UserOwnedImage UserOwnedImage { get; private set; } = null!;

    public DateTime Created { get; init; }
    public DateTime Expires { get; init; }
    public bool IsExpired => DateTime.UtcNow > Expires;
}