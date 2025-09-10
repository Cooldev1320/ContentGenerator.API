using Microsoft.EntityFrameworkCore;
using ContentGenerator.Core.Entities;
using ContentGenerator.Core.Interfaces;
using ContentGenerator.Infrastructure.Data;

namespace ContentGenerator.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet.AnyAsync(u => u.Username == username);
    }

    public async Task<IEnumerable<User>> GetUsersBySubscriptionTierAsync(Core.Enums.SubscriptionTier tier)
    {
        return await _dbSet
            .Where(u => u.SubscriptionTier == tier && u.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetExpiredSubscriptionsAsync()
    {
        return await _dbSet
            .Where(u => u.SubscriptionExpiresAt.HasValue && 
                       u.SubscriptionExpiresAt < DateTime.UtcNow &&
                       u.IsActive)
            .ToListAsync();
    }

    public async Task ResetMonthlyExportsAsync()
    {
        await _context.Database.ExecuteSqlRawAsync(
            "UPDATE contentgenerator.users SET monthly_exports_used = 0");
    }

    public override async Task<User?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(u => u.Subscriptions.OrderByDescending(s => s.CreatedAt))
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}