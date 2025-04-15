// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace AzureMcp.Commands.Server;

public class WarmupHostedService : IHostedService
{
    private readonly ITenantService _tenantService;
    private readonly ISubscriptionService _subscriptionService;

    public WarmupHostedService(ITenantService tenantService, ISubscriptionService subscriptionService)
    {
        _tenantService = tenantService;
        _subscriptionService = subscriptionService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _subscriptionService.GetSubscriptions(null, null);
        var tenants = await _tenantService.GetTenants();
        foreach (var tenant in tenants)
        {
            _ = await _subscriptionService.GetSubscriptions(tenant.Id, null);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
