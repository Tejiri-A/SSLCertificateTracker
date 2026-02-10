using Hangfire.Dashboard;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Allow any authenticated Windows user
        return httpContext.User.Identity?.IsAuthenticated == true;
        
    }
}