using System.ComponentModel.DataAnnotations;
using ContentGenerator.Core.Enums;

namespace ContentGenerator.Core.DTOs.Project;

public class CreateProjectDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    public Guid? TemplateId { get; set; }

    public object? CanvasData { get; set; }

    [Range(100, 5000)]
    public int Width { get; set; } = 1080;

    [Range(100, 5000)]
    public int Height { get; set; } = 1080;
}

public class UpdateProjectDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; set; }

    public object? CanvasData { get; set; }

    public string? ThumbnailUrl { get; set; }

    [Range(100, 5000)]
    public int? Width { get; set; }

    [Range(100, 5000)]
    public int? Height { get; set; }

    public ProjectStatus? Status { get; set; }
}

public class ExportProjectDto
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public string Format { get; set; } = "png"; // png, jpg, pdf

    [Range(72, 300)]
    public int Quality { get; set; } = 150; // DPI

    public bool IncludeWatermark { get; set; } = true;
}