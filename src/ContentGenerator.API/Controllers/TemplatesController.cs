using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.DTOs.Template;
using ContentGenerator.Core.Enums;
using ContentGenerator.Core.Interfaces;

namespace ContentGenerator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<TemplatesController> _logger;

    public TemplatesController(ITemplateService templateService, ILogger<TemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    /// <summary>
    /// Get templates with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TemplateListDto>>>> GetTemplates([FromQuery] TemplateFilterDto filter)
    {
        var result = await _templateService.GetTemplatesAsync(filter);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get template by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TemplateDto>>> GetTemplate(Guid id)
    {
        var result = await _templateService.GetTemplateByIdAsync(id);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return NotFound(result);
    }

    /// <summary>
    /// Get featured templates
    /// </summary>
    [HttpGet("featured")]
    public async Task<ActionResult<ApiResponse<List<TemplateListDto>>>> GetFeaturedTemplates([FromQuery] int count = 10)
    {
        var result = await _templateService.GetFeaturedTemplatesAsync(count);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get templates by category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<ApiResponse<List<TemplateListDto>>>> GetTemplatesByCategory(
        TemplateCategory category, 
        [FromQuery] int count = 20)
    {
        var result = await _templateService.GetTemplatesByCategoryAsync(category, count);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Create new template (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<TemplateDto>>> CreateTemplate([FromBody] CreateTemplateDto createTemplateDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<TemplateDto>.ErrorResult(errors));
        }

        var userId = GetCurrentUserId();
        var result = await _templateService.CreateTemplateAsync(createTemplateDto, userId);

        if (result.Success)
        {
            _logger.LogInformation("Template {TemplateName} created by user {UserId}", createTemplateDto.Name, userId);
            return CreatedAtAction(nameof(GetTemplate), new { id = result.Data!.Id }, result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Update template (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<TemplateDto>>> UpdateTemplate(Guid id, [FromBody] UpdateTemplateDto updateTemplateDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<TemplateDto>.ErrorResult(errors));
        }

        var result = await _templateService.UpdateTemplateAsync(id, updateTemplateDto);

        if (result.Success)
        {
            _logger.LogInformation("Template {TemplateId} updated", id);
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Delete template (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> DeleteTemplate(Guid id)
    {
        var result = await _templateService.DeleteTemplateAsync(id);

        if (result.Success)
        {
            _logger.LogInformation("Template {TemplateId} deleted", id);
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Toggle template status (Admin only)
    /// </summary>
    [HttpPost("{id}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> ToggleTemplateStatus(Guid id)
    {
        var result = await _templateService.ToggleTemplateStatusAsync(id);

        if (result.Success)
        {
            _logger.LogInformation("Template {TemplateId} status toggled", id);
            return Ok(result);
        }

        return BadRequest(result);
    }

    #region Private Methods

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    #endregion
}