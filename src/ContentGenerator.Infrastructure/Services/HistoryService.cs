using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.DTOs.History;
using ContentGenerator.Core.Entities;
using ContentGenerator.Core.Enums;
using ContentGenerator.Core.Interfaces;
using ContentGenerator.Infrastructure.Data;
using ContentGenerator.Infrastructure.Repositories;

namespace ContentGenerator.Infrastructure.Services;

public class HistoryService : IHistoryService
{
    private readonly IHistoryRepository _historyRepository;
    private readonly ApplicationDbContext _context;

    public HistoryService(IHistoryRepository historyRepository, ApplicationDbContext context)
    {
        _historyRepository = historyRepository;
        _context = context;
    }

    public async Task<ApiResponse> LogActionAsync(Guid userId, ActionType actionType, Guid? projectId = null, object? actionData = null)
    {
        try
        {
            var history = new History
            {
                UserId = userId,
                ProjectId = projectId,
                ActionType = actionType,
                ActionData = actionData
            };

            await _historyRepository.AddAsync(history);
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResult("Action logged successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to log action: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResult<HistoryDto>>> GetUserHistoryAsync(Guid userId, HistoryFilterDto filter)
    {
        try
        {
            var (historyItems, totalCount) = await _historyRepository.GetPagedUserHistoryAsync(
                userId,
                filter.Page > 0 ? (filter.Page - 1) * filter.PageSize : 0,
                filter.PageSize,
                filter.ActionType,
                filter.ProjectId,
                filter.FromDate,
                filter.ToDate,
                filter.SortBy,
                filter.SortDescending);

            var historyDtos = historyItems.Select(MapToHistoryDto).ToList();

            var result = new PagedResult<HistoryDto>
            {
                Items = historyDtos,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };

            return ApiResponse<PagedResult<HistoryDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<HistoryDto>>.ErrorResult($"Failed to get user history: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<HistoryDto>>> GetRecentHistoryAsync(Guid userId, int count = 10)
    {
        try
        {
            var history = await _historyRepository.GetRecentHistoryAsync(userId, count);
            var historyDtos = history.Select(MapToHistoryDto).ToList();

            return ApiResponse<List<HistoryDto>>.SuccessResult(historyDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<HistoryDto>>.ErrorResult($"Failed to get recent history: {ex.Message}");
        }
    }

    public async Task<ApiResponse> ClearHistoryAsync(Guid userId, DateTime? olderThan = null)
    {
        try
        {
            await _historyRepository.DeleteUserHistoryAsync(userId, olderThan);
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResult("History cleared successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to clear history: {ex.Message}");
        }
    }

    #region Private Methods

    private static HistoryDto MapToHistoryDto(History history)
    {
        return new HistoryDto
        {
            Id = history.Id,
            UserId = history.UserId,
            ProjectId = history.ProjectId,
            ActionType = history.ActionType,
            ActionData = history.ActionData,
            CreatedAt = history.CreatedAt,
            ProjectName = history.Project?.Name,
            ActionDescription = GetActionDescription(history.ActionType, history.ActionData)
        };
    }

    private static string GetActionDescription(ActionType actionType, object? actionData)
    {
        return actionType switch
        {
            ActionType.ProjectCreated => "Created a new project",
            ActionType.ProjectUpdated => "Updated project",
            ActionType.ProjectExported => "Exported project",
            ActionType.TemplateUsed => "Used a template",
            ActionType.SubscriptionUpgraded => "Upgraded subscription",
            ActionType.SubscriptionDowngraded => "Downgraded subscription",
            ActionType.SubscriptionCanceled => "Canceled subscription",
            _ => "Unknown action"
        };
    }

    #endregion
}