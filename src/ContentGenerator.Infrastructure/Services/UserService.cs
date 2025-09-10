using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using ContentGenerator.Core.DTOs.Auth;
using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.DTOs.Subscription;
using ContentGenerator.Core.Entities;
using ContentGenerator.Core.Interfaces;
using ContentGenerator.Infrastructure.Data;
using ContentGenerator.Infrastructure.Repositories;

namespace ContentGenerator.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IHistoryService _historyService;

    public UserService(
        IUserRepository userRepository,
        ApplicationDbContext context,
        IConfiguration configuration,
        IHistoryService historyService)
    {
        _userRepository = userRepository;
        _context = context;
        _configuration = configuration;
        _historyService = historyService;
    }

    public async Task<ApiResponse<TokenDto>> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null || !user.IsActive)
            {
                return ApiResponse<TokenDto>.ErrorResult("Invalid email or password");
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return ApiResponse<TokenDto>.ErrorResult("Invalid email or password");
            }

            var token = GenerateJwtToken(user);
            var userDto = MapToUserDto(user);

            return ApiResponse<TokenDto>.SuccessResult(new TokenDto
            {
                AccessToken = token,
                RefreshToken = GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = userDto
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<TokenDto>.ErrorResult($"Login failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TokenDto>> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            // Validate unique constraints
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
            {
                return ApiResponse<TokenDto>.ErrorResult("Email already exists");
            }

            if (await _userRepository.UsernameExistsAsync(registerDto.Username))
            {
                return ApiResponse<TokenDto>.ErrorResult("Username already exists");
            }

            // Create new user
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FullName = registerDto.FullName,
                SubscriptionTier = Core.Enums.SubscriptionTier.Free,
                MonthlyExportsLimit = 5,
                IsActive = true
            };

            await _userRepository.AddAsync(user);
            await _context.SaveChangesAsync();

            // Change this line in RegisterAsync method:
await _historyService.LogActionAsync(user.Id, Core.Enums.ActionType.UserRegistered, null, new { Action = "User registered" });
            var token = GenerateJwtToken(user);
            var userDto = MapToUserDto(user);

            return ApiResponse<TokenDto>.SuccessResult(new TokenDto
            {
                AccessToken = token,
                RefreshToken = GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = userDto
            }, "Registration successful");
        }
        catch (Exception ex)
        {
            return ApiResponse<TokenDto>.ErrorResult($"Registration failed: {ex.Message}");
        }
    }

    public Task<ApiResponse<TokenDto>> RefreshTokenAsync(string refreshToken)
    {
        // For simplicity, we'll implement basic refresh logic
        // In production, you'd want to store and validate refresh tokens properly
        return Task.FromResult(ApiResponse<TokenDto>.ErrorResult("Refresh token invalid or expired"));
    }

    public Task<ApiResponse> LogoutAsync(Guid userId)
    {
        // In a real implementation, you'd invalidate the refresh token
        return Task.FromResult(ApiResponse.SuccessResult("Logged out successfully"));
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserDto>.ErrorResult("User not found");
            }

            var userDto = MapToUserDto(user);
            return ApiResponse<UserDto>.SuccessResult(userDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<UserDto>.ErrorResult($"Failed to get user: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserDto>.ErrorResult("User not found");
            }

            // Check username uniqueness if changed
            if (!string.IsNullOrEmpty(updateUserDto.Username) && 
                updateUserDto.Username != user.Username &&
                await _userRepository.UsernameExistsAsync(updateUserDto.Username))
            {
                return ApiResponse<UserDto>.ErrorResult("Username already exists");
            }

            // Update fields
            if (!string.IsNullOrEmpty(updateUserDto.Username))
                user.Username = updateUserDto.Username;
            
            if (!string.IsNullOrEmpty(updateUserDto.FullName))
                user.FullName = updateUserDto.FullName;
            
            if (!string.IsNullOrEmpty(updateUserDto.AvatarUrl))
                user.AvatarUrl = updateUserDto.AvatarUrl;

            await _userRepository.UpdateAsync(user);
            await _context.SaveChangesAsync();

            var userDto = MapToUserDto(user);
            return ApiResponse<UserDto>.SuccessResult(userDto, "Profile updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<UserDto>.ErrorResult($"Failed to update user: {ex.Message}");
        }
    }

    public async Task<ApiResponse> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse.ErrorResult("User not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return ApiResponse.ErrorResult("Current password is incorrect");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            await _userRepository.UpdateAsync(user);
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResult("Password changed successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to change password: {ex.Message}");
        }
    }

    public async Task<ApiResponse> ResetMonthlyExportsAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse.ErrorResult("User not found");
            }

            user.MonthlyExportsUsed = 0;
            await _userRepository.UpdateAsync(user);
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResult("Monthly exports reset successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to reset exports: {ex.Message}");
        }
    }

    public async Task<ApiResponse> UpdateSubscriptionAsync(Guid userId, UpdateSubscriptionDto updateSubscriptionDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse.ErrorResult("User not found");
            }

            var oldTier = user.SubscriptionTier;
            user.SubscriptionTier = updateSubscriptionDto.Tier;
            user.SubscriptionExpiresAt = updateSubscriptionDto.ExpiresAt;
            
            if (updateSubscriptionDto.MonthlyExportsLimit.HasValue)
            {
                user.MonthlyExportsLimit = updateSubscriptionDto.MonthlyExportsLimit.Value;
            }

            await _userRepository.UpdateAsync(user);
            await _context.SaveChangesAsync();

            // Log subscription change
            await _historyService.LogActionAsync(userId, Core.Enums.ActionType.SubscriptionUpgraded, null, 
                new { OldTier = oldTier.ToString(), NewTier = updateSubscriptionDto.Tier.ToString() });

            return ApiResponse.SuccessResult("Subscription updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to update subscription: {ex.Message}");
        }
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT_KEY"] ?? "");
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<User?> GetUserFromTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);
            
            var userIdClaim = jwt.Claims.FirstOrDefault(x => x.Type == "userId");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }

            return await _userRepository.GetByIdAsync(userId);
        }
        catch
        {
            return null;
        }
    }

    #region Private Methods

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JWT_KEY"] ?? "");
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("userId", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("username", user.Username),
                new Claim("subscriptionTier", user.SubscriptionTier.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            SubscriptionTier = user.SubscriptionTier,
            SubscriptionExpiresAt = user.SubscriptionExpiresAt,
            MonthlyExportsUsed = user.MonthlyExportsUsed,
            MonthlyExportsLimit = user.MonthlyExportsLimit,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    #endregion
}