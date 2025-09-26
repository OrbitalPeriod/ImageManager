using ImageManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ImageManager.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, IdentityRole, string>(options)
{
    public DbSet<Character>  characters { get; set; }
    public DbSet<Image> images { get; set; }
    public DbSet<Tag> tags { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("identity");

        builder.Entity<Image>().HasMany(e => e.Tags).WithMany(e => e.Image);
        builder.Entity<Image>().HasMany(e => e.Characters).WithMany(e => e.Image);
        builder.Entity<User>().HasMany(e => e.Images).WithOne(e => e.User);
        builder.Entity<User>().HasMany(e => e.ShareTokens).WithOne(e => e.User).HasForeignKey(st => st.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Image>().HasMany(e => e.ShareTokens).WithOne(e => e.Image).HasForeignKey(st => st.ImageId).OnDelete(DeleteBehavior.Cascade);
    }
}