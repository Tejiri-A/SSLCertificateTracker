namespace SSLCertificateTracker.Models;
using System.ComponentModel.DataAnnotations;

public class SslCertificate
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    [Display(Name = "Domain Name")]
    public string DomainName { get; set; } = null!;
    
    [Required]
    [Display(Name = "Date Issued")]
    public DateTime DateIssued { get; set; }
    
    [Display(Name = "Date Re-issued")]
    public DateTime? DateReissued { get; set; }
    
    [Required]
    [Display(Name = "Expiration Date")]
    public DateTime ExpirationDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key for user
    public string? UserId { get; set; }
    public virtual ApplicationUser? User { get; set; }
}