using Microsoft.EntityFrameworkCore;
using ContentGenerator.Core.Entities;
using ContentGenerator.Core.Enums;
using ContentGenerator.Core.Interfaces;
using ContentGenerator.Infrastructure.Data;

namespace ContentGenerator.Infrastructure.Repositories;

public class HistoryRepository : BaseRepository<History>, IHistoryRepository
{
    public HistoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<History>> GetUserHistoryAsync(Guid userId, int? limit = null)
    {
        IOrderedQueryable<History> query = _dbSet
            .Where(h => h.UserId == userId)
            .Include(h => h.Project)
            .OrderByDescending(h => h.CreatedAt);

        if (limit.HasValue)
        {
            return await query.Take(limit.Value).ToListAsync();
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<History>> GetRecentHistoryAsync(Guid userId, int count = 10)
    {
        return await _dbSet
            .Where(h => h.UserId == userId)
            .Include(h => h.Project)
            .OrderByDescending(h => h.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<(IEnumerable<History> History, int TotalCount)> GetPagedUserHistoryAsync(
        Guid userId, int skip, int take, ActionType? actionType = null, 
        Guid? projectId = null, DateTime? fromDate = null, DateTime? toDate = null,
        string? sortBy = null, bool sortDescending = false)
    {
        var query = _dbSet.Where(h => h.UserId == userId);

        // Apply filters
        if (actionType.HasValue)
        {
            query = query.Where(h => h.ActionType == actionType.Value);
        }

        if (projectId.HasValue)
        {
            query = query.Where(h => h.ProjectId == projectId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(h => h.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(h => h.CreatedAt <= toDate.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        IOrderedQueryable<History> orderedQuery = sortBy?.ToLower() switch
        {
            "actiontype" => sortDescending ? query.OrderByDescending(h => h.ActionType) : query.OrderBy(h => h.ActionType),
            _ => sortDescending ? query.OrderByDescending(h => h.CreatedAt) : query.OrderBy(h => h.CreatedAt)
        };

        // Apply pagination and include related data
        var history = await orderedQuery
            .Include(h => h.Project)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (history, totalCount);
    }

    public async Task DeleteUserHistoryAsync(Guid userId, DateTime? olderThan = null)
    {
        var query = _dbSet.Where(h => h.UserId == userId);

        if (olderThan.HasValue)
        {
            query = query.Where(h => h.CreatedAt < olderThan.Value);
        }

        var historyToDelete = await query.ToListAsync();
        _dbSet.RemoveRange(historyToDelete);
    }

    public async Task<IEnumerable<History>> GetProjectHistoryAsync(Guid projectId)
    {
        return await _dbSet
            .Where(h => h.ProjectId == projectId)
            .Include(h => h.User)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }
}