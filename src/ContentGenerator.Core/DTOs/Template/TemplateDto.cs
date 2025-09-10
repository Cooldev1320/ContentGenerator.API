using ContentGenerator.Core.Enums;

namespace ContentGenerator.Core.DTOs.Template;

public class TemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TemplateCategory Category { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public object? TemplateData { get; set; }
    public bool IsPremium { get; set; }
    public bool IsActive { get; set; }
    public Guid? CreatedById { get; set; }
    public string? CreatedByUsername { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TemplateListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TemplateCategory Category { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public bool IsPremium { get; set; }
    public string? CreatedByUsername { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TemplateFilterDto
{
    public TemplateCategory? Category { get; set; }
    public bool? IsPremium { get; set; }
    public bool? IsActive { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}