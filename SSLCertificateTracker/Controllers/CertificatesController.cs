using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSLCertificateTracker.data;
using SSLCertificateTracker.Models;

namespace SSLCertificateTracker.Controllers;
[Authorize]
public class CertificatesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CertificatesController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // GET Certificates
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
        var certificates = await _context.SslCertificates
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.ExpirationDate).ToListAsync();
        
        return View(certificates);
    }
    
    // GET Certificates/Create
    public IActionResult Create()
    {
        return View();
    }
    
    // POST Certificates/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SslCertificate certificate)
    {
        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            certificate.UserId = userId;
            certificate.CreatedAt = DateTime.UtcNow;
            certificate.UpdatedAt = DateTime.UtcNow;

            _context.Add(certificate);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(certificate);
    }
    
    // GET Certificate/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var certificate = await _context.SslCertificates.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (certificate == null)
        {
            return NotFound();
        }

        return View(certificate);
    }
    
    
    // POST: Certificates/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SslCertificate certificate)
    {
        if (id != certificate.Id) return NotFound();
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var existingCert = await _context.SslCertificates
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            
        if (existingCert == null) return NotFound();
        
        if (ModelState.IsValid)
        {
            try
            {
                existingCert.DomainName = certificate.DomainName;
                existingCert.DateIssued = certificate.DateIssued;
                existingCert.DateReissued = certificate.DateReissued;
                existingCert.ExpirationDate = certificate.ExpirationDate;
                existingCert.UpdatedAt = DateTime.UtcNow;
                
                _context.Update(existingCert);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CertificateExists(certificate.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(certificate);
    }
    
    // POST: Certificates/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var certificate = await _context.SslCertificates
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            
        if (certificate == null) return NotFound();
        
        _context.SslCertificates.Remove(certificate);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CertificateExists(int id)
    {
        return _context.SslCertificates.Any(e => e.Id == id);
    }
}