using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SSLCertificateTracker.data;
using Microsoft.EntityFrameworkCore;

namespace SSLCertificateTracker.Filters
{
    public class ValidWindowsUserFilter : IAsyncAuthorizationFilter
    {
        private readonly ApplicationDbContext _context;

        public ValidWindowsUserFilter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Skip check for AccessDenied page to avoid loop
            var actionDescriptor = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
            if (actionDescriptor != null)
            {
                var controllerName = actionDescriptor.ControllerName;
                var actionName = actionDescriptor.ActionName;

                if (controllerName == "Account" && actionName == "AccessDenied")
                {
                    return;
                }
            }

            var user = context.HttpContext.User;
            
            // If not authenticated via Windows Auth yet
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                // This might happen if Anonymous is enabled, but we disabled it.
                // However, just in case:
                context.Result = new ChallengeResult();
                return;
            }

            var fullUserName = user.Identity.Name;
            Console.WriteLine($"[ValidWindowsUserFilter] Authenticated User: '{fullUserName}'");
            
            if (string.IsNullOrEmpty(fullUserName))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            // Extract IDG Number (often the username part of DOMAIN\Username)
            var idgNumber = fullUserName.Contains("\\") ? fullUserName.Split('\\')[1] : fullUserName;

            // Check if user exists in DB by searching various fields for a match
            // We check IdgNumber, UserName (full), and also case-insensitivity
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => 
                u.IdgNumber == idgNumber || 
                u.UserName == fullUserName || 
                u.IdgNumber == fullUserName ||
                u.UserName == idgNumber);

            if (dbUser == null)
            {
                Console.WriteLine($"[ValidWindowsUserFilter] Access Denied: User not found in database. Checked IDG: '{idgNumber}' and FullName: '{fullUserName}'");
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            if (!dbUser.IsActive)
            {
                Console.WriteLine($"[ValidWindowsUserFilter] Access Denied: User '{dbUser.IdgNumber}' found but is marked INACTIVE.");
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            // Update Name and Email from claims if available
            var identityEmail = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var identityName = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? user.Identity.Name;

            bool updated = false;
            // If email is provided by windows auth and different from DB
            if (!string.IsNullOrEmpty(identityEmail) && dbUser.Email != identityEmail)
            {
                dbUser.Email = identityEmail;
                updated = true;
            }
            // If we have a better name from windows auth
            if (!string.IsNullOrEmpty(identityName) && (string.IsNullOrEmpty(dbUser.Name) || dbUser.Name == dbUser.IdgNumber))
            {
                 // Usually identityName in Windows Auth is DOMAIN\User, we might want a friendly name if available
                 var givenName = user.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;
                 var surname = user.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value;
                 
                 if (!string.IsNullOrEmpty(givenName))
                 {
                     dbUser.Name = $"{givenName} {surname}".Trim();
                     updated = true;
                 }
            }

            if (updated)
            {
                _context.Users.Update(dbUser);
                await _context.SaveChangesAsync();
            }

            // Add dbUser.Id as ClaimTypes.NameIdentifier so controllers can use it
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, dbUser.Id),
                new System.Security.Claims.Claim("IdgNumber", dbUser.IdgNumber)
            };

            if (dbUser.IsAdmin)
            {
                claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin"));
            }

            var appIdentity = new System.Security.Claims.ClaimsIdentity(claims);
            user.AddIdentity(appIdentity);
        }
    }
}
