using AzureMCP.Commands;
using AzureMCP.Commands.Server;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace AzureMCP.Tests.Commands.Server;
public class ToolOperationsTest
{
    private readonly CommandFactory _commandFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMcpServer _server;

    public ToolOperationsTest()
    {
        _server = Substitute.For<IMcpServer>();
        _serviceProvider = Substitute.For<IServiceProvider>();
        _commandFactory = new CommandFactory(_serviceProvider);
    }

    [Fact]
    public async Task GetsAllTools()
    {
        var operations = new ToolOperations(_serviceProvider, _commandFactory);
        var requestContext = new RequestContext<ListToolsRequestParams>(_server, new ListToolsRequestParams());

        var handler = operations.ToolsCapability.ListToolsHandler;

        Assert.NotNull(handler);

        var result = await handler(requestContext, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Tools);

        foreach (var tool in result.Tools)
        {
            Assert.NotNull(tool);
            Assert.NotNull(tool.Name);
            Assert.NotNull(tool.Description);
        }
    }
}
