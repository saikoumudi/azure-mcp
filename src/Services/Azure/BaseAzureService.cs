using Azure.Identity;
using Azure.ResourceManager;
using AzureMCP.Arguments;

namespace AzureMCP.Services.Azure;

public abstract class BaseAzureService
{
    protected DefaultAzureCredential GetCredential(string? tenantId = null)
    {
        try
        {
            // Use the default credential
            var credential = tenantId == null
                ? new DefaultAzureCredential()
                : new DefaultAzureCredential(new DefaultAzureCredentialOptions { TenantId = tenantId });
            
            return credential;
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
        try
        {
            var credential = GetCredential(tenantId);
            
            var options = new ArmClientOptions();
            
            // Configure retry policy if provided
            if (retryPolicy != null)
            {
                // These properties are not nullable, so we can directly use them
                options.Retry.MaxRetries = retryPolicy.MaxRetries;
                options.Retry.Mode = retryPolicy.Mode;
                options.Retry.Delay = TimeSpan.FromSeconds(retryPolicy.DelaySeconds);
                options.Retry.MaxDelay = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
                options.Retry.NetworkTimeout = TimeSpan.FromSeconds(retryPolicy.NetworkTimeoutSeconds);
            }
            
            return new ArmClient(credential, default, options);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create ARM client: {ex.Message}", ex);
        }
    }
}