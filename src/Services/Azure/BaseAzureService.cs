using Azure.Identity;
using Azure.ResourceManager;
using AzureMCP.Arguments;

namespace AzureMCP.Services.Azure;

public abstract class BaseAzureService
{
    private DefaultAzureCredential? _credential;
    private string? _lastTenantId;
    private ArmClient? _armClient;
    private string? _lastArmClientTenantId;
    private RetryPolicyArguments? _lastRetryPolicy;

    protected DefaultAzureCredential GetCredential(string? tenantId = null)
    {
        // Return cached credential if it exists and tenant ID hasn't changed
        if (_credential != null && _lastTenantId == tenantId)
        {
            return _credential;
        }

        try
        {
            // Create new credential and cache it
            _credential = tenantId == null
                ? new DefaultAzureCredential()
                : new DefaultAzureCredential(new DefaultAzureCredentialOptions { TenantId = tenantId });
            _lastTenantId = tenantId;

            return _credential;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get credential: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates an Azure Resource Manager client with optional retry policy
    /// </summary>
    /// <param name="tenantId">Optional Azure tenant ID</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    protected ArmClient CreateArmClient(string? tenantId = null, RetryPolicyArguments? retryPolicy = null)
    {
        // Return cached client if parameters match
        if (_armClient != null &&
            _lastArmClientTenantId == tenantId &&
            RetryPolicyArguments.AreEqual(_lastRetryPolicy, retryPolicy))
        {
            return _armClient;
        }

        try
        {
            var credential = GetCredential(tenantId);
            var options = new ArmClientOptions();

            // Configure retry policy if provided
            if (retryPolicy != null)
            {
                options.Retry.MaxRetries = retryPolicy.MaxRetries;
                options.Retry.Mode = retryPolicy.Mode;
                options.Retry.Delay = TimeSpan.FromSeconds(retryPolicy.DelaySeconds);
                options.Retry.MaxDelay = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
                options.Retry.NetworkTimeout = TimeSpan.FromSeconds(retryPolicy.NetworkTimeoutSeconds);
            }

            _armClient = new ArmClient(credential, default, options);
            _lastArmClientTenantId = tenantId;
            _lastRetryPolicy = retryPolicy;

            return _armClient;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create ARM client: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates that the provided parameters are not null or empty
    /// </summary>
    /// <param name="parameters">Array of parameters to validate</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is null or empty</exception>
    protected void ValidateRequiredParameters(params string?[] parameters)
    {
        foreach (var param in parameters)
        {
            if (string.IsNullOrEmpty(param))
                throw new ArgumentException($"Parameter cannot be null or empty", nameof(param));
        }
    }
}