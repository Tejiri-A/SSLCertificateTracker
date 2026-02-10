using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SSLCertificateTracker.data;
using SSLCertificateTracker.Models;
using SSLCertificateTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Windows Authentication
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Negotiate.NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // specific policies if needed, otherwise default is used
    options.FallbackPolicy = options.DefaultPolicy;
});

// Add Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

// Register custom services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICertificateExpirationService, CertificateExpirationService>();
builder.Services.AddScoped<SSLCertificateTracker.Filters.ValidWindowsUserFilter>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<SSLCertificateTracker.Filters.ValidWindowsUserFilter>();
});

var app = builder.Build();

// Configure Hangfire dashboard (secure it properly)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = "SSL Certificate Jobs",
    DisplayStorageConnectionString = false
});

// Seed database with admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        // var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        await SeedData.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}


// Schedule recurring job to check certificates daily at 9 AM
RecurringJob.AddOrUpdate<ICertificateExpirationService>(
    "check-expiring-certificates",
    service => service.CheckExpiringCertificates(),
    "0 9 * * *",
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();