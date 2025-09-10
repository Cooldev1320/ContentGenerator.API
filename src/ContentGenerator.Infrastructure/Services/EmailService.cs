using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ContentGenerator.Infrastructure.Services;

public interface IEmailService
{
    Task<bool> SendWelcomeEmailAsync(string email, string username);
    Task<bool> SendPasswordResetEmailAsync(string email, string resetToken);
    Task<bool> SendSubscriptionNotificationAsync(string email, string subscriptionInfo);
    Task<bool> SendExportLimitNotificationAsync(string email, string username);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string username)
    {
        try
        {
            // In production, implement actual email sending logic using SendGrid, AWS SES, etc.
            _logger.LogInformation($"Sending welcome email to {email} for user {username}");
            
            var emailContent = $@"
                Welcome to Content Generator, {username}!
                
                Thank you for joining our platform. You can now:
                - Create stunning social media content
                - Use our template library
                - Export your designs
                
                Start creating: {_configuration["ASPNETCORE_URLS"]}/dashboard
                
                Best regards,
                Content Generator Team
            ";

            // Simulate email sending delay
            await Task.Delay(100);
            
            _logger.LogInformation($"Welcome email sent successfully to {email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send welcome email to {email}");
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken)
    {
        try
        {
            _logger.LogInformation($"Sending password reset email to {email}");

            var resetUrl = $"{_configuration["ASPNETCORE_URLS"]}/reset-password?token={resetToken}";
            var emailContent = $@"
                Password Reset Request
                
                Someone requested a password reset for your Content Generator account.
                
                If this was you, click the link below to reset your password:
                {resetUrl}
                
                If you didn't request this, please ignore this email.
                
                This link will expire in 24 hours.
                
                Best regards,
                Content Generator Team
            ";

            await Task.Delay(100);
            
            _logger.LogInformation($"Password reset email sent successfully to {email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send password reset email to {email}");
            return false;
        }
    }

    public async Task<bool> SendSubscriptionNotificationAsync(string email, string subscriptionInfo)
    {
        try
        {
            _logger.LogInformation($"Sending subscription notification to {email}");

            var emailContent = $@"
                Subscription Update
                
                Your Content Generator subscription has been updated:
                {subscriptionInfo}
                
                Visit your dashboard to see your new features:
                {_configuration["ASPNETCORE_URLS"]}/dashboard
                
                Best regards,
                Content Generator Team
            ";

            await Task.Delay(100);
            
            _logger.LogInformation($"Subscription notification sent successfully to {email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send subscription notification to {email}");
            return false;
        }
    }

    public async Task<bool> SendExportLimitNotificationAsync(string email, string username)
    {
        try
        {
            _logger.LogInformation($"Sending export limit notification to {email}");

            var emailContent = $@"
                Export Limit Reached
                
                Hi {username},
                
                You've reached your monthly export limit on Content Generator.
                
                Upgrade to Pro or Agency plan for unlimited exports:
                {_configuration["ASPNETCORE_URLS"]}/pricing
                
                Your limit will reset next month.
                
                Best regards,
                Content Generator Team
            ";

            await Task.Delay(100);
            
            _logger.LogInformation($"Export limit notification sent successfully to {email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send export limit notification to {email}");
            return false;
        }
    }
}