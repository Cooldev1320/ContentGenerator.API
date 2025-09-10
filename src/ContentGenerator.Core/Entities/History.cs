using System.ComponentModel.DataAnnotations;
using ContentGenerator.Core.Enums;
using Newtonsoft.Json;

namespace ContentGenerator.Core.Entities;

public class History : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    
    public Guid? ProjectId { get; set; }
    
    [Required]
    public ActionType ActionType { get; set; }
    
    public string? ActionDataJson { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Project? Project { get; set; }
    
    // Helper property for JSON serialization
    [JsonIgnore]
    public object? ActionData
    {
        get => string.IsNullOrEmpty(ActionDataJson) ? null : JsonConvert.DeserializeObject(ActionDataJson);
        set => ActionDataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
}