// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SSLCertificateTracker.Models;

namespace SSLCertificateTracker.data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<SslCertificate> SslCertificates { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }

       protected override void OnModelCreating(ModelBuilder builder)
       {
              base.OnModelCreating(builder);

              builder.Entity<ApplicationUser>().HasIndex(u => u.IdgNumber).IsUnique();
              builder.Entity<ApplicationUser>().HasIndex(u => u.UserName).IsUnique();

              builder.Entity<SslCertificate>().HasIndex(s => s.DomainName).IsUnique();
              
              builder.Entity<SslCertificate>().HasOne(s => s.User)
                     .WithMany()
                     .HasForeignKey(s => s.UserId)
                     .OnDelete(DeleteBehavior.Cascade);

              builder.Entity<UserSettings>(entity =>
              {
                     entity.HasKey(s => s.UserId);

                     entity.HasOne(s => s.User)
                            .WithOne(u => u.Settings)
                            .HasForeignKey<UserSettings>(s => s.UserId)
                            .OnDelete(DeleteBehavior.Cascade);
              });
       }
}