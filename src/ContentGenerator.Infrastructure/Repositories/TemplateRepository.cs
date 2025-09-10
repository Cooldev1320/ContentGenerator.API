using Microsoft.EntityFrameworkCore;
using ContentGenerator.Core.Entities;
using ContentGenerator.Core.Enums;
using ContentGenerator.Core.Interfaces;
using ContentGenerator.Infrastructure.Data;

namespace ContentGenerator.Infrastructure.Repositories;

public class TemplateRepository : BaseRepository<Template>, ITemplateRepository
{
    public TemplateRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Template>> GetByCategoryAsync(TemplateCategory category, bool activeOnly = true)
    {
        var query = _dbSet.Where(t => t.Category == category);
        
        if (activeOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        return await query
            .Include(t => t.CreatedBy)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Template>> GetFeaturedTemplatesAsync(int count = 10)
    {
        return await _dbSet
            .Where(t => t.IsActive)
            .Include(t => t.CreatedBy)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Template>> GetPremiumTemplatesAsync()
    {
        return await _dbSet
            .Where(t => t.IsPremium && t.IsActive)
            .Include(t => t.CreatedBy)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Template>> SearchTemplatesAsync(string searchTerm)
    {
        return await _dbSet
            .Where(t => t.IsActive && 
                       (t.Name.Contains(searchTerm) || 
                        (t.Description != null && t.Description.Contains(searchTerm))))
            .Include(t => t.CreatedBy)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Template> Templates, int TotalCount)> GetPagedTemplatesAsync(
        int skip, int take, TemplateCategory? category = null, bool? isPremium = null, 
        bool activeOnly = true, string? searchTerm = null, string? sortBy = null, bool sortDescending = false)
    {
        var query = _dbSet.AsQueryable();

        // Apply filters
        if (activeOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        if (category.HasValue)
        {
            query = query.Where(t => t.Category == category.Value);
        }

        if (isPremium.HasValue)
        {
            query = query.Where(t => t.IsPremium == isPremium.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => t.Name.Contains(searchTerm) || 
                                    (t.Description != null && t.Description.Contains(searchTerm)));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => sortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
            "category" => sortDescending ? query.OrderByDescending(t => t.Category) : query.OrderBy(t => t.Category),
            "ispremium" => sortDescending ? query.OrderByDescending(t => t.IsPremium) : query.OrderBy(t => t.IsPremium),
            _ => sortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        // Apply pagination and include related data
        var templates = await query
            .Include(t => t.CreatedBy)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (templates, totalCount);
    }

    public override async Task<Template?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}