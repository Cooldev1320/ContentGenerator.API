using System.ComponentModel.DataAnnotations;
using ContentGenerator.Core.Enums;

namespace ContentGenerator.Core.Entities;

public class User : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? FullName { get; set; }
    
    public string? AvatarUrl { get; set; }
    
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;
    
    public DateTime? SubscriptionExpiresAt { get; set; }
    
    public int MonthlyExportsUsed { get; set; } = 0;
    
    public int MonthlyExportsLimit { get; set; } = 5;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    public virtual ICollection<Template> CreatedTemplates { get; set; } = new List<Template>();
    public virtual ICollection<History> HistoryEntries { get; set; } = new List<History>();
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}