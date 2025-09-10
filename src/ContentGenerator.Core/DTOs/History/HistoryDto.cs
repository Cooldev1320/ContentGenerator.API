using ContentGenerator.Core.Enums;

namespace ContentGenerator.Core.DTOs.History;

public class HistoryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ProjectId { get; set; }
    public ActionType ActionType { get; set; }
    public object? ActionData { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Related data
    public string? ProjectName { get; set; }
    public string ActionDescription { get; set; } = string.Empty;
}

public class HistoryFilterDto
{
    public ActionType? ActionType { get; set; }
    public Guid? ProjectId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}