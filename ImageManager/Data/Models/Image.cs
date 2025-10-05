using System.Collections;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ImageManager.Data.Models;

public class Image
{
    public Guid Id { get; set; }
    public ulong Hash { get; set; }
    public AgeRating AgeRating { get; set; }

    public ICollection<UserOwnedImage> UserOwnedImages { get; set; } = [];

    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<Character> Characters { get; set; } = [];

    public DownloadedImage? DownloadedImage { get; set; }
    public int DownloadedImageId { get; set; }
}