using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSLCertificateTracker.data;
using SSLCertificateTracker.Models;

namespace SSLCertificateTracker.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Users
    public async Task<IActionResult> Index()
    {
        return View(await _context.Users.OrderBy(u => u.Name).ToListAsync());
    }

    // GET: Users/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApplicationUser user)
    {
        if (ModelState.IsValid)
        {
            // Check if IDG number already exists
            if (await _context.Users.AnyAsync(u => u.IdgNumber == user.IdgNumber))
            {
                ModelState.AddModelError("IdgNumber", "A user with this IDG Number already exists.");
                return View(user);
            }

            // For Windows Auth, UserName often matches IDG Number or DOMAIN\IDG Number
            // We'll store it as IDG Number for lookup if domain isn't provided
            if (string.IsNullOrEmpty(user.UserName))
            {
                user.UserName = user.IdgNumber;
            }

            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null) return NotFound();

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        return View(user);
    }

    // POST: Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, ApplicationUser user)
    {
        if (id != user.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var dbUser = await _context.Users.FindAsync(id);
                if (dbUser == null) return NotFound();

                dbUser.IdgNumber = user.IdgNumber;
                dbUser.Name = user.Name;
                dbUser.Email = user.Email;
                dbUser.IsActive = user.IsActive;
                dbUser.IsAdmin = user.IsAdmin;
                dbUser.UserName = user.UserName;

                _context.Update(dbUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    // GET: Users/Delete/5
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null) return NotFound();

        var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
        if (user == null) return NotFound();

        return View(user);
    }

    // POST: Users/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool UserExists(string id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}
