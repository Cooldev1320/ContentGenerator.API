using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Supabase;

namespace ContentGenerator.Infrastructure.External;

public interface ISupabaseService
{
    Task<string> UploadFileAsync(byte[] fileData, string fileName, string bucket = "content-generator");
    Task<bool> DeleteFileAsync(string fileName, string bucket = "content-generator");
    Task<string> GetPublicUrlAsync(string fileName, string bucket = "content-generator");
}

public class SupabaseService : ISupabaseService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly ILogger<SupabaseService> _logger;

    public SupabaseService(IConfiguration configuration, ILogger<SupabaseService> logger)
    {
        _logger = logger;
        
        var url = configuration["SUPABASE_URL"];
        var key = configuration["SUPABASE_SERVICE_ROLE_KEY"];

        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
        {
            _logger.LogWarning("Supabase configuration missing. File operations will be mocked.");
            _supabaseClient = null!;
        }
        else
        {
            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false
            };
            
            _supabaseClient = new Supabase.Client(url, key, options);
        }
    }

    public async Task<string> UploadFileAsync(byte[] fileData, string fileName, string bucket = "content-generator")
    {
        try
        {
            if (_supabaseClient == null)
            {
                _logger.LogWarning("Supabase not configured. Returning mock URL.");
                return $"https://mock-storage.com/{bucket}/{fileName}";
            }

            var response = await _supabaseClient.Storage
                .From(bucket)
                .Upload(fileData, fileName);

            if (!string.IsNullOrEmpty(response))
            {
                return GetPublicUrlAsync(fileName, bucket).Result;
            }

            throw new Exception("Upload failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading file {fileName} to Supabase");
            return $"https://mock-storage.com/{bucket}/{fileName}";
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName, string bucket = "content-generator")
    {
        try
        {
            if (_supabaseClient == null)
            {
                _logger.LogWarning("Supabase not configured. Mocking delete operation.");
                return true;
            }

            await _supabaseClient.Storage
                .From(bucket)
                .Remove(fileName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting file {fileName} from Supabase");
            return false;
        }
    }

    public async Task<string> GetPublicUrlAsync(string fileName, string bucket = "content-generator")
    {
        try
        {
            if (_supabaseClient == null)
            {
                return $"https://mock-storage.com/{bucket}/{fileName}";
            }

            var url = _supabaseClient.Storage
                .From(bucket)
                .GetPublicUrl(fileName);

            return await Task.FromResult(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting public URL for {fileName}");
            return $"https://mock-storage.com/{bucket}/{fileName}";
        }
    }
}