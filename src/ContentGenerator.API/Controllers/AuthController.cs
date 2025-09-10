using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContentGenerator.Core.DTOs.Auth;
using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.Interfaces;

namespace ContentGenerator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Login user
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<TokenDto>>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<TokenDto>.ErrorResult(errors));
        }

        var result = await _userService.LoginAsync(loginDto);
        
        if (result.Success)
        {
            _logger.LogInformation("User {Email} logged in successfully", loginDto.Email);
            return Ok(result);
        }

        _logger.LogWarning("Failed login attempt for email: {Email}", loginDto.Email);
        return Unauthorized(result);
    }

    /// <summary>
    /// Register new user
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<TokenDto>>> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<TokenDto>.ErrorResult(errors));
        }

        var result = await _userService.RegisterAsync(registerDto);

        if (result.Success)
        {
            _logger.LogInformation("User {Email} registered successfully", registerDto.Email);
            return CreatedAtAction(nameof(GetProfile), new { }, result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
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
    /// Update user profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile([FromBody] UpdateUserDto updateUserDto)
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
    /// Change password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
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

        var result = await _userService.ChangePasswordAsync(userId.Value, changePasswordDto);

        if (result.Success)
        {
            _logger.LogInformation("User {UserId} changed password", userId);
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<TokenDto>>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var result = await _userService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return Unauthorized(result);
    }

    /// <summary>
    /// Logout user
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResult("Invalid token"));
        }

        var result = await _userService.LogoutAsync(userId.Value);
        
        _logger.LogInformation("User {UserId} logged out", userId);
        return Ok(result);
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

public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = string.Empty;
}