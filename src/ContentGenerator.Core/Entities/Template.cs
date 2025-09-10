using System.ComponentModel.DataAnnotations;
using ContentGenerator.Core.Enums;
using Newtonsoft.Json;

namespace ContentGenerator.Core.Entities;

public class Template : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public TemplateCategory Category { get; set; }
    
    [Required]
    public string ThumbnailUrl { get; set; } = string.Empty;
    
    [Required]
    public string TemplateDataJson { get; set; } = string.Empty;
    
    public bool IsPremium { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    public Guid? CreatedById { get; set; }
    
    // Navigation properties
    public virtual User? CreatedBy { get; set; }
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    
    // Helper property for JSON serialization
    [JsonIgnore]
    public object? TemplateData
    {
        get => string.IsNullOrEmpty(TemplateDataJson) ? null : JsonConvert.DeserializeObject(TemplateDataJson);
        set => TemplateDataJson = value == null ? string.Empty : JsonConvert.SerializeObject(value);
    }
}