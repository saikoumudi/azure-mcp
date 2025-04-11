// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Models;
using AzureMcp.Models.Command;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using ModelContextProtocol.Utils.Json;
using System.CommandLine;
using System.Reflection;
using System.Text.Json;

namespace AzureMcp.Commands.Server;

public class ToolOperations
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CommandFactory _commandFactory;

    public ToolOperations(IServiceProvider serviceProvider, CommandFactory commandFactory)
    {
        _serviceProvider = serviceProvider;
        _commandFactory = commandFactory;
        ToolsCapability = new ToolsCapability
        {
            CallToolHandler = OnCallTools,
            ListToolsHandler = OnListTools,
        };
    }

    public ToolsCapability ToolsCapability { get; }

    private Task<ListToolsResult> OnListTools(RequestContext<ListToolsRequestParams> requestContext,
        CancellationToken cancellationToken)
    {
        var allCommands = _commandFactory.AllCommands;
        if (allCommands.Count == 0)
        {
            return Task.FromResult(new ListToolsResult { Tools = [] });
        }

        var tools = allCommands
            .Where(kvp => kvp.Value.GetType().GetCustomAttribute<HiddenCommandAttribute>() == null)
            .Select(kvp => GetTool(kvp.Key, kvp.Value))
            .ToList();

        var listToolsResult = new ListToolsResult { Tools = tools };

        return Task.FromResult(listToolsResult);
    }

    private async Task<CallToolResponse> OnCallTools(RequestContext<CallToolRequestParams> parameters,
        CancellationToken cancellationToken)
    {
        if (parameters.Params == null)
        {
            return new CallToolResponse
            {
                Content = [new Content { Text = "Could not parse parameters from tool request." }],
                IsError = true,
            };
        }

        var command = _commandFactory.FindCommandByName(parameters.Params.Name);
        if (command == null)
        {
            return new CallToolResponse
            {
                Content = [new Content { Text = $"Could not find command: {parameters.Params.Name}" }],
                IsError = true,
            };

        }
        var commandContext = new CommandContext(_serviceProvider);

        var args = parameters.Params.Arguments != null
            ? string.Join(" ", parameters.Params.Arguments.Select(kvp => $"--{kvp.Key} \"{kvp.Value}\""))
            : string.Empty;
        var realCommand = command.GetCommand();
        var commandOptions = realCommand.Parse(args);

        var commandResponse = await command.ExecuteAsync(commandContext, commandOptions);
        var jsonResponse = JsonSerializer.Serialize(commandResponse.Results);

        return new CallToolResponse
        {
            Content = [new Content { Text = jsonResponse, MimeType = "application/json" }],
        };
    }

    private static Tool GetTool(string fullName, IBaseCommand command)
    {
        var underlyingCommand = command.GetCommand();
        var tool = new Tool
        {
            Name = fullName,
            Description = underlyingCommand.Description,
        };

        // Get the GetCommand method info to check for McpServerToolAttribute
        var getCommandMethod = command.GetType().GetMethod(nameof(IBaseCommand.GetCommand));
        if (getCommandMethod?.GetCustomAttribute<McpServerToolAttribute>() is { } mcpServerToolAttr)
        {
            tool.Annotations = new ToolAnnotations()
            {
                DestructiveHint = mcpServerToolAttr.Destructive,
                IdempotentHint = mcpServerToolAttr.Idempotent,
                OpenWorldHint = mcpServerToolAttr.OpenWorld,
                ReadOnlyHint = mcpServerToolAttr.ReadOnly,
                Title = mcpServerToolAttr.Title,
            };
        }

        var args = command.GetArguments()?.ToList();

        var schema = new Dictionary<string, object>
        {
            ["type"] = "object"
        };

        if (args != null && args.Count > 0)
        {
            var arguments = args.ToDictionary(
                    p => p.Name,
                    p => new
                    {
                        type = p.Type.ToLower(),
                        description = p.Description,
                    });

            schema["properties"] = arguments;
            schema["required"] = args.Where(p => p.Required).Select(p => p.Name).ToArray();
        }

        tool.InputSchema = JsonSerializer.SerializeToElement(schema, McpJsonUtilities.DefaultOptions);

        return tool;
    }
}