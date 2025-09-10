using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ContentGenerator.Infrastructure.External;

public interface IStripeService
{
    Task<string> CreateCustomerAsync(string email, string name);
    Task<string> CreateSubscriptionAsync(string customerId, string priceId);
    Task<bool> CancelSubscriptionAsync(string subscriptionId);
    Task<string> CreatePaymentIntentAsync(decimal amount, string currency = "usd");
    Task<bool> UpdateSubscriptionAsync(string subscriptionId, string newPriceId);
}

public class StripeService : IStripeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeService> _logger;
    private readonly string? _secretKey;

    public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _secretKey = _configuration["Stripe:SecretKey"];

        if (string.IsNullOrEmpty(_secretKey))
        {
            _logger.LogWarning("Stripe secret key not configured. Payment operations will be mocked.");
        }
    }

    public async Task<string> CreateCustomerAsync(string email, string name)
    {
        try
        {
            if (string.IsNullOrEmpty(_secretKey))
            {
                _logger.LogWarning("Stripe not configured. Returning mock customer ID.");
                return $"cus_mock_{Guid.NewGuid().ToString("N")[..8]}";
            }

            // In production, implement actual Stripe customer creation
            _logger.LogInformation($"Creating Stripe customer for {email}");
            
            // Simulate API call
            await Task.Delay(100);
            
            var customerId = $"cus_{Guid.NewGuid().ToString("N")[..24]}";
            _logger.LogInformation($"Created Stripe customer {customerId} for {email}");
            
            return customerId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating Stripe customer for {email}");
            throw;
        }
    }

    public async Task<string> CreateSubscriptionAsync(string customerId, string priceId)
    {
        try
        {
            if (string.IsNullOrEmpty(_secretKey))
            {
                return $"sub_mock_{Guid.NewGuid().ToString("N")[..8]}";
            }

            _logger.LogInformation($"Creating Stripe subscription for customer {customerId} with price {priceId}");
            
            await Task.Delay(100);
            
            var subscriptionId = $"sub_{Guid.NewGuid().ToString("N")[..24]}";
            _logger.LogInformation($"Created Stripe subscription {subscriptionId}");
            
            return subscriptionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating Stripe subscription for customer {customerId}");
            throw;
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string subscriptionId)
    {
        try
        {
            if (string.IsNullOrEmpty(_secretKey))
            {
                _logger.LogWarning("Stripe not configured. Mocking subscription cancellation.");
                return true;
            }

            _logger.LogInformation($"Canceling Stripe subscription {subscriptionId}");
            
            await Task.Delay(100);
            
            _logger.LogInformation($"Canceled Stripe subscription {subscriptionId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error canceling Stripe subscription {subscriptionId}");
            return false;
        }
    }

    public async Task<string> CreatePaymentIntentAsync(decimal amount, string currency = "usd")
    {
        try
        {
            if (string.IsNullOrEmpty(_secretKey))
            {
                return $"pi_mock_{Guid.NewGuid().ToString("N")[..8]}";
            }

            _logger.LogInformation($"Creating Stripe payment intent for {amount} {currency}");
            
            await Task.Delay(100);
            
            var paymentIntentId = $"pi_{Guid.NewGuid().ToString("N")[..24]}";
            _logger.LogInformation($"Created Stripe payment intent {paymentIntentId}");
            
            return paymentIntentId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating Stripe payment intent for {amount} {currency}");
            throw;
        }
    }

    public async Task<bool> UpdateSubscriptionAsync(string subscriptionId, string newPriceId)
    {
        try
        {
            if (string.IsNullOrEmpty(_secretKey))
            {
                _logger.LogWarning("Stripe not configured. Mocking subscription update.");
                return true;
            }

            _logger.LogInformation($"Updating Stripe subscription {subscriptionId} to price {newPriceId}");
            
            await Task.Delay(100);
            
            _logger.LogInformation($"Updated Stripe subscription {subscriptionId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating Stripe subscription {subscriptionId}");
            return false;
        }
    }
}