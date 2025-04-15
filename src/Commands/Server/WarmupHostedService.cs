// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Services.Azure;
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
        await BaseAzureService.WarmupTokenCredentialAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
