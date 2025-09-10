using ContentGenerator.Core.Enums;

namespace ContentGenerator.Core.DTOs.Subscription;

public class UpdateSubscriptionDto
{
    public SubscriptionTier Tier { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MonthlyExportsLimit { get; set; }
}

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public SubscriptionTier Tier { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string? StripeCustomerId { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}