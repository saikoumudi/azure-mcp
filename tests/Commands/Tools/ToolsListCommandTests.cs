using AzureMCP.Commands;
using AzureMCP.Commands.Tools;
using AzureMCP.Models.Tools;
using McpDotNet.Server;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.CommandLine;
using System.CommandLine.Parsing;
using Xunit;

namespace AzureMCP.Tests.Commands.Tools
{
    public class ToolsListCommandTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMcpServer _mcpServer;

        public ToolsListCommandTests()
        {
            _mcpServer = Substitute.For<IMcpServer>();

            var collection = new ServiceCollection();
            collection.AddSingleton(_mcpServer);

            _serviceProvider = collection.BuildServiceProvider();
        }

        [Fact]
        public async Task GetsExpectedFullNames()
        {
            var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "azmcp cosmos database container item query",
                "azmcp cosmos database container list",
                "azmcp cosmos database list",
                "azmcp cosmos account list",
                "azmcp storage account list",
                "azmcp storage table list",
                "azmcp storage blob list",
                "azmcp storage blob container list",
                "azmcp storage blob container details",
                "azmcp monitor log query",
                "azmcp monitor workspace list",
                "azmcp monitor table list",
                "azmcp subscription list",
                "azmcp group list"
            };

            var commandFactory = new CommandFactory(_serviceProvider);
            var provider = new ServiceCollection()
                .AddSingleton(commandFactory)
                .BuildServiceProvider();
            var tools = new ToolsListCommand();
            var args = tools.GetCommand().Parse([]);
            var context = new Models.CommandContext(provider);
            var actual = await tools.ExecuteAsync(context, args);

            Assert.NotNull(actual);
            Assert.IsType<List<CommandInfo>>(actual?.Results);

            var collection = (List<CommandInfo>)actual.Results;
            var actualNames = collection.Select(x => x.FullPath).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var ex in expected)
            {
                var withoutHyphens = ex.Replace('-', ' ');
                Assert.True(actualNames.Remove(withoutHyphens), "Expected but was not in hash set.  Expected: " + withoutHyphens);
            }

            Assert.Empty(actualNames);
        }
    }
}
