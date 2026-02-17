using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSLCertificateTracker.data;
using SSLCertificateTracker.Models;
using System.Security.Claims;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("AccessDenied");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return RedirectToAction("AccessDenied");
        }
        
        var userSettings = await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == user.Id);
        
        var model = new ProfileViewModel
        {
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            NotificationEmail = userSettings?.NotificationEmail ?? user.Email,
            EnableEmailNotifications = userSettings?.EnableEmailNotifications ?? true,
            DaysBeforeExpiryToNotify = userSettings?.DaysBeforeExpiryToNotify ?? 30
        };
        
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
             return RedirectToAction("AccessDenied");
        }

        if (ModelState.IsValid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
               return RedirectToAction("AccessDenied");
            }
            
            var userSettings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == user.Id);
            
            // Update user
            user.Name = model.Name;
            _context.Users.Update(user);
            
            // Update or create user settings
            if (userSettings == null)
            {
                userSettings = new UserSettings
                {
                    UserId = user.Id,
                    NotificationEmail = model.NotificationEmail,
                    EnableEmailNotifications = model.EnableEmailNotifications,
                    DaysBeforeExpiryToNotify = model.DaysBeforeExpiryToNotify
                };
                _context.UserSettings.Add(userSettings);
            }
            else
            {
                userSettings.NotificationEmail = model.NotificationEmail;
                userSettings.EnableEmailNotifications = model.EnableEmailNotifications;
                userSettings.DaysBeforeExpiryToNotify = model.DaysBeforeExpiryToNotify;
            }
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }
        
        return View(model);
    }
}