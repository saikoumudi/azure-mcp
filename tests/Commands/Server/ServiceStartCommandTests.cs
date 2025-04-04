using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace AzureMCP.Commands.Server.Tests;

public class ServiceStartCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICacheService _mockCacheService;
    private readonly ISubscriptionService _mockSubscriptionService;
    private readonly IStorageService _mockStorageService;
    private readonly ICosmosService _mockCosmosService;
    private readonly IMonitorService _mockMonitorService;
    private readonly IResourceGroupService _mockResourceGroupService;
    private readonly IAppConfigService _mockAppConfigService;

    public ServiceStartCommandTests()
    {
        // Setup mocks
        var builder = new ServiceCollection();
        _mockCacheService = Substitute.For<ICacheService>();
        _mockSubscriptionService = Substitute.For<ISubscriptionService>();
        _mockStorageService = Substitute.For<IStorageService>();
        _mockCosmosService = Substitute.For<ICosmosService>();
        _mockMonitorService = Substitute.For<IMonitorService>();
        _mockResourceGroupService = Substitute.For<IResourceGroupService>();
        _mockAppConfigService = Substitute.For<IAppConfigService>();

        // Configure service provider
        builder.AddSingleton(_mockCacheService);
        builder.AddSingleton(_mockSubscriptionService);
        builder.AddSingleton(_mockStorageService);
        builder.AddSingleton(_mockCosmosService);
        builder.AddSingleton(_mockMonitorService);
        builder.AddSingleton(_mockResourceGroupService);
        builder.AddSingleton(_mockAppConfigService);
        builder.AddSingleton<CommandFactory>(provider => {
            return new CommandFactory(provider);
        });

        _serviceProvider = builder.BuildServiceProvider();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = new ServiceStartCommand(_serviceProvider);

        // Assert
        Assert.Equal("start", command.GetCommand().Name);
        Assert.Equal("Starts Azure MCP Server.", command.GetCommand().Description);
    }

    [Fact]
    public void GetArgumentChain_ReturnsExpectedArguments()
    {
        // Arrange
        var command = new ServiceStartCommand(_serviceProvider);

        // Act
        var args = command.GetArgumentChain()?.ToList();

        // Assert
        Assert.NotNull(args);
        Assert.Equal(2, args.Count);
        Assert.Contains(args, a => a.Name == ArgumentDefinitions.Service.Transport.Name);
        Assert.Contains(args, a => a.Name == ArgumentDefinitions.Service.Port.Name);
    }

    [Fact]
    public void ClearArgumentChain_RemovesAllArguments()
    {
        // Arrange
        var command = new ServiceStartCommand(_serviceProvider);

        // Act
        command.ClearArgumentChain();

        // Assert
        var arguments = command.GetArgumentChain();
        Assert.NotNull(arguments);
        Assert.Empty(arguments);
    }

    [Fact]
    public void AddArgumentToChain_AddsNewArgument()
    {
        // Arrange
        var command = new ServiceStartCommand(_serviceProvider);
        var newArg = new ArgumentDefinition<string>("test", "test description");
        var arguments = command.GetArgumentChain();
        
        Assert.NotNull(arguments);

        var initialCount = arguments.Count();
        
        // Act
        command.AddArgumentToChain(newArg);

        // Assert
        Assert.Equal(initialCount + 1, arguments.Count());
        Assert.Contains(arguments, a => a.Name == "test");
    }
}
