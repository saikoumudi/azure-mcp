using Azure.Core;
using Azure.Core.Pipeline;
using Azure.ResourceManager;
using AzureMCP.Arguments;
using AzureMCP.Services.Azure;
using AzureMCP.Services.Interfaces;
using NSubstitute;
using Xunit;

namespace AzureMCP.Tests.Services.Azure;

public class BaseAzureServiceTests
{
    [Fact]
    public void AddsCorrectPolicies()
    {
        // Arrange
        var tenantName = "test-tenant-name";
        var tenantId = "test-tenant-id";

        var tenantService = Substitute.For<ITenantService>();
        tenantService.GetTenantId(tenantName).Returns(tenantId);

        var options = Substitute.For<ClientOptions>();
        var azureService = new TestAzureService();

        // Act
        azureService.TestAddDefaultPolicies(options);

        // Assert
        options.Received().AddPolicy(
            Arg.Is<HttpPipelinePolicy>(x => x is UserAgentPolicy),
            Arg.Is(HttpPipelinePosition.BeforeTransport));
    }

    private class TestAzureService : BaseAzureService
    {
        public TestAzureService(ITenantService? tenantService = null)
            : base(tenantService)
        {
        }

        public Task<ArmClient> GetArmClientAsync(string? tenant = null, RetryPolicyArguments? retryPolicy = null) => CreateArmClientAsync(tenant, retryPolicy);

        public T TestAddDefaultPolicies<T>(T options) where T : ClientOptions => AddDefaultPolicies(options);
    }
}
