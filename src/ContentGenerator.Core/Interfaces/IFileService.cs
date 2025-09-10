using ContentGenerator.Core.DTOs.Common;

namespace ContentGenerator.Core.Interfaces;

public interface IFileService
{
    Task<ApiResponse<string>> UploadImageAsync(Stream imageStream, string fileName, string contentType);
    Task<ApiResponse<string>> UploadAvatarAsync(Stream imageStream, string fileName, Guid userId);
    Task<ApiResponse<string>> UploadThumbnailAsync(Stream imageStream, string fileName, Guid projectId);
    Task<ApiResponse<byte[]>> GenerateProjectImageAsync(object canvasData, int width, int height, string format = "png");
    Task<ApiResponse> DeleteFileAsync(string fileUrl);
    Task<ApiResponse<string>> GetSignedUrlAsync(string filePath, int expiryMinutes = 60);
    Task<ApiResponse<List<string>>> GetUserFilesAsync(Guid userId);
}