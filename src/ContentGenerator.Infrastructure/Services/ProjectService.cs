using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.DTOs.Project;
using ContentGenerator.Core.Entities;
using ContentGenerator.Core.Enums;
using ContentGenerator.Core.Interfaces;
using ContentGenerator.Infrastructure.Data;

namespace ContentGenerator.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly Repositories.IProjectRepository _projectRepository;
    private readonly Repositories.IUserRepository _userRepository;
    private readonly Repositories.ITemplateRepository _templateRepository;
    private readonly IHistoryService _historyService;
    private readonly IFileService _fileService;
    private readonly ApplicationDbContext _context;

    public ProjectService(
        Repositories.IProjectRepository projectRepository,
        Repositories.IUserRepository userRepository,
        Repositories.ITemplateRepository templateRepository,
        IHistoryService historyService,
        IFileService fileService,
        ApplicationDbContext context)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _templateRepository = templateRepository;
        _historyService = historyService;
        _fileService = fileService;
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<ProjectListDto>>> GetUserProjectsAsync(Guid userId, ProjectFilterDto filter)
    {
        try
        {
            var (projects, totalCount) = await _projectRepository.GetPagedUserProjectsAsync(
                userId,
                filter.Page > 0 ? (filter.Page - 1) * filter.PageSize : 0,
                filter.PageSize,
                filter.Status,
                filter.SearchTerm,
                filter.SortBy,
                filter.SortDescending);

            var projectDtos = projects.Select(MapToProjectListDto).ToList();

            var result = new PagedResult<ProjectListDto>
            {
                Items = projectDtos,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };

            return ApiResponse<PagedResult<ProjectListDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<ProjectListDto>>.ErrorResult($"Failed to get user projects: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(Guid id, Guid userId)
    {
        try
        {
            var project = await _projectRepository.GetUserProjectByIdAsync(id, userId);
            if (project == null)
            {
                return ApiResponse<ProjectDto>.ErrorResult("Project not found");
            }

            var projectDto = MapToProjectDto(project);
            return ApiResponse<ProjectDto>.SuccessResult(projectDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProjectDto>.ErrorResult($"Failed to get project: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectDto createProjectDto, Guid userId)
    {
        try
        {
            // Validate template if provided
            Template? template = null;
            if (createProjectDto.TemplateId.HasValue)
            {
                template = await _templateRepository.GetByIdAsync(createProjectDto.TemplateId.Value);
                if (template == null || !template.IsActive)
                {
                    return ApiResponse<ProjectDto>.ErrorResult("Template not found or inactive");
                }

                // Check if user can use premium templates
                var user = await _userRepository.GetByIdAsync(userId);
                if (template.IsPremium && user?.SubscriptionTier == SubscriptionTier.Free)
                {
                    return ApiResponse<ProjectDto>.ErrorResult("Premium template requires subscription");
                }
            }

            var project = new Project
            {
                UserId = userId,
                TemplateId = createProjectDto.TemplateId,
                Name = createProjectDto.Name,
                CanvasData = createProjectDto.CanvasData ?? (template?.TemplateData ?? new { }),
                Width = createProjectDto.Width,
                Height = createProjectDto.Height,
                Status = ProjectStatus.Draft
            };

            await _projectRepository.AddAsync(project);
            await _context.SaveChangesAsync();

            // Log project creation
            await _historyService.LogActionAsync(userId, ActionType.ProjectCreated, project.Id,
                new { ProjectName = project.Name, TemplateUsed = template?.Name });

            // Log template usage if template was used
            if (template != null)
            {
                await _historyService.LogActionAsync(userId, ActionType.TemplateUsed, project.Id,
                    new { TemplateName = template.Name, TemplateCategory = template.Category.ToString() });
            }

            var projectDto = MapToProjectDto(project);
            return ApiResponse<ProjectDto>.SuccessResult(projectDto, "Project created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProjectDto>.ErrorResult($"Failed to create project: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectDto updateProjectDto, Guid userId)
    {
        try
        {
            var project = await _projectRepository.GetUserProjectByIdAsync(id, userId);
            if (project == null)
            {
                return ApiResponse<ProjectDto>.ErrorResult("Project not found");
            }

            // Update fields
            if (!string.IsNullOrEmpty(updateProjectDto.Name))
                project.Name = updateProjectDto.Name;

            if (updateProjectDto.CanvasData != null)
                project.CanvasData = updateProjectDto.CanvasData;

            if (!string.IsNullOrEmpty(updateProjectDto.ThumbnailUrl))
                project.ThumbnailUrl = updateProjectDto.ThumbnailUrl;

            if (updateProjectDto.Width.HasValue)
                project.Width = updateProjectDto.Width.Value;

            if (updateProjectDto.Height.HasValue)
                project.Height = updateProjectDto.Height.Value;

            if (updateProjectDto.Status.HasValue)
                project.Status = updateProjectDto.Status.Value;

            await _projectRepository.UpdateAsync(project);
            await _context.SaveChangesAsync();

            // Log project update
            await _historyService.LogActionAsync(userId, ActionType.ProjectUpdated, project.Id,
                new { ProjectName = project.Name, UpdatedFields = GetUpdatedFields(updateProjectDto) });

            var projectDto = MapToProjectDto(project);
            return ApiResponse<ProjectDto>.SuccessResult(projectDto, "Project updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProjectDto>.ErrorResult($"Failed to update project: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteProjectAsync(Guid id, Guid userId)
    {
        try
        {
            var project = await _projectRepository.GetUserProjectByIdAsync(id, userId);
            if (project == null)
            {
                return ApiResponse.ErrorResult("Project not found");
            }

            await _projectRepository.DeleteAsync(project);
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResult("Project deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to delete project: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProjectDto>> DuplicateProjectAsync(Guid id, Guid userId, string? newName = null)
    {
        try
        {
            var originalProject = await _projectRepository.GetUserProjectByIdAsync(id, userId);
            if (originalProject == null)
            {
                return ApiResponse<ProjectDto>.ErrorResult("Project not found");
            }

            var duplicatedProject = new Project
            {
                UserId = userId,
                TemplateId = originalProject.TemplateId,
                Name = newName ?? $"{originalProject.Name} (Copy)",
                CanvasData = originalProject.CanvasData,
                Width = originalProject.Width,
                Height = originalProject.Height,
                Status = ProjectStatus.Draft
            };

            await _projectRepository.AddAsync(duplicatedProject);
            await _context.SaveChangesAsync();

            // Log project duplication
            await _historyService.LogActionAsync(userId, ActionType.ProjectCreated, duplicatedProject.Id,
                new { ProjectName = duplicatedProject.Name, DuplicatedFrom = originalProject.Name });

            var projectDto = MapToProjectDto(duplicatedProject);
            return ApiResponse<ProjectDto>.SuccessResult(projectDto, "Project duplicated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProjectDto>.ErrorResult($"Failed to duplicate project: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> ExportProjectAsync(ExportProjectDto exportProjectDto, Guid userId)
    {
        try
        {
            var project = await _projectRepository.GetUserProjectByIdAsync(exportProjectDto.ProjectId, userId);
            if (project == null)
            {
                return ApiResponse<string>.ErrorResult("Project not found");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<string>.ErrorResult("User not found");
            }

            // Check export limits
            if (user.MonthlyExportsUsed >= user.MonthlyExportsLimit)
            {
                return ApiResponse<string>.ErrorResult("Monthly export limit exceeded");
            }

            // Generate the image
            var imageResult = await _fileService.GenerateProjectImageAsync(
                project.CanvasData!, 
                project.Width, 
                project.Height, 
                exportProjectDto.Format);

            if (!imageResult.Success || imageResult.Data == null)
            {
                return ApiResponse<string>.ErrorResult("Failed to generate image");
            }

            // Upload to storage
            var fileName = $"export_{project.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.{exportProjectDto.Format}";
            using var stream = new MemoryStream(imageResult.Data);
            var uploadResult = await _fileService.UploadImageAsync(stream, fileName, $"image/{exportProjectDto.Format}");

            if (!uploadResult.Success || uploadResult.Data == null)
            {
                return ApiResponse<string>.ErrorResult("Failed to upload exported image");
            }

            // Update user export count
            user.MonthlyExportsUsed++;
            await _userRepository.UpdateAsync(user);

            // Update project export status
            project.ExportedAt = DateTime.UtcNow;
            project.Status = ProjectStatus.Completed;
            await _projectRepository.UpdateAsync(project);

            await _context.SaveChangesAsync();

            // Log export
            await _historyService.LogActionAsync(userId, ActionType.ProjectExported, project.Id,
                new { ProjectName = project.Name, Format = exportProjectDto.Format, Quality = exportProjectDto.Quality });

            return ApiResponse<string>.SuccessResult(uploadResult.Data, "Project exported successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResult($"Failed to export project: {ex.Message}");
        }
    }

    public async Task<ApiResponse> UpdateThumbnailAsync(Guid id, string thumbnailUrl, Guid userId)
    {
        try
        {
            var project = await _projectRepository.GetUserProjectByIdAsync(id, userId);
            if (project == null)
            {
                return ApiResponse.ErrorResult("Project not found");
            }

            project.ThumbnailUrl = thumbnailUrl;
            await _projectRepository.UpdateAsync(project);
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResult("Thumbnail updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to update thumbnail: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ProjectListDto>>> GetRecentProjectsAsync(Guid userId, int count = 5)
    {
        try
        {
            var projects = await _projectRepository.GetRecentProjectsAsync(userId, count);
            var projectDtos = projects.Select(MapToProjectListDto).ToList();

            return ApiResponse<List<ProjectListDto>>.SuccessResult(projectDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ProjectListDto>>.ErrorResult($"Failed to get recent projects: {ex.Message}");
        }
    }

    #region Private Methods

    private static ProjectDto MapToProjectDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            UserId = project.UserId,
            TemplateId = project.TemplateId,
            Name = project.Name,
            CanvasData = project.CanvasData,
            ThumbnailUrl = project.ThumbnailUrl,
            Width = project.Width,
            Height = project.Height,
            Status = project.Status,
            ExportedAt = project.ExportedAt,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            TemplateName = project.Template?.Name,
            UserUsername = project.User?.Username
        };
    }

    private static ProjectListDto MapToProjectListDto(Project project)
    {
        return new ProjectListDto
        {
            Id = project.Id,
            Name = project.Name,
            ThumbnailUrl = project.ThumbnailUrl,
            Width = project.Width,
            Height = project.Height,
            Status = project.Status,
            ExportedAt = project.ExportedAt,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            TemplateName = project.Template?.Name
        };
    }

    private static List<string> GetUpdatedFields(UpdateProjectDto updateDto)
    {
        var fields = new List<string>();
        
        if (!string.IsNullOrEmpty(updateDto.Name)) fields.Add("Name");
        if (updateDto.CanvasData != null) fields.Add("CanvasData");
        if (!string.IsNullOrEmpty(updateDto.ThumbnailUrl)) fields.Add("ThumbnailUrl");
        if (updateDto.Width.HasValue) fields.Add("Width");
        if (updateDto.Height.HasValue) fields.Add("Height");
        if (updateDto.Status.HasValue) fields.Add("Status");

        return fields;
    }

    #endregion
}