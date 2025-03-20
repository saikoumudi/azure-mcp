using System.CommandLine;
using Azure;
using Azure.Identity;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using Azure.Core;
using AzureMCP.Extensions;
using AzureMCP.Arguments;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands;

public abstract class BaseCommand<TArgs> : ICommand where TArgs : BaseArguments, new()
{
    protected readonly Option<string> _tenantOption;
    protected readonly Option<string> _subscriptionOption;
    protected readonly Option<AuthMethod> _authMethodOption;
    protected readonly Option<string> _resourceGroupOption;

    // Add retry policy options
    protected readonly Option<int> _retryMaxRetries;
    protected readonly Option<double> _retryDelayOption;
    protected readonly Option<double> _retryMaxDelayOption;
    protected readonly Option<RetryMode> _retryModeOption;
    protected readonly Option<double> _retryNetworkTimeoutOption;

    // Argument chain infrastructure
    protected List<ArgumentDefinition<string>> _argumentChain = [];

    protected BaseCommand()
    {
        // Initialize options
        _tenantOption = ArgumentDefinitions.Common.TenantId.ToOption();
        _subscriptionOption = ArgumentDefinitions.Common.SubscriptionId.ToOption();
        _authMethodOption = ArgumentDefinitions.Common.AuthMethod.ToOption();
        _resourceGroupOption = ArgumentDefinitions.Common.ResourceGroup.ToOption();

        // Initialize retry policy options
        _retryDelayOption = ArgumentDefinitions.RetryPolicy.Delay.ToOption();
        _retryMaxDelayOption = ArgumentDefinitions.RetryPolicy.MaxDelay.ToOption();
        _retryMaxRetries = ArgumentDefinitions.RetryPolicy.MaxRetries.ToOption();
        _retryModeOption = ArgumentDefinitions.RetryPolicy.Mode.ToOption();
        _retryNetworkTimeoutOption = ArgumentDefinitions.RetryPolicy.NetworkTimeout.ToOption();
    }

    // New method to register an argument chain
    protected void RegisterArgumentChain(params ArgumentChain<TArgs>[] argumentDefinitions)
    {
        var fullChain = new List<ArgumentDefinition<string>>
        {
            // Add common arguments
            CreateAuthMethodArgument(),
            CreateTenantIdArgument()
        };

        // Add command-specific arguments
        var subscriptionArg = CreateSubscriptionIdArgument();
        if (subscriptionArg != null)
        {
            fullChain.Add(subscriptionArg);
        }
        fullChain.AddRange(argumentDefinitions);

        _argumentChain = fullChain;
    }

