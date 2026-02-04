using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SSLCertificateTracker.Models;

public class UserSettings
{
     
    [Required] 
    public string UserId { get; set; } = null!;

    [EmailAddress]
    [Required]
    public string NotificationEmail { get; set; } = null!;

    public bool EnableEmailNotifications { get; set; } = true;
    
    public int DaysBeforeExpiryToNotify { get; set; } = 30;

    [Required] public virtual ApplicationUser User { get; set; } = null!;
}