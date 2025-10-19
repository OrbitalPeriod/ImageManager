using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImageManager.Repositories;

namespace ImageManager.Data.Models;

public class Image : IEntity<Guid>
{
    [Key]
    public Guid Id { get; init; }
    public required ulong Hash { get; init; }
    public required AgeRating AgeRating { get; init; }

    public ICollection<UserOwnedImage> UserOwnedImages { get; init; } = [];

    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<Character> Characters { get; init; } = [];

    public required int? DownloadedImageId { get; init; }
    public DownloadedImage? DownloadedImage { get; private set; }
}