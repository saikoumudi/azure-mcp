using AzureMCP.Arguments;
using AzureMCP.Models;

namespace AzureMCP.Services.Interfaces;

public interface ISubscriptionService
{
    Task<List<ArgumentOption>> GetSubscriptions(string? tenantId = null, RetryPolicyArguments? retryPolicy = null);
}