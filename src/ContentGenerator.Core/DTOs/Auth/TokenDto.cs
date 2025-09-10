using System.ComponentModel.DataAnnotations;
using ContentGenerator.Core.Enums;

namespace ContentGenerator.Core.DTOs.Auth;

public class TokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public SubscriptionTier SubscriptionTier { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }
    public int MonthlyExportsUsed { get; set; }
    public int MonthlyExportsLimit { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateUserDto
{
    [StringLength(50, MinimumLength = 3)]
    public string? Username { get; set; }

    [StringLength(100)]
    public string? FullName { get; set; }

    public string? AvatarUrl { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}