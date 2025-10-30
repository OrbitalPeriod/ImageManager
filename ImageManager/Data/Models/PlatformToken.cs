using System.ComponentModel.DataAnnotations;
using ImageManager.Repositories;

namespace ImageManager.Data.Models;

public class PlatformToken : IEntity<Guid>
{
    [Key]
    public Guid Id { get; private set; }
    public required string PlatformUserId { get; init; }
    public DateTime? Expires { get; init; }
    public bool IsExpired => DateTime.UtcNow > Expires;
    public required string Token { get; init; }
    public Platform Platform { get; init; }
    public bool CheckPrivate { get; init; } = false;

    public required string UserId { get; init; }
    public User User { get; private set; } = null!;
}