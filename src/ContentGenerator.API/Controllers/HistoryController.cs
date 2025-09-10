using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.DTOs.History;
using ContentGenerator.Core.Interfaces;

namespace ContentGenerator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HistoryController : ControllerBase
{
    private readonly IHistoryService _historyService;
    private readonly ILogger<HistoryController> _logger;

    public HistoryController(IHistoryService historyService, ILogger<HistoryController> logger)
    {
        _historyService = historyService;
        _logger = logger;
    }

    /// <summary>
    /// Get user history with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<HistoryDto>>>> GetHistory([FromQuery] HistoryFilterDto filter)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<PagedResult<HistoryDto>>.ErrorResult("Invalid token"));
        }

        var result = await _historyService.GetUserHistoryAsync(userId.Value, filter);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get recent user history
    /// </summary>
    [HttpGet("recent")]
    public async Task<ActionResult<ApiResponse<List<HistoryDto>>>> GetRecentHistory([FromQuery] int count = 10)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<List<HistoryDto>>.ErrorResult("Invalid token"));
        }

        var result = await _historyService.GetRecentHistoryAsync(userId.Value, count);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Clear user history
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult<ApiResponse>> ClearHistory([FromQuery] DateTime? olderThan = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResult("Invalid token"));
        }

        var result = await _historyService.ClearHistoryAsync(userId.Value, olderThan);

        if (result.Success)
        {
            _logger.LogInformation("User {UserId} cleared history", userId);
            return Ok(result);
        }

        return BadRequest(result);
    }

    #region Private Methods

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    #endregion
}