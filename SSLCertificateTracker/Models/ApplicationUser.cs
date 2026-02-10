using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SSLCertificateTracker.Models;

public class ApplicationUser
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = null!;

    [Required]
    [StringLength(256)]
    public string UserName { get; set; } = null!;

    [EmailAddress]
    [StringLength(256)]
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual UserSettings Settings { get; set; } = null!;
}