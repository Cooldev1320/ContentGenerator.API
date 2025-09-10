using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using ContentGenerator.Core.DTOs.Common;

namespace ContentGenerator.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, (DateTime, int)> _requests = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _maxRequests = configuration.GetValue<int>("RateLimit:MaxRequests", 100);
        _timeWindow = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimit:WindowMinutes", 1));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var now = DateTime.UtcNow;

        // Clean up old entries
        CleanupOldEntries(now);

        if (_requests.TryGetValue(clientId, out var requestInfo))
        {
            var (firstRequestTime, requestCount) = requestInfo;

            if (now - firstRequestTime < _timeWindow)
            {
                if (requestCount >= _maxRequests)
                {
                    await HandleRateLimitExceeded(context);
                    return;
                }

                _requests[clientId] = (firstRequestTime, requestCount + 1);
            }
            else
            {
                _requests[clientId] = (now, 1);
            }
        }
        else
        {
            _requests[clientId] = (now, 1);
        }

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Use user ID if authenticated, otherwise use IP address
        var userIdClaim = context.User?.FindFirst("userId");
        if (userIdClaim != null)
        {
            return $"user_{userIdClaim.Value}";
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip_{ipAddress}";
    }

    private void CleanupOldEntries(DateTime now)
    {
        var keysToRemove = new List<string>();

        foreach (var kvp in _requests)
        {
            if (now - kvp.Value.Item1 > _timeWindow)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _requests.TryRemove(key, out _);
        }
    }

    private async Task HandleRateLimitExceeded(HttpContext context)
    {
        _logger.LogWarning("Rate limit exceeded for client: {ClientId}", GetClientIdentifier(context));

        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";

        var response = ApiResponse.ErrorResult("Rate limit exceeded. Please try again later.");
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}