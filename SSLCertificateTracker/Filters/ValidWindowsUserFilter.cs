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

            var userName = user.Identity.Name;
            Console.WriteLine($"[ValidWindowsUserFilter] Authenticated User: '{userName}'");
            
            if (string.IsNullOrEmpty(userName))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            // Check if user exists in DB
            // Note: In high traffic, you might want to cache this check.
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            if (dbUser == null)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            // Add dbUser.Id as ClaimTypes.NameIdentifier so controllers can use it
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, dbUser.Id)
            };
            var appIdentity = new System.Security.Claims.ClaimsIdentity(claims);
            user.AddIdentity(appIdentity);
        }
    }
}
