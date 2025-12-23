using System.ComponentModel.DataAnnotations;
using ImageManager.Repositories;

namespace ImageManager.Data.Models;

public class PlatformSyncLog : IEntity<Guid>
{
    [Key]
    public Guid Id { get; init; }

    public required PlatformToken PlatformToken { get; init; }

    public required bool Success { get; set; }
    public required string Message { get; set; }
}