using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContentGenerator.Core.DTOs.Auth;
using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.DTOs.Subscription;
using ContentGenerator.Core.Interfaces;

namespace ContentGenerator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user details
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<UserDto>.ErrorResult("Invalid token"));
        }

        var result = await _userService.GetUserByIdAsync(userId.Value);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return NotFound(result);
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateCurrentUser([FromBody] UpdateUserDto updateUserDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<UserDto>.ErrorResult("Invalid token"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<UserDto>.ErrorResult(errors));
        }

        var result = await _userService.UpdateUserAsync(userId.Value, updateUserDto);

        if (result.Success)
        {
            _logger.LogInformation("User {UserId} updated profile", userId);
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Update subscription
    /// </summary>
    [HttpPut("subscription")]
    public async Task<ActionResult<ApiResponse>> UpdateSubscription([FromBody] UpdateSubscriptionDto updateSubscriptionDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResult("Invalid token"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.ErrorResult(errors));
        }

        var result = await _userService.UpdateSubscriptionAsync(userId.Value, updateSubscriptionDto);

        if (result.Success)
        {
            _logger.LogInformation("User {UserId} updated subscription to {Tier}", userId, updateSubscriptionDto.Tier);
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Reset monthly exports (Admin only)
    /// </summary>
    [HttpPost("{userId}/reset-exports")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> ResetMonthlyExports(Guid userId)
    {
        var result = await _userService.ResetMonthlyExportsAsync(userId);

        if (result.Success)
        {
            _logger.LogInformation("Monthly exports reset for user {UserId}", userId);
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [HttpGet("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(Guid userId)
    {
        var result = await _userService.GetUserByIdAsync(userId);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return NotFound(result);
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