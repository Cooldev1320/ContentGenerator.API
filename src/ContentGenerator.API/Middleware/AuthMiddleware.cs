using System.IdentityModel.Tokens.Jwt;
using ContentGenerator.Core.Interfaces;

namespace ContentGenerator.API.Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthMiddleware> _logger;

    public AuthMiddleware(RequestDelegate next, ILogger<AuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        var token = GetTokenFromHeader(context);
        
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var isValid = await userService.ValidateTokenAsync(token);
                if (isValid)
                {
                    var user = await userService.GetUserFromTokenAsync(token);
                    if (user != null)
                    {
                        // Add user info to context for later use
                        context.Items["User"] = user;
                        context.Items["UserId"] = user.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
            }
        }

        await _next(context);
    }

    private static string? GetTokenFromHeader(HttpContext context)
    {
        var authorization = context.Request.Headers.Authorization.FirstOrDefault();
        
        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
        {
            return authorization["Bearer ".Length..];
        }

        return null;
    }
}