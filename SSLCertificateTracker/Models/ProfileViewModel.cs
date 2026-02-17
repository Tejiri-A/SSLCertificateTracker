using System.ComponentModel.DataAnnotations;

public class ProfileViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    [Display(Name = "Notification Email")]
    public string NotificationEmail { get; set; } = null!;
    
    [Display(Name = "Enable Email Notifications")]
    public bool EnableEmailNotifications { get; set; } 
    
    [Range(1, 90)]
    [Display(Name = "Days Before Expiry to Notify")]
    public int DaysBeforeExpiryToNotify { get; set; }
}