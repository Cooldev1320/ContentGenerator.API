using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.Interfaces;

namespace ContentGenerator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileService fileService, ILogger<FilesController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Upload image file
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<string>.ErrorResult("No file provided"));
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(ApiResponse<string>.ErrorResult("Invalid file type. Only JPEG, PNG, and GIF are allowed"));
        }

        // Validate file size (max 10MB)
        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(ApiResponse<string>.ErrorResult("File size too large. Maximum 10MB allowed"));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _fileService.UploadImageAsync(stream, file.FileName, file.ContentType);

            if (result.Success)
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("File {FileName} uploaded by user {UserId}", file.FileName, userId);
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
            return StatusCode(500, ApiResponse<string>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Upload avatar image
    /// </summary>
    [HttpPost("avatar")]
    public async Task<ActionResult<ApiResponse<string>>> UploadAvatar(IFormFile file)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<string>.ErrorResult("Invalid token"));
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<string>.ErrorResult("No file provided"));
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(ApiResponse<string>.ErrorResult("Invalid file type. Only JPEG and PNG are allowed"));
        }

        // Validate file size (max 5MB for avatars)
        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(ApiResponse<string>.ErrorResult("File size too large. Maximum 5MB allowed"));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _fileService.UploadAvatarAsync(stream, file.FileName, userId.Value);

            if (result.Success)
            {
                _logger.LogInformation("Avatar uploaded by user {UserId}", userId);
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar for user {UserId}", userId);
            return StatusCode(500, ApiResponse<string>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Upload project thumbnail
    /// </summary>
    [HttpPost("thumbnail/{projectId}")]
    public async Task<ActionResult<ApiResponse<string>>> UploadThumbnail(Guid projectId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<string>.ErrorResult("No file provided"));
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(ApiResponse<string>.ErrorResult("Invalid file type. Only JPEG and PNG are allowed"));
        }

        // Validate file size (max 2MB for thumbnails)
        if (file.Length > 2 * 1024 * 1024)
        {
            return BadRequest(ApiResponse<string>.ErrorResult("File size too large. Maximum 2MB allowed"));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _fileService.UploadThumbnailAsync(stream, file.FileName, projectId);

            if (result.Success)
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Thumbnail uploaded for project {ProjectId} by user {UserId}", projectId, userId);
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading thumbnail for project {ProjectId}", projectId);
            return StatusCode(500, ApiResponse<string>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Get user files
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetUserFiles()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse<List<string>>.ErrorResult("Invalid token"));
        }

        var result = await _fileService.GetUserFilesAsync(userId.Value);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Delete file
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult<ApiResponse>> DeleteFile([FromQuery] string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl))
        {
            return BadRequest(ApiResponse.ErrorResult("File URL is required"));
        }

        var result = await _fileService.DeleteFileAsync(fileUrl);

        if (result.Success)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("File deleted by user {UserId}: {FileUrl}", userId, fileUrl);
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get signed URL for file access
    /// </summary>
    [HttpGet("signed-url")]
    public async Task<ActionResult<ApiResponse<string>>> GetSignedUrl([FromQuery] string filePath, [FromQuery] int expiryMinutes = 60)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return BadRequest(ApiResponse<string>.ErrorResult("File path is required"));
        }

        var result = await _fileService.GetSignedUrlAsync(filePath, expiryMinutes);
        
        if (result.Success)
        {
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