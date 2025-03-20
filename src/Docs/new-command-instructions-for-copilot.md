# azmcp Command Implementation Guide for AI Assistants

## Command Structure 

Commands follow this exact pattern:
```
azmcp <service> <resource> <operation>
```

Where:
- `<service>` - Azure service name (lowercase)
- `<resource>` - Resource type (plural noun, lowercase)
- `<operation>` - Action to perform (verb, lowercase)

Example: `azmcp cosmos databases list`

## File Structure

A complete command implementation requires the following files in this exact structure:

1. Arguments class: `src/Arguments/{Service}/{SubService}/{Resource}/{Resource}{Operation}Arguments.cs`
   Example: `src/Arguments/Storage/Blobs/Containers/ContainersDetailsArguments.cs`

2. Service interface method: `src/Services/Interfaces/I{Service}Service.cs`
   Example: `src/Services/Interfaces/IStorageService.cs`

3. Service implementation: `src/Services/Azure/{Service}/{Service}Service.cs`
   Example: `src/Services/Azure/Storage/StorageService.cs`

4. Command class: `src/Commands/{Service}/{SubService}/{Resource}/{Resource}{Operation}Command.cs`
   Example: `src/Commands/Storage/Blobs/Containers/ContainersDetailsCommand.cs`

5. Registration in: `src/Commands/CommandFactory.cs`

### File Organization Rules

1. Arguments and Commands should mirror the command structure:
   - `azmcp storage blobs containers list` → 
     - `src/Arguments/Storage/Blobs/Containers/ContainersListArguments.cs`
     - `src/Commands/Storage/Blobs/Containers/ContainersListCommand.cs`

2. Services are flat (not nested):
   - `src/Services/Interfaces/IStorageService.cs`
   - `src/Services/Azure/Storage/StorageService.cs`

3. Command namespaces should match folder structure:
   ```csharp
   namespace AzureMCP.Commands.Storage.Blobs.Containers;
   namespace AzureMCP.Arguments.Storage.Blobs.Containers;
   ```

## Step 1: Create Arguments Class

Location: `src/Arguments/{Service}/{Resource}{Operation}Arguments.cs`

Template:
```csharp
namespace AzureMCP.Arguments.{Service};

public class {Resource}{Operation}Arguments : Base{Service}Arguments
{
    // IMPORTANT: When adding properties that already exist in the base class,
    // you must use the 'new' keyword to explicitly indicate property hiding
    public new string? Account { get; set; }
    
    // Add other command-specific properties here
    public string? {SpecificParam} { get; set; }
}
```

Notes:
- Inherit from the appropriate base arguments class (`Base{Service}Arguments`)
- Base classes already include common properties like `SubscriptionId`, `TenantId`, and `AuthMethod`
- Use nullable properties with `?` for all properties

## Step 2: Define Service Interface Method

Location: `src/Services/Interfaces/I{Service}Service.cs`

Template:
```csharp
public interface I{Service}Service
{
    // IMPORTANT: Add your interface method before implementing in the service
    Task<TResult> {Operation}{Resource}(
        string {requiredParam1}, 
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null);
}
```

Notes:
- Use verb + resource name for method naming (e.g., `ListContainers`)
- Always include `subscriptionId` and `tenantId` parameters
- Return types are typically `Task<List<string>>` or `Task<object>`

## Step 3: Implement Service Method

Location: `src/Services/Azure/{Service}/{Service}Service.cs`

Template:
```csharp
public class {Service}Service : Base{Service}Service, I{Service}Service
{
    // Add your implementation
    public async Task<List<string>> {Operation}{Resource}(
        string {requiredParam1}, 
        string subscriptionId, 
        string? tenantId = null)
    {
        // Get credential for authentication
        var credential = await GetCredential(tenantId);
        
        try
        {
            // Create client
            var client = new {ServiceClient}(
                endpoint, 
                credential);
            
            // Call Azure SDK method
            var response = await client.{SdkMethod}Async();
            
            // Transform and return response
            return response.Value.Select(item => item.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{OperationName} failed", nameof({Operation}{Resource}));
            throw;
        }
    }
}
```

Notes:
- Use the appropriate Azure SDK client
- Always handle exceptions properly but allow them to propagate
- Transform responses to simple formats (typically lists of strings)

## Step 4: Create Command Class

Location: `src/Commands/{Service}/{Resource}{Operation}Command.cs`

