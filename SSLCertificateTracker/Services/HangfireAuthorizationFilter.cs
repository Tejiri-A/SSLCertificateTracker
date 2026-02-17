using Hangfire.Dashboard;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Allow only Admin users
        return httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("Admin");
        
    }
}