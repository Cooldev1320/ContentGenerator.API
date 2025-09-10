using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.DTOs.Template;

namespace ContentGenerator.Core.Interfaces;

public interface ITemplateService
{
    Task<ApiResponse<PagedResult<TemplateListDto>>> GetTemplatesAsync(TemplateFilterDto filter);
    Task<ApiResponse<TemplateDto>> GetTemplateByIdAsync(Guid id);
    Task<ApiResponse<TemplateDto>> CreateTemplateAsync(CreateTemplateDto createTemplateDto, Guid? createdById = null);
    Task<ApiResponse<TemplateDto>> UpdateTemplateAsync(Guid id, UpdateTemplateDto updateTemplateDto);
    Task<ApiResponse> DeleteTemplateAsync(Guid id);
    Task<ApiResponse<List<TemplateListDto>>> GetFeaturedTemplatesAsync(int count = 10);
    Task<ApiResponse<List<TemplateListDto>>> GetTemplatesByCategoryAsync(ContentGenerator.Core.Enums.TemplateCategory category, int count = 20);
    Task<ApiResponse> ToggleTemplateStatusAsync(Guid id);
}