Template:
```csharp
using System.CommandLine;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using AzureMCP.Arguments.{Service};

namespace AzureMCP.Commands.{Service};

public class {Resource}{Operation}Command : Base{Service}Command
{
    public {Resource}{Operation}Command() : base()
    {
        // Register argument chain using CreateBaseArgument method
        RegisterArgumentChain<{Resource}{Operation}Arguments>(
            // Required arguments - use CreateBaseArgument for custom arguments
            CreateBaseArgument<{Resource}{Operation}Arguments>(
                "account",
                "Account name",
                args => args.Account),
            CreateBaseArgument<{Resource}{Operation}Arguments>(

                "resource",
                "Resource name",
                args => args.Resource)
        );
    }

> **IMPORTANT**: Even if your command only needs base arguments (like `SubscriptionId`, `TenantId`, or `AuthMethod`), 
> you **must** call `RegisterArgumentChain<TArgs>()` with no arguments to ensure proper argument registration and validation.
> 
> Example for a command that only uses base arguments:
> ```csharp
> public AccountsListCommand() : base()
> {
>     // Register the argument chain with no additional arguments
>     // This ensures base arguments like SubscriptionId are properly required
>     RegisterArgumentChain<AccountsListArguments>();
> }
> ```
> 
> Failing to register the argument chain will cause the command to skip validation of base arguments,
> potentially resulting in runtime errors with messages like "SubscriptionId cannot be null or empty".

    public override Command GetCommand()
    {
        // Create command with name and description
        var command = new Command(
            "{operation}", 
            "{Description of what this command does, its requirements, and its output format.}");
        
        // Add common options
        AddBaseOptionsToCommand(command);
        
        // Add command-specific options WITHOUT setting IsRequired - this is handled by the argument chain
        command.AddOption(_accountOption);
        command.AddOption(_resourceOption);
        
        return command;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, object commandOptions)
    {
        // Parse command options
        var parseResult = (System.CommandLine.Parsing.ParseResult)commandOptions;
        var options = ParseOptions<{Resource}{Operation}Arguments>(parseResult);

        try
        {
            // Process argument chain and return early if required arguments are missing
            if (!await ProcessArgumentChain(context, options))
            {
                return context.Response;
            }

            // Get service from DI
            var service = context.GetService<I{Service}Service>();
            
            // Call service method with required arguments
            var results = await service.{Operation}{Resource}(
                options.{RequiredParam1}!,
                options.SubscriptionId!,
                options.TenantId
            );
                
            // Set results in response
            context.Response.Results = results?.Count > 0 ? 
                new { {resultName} = results } : 
                null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}
```

> **IMPORTANT NOTES**: 
> 1. Do not set `IsRequired = true` on System.CommandLine.Option instances in commands. The required status 
>    is handled by the argument chain system through `CreateBaseArgument` or other argument creation methods.
> 2. Even if your command only needs base arguments (like `SubscriptionId`, `TenantId`, or `AuthMethod`), 
>    you **must** call `RegisterArgumentChain<TArgs>()` with no arguments to ensure proper argument registration and validation.
> 
> Example for a command that only uses base arguments:
> ```csharp
> public AccountsListCommand() : base()
> {
>     // Register the argument chain with no additional arguments
>     // This ensures base arguments like SubscriptionId are properly required
>     RegisterArgumentChain<AccountsListArguments>();
> }
> ```
> 
> Failing to register the argument chain will cause the command to skip validation of base arguments,
> potentially resulting in runtime errors with messages like "SubscriptionId cannot be null or empty".

## Step 5: Register Command in CommandFactory

Location: `src/Commands/CommandFactory.cs`

Add your command to the `RegisterCommandGroups` method:

```csharp
private void RegisterCommandGroups()
{
    // Find or create the appropriate command group
    var resourceGroup = new CommandGroup(
        "{resource}", 
        "{Resource} operations - Description");
    serviceGroup.AddSubGroup(resourceGroup);
    
    // Register your command
    resourceGroup.AddCommand("{operation}", new {Resource}{Operation}Command());
}
```

## Step 6: Update README.md Documentation

Location: `README.md`

After implementing a new command, update the "Available Commands" section in the README.md file:

1. Find the appropriate service section (e.g., "Storage Operations", "Cosmos DB Operations")
2. Add your new command in the bash codeblock using this format:
   ```bash
   # Brief description of what the command does
   azmcp {service} {resource} {operation} --required-arg <required-arg> [--optional-arg <optional-arg>]
   ```

## Argument Chain System Reference

### Creating Arguments

There are two ways to create arguments:

1. Using base command helper methods (preferred):
```csharp
// In derived commands:
RegisterArgumentChain(
    CreateAccountArgument(GetAccountOptions),
    CreateContainerArgument(GetContainerOptions)
);
```

