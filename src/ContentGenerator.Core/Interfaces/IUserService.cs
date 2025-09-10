using ContentGenerator.Core.DTOs.Auth;
using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.DTOs.Subscription;
using ContentGenerator.Core.Entities;

namespace ContentGenerator.Core.Interfaces;

public interface IUserService
{
    Task<ApiResponse<TokenDto>> LoginAsync(LoginDto loginDto);
    Task<ApiResponse<TokenDto>> RegisterAsync(RegisterDto registerDto);
    Task<ApiResponse<TokenDto>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse> LogoutAsync(Guid userId);
    Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId);
    Task<ApiResponse<UserDto>> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto);
    Task<ApiResponse> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
    Task<ApiResponse> ResetMonthlyExportsAsync(Guid userId);
    Task<ApiResponse> UpdateSubscriptionAsync(Guid userId, UpdateSubscriptionDto updateSubscriptionDto);
    Task<bool> ValidateTokenAsync(string token);
    Task<User?> GetUserFromTokenAsync(string token);
}