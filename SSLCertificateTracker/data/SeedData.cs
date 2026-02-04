using Microsoft.AspNetCore.Identity;
using SSLCertificateTracker.data;
using SSLCertificateTracker.Models;

public static class SeedData
{
    public static async Task Initialize(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // Create roles
        string[] roleNames = { "Admin", "User" };
        
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create admin user
        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrator",
                EmailConfirmed = true
            };
            
            var createAdmin = await userManager.CreateAsync(adminUser, "Admin@123");
            if (createAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                
                // Create user settings
                var userSettings = new UserSettings
                {
                    UserId = adminUser.Id,
                    NotificationEmail = adminEmail,
                    EnableEmailNotifications = true,
                    DaysBeforeExpiryToNotify = 30
                };
                
                context.UserSettings.Add(userSettings);
                await context.SaveChangesAsync();
            }
        }
    }
}