2. Creating custom arguments:
```csharp
ArgumentChain<TArgs>
    .Create("name", "description")
    .WithValueAccessor(args => args.Property)
    .WithValueLoader(async (context, args) => await LoadValues())
    .WithIsRequired(true);
```

### Base Command Helper Methods

Base command classes provide these argument creation methods:

```csharp
// In BaseCommand<TArgs>:
CreateAuthMethodArgument()
CreateTenantIdArgument() 
CreateSubscriptionIdArgument()

// In BaseStorageCommand<TArgs>:
CreateAccountArgument(accountOptionsLoader)
CreateContainerArgument(containerOptionsLoader)
CreateTableArgument(tableOptionsLoader)

// In BaseCosmosCommand<TArgs>:
CreateAccountArgument()
CreateDatabaseArgument()
CreateContainerArgument()
CreateQueryArgument()

// In BaseMonitorCommand<TArgs>:
CreateWorkspaceIdArgument(workspaceOptionsLoader)
```

### Argument Chain Registration Rules

The RegisterArgumentChain method:
1. Always adds AuthMethod (optional)
2. Always adds TenantId (optional)
3. Always adds SubscriptionId (required)
4. Then adds your custom arguments

Key rules:
```csharp
// 1. Base commands should NOT register arguments:
public abstract class BaseServiceCommand<TArgs> : BaseCommand<TArgs> 
{
    protected BaseServiceCommand() 
    {
        // ONLY initialize options, do not call RegisterArgumentChain
        _accountOption = ArgumentDefinitions.Service.Account.ToOption();
    }
}

// 2. Commands using only base arguments MUST register empty chain:
public class AccountsListCommand : BaseServiceCommand<AccountsListArguments>
{
    public AccountsListCommand() : base()
    {
        RegisterArgumentChain(); // Required!
    }
}

// 3. Commands with custom arguments add them to chain:
public class ContainersListCommand : BaseServiceCommand<ContainersListArguments>
{
    public ContainersListCommand() : base()
    {
        RegisterArgumentChain(
            CreateAccountArgument(GetAccountOptions),
            CreateContainerArgument(GetContainerOptions)
        );
    }
}

// 4. Special case - excluding base arguments:
public class SubscriptionsListCommand : BaseCommand<SubscriptionListArguments>
{
    public SubscriptionsListCommand() : base()
    {
        // Do NOT call RegisterArgumentChain
        // Instead manage argument chain manually if needed
    }
}
```

## Error Handling

To handle service-specific errors, override these methods in your command class:

```csharp
protected override string GetErrorMessage(Exception ex) => ex switch
{
    ServiceException serviceEx => serviceEx.Message,
    _ => base.GetErrorMessage(ex)
};

protected override int GetStatusCode(Exception ex) => ex switch
{
    ServiceException serviceEx => (int)serviceEx.Status,
    _ => base.GetStatusCode(ex)
};
```

## Complete Example

Here's a complete implementation example for reference:

### Arguments Class
```csharp
namespace AzureMCP.Arguments.Storage;

public class ContainersListArguments : BaseStorageArguments
{
    public string? Container { get; set; }
}
```

### Service Interface Method
```csharp
public interface IStorageService
{
    Task<List<string>> ListContainers(
        string accountName,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null);
}
```

### Command Class
```csharp
namespace AzureMCP.Commands.Storage;

public class ContainersListCommand : BaseStorageCommand
{
    public ContainersListCommand() : base()
    {
        RegisterArgumentChain<ContainersListArguments>(
            // Note: Use service-specific option loaders
            CreateAccountArgument<ContainersListArguments>(GetAccountOptions)
        );
    }

    public override Command GetCommand()
    {
        var command = new Command(
            "list", 
            "List all containers in a storage account.");
            
        AddBaseOptionsToCommand(command);
        return command;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, object commandOptions)
    {
        var parseResult = (System.CommandLine.Parsing.ParseResult)commandOptions;
        var options = ParseOptions<ContainersListArguments>(parseResult);

        try
        {
            if (!await ProcessArgumentChain(context, options))
            {
                return context.Response;
            }

            var storageService = context.GetService<IStorageService>();
            var containers = await storageService.ListContainers(
                options.Account!, 
                options.SubscriptionId!, 
                options.TenantId,
                options.RetryPolicy);
                
            context.Response.Results = containers?.Count > 0 ? 
                new { containers } : 
                null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}
```

