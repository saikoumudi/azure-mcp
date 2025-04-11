// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;
using Azure.ResourceManager;
using AzureMcp.Arguments;
using AzureMcp.Services.Azure;
using AzureMcp.Services.Interfaces;
using NSubstitute;
using Xunit;

namespace AzureMcp.Tests.Services.Azure;

public class BaseAzureServiceTests
{
    private readonly ITenantService _tenantService = Substitute.For<ITenantService>();
    private readonly ClientOptions _options = Substitute.For<ClientOptions>();
    private readonly TestAzureService _azureService;

    public BaseAzureServiceTests()
    {
        _tenantService.GetTenantId("test-tenant-name").Returns("test-tenant-id");
        _azureService = new TestAzureService();
    }

    [Fact]
    public void AddsCorrectPolicies()
    {
        // Act
        _azureService.TestAddDefaultPolicies(_options);

        // Assert
        _options.Received().AddPolicy(
            Arg.Is<HttpPipelinePolicy>(x => x is UserAgentPolicy),
            Arg.Is(HttpPipelinePosition.BeforeTransport));
    }

    private sealed class TestAzureService(ITenantService? tenantService = null) : BaseAzureService(tenantService)
    {
        public Task<ArmClient> GetArmClientAsync(string? tenant = null, RetryPolicyArguments? retryPolicy = null) =>
            CreateArmClientAsync(tenant, retryPolicy);

        public T TestAddDefaultPolicies<T>(T options) where T : ClientOptions =>
            AddDefaultPolicies(options);
    }
}