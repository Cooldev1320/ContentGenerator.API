using System.ComponentModel.DataAnnotations;
using ContentGenerator.Core.Enums;
using Newtonsoft.Json;

namespace ContentGenerator.Core.Entities;

public class Project : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    
    public Guid? TemplateId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string CanvasDataJson { get; set; } = string.Empty;
    
    public string? ThumbnailUrl { get; set; }
    
    public int Width { get; set; } = 1080;
    
    public int Height { get; set; } = 1080;
    
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    
    public DateTime? ExportedAt { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Template? Template { get; set; }
    public virtual ICollection<History> HistoryEntries { get; set; } = new List<History>();
    
    // Helper property for JSON serialization
    [JsonIgnore]
    public object? CanvasData
    {
        get => string.IsNullOrEmpty(CanvasDataJson) ? null : JsonConvert.DeserializeObject(CanvasDataJson);
        set => CanvasDataJson = value == null ? string.Empty : JsonConvert.SerializeObject(value);
    }
}