    // Helper methods to create common arguments
    protected ArgumentChain<TArgs> CreateAuthMethodArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Common.AuthMethod.Name, ArgumentDefinitions.Common.AuthMethod.Description)
            .WithCommandExample($"{GetCommandPath()} --auth-method <auth-method>")
            .WithValueAccessor(args => args.AuthMethod?.ToString() ?? string.Empty)
            .WithValueLoader(async (context, args) => await GetAuthMethodOptions(context))
            .WithDefaultValue(AuthMethodArguments.GetDefaultAuthMethod().ToString())
            .WithIsRequired(false);
    }

    protected ArgumentChain<TArgs> CreateTenantIdArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Common.TenantId.Name, ArgumentDefinitions.Common.TenantId.Description)
            .WithCommandExample($"{GetCommandPath()} --tenant-id <tenant-id>")
            .WithValueAccessor(args => args.TenantId ?? string.Empty)
            .WithIsRequired(false);
    }

    protected ArgumentChain<TArgs>? CreateResourceGroupArgument()
    {
        if (!typeof(BaseArgumentsWithSubscriptionId).IsAssignableFrom(typeof(TArgs)))
        {
            return null;
        }

        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Common.ResourceGroup.Name, ArgumentDefinitions.Common.ResourceGroup.Description)
            .WithCommandExample($"{GetCommandPath()} --resource-group <resource-group>")
            .WithValueAccessor(args => (args as BaseArgumentsWithSubscriptionId)?.ResourceGroup ?? string.Empty)
            .WithValueLoader(async (context, args) => 
            {
                var subArgs = args as BaseArgumentsWithSubscriptionId;
                if (subArgs?.SubscriptionId == null)
                {
                    return new List<ArgumentOption>();
                }
                return await GetResourceGroupOptions(context, subArgs.SubscriptionId);
            })
            .WithIsRequired(true);
    }

    protected ArgumentChain<TArgs>? CreateSubscriptionIdArgument()
    {
        if (typeof(BaseArgumentsWithSubscriptionId).IsAssignableFrom(typeof(TArgs)))
        {
            return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Common.SubscriptionId.Name, ArgumentDefinitions.Common.SubscriptionId.Description)
            .WithCommandExample($"{GetCommandPath()} --subscription-id <subscription-id>")
            .WithValueAccessor(args => (args as BaseArgumentsWithSubscriptionId)?.SubscriptionId ?? string.Empty)
            .WithValueLoader(async (context, args) => await GetSubscriptionOptions(context))
            .WithIsRequired(true);
        }
        return null;
    }

    // Helper method to get auth method options
    protected virtual async Task<List<ArgumentOption>> GetAuthMethodOptions(CommandContext context)
    {
        // Use the helper method from AuthMethodArguments
        return await Task.FromResult(AuthMethodArguments.GetAuthMethodOptions());
    }

    // Helper method to get subscription options
    protected virtual async Task<List<ArgumentOption>> GetSubscriptionOptions(CommandContext context)
    {
        try
        {
            var subscriptionService = context.GetService<ISubscriptionService>();
            var subscriptions = await subscriptionService.GetSubscriptions();
            return subscriptions ?? [];
        }
        catch
        {
            // Silently handle subscription fetch failures
            return [];
        }
    }

    protected async Task<List<ArgumentOption>> GetResourceGroupOptions(CommandContext context, string subscriptionId)
    {
        if (string.IsNullOrEmpty(subscriptionId)) return [];

        var resourceGroupService = context.GetService<IResourceGroupService>();
        var resourceGroups = await resourceGroupService.GetResourceGroups(subscriptionId);

        return resourceGroups?.Select(rg => new ArgumentOption { Name = rg.Name, Id = rg.Id }).ToList() ?? [];
    }


    // Helper to get the command path for examples
    protected virtual string GetCommandPath()
    {
        // Get the command type name without the "Command" suffix
        string commandName = GetType().Name.Replace("Command", "");

        // Get the namespace to determine the service name
        string namespaceName = GetType().Namespace ?? "";
        string serviceName = "";

        // Extract service name from namespace (e.g., AzureMCP.Commands.Cosmos -> cosmos)
        if (namespaceName.Contains(".Commands."))
        {
            string[] parts = namespaceName.Split(".Commands.");
            if (parts.Length > 1)
            {
                string[] subParts = parts[1].Split('.');
                if (subParts.Length > 0)
                {
                    serviceName = subParts[0].ToLowerInvariant();
                }
            }
        }

        // Insert spaces before capital letters in the command name
        string formattedName = string.Concat(commandName.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).Trim();

        // Convert to lowercase and replace spaces with spaces (for readability in command examples)
        string commandPath = formattedName.ToLowerInvariant().Replace(" ", " ");

        // Prepend the service name if available
        if (!string.IsNullOrEmpty(serviceName))
        {
            commandPath = serviceName + " " + commandPath;
        }

        return commandPath;
    }

    // Process the argument chain
    protected async Task<bool> ProcessArgumentChain(CommandContext context, TArgs args)
    {
        // Ensure we have arguments to process
        if (_argumentChain == null || !_argumentChain.Any())
        {
            return true;
        }

        // First, add all arguments to the response and apply default values if needed
        foreach (var argDef in _argumentChain)
        {
            if (argDef is ArgumentChain<TArgs> typedArgDef)
            {
                // Get the current value
                string value = typedArgDef.ValueAccessor(args) ?? string.Empty;

                // If the value is empty but there's a default value, use the default value
                if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(typedArgDef.DefaultValue))
                {
                    // Try to set the default value on the args object using reflection
                    try
                    {
                        var prop = typeof(TArgs).GetProperty(typedArgDef.Name,
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.IgnoreCase);

                        if (prop != null && prop.CanWrite)
                        {
                            prop.SetValue(args, typedArgDef.DefaultValue);
                            value = typedArgDef.DefaultValue;
                        }
                    }
                    catch (Exception)
                    {
                        // Silently handle reflection errors
                    }
                }

                // Add the argument info to the response
                // Only include default value if no value is provided
                string? defaultToUse = string.IsNullOrEmpty(value) ? typedArgDef.DefaultValue : null;

                AddArgumentInfo(context, typedArgDef.Name, value,
                    typedArgDef.Description, typedArgDef.Command, defaultToUse, required: typedArgDef.Required);
            }
        }

        // Then, process required arguments that are missing values
        bool allRequiredArgumentsProvided = true;
        foreach (var argDef in _argumentChain)
        {
            if (argDef is ArgumentChain<TArgs> typedArgDef && typedArgDef.Required)
            {
                // Get the current value
                string value = typedArgDef.ValueAccessor(args) ?? string.Empty;

                // If the value is missing and this is a required argument
                if (string.IsNullOrEmpty(value))
                {
                    // Check if there's a default value
                    if (!string.IsNullOrEmpty(typedArgDef.DefaultValue))
                    {
                        // We consider this argument as provided since it has a default value
                        continue;
                    }

                    // Find the argument in the response
                    var argInfo = context.Response.Arguments?.FirstOrDefault(a => a.Name == typedArgDef.Name);
                    if (argInfo != null && typedArgDef.ValueLoader != null)
                    {
                        // Load suggested values
                        var suggestedValues = await typedArgDef.ValueLoader(context, args);
                        if (suggestedValues?.Any() == true)
                        {
                            argInfo.Values = suggestedValues;
                        }
                    }

                    // Mark that we're missing a required argument
                    allRequiredArgumentsProvided = false;
                }
            }
        }

        // Return true only if all required arguments are provided
        return allRequiredArgumentsProvided;
    }

    protected RetryPolicyArguments GetRetryPolicyArguments(System.CommandLine.Parsing.ParseResult parseResult)
    {
        return new RetryPolicyArguments
        {
            DelaySeconds = parseResult.GetValueForOption(_retryDelayOption),
            MaxDelaySeconds = parseResult.GetValueForOption(_retryMaxDelayOption),
            MaxRetries = parseResult.GetValueForOption(_retryMaxRetries),
            Mode = parseResult.GetValueForOption(_retryModeOption),
            NetworkTimeoutSeconds = parseResult.GetValueForOption(_retryNetworkTimeoutOption)
        };
    }

    protected void AddRetryOptionsToCommand(Command command)
    {
        command.AddOption(_retryDelayOption);
        command.AddOption(_retryMaxDelayOption);
        command.AddOption(_retryMaxRetries);
        command.AddOption(_retryModeOption);
        command.AddOption(_retryNetworkTimeoutOption);
    }

    protected void AddCommonOptionsToCommand(Command command)
    {
        command.AddOption(_tenantOption);
        command.AddOption(_subscriptionOption);
        command.AddOption(_authMethodOption);
    }

    protected void AddBaseOptionsToCommand(Command command)
    {
        AddCommonOptionsToCommand(command);
        AddRetryOptionsToCommand(command);
    }

    public abstract Command GetCommand();

    public IEnumerable<ArgumentDefinition<string>>? GetArgumentChain() => _argumentChain?.ToList();

    public void ClearArgumentChain() => _argumentChain.Clear();

    public void AddArgumentToChain(ArgumentDefinition<string> argument) => _argumentChain.Add(argument);

    public abstract Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult commandOptions);

    protected virtual TArgs BindArguments(ParseResult parseResult)
    {
        var args = new TArgs
        {
            TenantId = parseResult.GetValueForOption(_tenantOption),
            AuthMethod = parseResult.GetValueForOption(_authMethodOption)
        };

        if (args is BaseArgumentsWithSubscriptionId baseArgs)
        {
            // Bind base arguments
            baseArgs.SubscriptionId = parseResult.GetValueForOption(_subscriptionOption);
        }

        // Only create RetryPolicy if any retry options are specified
        if (parseResult.HasAnyRetryOptions())
        {
            args.RetryPolicy = new RetryPolicyArguments
            {
                MaxRetries = parseResult.GetValueForOption(_retryMaxRetries),
                DelaySeconds = parseResult.GetValueForOption(_retryDelayOption),
                MaxDelaySeconds = parseResult.GetValueForOption(_retryMaxDelayOption),
                Mode = parseResult.GetValueForOption(_retryModeOption),
                NetworkTimeoutSeconds = parseResult.GetValueForOption(_retryNetworkTimeoutOption)
            };
        }

        return args;
    }

    protected void AddArgumentInfo(CommandContext context, string name, string value, string description, string command, string? defaultValue = null, List<ArgumentOption>? values = null, bool required = false)
    {
        context.Response.Arguments ??= [];

        var argumentInfo = new ArgumentInfo(name, description, value, command, defaultValue, required: required);

        if (string.IsNullOrEmpty(value))
        {
            argumentInfo.Values = values;
        }

        context.Response.Arguments.Add(argumentInfo);
        context.Response.Arguments.SortArguments();
    }

    protected void HandleException(CommandResponse response, Exception ex)
    {
        response.Arguments ??= [];
        response.Status = GetStatusCode(ex);
        response.Message = GetErrorMessage(ex);
        response.Results = null;
        response.Arguments.SortArguments();
    }

    protected virtual string GetErrorMessage(Exception ex) => ex switch
    {
        AuthenticationFailedException authEx =>
            $"Authentication failed. Please run 'az login' to sign in to Azure. Details: {authEx.Message}",
        RequestFailedException rfEx => rfEx.Message,
        HttpRequestException httpEx =>
            $"Service unavailable or network connectivity issues. Details: {httpEx.Message}",
        _ => ex.Message  // Just return the actual exception message
    };

    protected virtual int GetStatusCode(Exception ex) => ex switch
    {
        AuthenticationFailedException => 401,
        RequestFailedException rfEx => rfEx.Status,
        HttpRequestException => 503,
        _ => 500
    };
}
