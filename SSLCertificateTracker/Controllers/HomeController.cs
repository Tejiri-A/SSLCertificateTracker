using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSLCertificateTracker.data;
using SSLCertificateTracker.Models;

namespace SSLCertificateTracker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = System.Security.Claims.ClaimTypes.NameIdentifier;
            var currentUserId = User.FindFirst(userId)?.Value;
            
            var certificates = await _context.SslCertificates
                .Where(c => c.UserId == currentUserId)
                .OrderByDescending(c => c.ExpirationDate)
                .ToListAsync();
                
            return View(certificates);
        }
        
        return View(new List<SslCertificate>());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}