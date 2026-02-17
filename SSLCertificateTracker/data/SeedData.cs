using SSLCertificateTracker.data;
using SSLCertificateTracker.Models;
using Microsoft.EntityFrameworkCore;

public static class SeedData
{
    public static async Task Initialize(ApplicationDbContext context)
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // Check if any users exist
        if (context.Users.Any())
        {
            return; // DB has been seeded
        }

        // Create initial admin user from insert_user.sql info
        var adminUser = new ApplicationUser
        {
            IdgNumber = "user", // Based on whoami from insert_user.sql
            Name = "Tejiri Amrasa",
            UserName = @"desktop-uotjeai\user",
            Email = "tjthecreator8@gmail.com",
            IsActive = true,
            IsAdmin = true,
            CreatedAt = DateTime.UtcNow
        };

        var settings = new UserSettings
        {
            UserId = adminUser.Id,
            NotificationEmail = adminUser.Email ?? "tjthecreator8@gmail.com",
            EnableEmailNotifications = true,
            DaysBeforeExpiryToNotify = 30
        };

        adminUser.Settings = settings;
        
        context.Users.Add(adminUser);
        context.UserSettings.Add(settings);
        await context.SaveChangesAsync();
    }
}