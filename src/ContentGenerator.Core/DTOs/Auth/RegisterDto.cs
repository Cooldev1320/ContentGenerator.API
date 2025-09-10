using System.ComponentModel.DataAnnotations;

namespace ContentGenerator.Core.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [StringLength(100)]
    public string? FullName { get; set; }

    public bool AcceptTerms { get; set; } = false;
}