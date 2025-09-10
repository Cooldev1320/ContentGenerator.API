using System.ComponentModel.DataAnnotations;
using ContentGenerator.Core.Enums;

namespace ContentGenerator.Core.Entities;

public class Subscription : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public SubscriptionTier Tier { get; set; }
    
    [StringLength(255)]
    public string? StripeSubscriptionId { get; set; }
    
    [StringLength(255)]
    public string? StripeCustomerId { get; set; }
    
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    
    public DateTime? CurrentPeriodStart { get; set; }
    
    public DateTime? CurrentPeriodEnd { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}