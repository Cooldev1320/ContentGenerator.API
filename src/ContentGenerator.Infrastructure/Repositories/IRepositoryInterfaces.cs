using ContentGenerator.Core.Entities;
using ContentGenerator.Core.Enums;
using ContentGenerator.Core.Interfaces;

namespace ContentGenerator.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
    Task<IEnumerable<User>> GetUsersBySubscriptionTierAsync(SubscriptionTier tier);
    Task<IEnumerable<User>> GetExpiredSubscriptionsAsync();
    Task ResetMonthlyExportsAsync();
}

public interface ITemplateRepository : IRepository<Template>
{
    Task<IEnumerable<Template>> GetByCategoryAsync(TemplateCategory category, bool activeOnly = true);
    Task<IEnumerable<Template>> GetFeaturedTemplatesAsync(int count = 10);
    Task<IEnumerable<Template>> GetPremiumTemplatesAsync();
    Task<IEnumerable<Template>> SearchTemplatesAsync(string searchTerm);
    Task<(IEnumerable<Template> Templates, int TotalCount)> GetPagedTemplatesAsync(
        int skip, int take, TemplateCategory? category = null, bool? isPremium = null, 
        bool activeOnly = true, string? searchTerm = null, string? sortBy = null, bool sortDescending = false);
}

public interface IProjectRepository : IRepository<Project>
{
    Task<IEnumerable<Project>> GetUserProjectsAsync(Guid userId, ProjectStatus? status = null);
    Task<Project?> GetUserProjectByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<Project>> GetRecentProjectsAsync(Guid userId, int count = 5);
    Task<(IEnumerable<Project> Projects, int TotalCount)> GetPagedUserProjectsAsync(
        Guid userId, int skip, int take, ProjectStatus? status = null, 
        string? searchTerm = null, string? sortBy = null, bool sortDescending = false);
    Task<int> GetUserProjectCountAsync(Guid userId);
    Task<bool> UserOwnsProjectAsync(Guid projectId, Guid userId);
}

public interface IHistoryRepository : IRepository<History>
{
    Task<IEnumerable<History>> GetUserHistoryAsync(Guid userId, int? limit = null);
    Task<IEnumerable<History>> GetRecentHistoryAsync(Guid userId, int count = 10);
    Task<(IEnumerable<History> History, int TotalCount)> GetPagedUserHistoryAsync(
        Guid userId, int skip, int take, ActionType? actionType = null, 
        Guid? projectId = null, DateTime? fromDate = null, DateTime? toDate = null,
        string? sortBy = null, bool sortDescending = false);
    Task DeleteUserHistoryAsync(Guid userId, DateTime? olderThan = null);
    Task<IEnumerable<History>> GetProjectHistoryAsync(Guid projectId);
}

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<Subscription?> GetActiveUserSubscriptionAsync(Guid userId);
    Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);
    Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(Guid userId);
    Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync();
    Task<IEnumerable<Subscription>> GetSubscriptionsByStatusAsync(SubscriptionStatus status);
    Task DeactivateUserSubscriptionsAsync(Guid userId);
}