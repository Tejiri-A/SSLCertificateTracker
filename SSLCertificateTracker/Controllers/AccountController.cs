using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSLCertificateTracker.data;
using SSLCertificateTracker.Models;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };
            
            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                
                // Create default user settings
                var userSettings = new UserSettings
                {
                    UserId = user.Id,
                    NotificationEmail = model.Email,
                    EnableEmailNotifications = true,
                    DaysBeforeExpiryToNotify = 30
                };
                
                _context.UserSettings.Add(userSettings);
                await _context.SaveChangesAsync();
                
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Certificates");
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        
        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }
        
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }
        
        var userSettings = await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == user.Id);
        
        var model = new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            NotificationEmail = userSettings?.NotificationEmail ?? user.Email,
            EnableEmailNotifications = userSettings?.EnableEmailNotifications ?? true,
            DaysBeforeExpiryToNotify = userSettings?.DaysBeforeExpiryToNotify ?? 30
        };
        
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            
            var userSettings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == user.Id);
            
            // Update user
            user.FullName = model.FullName;
            await _userManager.UpdateAsync(user);
            
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

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Certificates");
    }
}