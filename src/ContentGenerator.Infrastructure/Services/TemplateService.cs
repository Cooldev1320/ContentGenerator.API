using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.DTOs.Template;
using ContentGenerator.Core.Entities;
using ContentGenerator.Core.Interfaces;
using ContentGenerator.Infrastructure.Data;

namespace ContentGenerator.Infrastructure.Services;

public class TemplateService : ITemplateService
{
    private readonly Repositories.ITemplateRepository _templateRepository;
    private readonly ApplicationDbContext _context;
    private readonly IHistoryService _historyService;

    public TemplateService(
        Repositories.ITemplateRepository templateRepository,
        ApplicationDbContext context,
        IHistoryService historyService)
    {
        _templateRepository = templateRepository;
        _context = context;
        _historyService = historyService;
    }

    public async Task<ApiResponse<PagedResult<TemplateListDto>>> GetTemplatesAsync(TemplateFilterDto filter)
    {
        try
        {
            var (templates, totalCount) = await _templateRepository.GetPagedTemplatesAsync(
                filter.Page > 0 ? (filter.Page - 1) * filter.PageSize : 0,
                filter.PageSize,
                filter.Category,
                filter.IsPremium,
                filter.IsActive ?? true,
                filter.SearchTerm,
                filter.SortBy,
                filter.SortDescending);

            var templateDtos = templates.Select(MapToTemplateListDto).ToList();

            var result = new PagedResult<TemplateListDto>
            {
                Items = templateDtos,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };

            return ApiResponse<PagedResult<TemplateListDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<TemplateListDto>>.ErrorResult($"Failed to get templates: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TemplateDto>> GetTemplateByIdAsync(Guid id)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null)
            {
                return ApiResponse<TemplateDto>.ErrorResult("Template not found");
            }

            var templateDto = MapToTemplateDto(template);
            return ApiResponse<TemplateDto>.SuccessResult(templateDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<TemplateDto>.ErrorResult($"Failed to get template: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TemplateDto>> CreateTemplateAsync(CreateTemplateDto createTemplateDto, Guid? createdById = null)
    {
        try
        {
            var template = new Template
            {
                Name = createTemplateDto.Name,
                Description = createTemplateDto.Description,
                Category = createTemplateDto.Category,
                ThumbnailUrl = createTemplateDto.ThumbnailUrl,
                TemplateData = createTemplateDto.TemplateData,
                IsPremium = createTemplateDto.IsPremium,
                CreatedById = createdById,
                IsActive = true
            };

            await _templateRepository.AddAsync(template);
            await _context.SaveChangesAsync();

            // Log template creation if user is provided
            if (createdById.HasValue)
            {
                await _historyService.LogActionAsync(createdById.Value, Core.Enums.ActionType.TemplateUsed, null,
                    new { Action = "Template created", TemplateName = template.Name });
            }

            var templateDto = MapToTemplateDto(template);
            return ApiResponse<TemplateDto>.SuccessResult(templateDto, "Template created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<TemplateDto>.ErrorResult($"Failed to create template: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TemplateDto>> UpdateTemplateAsync(Guid id, UpdateTemplateDto updateTemplateDto)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null)
            {
                return ApiResponse<TemplateDto>.ErrorResult("Template not found");
            }

            // Update fields
            if (!string.IsNullOrEmpty(updateTemplateDto.Name))
                template.Name = updateTemplateDto.Name;

            if (updateTemplateDto.Description != null)
                template.Description = updateTemplateDto.Description;

            if (updateTemplateDto.Category.HasValue)
                template.Category = updateTemplateDto.Category.Value;

            if (!string.IsNullOrEmpty(updateTemplateDto.ThumbnailUrl))
                template.ThumbnailUrl = updateTemplateDto.ThumbnailUrl;

            if (updateTemplateDto.TemplateData != null)
                template.TemplateData = updateTemplateDto.TemplateData;

            if (updateTemplateDto.IsPremium.HasValue)
                template.IsPremium = updateTemplateDto.IsPremium.Value;

            if (updateTemplateDto.IsActive.HasValue)
                template.IsActive = updateTemplateDto.IsActive.Value;

            await _templateRepository.UpdateAsync(template);
            await _context.SaveChangesAsync();

            var templateDto = MapToTemplateDto(template);
            return ApiResponse<TemplateDto>.SuccessResult(templateDto, "Template updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<TemplateDto>.ErrorResult($"Failed to update template: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteTemplateAsync(Guid id)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null)
            {
                return ApiResponse.ErrorResult("Template not found");
            }

            // Soft delete by setting IsActive to false
            template.IsActive = false;
            await _templateRepository.UpdateAsync(template);
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResult("Template deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to delete template: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TemplateListDto>>> GetFeaturedTemplatesAsync(int count = 10)
    {
        try
        {
            var templates = await _templateRepository.GetFeaturedTemplatesAsync(count);
            var templateDtos = templates.Select(MapToTemplateListDto).ToList();

            return ApiResponse<List<TemplateListDto>>.SuccessResult(templateDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TemplateListDto>>.ErrorResult($"Failed to get featured templates: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TemplateListDto>>> GetTemplatesByCategoryAsync(Core.Enums.TemplateCategory category, int count = 20)
    {
        try
        {
            var templates = await _templateRepository.GetByCategoryAsync(category, true);
            var templateDtos = templates.Take(count).Select(MapToTemplateListDto).ToList();

            return ApiResponse<List<TemplateListDto>>.SuccessResult(templateDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TemplateListDto>>.ErrorResult($"Failed to get templates by category: {ex.Message}");
        }
    }

    public async Task<ApiResponse> ToggleTemplateStatusAsync(Guid id)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null)
            {
                return ApiResponse.ErrorResult("Template not found");
            }

            template.IsActive = !template.IsActive;
            await _templateRepository.UpdateAsync(template);
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResult($"Template {(template.IsActive ? "activated" : "deactivated")} successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to toggle template status: {ex.Message}");
        }
    }

    #region Private Methods

    private static TemplateDto MapToTemplateDto(Template template)
    {
        return new TemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Category = template.Category,
            ThumbnailUrl = template.ThumbnailUrl,
            TemplateData = template.TemplateData,
            IsPremium = template.IsPremium,
            IsActive = template.IsActive,
            CreatedById = template.CreatedById,
            CreatedByUsername = template.CreatedBy?.Username,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }

    private static TemplateListDto MapToTemplateListDto(Template template)
    {
        return new TemplateListDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Category = template.Category,
            ThumbnailUrl = template.ThumbnailUrl,
            IsPremium = template.IsPremium,
            CreatedByUsername = template.CreatedBy?.Username,
            CreatedAt = template.CreatedAt
        };
    }

    #endregion
}