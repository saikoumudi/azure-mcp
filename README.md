# Azure MCP Server

An MCP Server and command-line interface designed for AI agents to interact with Azure services through standardized JSON communication patterns, following the Model Context Protocol (MCP) specification.

## Overview

The Azure MCP Server implements the Model Context Protocol (MCP) specification (https://modelcontextprotocol.io) to provide a standardized interface between AI agents and Azure services. This enables seamless integration of Azure capabilities into AI workflows through:

- MCP-compliant JSON schemas for service discovery and interaction
- Structured command and response patterns for AI agent consumption
- Context-aware parameter suggestions and auto-completion
- Standardized error handling and response formats

The server acts as a bridge, translating AI agent requests into Azure service operations while maintaining consistent interaction patterns defined by the MCP specification.

## Getting Started

Build the project:

1. Execute `src/build.ps1` to build binary `.dist/azmcp.exe` (`.dist/azmcp` on Linux and MacOS)

Start the MCP server:
```bash
azmcp server start
```

Start the MCP Server (SSE):
```bash
azmcp server start --transport sse
```

## Contributing

We welcome contributions to Azure MCP! Whether you're fixing bugs, adding new features, or improving documentation, your contributions are welcome.

Please read our [Contributing Guide](https://github.com/Azure/azure-mcp/blob/main/CONTRIBUTING.md) for guidelines on:
- Setting up your development environment
- Adding new commands
- Code style and testing requirements
- Making pull requests

# Using the Azure MCP Server in VS Code with GitHub Copilot

To use the Azure MCP with VS Code Insiders with GitHub Copilot Agent Mode, follow these instructions:

1. Install [VS Code Insiders](https://code.visualstudio.com/insiders/).
1. Install the pre-release versions of the [GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) and [GitHub Copilot Chat](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot-chat) extensions in VS Code Insiders.
1. Open VS Code Insiders in an empty folder.
1. Add `.vscode/mcp.json`:
```json
{
    "servers": {
        "Azure MCP Server": {
            "command": "npx",
            "args": [
                "-y",
                "--registry",
                "https://pkgs.dev.azure.com/azure-sdk/internal/_packaging/azure-sdk-for-js-pr/npm/registry/",
                "@azure/mcp",
                "server",
                "start"
            ]
        }
    }
}
```
1. Run `npm install -g vsts-npm-auth`
1. Create file `.npmrc`
```
registry=https://pkgs.dev.azure.com/azure-sdk/internal/_packaging/azure-sdk-for-js-pr/npm/registry/
always-auth=true
```
1. Run `vsts-npm-auth -config .npmrc`
1. Open GitHub Copilot and switch to Agent mode. You should see Azure MCP Server in the list of tools
1. Try a prompt that tells the agent to use the Azure MCP server, such as "List my Azure Storage containers."
1. The agent should be able to use the Azure MCP Server tools to complete your query.

## Using the Azure MCP Server in Cursor

Cursor support is not available at the moment - we are investigating usage, stay tuned.

## Tools

`azmcp tools list`

## Commands

`azmcp cosmos database list --subscription <subscription> --account-name <account-name>`

```json
{
  "status": 200,
  "message": "Success",
  "args": [
    {
      "name": "subscription",
      "description": "Azure Subscription ID",
      "command": "azmcp cosmos database list --subscription <subscription>",
      "value": "25fd0362-aa79-488b-b37b-d6e892009fdf"
    },
    {
      "name": "account-name",
      "description": "Cosmos DB Account Name",
      "command": "azmcp cosmos database list --account-name <account-name>",
      "value": "jongcosmostrash"
    }
  ],
  "results": {
    "databases": [
      "ToDoList"
    ]
  },
  "duration": 1234
}
```

## Example Schema with Missing Arg and Suggested Values

`azmcp cosmos database list --subscription <subscription>`

```json
{
  "status": 400,
  "message": "Missing required args",
  "args": [
    {
      "name": "subscription",
      "description": "Azure Subscription ID",
      "command": "azmcp cosmos database list --subscription <subscription>",
      "value": "25fd0362-aa79-488b-b37b-d6e892009fdf",
      "values": []
    },
    {
      "name": "account-name",
      "description": "Cosmos DB Account Name",
      "command": "azmcp cosmos database list --account-name <account-name>",
      "value": "",
      "values": [
        {
          "name": "jongcosmostrash",
          "id": "jongcosmostrash"
        }
      ]
    }
  ],
  "duration": 156
}
```

`azmcp cosmos database list`

```json
{
  "status": 400,
  "message": "Missing required args",
  "args": [
    {
      "name": "subscription",
      "description": "Azure Subscription ID",
      "command": "azmcp cosmos database list --subscription <subscription>",
      "value": "",
      "values": [
        {
          "name": "foo sub",
          "id": "823cb539-d44d-43ee-8dc8-023fd4f27396"
        }
      ]
    },
    {
      "name": "account-name",
      "description": "Cosmos DB Account Name",
      "command": "azmcp cosmos database list --account-name <account-name>",
      "value": ""
    }
  ],
  "duration": 89
}
```

## Error Handling

The CLI returns structured JSON responses for errors, including:
- Missing required args
- Invalid arg values
- Service availability issues
- Authentication errors

## Response Format

All responses follow a consistent JSON format:
```json
{
  "status": "200|403|500, etc",
  "message": "",
  "args": [],
  "results": [],
  "duration": 123
}
```

## Command Reference

### Global Args

The following args are available for all commands:

| Arg | Required | Default | Description |
|-----------|----------|---------|-------------|
| `--subscription` | Yes | - | Azure subscription ID for target resources |
| `--tenant-id` | No | - | Azure tenant ID for authentication |
| `--auth-method` | No | 'credential' | Authentication method ('credential', 'key', 'connectionString') |
| `--retry-max-retries` | No | 3 | Maximum retry attempts for failed operations |
| `--retry-delay` | No | 2 | Delay between retry attempts (seconds) |
| `--retry-max-delay` | No | 10 | Maximum delay between retries (seconds) |
| `--retry-mode` | No | 'exponential' | Retry strategy ('fixed' or 'exponential') |
| `--retry-network-timeout` | No | 100 | Network operation timeout (seconds) |

### Available Commands

#### Server Operations
```bash
# Start the MCP server
azmcp server start [--transport <transport>]
```

#### Subscription Management
```bash
# List available Azure subscriptions
azmcp subscription list [--tenant-id <tenant-id>]
```

#### Cosmos DB Operations
```bash
# List Cosmos DB accounts in a subscription
azmcp cosmos account list --subscription <subscription>

# List databases in a Cosmos DB account
azmcp cosmos database list --subscription <subscription> --account-name <account-name>

# List containers in a Cosmos DB database
azmcp cosmos database container list --subscription <subscription> --account-name <account-name> --database-name <database-name>

# Query items in a Cosmos DB container
azmcp cosmos database container item query --subscription <subscription> \
                       --account-name <account-name> \
                       --database-name <database-name> \
                       --container-name <container-name> \
                       [--query "SELECT * FROM c"]
```

#### Storage Operations
```bash
# List Storage accounts in a subscription
azmcp storage account list --subscription <subscription>

# List tables in a Storage account
azmcp storage table list --subscription <subscription> --account-name <account-name>

# List blobs in a Storage container
azmcp storage blob list --subscription <subscription> --account-name <account-name> --container-name <container-name>

# List containers in a Storage blob service
azmcp storage blob container list --subscription <subscription> --account-name <account-name>

# Get detailed properties of a storage container
azmcp storage blob container details --subscription <subscription> --account-name <account-name> --container-name <container-name>
```

#### Monitor Operations
```bash
# List Log Analytics workspaces in a subscription
azmcp monitor workspace list --subscription <subscription>

# List tables in a Log Analytics workspace
azmcp monitor table list --subscription <subscription> --workspace-name <workspace-name> --resource-group <resource-group>

# Query logs from Azure Monitor using KQL
azmcp monitor log query --subscription <subscription> \
                        --workspace-id <workspace-id> \
                        --table-name <table-name> \
                        --query "<kql-query>" \
                        [--hours <hours>] \
                        [--limit <limit>]

# Examples:
# Query logs from a specific table
azmcp monitor log query --subscription <subscription> \
                        --workspace-id <workspace-id> \
                        --table-name "AppEvents_CL" \
                        --query "| order by TimeGenerated desc"
```

#### App Configuration Operations
```bash
# List App Configuration stores in a subscription
azmcp appconfig account list --subscription <subscription>

# List all key-value settings in an App Configuration store
azmcp appconfig kv list --subscription <subscription> --account-name <account-name> [--key <key>] [--label <label>]

# Show a specific key-value setting
azmcp appconfig kv show --subscription <subscription> --account-name <account-name> --key <key> [--label <label>]

# Set a key-value setting
azmcp appconfig kv set --subscription <subscription> --account-name <account-name> --key <key> --value <value> [--label <label>]

# Lock a key-value setting (make it read-only)
azmcp appconfig kv lock --subscription <subscription> --account-name <account-name> --key <key> [--label <label>]

# Unlock a key-value setting (make it editable)
azmcp appconfig kv unlock --subscription <subscription> --account-name <account-name> --key <key> [--label <label>]

# Delete a key-value setting
azmcp appconfig kv delete --subscription <subscription> --account-name <account-name> --key <key> [--label <label>]
```

#### Resource Group Operations
```bash
# List resource groups in a subscription
azmcp group list --subscription <subscription>
```

#### CLI Utilities
```bash
# List all available commands and tools
azmcp tools list
```

## Installing internally published version

All of the builds currently publish to the internal / private npm feed:  
https://dev.azure.com/azure-sdk/internal/_artifacts/feed/azure-sdk-for-js-pr

### Authenticate
You'll need to authenticate an `.npmrc` file to access the feed using `npm` or `npx`.

To create the `.npmrc` file, run `eng/scripts/New-Npmrc.ps1 -Authenticate`.

This will add credentials for the feed url into your `~/.npmrc` file, not the local file.  

While the local file exists, it will tell `npm` to use the dev feed for all packages. If you delete the file, or run `npm`/`npx` from another directory, you'll need to pass `--registry https://pkgs.dev.azure.com/azure-sdk/internal/_packaging/azure-sdk-for-js-pr/npm/registry/` to use the feed.

For details on the script, see the [NPM connection instructions](https://dev.azure.com/azure-sdk/internal/_artifacts/feed/azure-sdk-for-js-pr/connect).

### Install

You can globally install the package and call it:
```
npm install -g --registry https://pkgs.dev.azure.com/azure-sdk/internal/_packaging/azure-sdk-for-js-pr/npm/registry/ @azure/mcp

azmcp server start
```

Or, you can use npx to install and run in one command:
```bash
npx -y --registry https://pkgs.dev.azure.com/azure-sdk/internal/_packaging/azure-sdk-for-js-pr/npm/registry/ @azure/mcp server start
```

If you still have the `.npmrc` in your local directory, you can omit the `--registry` option.


