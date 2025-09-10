using Microsoft.EntityFrameworkCore;
using ContentGenerator.Core.Entities;
using ContentGenerator.Core.Enums;
using ContentGenerator.Core.Interfaces;
using ContentGenerator.Infrastructure.Data;

namespace ContentGenerator.Infrastructure.Repositories;

public class SubscriptionRepository : BaseRepository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Subscription?> GetActiveUserSubscriptionAsync(Guid userId)
    {
        return await _dbSet
            .Include(s => s.User)
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
    {
        return await _dbSet
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);
    }

    public async Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(Guid userId)
    {
        return await _dbSet
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync()
    {
        return await _dbSet
            .Include(s => s.User)
            .Where(s => s.Status == SubscriptionStatus.Active && 
                       s.CurrentPeriodEnd.HasValue && 
                       s.CurrentPeriodEnd < DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByStatusAsync(SubscriptionStatus status)
    {
        return await _dbSet
            .Include(s => s.User)
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task DeactivateUserSubscriptionsAsync(Guid userId)
    {
        var activeSubscriptions = await _dbSet
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active)
            .ToListAsync();

        foreach (var subscription in activeSubscriptions)
        {
            subscription.Status = SubscriptionStatus.Canceled;
        }
    }
}