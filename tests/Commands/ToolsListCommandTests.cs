using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMCP.Commands;
using AzureMCP.Commands.Tools;
using AzureMCP.Models.Tools;
using McpDotNet.Server;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace AzureMCP.Tests.Commands
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
            var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            expected.Add("azmcp cosmos database container item query");
            expected.Add("azmcp cosmos database container list");
            expected.Add("azmcp cosmos database list");
            expected.Add("azmcp cosmos account list");
            expected.Add("azmcp storage account list");
            expected.Add("azmcp storage table list");
            expected.Add("azmcp storage blob list");
            expected.Add("azmcp storage blob container list");
            expected.Add("azmcp storage blob container details");
            expected.Add("azmcp monitor log query");
            expected.Add("azmcp monitor workspace list");
            expected.Add("azmcp monitor table list");
            expected.Add("azmcp subscription list");
            expected.Add("azmcp group list");

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
