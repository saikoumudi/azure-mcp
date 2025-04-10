// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMCP.Services.Interfaces;

using AzureMCP.Arguments;
using AzureMCP.Models;
using global::Azure.Storage.Blobs.Models;

public interface IStorageService
{
    Task<List<string>> GetStorageAccounts(string subscriptionId, string? tenant = null, RetryPolicyArguments? retryPolicy = null);
    Task<List<string>> ListContainers(string accountName, string subscriptionId, string? tenant = null, RetryPolicyArguments? retryPolicy = null);
    Task<List<string>> ListTables(
        string accountName,
        string subscriptionId,
        AuthMethod authMethod = AuthMethod.Credential,
        string? connectionString = null,
        string? tenant = null,
        RetryPolicyArguments? retryPolicy = null);
    Task<List<string>> ListBlobs(string accountName, string containerName, string subscriptionId, string? tenant = null, RetryPolicyArguments? retryPolicy = null);
    Task<BlobContainerProperties> GetContainerDetails(
        string accountName,
        string containerName,
        string subscriptionId,
        string? tenant = null,
        RetryPolicyArguments? retryPolicy = null);
}