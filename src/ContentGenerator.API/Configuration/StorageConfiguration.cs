using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.IIS;
using ContentGenerator.Infrastructure.External;

namespace ContentGenerator.API.Configuration;

public static class StorageConfiguration
{
    public static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure HTTP clients for external services
        services.AddHttpClient<IUnsplashService, UnsplashService>(client =>
        {
            client.BaseAddress = new Uri("https://api.unsplash.com/");
            client.DefaultRequestHeaders.Add("User-Agent", "ContentGenerator/1.0");
        });

        // Register external services
        services.AddScoped<ISupabaseService, SupabaseService>();
        services.AddScoped<IStripeService, StripeService>();

        // Configure file upload limits
        services.Configure<IISServerOptions>(options =>
        {
            options.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
        });

        services.Configure<FormOptions>(options =>
        {
            options.ValueLengthLimit = int.MaxValue;
            options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
            options.MultipartHeadersLengthLimit = int.MaxValue;
        });

        return services;
    }
}