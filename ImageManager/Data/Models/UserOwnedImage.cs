using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImageManager.Repositories;

namespace ImageManager.Data.Models;

public class UserOwnedImage : IEntity<Guid>
{
    [Key]
    public Guid Id { get; private set; }

    public required string UserId { get; init; }
    public User User { get; private set; } = null!;

    public Publicity Publicity { get; set; }
    public ICollection<ShareToken> ShareTokens { get; set; } = [];
    
    public required Guid ImageId { get; init; }
    public Image Image { get; private set; } = null!;

}