using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.DTOs.Project;

namespace ContentGenerator.Core.Interfaces;

public interface IProjectService
{
    Task<ApiResponse<PagedResult<ProjectListDto>>> GetUserProjectsAsync(Guid userId, ProjectFilterDto filter);
    Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(Guid id, Guid userId);
    Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectDto createProjectDto, Guid userId);
    Task<ApiResponse<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectDto updateProjectDto, Guid userId);
    Task<ApiResponse> DeleteProjectAsync(Guid id, Guid userId);
    Task<ApiResponse<ProjectDto>> DuplicateProjectAsync(Guid id, Guid userId, string? newName = null);
    Task<ApiResponse<string>> ExportProjectAsync(ExportProjectDto exportProjectDto, Guid userId);
    Task<ApiResponse> UpdateThumbnailAsync(Guid id, string thumbnailUrl, Guid userId);
    Task<ApiResponse<List<ProjectListDto>>> GetRecentProjectsAsync(Guid userId, int count = 5);
}