using ImageManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, IdentityRole, string>(options)
{
    public DbSet<Character> Characters { get; set; }
    public DbSet<DownloadedImage> DownloadedImages { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<ShareToken> ShareTokens { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PlatformToken> PlatformTokens { get; set; }
    public DbSet<UserOwnedImage> UserOwnedImages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("identity");

        builder.Entity<Image>().HasMany(e => e.Tags).WithMany(e => e.Image);
        builder.Entity<Image>().HasMany(e => e.Characters).WithMany(e => e.Images);
        builder.Entity<Image>().HasOne(i => i.DownloadedImage).WithOne(di => di.Image).IsRequired(false).HasForeignKey<DownloadedImage>(di => di.ImageId).IsRequired(false);
        builder.Entity<UserOwnedImage>().HasMany(e => e.ShareTokens).WithOne(e => e.UserOwnedImage).HasForeignKey(st => st.UserOwnedImageId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<User>().HasMany(e => e.Images).WithOne(e => e.User);
        builder.Entity<User>().HasMany(e => e.ShareTokens).WithOne(e => e.User).HasForeignKey(st => st.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<User>().HasMany(e => e.PlatformTokens).WithOne(e => e.User);
    }
}