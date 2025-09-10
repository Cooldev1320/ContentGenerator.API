using Microsoft.EntityFrameworkCore;
using ContentGenerator.Core.Entities;
using ContentGenerator.Core.Enums;
using ContentGenerator.Core.Interfaces;
using ContentGenerator.Infrastructure.Data;

namespace ContentGenerator.Infrastructure.Repositories;

public class ProjectRepository : BaseRepository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Project>> GetUserProjectsAsync(Guid userId, ProjectStatus? status = null)
    {
        var query = _dbSet.Where(p => p.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        return await query
            .Include(p => p.Template)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Project?> GetUserProjectByIdAsync(Guid id, Guid userId)
    {
        return await _dbSet
            .Include(p => p.Template)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
    }

    public async Task<IEnumerable<Project>> GetRecentProjectsAsync(Guid userId, int count = 5)
    {
        return await _dbSet
            .Where(p => p.UserId == userId)
            .Include(p => p.Template)
            .OrderByDescending(p => p.UpdatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Project> Projects, int TotalCount)> GetPagedUserProjectsAsync(
        Guid userId, int skip, int take, ProjectStatus? status = null, 
        string? searchTerm = null, string? sortBy = null, bool sortDescending = false)
    {
        var query = _dbSet.Where(p => p.UserId == userId);

        // Apply filters
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        IOrderedQueryable<Project> orderedQuery = sortBy?.ToLower() switch
        {
            "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "status" => sortDescending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
            "createdat" => sortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            "exportedat" => sortDescending ? query.OrderByDescending(p => p.ExportedAt) : query.OrderBy(p => p.ExportedAt),
            _ => sortDescending ? query.OrderByDescending(p => p.UpdatedAt) : query.OrderBy(p => p.UpdatedAt)
        };

        // Apply pagination and include related data
        var projects = await orderedQuery
            .Include(p => p.Template)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (projects, totalCount);
    }

    public async Task<int> GetUserProjectCountAsync(Guid userId)
    {
        return await _dbSet.CountAsync(p => p.UserId == userId);
    }

    public async Task<bool> UserOwnsProjectAsync(Guid projectId, Guid userId)
    {
        return await _dbSet.AnyAsync(p => p.Id == projectId && p.UserId == userId);
    }

    public override async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Template)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}