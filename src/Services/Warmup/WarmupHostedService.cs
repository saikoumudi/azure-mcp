// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Services.Azure;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace AzureMcp.Services.Warmup;

public class WarmupHostedService : IHostedService
{
    private readonly ITenantService _tenantService;

    public WarmupHostedService(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await BaseAzureService.WarmupSharedCredentialAsync();
            var tenants = await _tenantService.GetTenants();
            var tenantIds = tenants
                .Where(t => !string.IsNullOrWhiteSpace(t.Id))
                .Select(t => t.Id!)
                .ToList();

            await BaseAzureService.WarmupTenantCredentialsAsync(tenantIds);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WarmupHostedService failed: {ex.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
