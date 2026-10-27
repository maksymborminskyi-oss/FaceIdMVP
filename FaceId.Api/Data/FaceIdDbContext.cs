using FaceId.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FaceId.Api.Data;

public class FaceIdDbContext : DbContext
{
    public FaceIdDbContext(DbContextOptions<FaceIdDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<Face> Faces { get; set; }
    public DbSet<FacePhoto> FacePhotos { get; set; }
    public DbSet<AccessLog> AccessLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("app_user");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                .HasColumnName("id");

            entity.Property(u => u.FirstName)
                .HasColumnName("first_name");

            entity.Property(u => u.LastName)
                .HasColumnName("last_name");

            entity.Property(u => u.Email)
                .HasColumnName("email");

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.CreatedAt)
                .HasColumnName("created_at");
        });

        modelBuilder.Entity<Face>(entity =>
        {
            entity.ToTable("face");

            entity.HasKey(f => f.Id);

            entity.Property(f => f.Id)
                .HasColumnName("id");

            entity.Property(f => f.AppUserId)
                .HasColumnName("app_user_id");

            entity.Property(f => f.Embedding)
                .HasColumnName("embedding")
                .HasColumnType("real[]");

            entity.Property(f => f.CreatedAt)
                .HasColumnName("created_at");

            entity.HasIndex(f => f.AppUserId)
                .IsUnique();

            entity.HasOne(f => f.User)
                .WithOne(u => u.Face)
                .HasForeignKey<Face>(f => f.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FacePhoto>(entity =>
        {
            entity.ToTable("face_photo");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id)
                .HasColumnName("id");

            entity.Property(p => p.FaceId)
                .HasColumnName("face_id");

            entity.Property(p => p.PhotoPath)
                .HasColumnName("photo_path")
                .IsRequired();

            entity.Property(p => p.CreatedAt)
                .HasColumnName("created_at");

            entity.HasOne(p => p.Face)
                .WithMany(f => f.Photos)
                .HasForeignKey(p => p.FaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AccessLog>(entity =>
        {
            entity.ToTable("access_log");

            entity.HasKey(log => log.Id);

            entity.Property(log => log.Id)
                .HasColumnName("id");

            entity.Property(log => log.UserId)
                .HasColumnName("user_id");

            entity.Property(log => log.RecognizedAt)
                .HasColumnName("recognized_at");

            entity.Property(log => log.Status)
                .HasColumnName("status")
                .IsRequired();

            entity.HasOne(log => log.User)
                .WithMany(user => user.AccessLogs)
                .HasForeignKey(log => log.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}