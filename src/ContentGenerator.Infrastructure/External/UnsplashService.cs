using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ContentGenerator.Infrastructure.External;

public interface IUnsplashService
{
    Task<List<UnsplashImage>> SearchImagesAsync(string query, int count = 20);
    Task<List<UnsplashImage>> GetFeaturedImagesAsync(int count = 20);
    Task<UnsplashImage?> GetRandomImageAsync(string? category = null);
}

public class UnsplashImage
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public UnsplashUrls Urls { get; set; } = new();
    public UnsplashUser User { get; set; } = new();
    public int Width { get; set; }
    public int Height { get; set; }
}

public class UnsplashUrls
{
    public string Raw { get; set; } = string.Empty;
    public string Full { get; set; } = string.Empty;
    public string Regular { get; set; } = string.Empty;
    public string Small { get; set; } = string.Empty;
    public string Thumb { get; set; } = string.Empty;
}

public class UnsplashUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class UnsplashService : IUnsplashService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UnsplashService> _logger;
    private readonly string? _accessKey;

    public UnsplashService(HttpClient httpClient, IConfiguration configuration, ILogger<UnsplashService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _accessKey = _configuration["Unsplash:AccessKey"];
        
        _httpClient.BaseAddress = new Uri("https://api.unsplash.com/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Client-ID {_accessKey}");
    }

    public async Task<List<UnsplashImage>> SearchImagesAsync(string query, int count = 20)
    {
        try
        {
            if (string.IsNullOrEmpty(_accessKey))
            {
                _logger.LogWarning("Unsplash access key not configured. Returning mock data.");
                return GetMockImages(count);
            }

            var response = await _httpClient.GetAsync($"search/photos?query={Uri.EscapeDataString(query)}&per_page={count}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Unsplash API error: {response.StatusCode}");
                return GetMockImages(count);
            }

            var json = await response.Content.ReadAsStringAsync();
            var searchResult = JsonConvert.DeserializeObject<UnsplashSearchResult>(json);
            
            return searchResult?.Results ?? GetMockImages(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching Unsplash images for query: {query}");
            return GetMockImages(count);
        }
    }

    public async Task<List<UnsplashImage>> GetFeaturedImagesAsync(int count = 20)
    {
        try
        {
            if (string.IsNullOrEmpty(_accessKey))
            {
                return GetMockImages(count);
            }

            var response = await _httpClient.GetAsync($"photos/featured?per_page={count}");
            
            if (!response.IsSuccessStatusCode)
            {
                return GetMockImages(count);
            }

            var json = await response.Content.ReadAsStringAsync();
            var images = JsonConvert.DeserializeObject<List<UnsplashImage>>(json);
            
            return images ?? GetMockImages(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured Unsplash images");
            return GetMockImages(count);
        }
    }

    public async Task<UnsplashImage?> GetRandomImageAsync(string? category = null)
    {
        try
        {
            if (string.IsNullOrEmpty(_accessKey))
            {
                return GetMockImages(1).FirstOrDefault();
            }

            var url = "photos/random";
            if (!string.IsNullOrEmpty(category))
            {
                url += $"?collections={Uri.EscapeDataString(category)}";
            }

            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return GetMockImages(1).FirstOrDefault();
            }

            var json = await response.Content.ReadAsStringAsync();
            var image = JsonConvert.DeserializeObject<UnsplashImage>(json);
            
            return image;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting random Unsplash image for category: {category}");
            return GetMockImages(1).FirstOrDefault();
        }
    }

    private static List<UnsplashImage> GetMockImages(int count)
    {
        var mockImages = new List<UnsplashImage>();
        
        for (int i = 0; i < count; i++)
        {
            mockImages.Add(new UnsplashImage
            {
                Id = $"mock_{i}",
                Description = $"Mock image {i + 1}",
                Width = 1080,
                Height = 1080,
                Urls = new UnsplashUrls
                {
                    Regular = $"https://picsum.photos/1080/1080?random={i}",
                    Small = $"https://picsum.photos/400/400?random={i}",
                    Thumb = $"https://picsum.photos/200/200?random={i}"
                },
                User = new UnsplashUser
                {
                    Id = "mock_user",
                    Username = "mockuser",
                    Name = "Mock User"
                }
            });
        }
        
        return mockImages;
    }
}

public class UnsplashSearchResult
{
    public List<UnsplashImage> Results { get; set; } = new();
    public int Total { get; set; }
    public int TotalPages { get; set; }
}