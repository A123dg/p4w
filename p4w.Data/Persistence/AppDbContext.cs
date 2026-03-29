using Microsoft.EntityFrameworkCore;
using p4w.Core.Models;

namespace p4w.Data.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<MediaLink> MediaLinks => Set<MediaLink>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.GoogleUserId).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);

            entity.HasOne(d => d.Role)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("Location");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LocationName).HasMaxLength(255);
            entity.Property(e => e.Address).HasMaxLength(500);

            entity.HasOne(d => d.Owner)
                .WithMany(p => p.OwnedLocations)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("Review");
            entity.HasKey(e => e.Id);

            entity.HasOne(d => d.User)
                .WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Location)
                .WithMany(p => p.Reviews)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comment");
            entity.HasKey(e => e.Id);

            entity.HasOne(d => d.Review)
                .WithMany(p => p.Comments)
                .HasForeignKey(d => d.ReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.User)
                .WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Parent)
                .WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Media>(entity =>
        {
            entity.ToTable("Media");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).HasMaxLength(1000);
            entity.Property(e => e.MimeType).HasMaxLength(255);
        });

        modelBuilder.Entity<MediaLink>(entity =>
        {
            entity.ToTable("MediaLink");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.MediaType).HasMaxLength(100);

            entity.HasOne(d => d.User)
                .WithMany(p => p.MediaLinks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Media)
                .WithMany(p => p.MediaLinks)
                .HasForeignKey(d => d.MediaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("Report");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.TargetType).HasMaxLength(100);

            entity.HasOne(d => d.User)
                .WithMany(p => p.Reports)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
