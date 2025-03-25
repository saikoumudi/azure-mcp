using AzureMCP.Arguments;
using AzureMCP.Commands.Subscriptions;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.CommandLine;
using System.CommandLine.Parsing;
using Xunit;

namespace AzureMCP.Tests.Commands.Subscriptions
{
    public class SubscriptionListCommandTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMcpServer _mcpServer;
        private readonly ISubscriptionService _subscriptionService;
        private readonly SubscriptionsListCommand _command;
        private readonly CommandContext _context;
        private readonly Parser _parser;

        public SubscriptionListCommandTests()
        {
            _mcpServer = Substitute.For<IMcpServer>();
            _subscriptionService = Substitute.For<ISubscriptionService>();

            var collection = new ServiceCollection();
            collection.AddSingleton(_mcpServer);
            collection.AddSingleton(_subscriptionService);

            _serviceProvider = collection.BuildServiceProvider();
            _command = new SubscriptionsListCommand();
            _context = new CommandContext(_serviceProvider);
            _parser = new Parser(_command.GetCommand());
        }

        [Fact]
        public async Task ExecuteAsync_NoParameters_ReturnsSubscriptions()
        {
            // Arrange
            var expectedSubscriptions = new List<ArgumentOption>
            {
                new() { Id = "sub1", Name = "Subscription 1" },
                new() { Id = "sub2", Name = "Subscription 2" }
            };

            _subscriptionService
                .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyArguments>())
                .Returns(expectedSubscriptions);

            var args = _parser.Parse("");

            // Act
            var result = await _command.ExecuteAsync(_context, args);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.NotNull(result.Results);
            await _subscriptionService.Received(1).GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyArguments>());
        }

        [Fact]
        public async Task ExecuteAsync_WithTenantId_PassesTenantToService()
        {
            // Arrange
            var tenantId = "test-tenant-id";
            var args = _parser.Parse($"--tenant-id {tenantId}");

            _subscriptionService
                .GetSubscriptions(Arg.Is<string>(x => x == tenantId), Arg.Any<RetryPolicyArguments>())
                .Returns([new() { Id = "sub1", Name = "Sub1" }]);

            // Act
            var result = await _command.ExecuteAsync(_context, args);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            await _subscriptionService.Received(1).GetSubscriptions(
                Arg.Is<string>(x => x == tenantId),
                Arg.Any<RetryPolicyArguments>());
        }

        [Fact]
        public async Task ExecuteAsync_EmptySubscriptionList_ReturnsNullResults()
        {
            // Arrange
            _subscriptionService
                .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyArguments>())
                .Returns([]);

            var args = _parser.Parse("");

            // Act
            var result = await _command.ExecuteAsync(_context, args);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Null(result.Results);
        }

        [Fact]
        public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorInResponse()
        {
            // Arrange
            var expectedError = "Test error message";
            _subscriptionService
                .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyArguments>())
                .Returns(Task.FromException<List<ArgumentOption>>(new Exception(expectedError)));

            var args = _parser.Parse("");

            // Act
            var result = await _command.ExecuteAsync(_context, args);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.Status);
            Assert.Contains(expectedError, result.Message);
        }

        [Fact]
        public async Task ExecuteAsync_WithAuthMethod_PassesAuthMethodToCommand()
        {
            // Arrange
            var authMethod = AuthMethod.Credential.ToString().ToLowerInvariant();
            var args = _parser.Parse($"--auth-method {authMethod}");

            _subscriptionService
                .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyArguments>())
                .Returns([new() { Id = "sub1", Name = "Sub1" }]);

            // Act
            var result = await _command.ExecuteAsync(_context, args);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            await _subscriptionService.Received(1).GetSubscriptions(
                Arg.Any<string>(),
                Arg.Any<RetryPolicyArguments>());
        }
    }
}
