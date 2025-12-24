using System.ComponentModel.DataAnnotations;
using ImageManager.Repositories;

namespace ImageManager.Data.Models;

public class PlatformSyncLog : IEntity<Guid>
{
    [Key]
    public Guid Id { get; init; }

    public Guid PlatformTokenId { get; init; }

    public PlatformToken PlatformToken { get; private set; } = null!;

    public required bool Success { get; init; }
    public required string Message { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
