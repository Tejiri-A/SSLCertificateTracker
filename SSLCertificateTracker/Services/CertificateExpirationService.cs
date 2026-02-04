using Microsoft.EntityFrameworkCore;
using SSLCertificateTracker.data;

namespace SSLCertificateTracker.Services;

public interface ICertificateExpirationService
{
    Task CheckExpiringCertificates();
}

public class CertificateExpirationService : ICertificateExpirationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<CertificateExpirationService> _logger;

    public CertificateExpirationService(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<CertificateExpirationService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task CheckExpiringCertificates()
    {
        _logger.LogInformation("Checking for expiring SSL certificates...");

        var today = DateTime.UtcNow.Date;
        var notificationDate = today.AddDays(30);
        
        // Find certificates expiring within 30 days
        var expiringCertificates = await _context.SslCertificates
            .Include(c => c.User)
            .ThenInclude(u => u.Settings)
            .Where(c => c.ExpirationDate <= notificationDate &&
                        c.ExpirationDate.Date > today &&
                        c.User.Settings.EnableEmailNotifications).ToListAsync();

        foreach (var cert in expiringCertificates)
        {
            var daysUntilExpiry = (cert.ExpirationDate.Date - today).Days;

            if (daysUntilExpiry <= cert.User.Settings.DaysBeforeExpiryToNotify)
            {
                try
                {
                    var subject = $"SSL Certificate Expiry Alert: {cert.DomainName}";
                    var message = $@"
                    <h3>SSL Certificate Expiry Notification</h3>
                    <p><strong>Domain:</strong> {cert.DomainName}</p>
                    <p><strong>Expiration Date:</strong> {cert.ExpirationDate.ToShortDateString()}</p>
                    <p><strong>Days until expiry:</strong> {daysUntilExpiry}</p>
                    <p><strong>Issued On:</strong> {cert.DateIssued.ToShortDateString()}</p>
                    <p>Please take action to renew this certificate.</p>";

                    await _emailService.SendEmailAsync(cert.User.Settings.NotificationEmail, subject, message);
                    
                    _logger.LogInformation($"Sent expiry notification for {cert.DomainName} to {cert.DateIssued}");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to send notification for {cert.DomainName} to {cert.DateIssued}");
                }
            }

        }
    }
}

