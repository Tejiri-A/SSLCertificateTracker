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

        /*
        // Create admin user logic for Windows Auth if needed.
        // For now, we rely on existing Windows Authentication users.
        // You can manually add a user here that matches your Windows User Name.
        
        var adminUser = new ApplicationUser
        {
            UserName = @"DOMAIN\Username", // Replace with your actual domain\username
            FullName = "Administrator",
            Email = "admin@example.com",
            CreatedAt = DateTime.UtcNow
        };
        
        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
        */
    }
}