using AzureMCP.Models;
using AzureMCP.Arguments;

namespace AzureMCP.Services.Interfaces;

public interface ISubscriptionService
{
    Task<List<ArgumentOption>> GetSubscriptions(string? tenantId = null, RetryPolicyArguments? retryPolicy = null);
}