### CommandFactory Registration
```csharp
private void RegisterCommandGroups()
{
    var storage = new CommandGroup("storage", "Storage operations");
    _rootGroup.AddSubGroup(storage);
    
    var containers = new CommandGroup(
        "containers", 
        "Storage container operations");
    storage.AddSubGroup(containers);
    
    containers.AddCommand("list", new Storage.ContainersListCommand());
}
```

## Service-Specific Base Commands

When creating a new base command class for a service:

1. Required using directives:
```csharp
using System.CommandLine;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using AzureMCP.Arguments;  // Required for BaseArguments
```

2. Always add proper type constraints:
```csharp
public abstract class Base{Service}Command : BaseCommand
{
    protected ArgumentChain<TArgs> Create{Resource}Argument<TArgs>() 
        where TArgs : BaseArguments  // Required type constraint
    {
        // Implementation
    }
}
```

This ensures proper type checking and inheritance from BaseArguments.

## Common Implementation Pitfalls

1. **Wrong Option Loader**: Always use service-specific option loaders:
   ```csharp
   // INCORRECT
   CreateAccountArgument<TArgs>(GetAccountOptions)
   
   // CORRECT for Storage
   CreateAccountArgument<TArgs>(GetStorageAccountOptions)
   
   // CORRECT for Cosmos
   CreateAccountArgument<TArgs>(GetCosmosAccountOptions)
   ```

2. **Base Arguments Properties**: Don't redefine properties that exist in base classes:
   ```csharp
   // INCORRECT - Account already exists in BaseStorageArguments
   public class ContainersListArguments : BaseStorageArguments
   {
       public string? Account { get; set; }  // Wrong! This hides base property
   }
   
   // CORRECT - Only add new properties
   public class ContainersListArguments : BaseStorageArguments
   {
       public string? Container { get; set; }  // Only add properties not in base
   }
   ```

3. **Option IsRequired Property**: Never set IsRequired directly on System.CommandLine.Option instances:
   ```csharp
   // INCORRECT - Setting IsRequired on the option
   protected Option<string> _workspaceOption = new Option<string>(
       "--workspace-id",
       "The Log Analytics workspace ID to query") 
       { IsRequired = true };
   
   // CORRECT - Let the argument chain handle required status
   protected Option<string> _workspaceOption = new Option<string>(
       "--workspace-id",
       "The Log Analytics workspace ID to query");
   ```

4. **Argument Chain Registration**: Always register the chain even if only using base arguments:
   ```csharp
   // INCORRECT - Missing registration
   public YourCommand() : base()
   {
   }
   
   // CORRECT - Empty registration for base args
   public YourCommand() : base()
   {
       RegisterArgumentChain<YourArguments>();
   }
   ```

## IMPORTANT: Base Command Methods Reference

The following methods are available in base commands for argument creation:

```csharp
// For custom arguments:
protected ArgumentChain<TArgs> CreateBaseArgument<TArgs>(

    string name,
    string description,
    Expression<Func<TArgs, string?>> valueAccessor,
    bool isRequired = true) where TArgs : BaseArguments

// For standard resource arguments:
protected ArgumentChain<TArgs> CreateAccountArgument<TArgs>()
protected ArgumentChain<TArgs> CreateDatabaseArgument<TArgs>()
protected ArgumentChain<TArgs> CreateContainerArgument<TArgs>()
protected ArgumentChain<TArgs> CreateTableArgument<TArgs>()
protected ArgumentChain<TArgs> CreateSubscriptionIdArgument<TArgs>()
protected ArgumentChain<TArgs> CreateTenantIdArgument<TArgs>()
```

## Implementation Order Best Practices

To avoid dependency errors, implement files in this order:

1. Interface method in `I{Service}Service.cs`
2. Service implementation in `{Service}Service.cs`
3. Arguments class
4. Command class
5. CommandFactory registration

This ensures all dependencies exist before they're referenced.

## Registering Arguments

Every command must call `RegisterArgumentChain()` to ensure proper argument validation:

```csharp
public YourCommand() : base()
{
    // For commands that only need base arguments (auth, tenant, subscription):
    RegisterArgumentChain();
    
    // For commands that need additional arguments:
    RegisterArgumentChain(
        CreateAccountArgument(),
        CreateContainerArgument()
    );
}
```

Exception: Commands that explicitly don't want certain base arguments (like `SubscriptionsListCommand`) 
should not call `RegisterArgumentChain()` and instead manage their own argument chain.

> **IMPORTANT**: Base command classes should not call `RegisterArgumentChain()` in their constructor
> since it would prevent derived classes from registering their specific arguments.
````
