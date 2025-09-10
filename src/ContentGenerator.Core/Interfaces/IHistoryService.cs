using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.DTOs.History;
using ContentGenerator.Core.Enums;

namespace ContentGenerator.Core.Interfaces;

public interface IHistoryService
{
    Task<ApiResponse> LogActionAsync(Guid userId, ActionType actionType, Guid? projectId = null, object? actionData = null);
    Task<ApiResponse<PagedResult<HistoryDto>>> GetUserHistoryAsync(Guid userId, HistoryFilterDto filter);
    Task<ApiResponse<List<HistoryDto>>> GetRecentHistoryAsync(Guid userId, int count = 10);
    Task<ApiResponse> ClearHistoryAsync(Guid userId, DateTime? olderThan = null);
}