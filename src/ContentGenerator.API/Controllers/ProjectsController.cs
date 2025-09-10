using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.DTOs.Project;
using ContentGenerator.Core.Interfaces;

namespace ContentGenerator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Get user projects with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProjectListDto>>>> GetProjects([FromQuery] ProjectFilterDto filter)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<PagedResult<ProjectListDto>>.ErrorResult("Invalid token"));
        }

        var result = await _projectService.GetUserProjectsAsync(userId.Value, filter);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get project by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetProject(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<ProjectDto>.ErrorResult("Invalid token"));
        }

        var result = await _projectService.GetProjectByIdAsync(id, userId.Value);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return NotFound(result);
    }

    /// <summary>
    /// Get recent projects
    /// </summary>
    [HttpGet("recent")]
    public async Task<ActionResult<ApiResponse<List<ProjectListDto>>>> GetRecentProjects([FromQuery] int count = 5)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<List<ProjectListDto>>.ErrorResult("Invalid token"));
        }

        var result = await _projectService.GetRecentProjectsAsync(userId.Value, count);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Create new project
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> CreateProject([FromBody] CreateProjectDto createProjectDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<ProjectDto>.ErrorResult("Invalid token"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<ProjectDto>.ErrorResult(errors));
        }

        var result = await _projectService.CreateProjectAsync(createProjectDto, userId.Value);

        if (result.Success)
        {
            _logger.LogInformation("Project {ProjectName} created by user {UserId}", createProjectDto.Name, userId);
            return CreatedAtAction(nameof(GetProject), new { id = result.Data!.Id }, result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Update project
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> UpdateProject(Guid id, [FromBody] UpdateProjectDto updateProjectDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<ProjectDto>.ErrorResult("Invalid token"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<ProjectDto>.ErrorResult(errors));
        }

        var result = await _projectService.UpdateProjectAsync(id, updateProjectDto, userId.Value);

        if (result.Success)
        {
            _logger.LogInformation("Project {ProjectId} updated by user {UserId}", id, userId);
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Delete project
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteProject(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResult("Invalid token"));
        }

        var result = await _projectService.DeleteProjectAsync(id, userId.Value);

        if (result.Success)
        {
            _logger.LogInformation("Project {ProjectId} deleted by user {UserId}", id, userId);
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Duplicate project
    /// </summary>
    [HttpPost("{id}/duplicate")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> DuplicateProject(Guid id, [FromBody] DuplicateProjectDto? duplicateDto = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<ProjectDto>.ErrorResult("Invalid token"));
        }

        var result = await _projectService.DuplicateProjectAsync(id, userId.Value, duplicateDto?.NewName);

        if (result.Success)
        {
            _logger.LogInformation("Project {ProjectId} duplicated by user {UserId}", id, userId);
            return CreatedAtAction(nameof(GetProject), new { id = result.Data!.Id }, result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Export project
    /// </summary>
    [HttpPost("{id}/export")]
    public async Task<ActionResult<ApiResponse<string>>> ExportProject(Guid id, [FromBody] ExportProjectDto exportProjectDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<string>.ErrorResult("Invalid token"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<string>.ErrorResult(errors));
        }

        exportProjectDto.ProjectId = id;
        var result = await _projectService.ExportProjectAsync(exportProjectDto, userId.Value);

        if (result.Success)
        {
            _logger.LogInformation("Project {ProjectId} exported by user {UserId}", id, userId);
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Update project thumbnail
    /// </summary>
    [HttpPost("{id}/thumbnail")]
    public async Task<ActionResult<ApiResponse>> UpdateThumbnail(Guid id, [FromBody] UpdateThumbnailDto updateThumbnailDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResult("Invalid token"));
        }

        var result = await _projectService.UpdateThumbnailAsync(id, updateThumbnailDto.ThumbnailUrl, userId.Value);

        if (result.Success)
        {
            _logger.LogInformation("Project {ProjectId} thumbnail updated by user {UserId}", id, userId);
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

public class DuplicateProjectDto
{
    public string? NewName { get; set; }
}

public class UpdateThumbnailDto
{
    public string ThumbnailUrl { get; set; } = string.Empty;
}