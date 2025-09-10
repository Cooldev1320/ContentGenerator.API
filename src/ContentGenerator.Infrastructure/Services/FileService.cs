using ContentGenerator.Core.DTOs.Common;
using ContentGenerator.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Runtime.Versioning;
using Newtonsoft.Json;

namespace ContentGenerator.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IConfiguration _configuration;
    private readonly string _uploadsPath;
    private readonly string _baseUrl;

    public FileService(IConfiguration configuration)
    {
        _configuration = configuration;
        _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _baseUrl = _configuration["ASPNETCORE_URLS"] ?? "http://localhost:5000";
        
        // Ensure uploads directory exists
        Directory.CreateDirectory(_uploadsPath);
        Directory.CreateDirectory(Path.Combine(_uploadsPath, "avatars"));
        Directory.CreateDirectory(Path.Combine(_uploadsPath, "thumbnails"));
        Directory.CreateDirectory(Path.Combine(_uploadsPath, "exports"));
    }

    public async Task<ApiResponse<string>> UploadImageAsync(Stream imageStream, string fileName, string contentType)
    {
        try
        {
            var sanitizedFileName = SanitizeFileName(fileName);
            var filePath = Path.Combine(_uploadsPath, "exports", sanitizedFileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await imageStream.CopyToAsync(fileStream);

            var fileUrl = $"{_baseUrl}/uploads/exports/{sanitizedFileName}";
            return ApiResponse<string>.SuccessResult(fileUrl, "Image uploaded successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResult($"Failed to upload image: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> UploadAvatarAsync(Stream imageStream, string fileName, Guid userId)
    {
        try
        {
            var extension = Path.GetExtension(fileName);
            var sanitizedFileName = $"avatar_{userId}{extension}";
            var filePath = Path.Combine(_uploadsPath, "avatars", sanitizedFileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await imageStream.CopyToAsync(fileStream);

            var fileUrl = $"{_baseUrl}/uploads/avatars/{sanitizedFileName}";
            return ApiResponse<string>.SuccessResult(fileUrl, "Avatar uploaded successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResult($"Failed to upload avatar: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> UploadThumbnailAsync(Stream imageStream, string fileName, Guid projectId)
    {
        try
        {
            var extension = Path.GetExtension(fileName);
            var sanitizedFileName = $"thumb_{projectId}{extension}";
            var filePath = Path.Combine(_uploadsPath, "thumbnails", sanitizedFileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await imageStream.CopyToAsync(fileStream);

            var fileUrl = $"{_baseUrl}/uploads/thumbnails/{sanitizedFileName}";
            return ApiResponse<string>.SuccessResult(fileUrl, "Thumbnail uploaded successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResult($"Failed to upload thumbnail: {ex.Message}");
        }
    }

    public async Task<ApiResponse<byte[]>> GenerateProjectImageAsync(object canvasData, int width, int height, string format = "png")
    {
        try
        {
            // For cross-platform compatibility, we'll use a simple text-based approach
            // In production, you'd use SkiaSharp or ImageSharp for cross-platform image generation
            
            if (OperatingSystem.IsWindows())
            {
                return await GenerateImageWithSystemDrawingAsync(canvasData, width, height, format);
            }
            else
            {
                return await GeneratePlaceholderImageAsync(width, height, format);
            }
        }
        catch (Exception ex)
        {
            return ApiResponse<byte[]>.ErrorResult($"Failed to generate image: {ex.Message}");
        }
    }

    [SupportedOSPlatform("windows")]
    private async Task<ApiResponse<byte[]>> GenerateImageWithSystemDrawingAsync(object canvasData, int width, int height, string format)
    {
        using var bitmap = new System.Drawing.Bitmap(width, height);
        using var graphics = System.Drawing.Graphics.FromImage(bitmap);
        
        // Fill with white background
        graphics.Clear(System.Drawing.Color.White);

        // Parse canvas data and render elements
        await RenderCanvasElementsAsync(graphics, canvasData, width, height);

        // Convert to byte array
        using var stream = new MemoryStream();
        var imageFormat = format.ToLower() switch
        {
            "jpg" or "jpeg" => System.Drawing.Imaging.ImageFormat.Jpeg,
            "png" => System.Drawing.Imaging.ImageFormat.Png,
            _ => System.Drawing.Imaging.ImageFormat.Png
        };

        bitmap.Save(stream, imageFormat);
        var imageBytes = stream.ToArray();

        return ApiResponse<byte[]>.SuccessResult(imageBytes, "Image generated successfully");
    }

    private async Task<ApiResponse<byte[]>> GeneratePlaceholderImageAsync(int width, int height, string format)
    {
        // For non-Windows platforms, generate a simple placeholder
        // In production, use SkiaSharp or ImageSharp
        
        var svgContent = $@"
        <svg width=""{width}"" height=""{height}"" xmlns=""http://www.w3.org/2000/svg"">
            <rect width=""{width}"" height=""{height}"" fill=""#f0f0f0""/>
            <text x=""{width/2}"" y=""{height/2}"" text-anchor=""middle"" dominant-baseline=""middle"" 
                  font-family=""Arial"" font-size=""24"" fill=""#333"">
                Generated Content
            </text>
        </svg>";

        var svgBytes = System.Text.Encoding.UTF8.GetBytes(svgContent);
        
        // Note: In production, convert SVG to actual image format
        await Task.Delay(1); // Simulate async work
        
        return ApiResponse<byte[]>.SuccessResult(svgBytes, "Placeholder image generated");
    }

    public Task<ApiResponse> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var fileName = Path.GetFileName(fileUrl);
            var possiblePaths = new[]
            {
                Path.Combine(_uploadsPath, "exports", fileName),
                Path.Combine(_uploadsPath, "avatars", fileName),
                Path.Combine(_uploadsPath, "thumbnails", fileName)
            };

            foreach (var filePath in possiblePaths)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(ApiResponse.SuccessResult("File deleted successfully"));
                }
            }

            return Task.FromResult(ApiResponse.ErrorResult("File not found"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ApiResponse.ErrorResult($"Failed to delete file: {ex.Message}"));
        }
    }

    public Task<ApiResponse<string>> GetSignedUrlAsync(string filePath, int expiryMinutes = 60)
    {
        // For local file storage, we'll just return the direct URL
        // In production with cloud storage, you'd generate a signed URL
        var url = $"{_baseUrl}/{filePath}";
        return Task.FromResult(ApiResponse<string>.SuccessResult(url, "Signed URL generated"));
    }

    public async Task<ApiResponse<List<string>>> GetUserFilesAsync(Guid userId)
    {
        try
        {
            var files = new List<string>();
            
            // Get user avatar
            var avatarDir = Path.Combine(_uploadsPath, "avatars");
            if (Directory.Exists(avatarDir))
            {
                var avatarFiles = Directory.GetFiles(avatarDir, $"avatar_{userId}.*");
                files.AddRange(avatarFiles.Select(f => $"{_baseUrl}/uploads/avatars/{Path.GetFileName(f)}"));
            }

            // Get user project thumbnails (this would require database lookup in real implementation)
            var thumbnailDir = Path.Combine(_uploadsPath, "thumbnails");
            if (Directory.Exists(thumbnailDir))
            {
                var thumbnailFiles = Directory.GetFiles(thumbnailDir, "thumb_*.jpg")
                    .Concat(Directory.GetFiles(thumbnailDir, "thumb_*.png"));
                files.AddRange(thumbnailFiles.Select(f => $"{_baseUrl}/uploads/thumbnails/{Path.GetFileName(f)}"));
            }

            return ApiResponse<List<string>>.SuccessResult(files);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<string>>.ErrorResult($"Failed to get user files: {ex.Message}");
        }
    }

    #region Private Methods

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        return string.IsNullOrEmpty(sanitized) ? $"file_{Guid.NewGuid()}" : sanitized;
    }

    [SupportedOSPlatform("windows")]
    private async Task RenderCanvasElementsAsync(System.Drawing.Graphics graphics, object canvasData, int width, int height)
    {
        try
        {
            // Simple canvas rendering - in production, you'd use a proper graphics library
            var json = JsonConvert.SerializeObject(canvasData);
            var canvasObject = JsonConvert.DeserializeObject<dynamic>(json);

            // Basic text rendering example
            if (canvasObject?.elements != null)
            {
                foreach (var element in canvasObject.elements)
                {
                    if (element?.type?.ToString() == "text")
                    {
                        var text = element?.content?.ToString() ?? "Sample Text";
                        var x = element?.x?.ToObject<float>() ?? 50;
                        var y = element?.y?.ToObject<float>() ?? 50;
                        var fontSize = element?.fontSize?.ToObject<float>() ?? 24;

                        using var font = new System.Drawing.Font("Arial", fontSize);
                        using var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                        graphics.DrawString(text, font, brush, x, y);
                    }
                }
            }
            else
            {
                // Fallback: draw sample content
                using var font = new System.Drawing.Font("Arial", 24);
                using var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                graphics.DrawString("Content Generator", font, brush, 50, 50);
            }

            await Task.Delay(1); // Placeholder for async operations
        }
        catch (Exception)
        {
            // Fallback rendering
            using var font = new System.Drawing.Font("Arial", 24);
            using var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Gray);
            graphics.DrawString("Generated Content", font, brush, 50, 50);
        }
    }

    #endregion
}