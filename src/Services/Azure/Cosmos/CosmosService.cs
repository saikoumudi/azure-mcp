using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;
using Microsoft.Azure.Cosmos;
using AzureMCP.Services.Interfaces;
using System.Text.Json;
using AzureMCP.Arguments;

namespace AzureMCP.Services.Azure.Cosmos;

public class CosmosService : BaseAzureService, ICosmosService
{
    private const string CosmosBaseUri = "https://{0}.documents.azure.com:443/";

    public async Task<List<string>> GetCosmosAccounts(string subscriptionId, string? tenantId = null, RetryPolicyArguments? retryPolicy = null)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID cannot be null or empty", nameof(subscriptionId));

        var armClient = CreateArmClient(tenantId, retryPolicy);
        var subscription = await armClient.GetSubscriptionResource(
            new ResourceIdentifier($"/subscriptions/{subscriptionId}")).GetAsync();

        var accounts = new List<string>();
        try
        {
            await foreach (var account in subscription.Value.GetCosmosDBAccountsAsync())
            {
                if (account?.Data?.Name != null)
                {
                    accounts.Add(account.Data.Name);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Cosmos DB accounts: {ex.Message}", ex);
        }
        
        return accounts;
    }

    public async Task<List<string>> ListDatabases(string accountName, string subscriptionId, string? tenantId = null, RetryPolicyArguments? retryPolicy = null)
    {
        if (string.IsNullOrEmpty(accountName))
            throw new ArgumentException("Account name cannot be null or empty", nameof(accountName));
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID cannot be null or empty", nameof(subscriptionId));

        var armClient = CreateArmClient(tenantId, retryPolicy);
        var cosmosAccount = await GetCosmosAccountAsync(armClient, subscriptionId, accountName);
        var keys = await cosmosAccount.GetKeysAsync();

        var clientOptions = new CosmosClientOptions();
        if (retryPolicy != null)
        {
            clientOptions.MaxRetryAttemptsOnRateLimitedRequests = retryPolicy.MaxRetries;
            clientOptions.MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
        }

        var client = new CosmosClient(
            string.Format(CosmosBaseUri, accountName),
            keys.Value.PrimaryMasterKey,
            clientOptions
        );

        var databases = new List<string>();
        try
        {
            var iterator = client.GetDatabaseQueryIterator<DatabaseProperties>();
            while (iterator.HasMoreResults)
            {
                var results = await iterator.ReadNextAsync();
                databases.AddRange(results.Select(r => r.Id));
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing databases: {ex.Message}", ex);
        }

        return databases;
    }

    public async Task<List<string>> ListContainers(string accountName, string databaseName, string subscriptionId, string? tenantId = null, RetryPolicyArguments? retryPolicy = null)
    {
        if (string.IsNullOrEmpty(accountName))
            throw new ArgumentException("Account name cannot be null or empty", nameof(accountName));
        if (string.IsNullOrEmpty(databaseName))
            throw new ArgumentException("Database name cannot be null or empty", nameof(databaseName));
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID cannot be null or empty", nameof(subscriptionId));

        var armClient = CreateArmClient(tenantId, retryPolicy);
        var cosmosAccount = await GetCosmosAccountAsync(armClient, subscriptionId, accountName);
        var keys = await cosmosAccount.GetKeysAsync();

        var clientOptions = new CosmosClientOptions();
        if (retryPolicy != null)
        {
            clientOptions.MaxRetryAttemptsOnRateLimitedRequests = retryPolicy.MaxRetries;
            clientOptions.MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
        }

        var client = new CosmosClient(
            string.Format(CosmosBaseUri, accountName),
            keys.Value.PrimaryMasterKey,
            clientOptions
        );

        var containers = new List<string>();
        try
        {
            var database = client.GetDatabase(databaseName);
            var iterator = database.GetContainerQueryIterator<ContainerProperties>();
            while (iterator.HasMoreResults)
            {
                var results = await iterator.ReadNextAsync();
                containers.AddRange(results.Select(r => r.Id));
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing containers: {ex.Message}", ex);
        }

        return containers;
    }

    public async Task<List<JsonDocument>> QueryItems(
        string accountName,
        string databaseName,
        string containerName,
        string? query,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null)
    {
        if (string.IsNullOrEmpty(accountName))
            throw new ArgumentException("Account name cannot be null or empty", nameof(accountName));
        if (string.IsNullOrEmpty(databaseName))
            throw new ArgumentException("Database name cannot be null or empty", nameof(databaseName));
        if (string.IsNullOrEmpty(containerName))
            throw new ArgumentException("Container name cannot be null or empty", nameof(containerName));
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID cannot be null or empty", nameof(subscriptionId));

        var armClient = CreateArmClient(tenantId, retryPolicy);
        var cosmosAccount = await GetCosmosAccountAsync(armClient, subscriptionId, accountName);
        var keys = await cosmosAccount.GetKeysAsync();

        var clientOptions = new CosmosClientOptions { AllowBulkExecution = true };
        if (retryPolicy != null)
        {
            clientOptions.MaxRetryAttemptsOnRateLimitedRequests = retryPolicy.MaxRetries;
            clientOptions.MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
        }

        var client = new CosmosClient(
            string.Format(CosmosBaseUri, accountName),
            keys.Value.PrimaryMasterKey,
            clientOptions
        );

        try
        {
            var container = client.GetContainer(databaseName, containerName);
            var baseQuery = string.IsNullOrEmpty(query) ? "SELECT * FROM c" : query;
            var queryDef = new QueryDefinition(baseQuery);

            var items = new List<JsonDocument>();
            var queryIterator = container.GetItemQueryIterator<System.Dynamic.ExpandoObject>(
                queryDef,
                requestOptions: new QueryRequestOptions { MaxItemCount = -1 }
            );

            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                foreach (var item in response)
                {
                    var json = JsonSerializer.Serialize(item);
                    items.Add(JsonDocument.Parse(json));
                }
            }

            return items;
        }
        catch (CosmosException ex)
        {
            throw new Exception($"Cosmos DB error occurred while querying items: {ex.StatusCode} - {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error querying items: {ex.Message}", ex);
        }
    }

    private async Task<CosmosDBAccountResource> GetCosmosAccountAsync(
        ArmClient armClient, 
        string subscriptionId, 
        string accountName)
    {
        var subscription = await armClient.GetSubscriptionResource(
            new ResourceIdentifier($"/subscriptions/{subscriptionId}")).GetAsync();

        await foreach (var account in subscription.Value.GetCosmosDBAccountsAsync())
        {
            if (account.Data.Name == accountName)
            {
                return account;
            }
        }
        throw new Exception($"Cosmos DB account '{accountName}' not found in subscription '{subscriptionId}'");
    }
}

