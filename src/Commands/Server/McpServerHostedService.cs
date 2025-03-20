using System.CommandLine;
using System.Text.Json;
using AzureMCP.Models;
using McpDotNet.Protocol.Types;
using McpDotNet.Server;
using Microsoft.Extensions.Hosting;

namespace AzureMCP.Commands.Server
{
    public class McpServerHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMcpServer _mcpServer;
        private readonly CommandFactory _commandFactory;

        public McpServerHostedService(IMcpServer mcpServer, IServiceProvider serviceProvider,
            CommandFactory commandFactory)
        {
            _serviceProvider = serviceProvider;
            _mcpServer = mcpServer;
            _commandFactory = commandFactory;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var allCommands = _commandFactory.AllCommands;
            if (allCommands.Count == 0)
            {
                Console.Error.WriteLine("Could not resolve CommandFactory.AllCommands when getting tools list.");
                return;
            }

            _mcpServer.ListToolsHandler = (parameters, cancellationToken) => OnListTools(allCommands, parameters, cancellationToken);
            _mcpServer.CallToolHandler = OnCallTools;

            await base.StartAsync(cancellationToken);
        }

        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            if (_mcpServer != null)
            {
                await _mcpServer.DisposeAsync().AsTask();
            }

            await base.StopAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _mcpServer != null ? _mcpServer.StartAsync(stoppingToken) : Task.CompletedTask;
        }

        private static Task<ListToolsResult> OnListTools(IReadOnlyDictionary<string, ICommand> commands,
            RequestContext<ListToolsRequestParams> requestContext, CancellationToken cancellationToken)
        {
            var tools = commands.Select(kvp => GetTool(kvp.Key, kvp.Value)).ToList();
            var listToolsResult = new ListToolsResult { Tools = tools };
            //listToolsResult.ConversationId = requestContext;

            return Task.FromResult(listToolsResult);
        }

        private async Task<CallToolResponse> OnCallTools(RequestContext<CallToolRequestParams> parameters, CancellationToken cancellationToken)
        {
            if (parameters.Params == null)
            {
                return new CallToolResponse
                {
                    Content = [new Content {
                            Text = "Could not parse parameters from tool request.",
                            Type = "text",
                            MimeType = "text/plain"
                        }],
                };
            }

            var command = _commandFactory.FindCommandByName(parameters.Params.Name);
            if (command == null)
            {
                return new CallToolResponse
                {
                    Content = [new Content
                        {
                            Text = $"Could not find command: {parameters.Params.Name}",
                            Type = "text", MimeType = "text/plain"
                        }],
                };

            }
            var commandContext = new CommandContext(_serviceProvider);

            var args = parameters.Params.Arguments != null
                ? string.Join(" ", parameters.Params.Arguments.Select(kvp => $"--{kvp.Key} {kvp.Value}"))
                : string.Empty;
            var realCommand = command.GetCommand();
            var commandOptions = realCommand.Parse(args);

            var commandResponse = await command.ExecuteAsync(commandContext, commandOptions);
            var jsonResponse = JsonSerializer.Serialize(commandResponse.Results);

            return new CallToolResponse
            {
                Content = [new Content { Text = jsonResponse, Type = "text", MimeType = "application/json" }],
            };
        }

        private static Tool GetTool(string fullName, ICommand command)
        {
            var underlyingCommand = command.GetCommand();
            var tool = new Tool
            {
                Name = fullName,
                Description = underlyingCommand.Description,
            };

            var argumentsChain = command.GetArgumentChain()?.ToList();

            if (argumentsChain != null && argumentsChain.Count > 0)
            {
                tool.InputSchema = new JsonSchema
                {
                    Type = "object",
                    Properties = argumentsChain.ToDictionary(
                        p => p.Name,
                        p => new JsonSchemaProperty
                        {
                            Type = p.Type.ToLower(),
                            Description = p.Description,
                        }),
                    Required = [.. argumentsChain.Where(p => p.Required).Select(p => p.Name)]
                };
            }
            else
            {
                tool.InputSchema = new JsonSchema
                {
                    Type = "object"
                };
            }
            return tool;
        }
    }
}