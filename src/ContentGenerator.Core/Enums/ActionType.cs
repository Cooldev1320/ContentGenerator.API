namespace ContentGenerator.Core.Enums;

public enum ActionType
{
    // User actions
    UserRegistered,
    
    // Project actions
    ProjectCreated,
    ProjectUpdated,
    ProjectExported,
    
    // Template actions
    TemplateUsed,
    
    // Subscription actions
    SubscriptionUpgraded,
    SubscriptionDowngraded,
    SubscriptionCanceled
}