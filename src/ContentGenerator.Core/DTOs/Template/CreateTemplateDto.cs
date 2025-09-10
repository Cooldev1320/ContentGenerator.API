using System.ComponentModel.DataAnnotations;
using ContentGenerator.Core.Enums;

namespace ContentGenerator.Core.DTOs.Template;

public class CreateTemplateDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public TemplateCategory Category { get; set; }

    [Required]
    [Url]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [Required]
    public object TemplateData { get; set; } = null!;

    public bool IsPremium { get; set; } = false;
}

public class UpdateTemplateDto
{
    [StringLength(100, MinimumLength = 3)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public TemplateCategory? Category { get; set; }

    [Url]
    public string? ThumbnailUrl { get; set; }

    public object? TemplateData { get; set; }

    public bool? IsPremium { get; set; }

    public bool? IsActive { get; set; }
}