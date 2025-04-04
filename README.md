# Azure MCP Server

An MCP Server and command-line interface designed for AI agents to interact with Azure services through standardized JSON communication patterns, following the Model Context Protocol (MCP) specification.

## Overview

The Azure MCP Server implements the Model Context Protocol (MCP) specification (https://modelcontextprotocol.io) to provide a standardized interface between AI agents and Azure services. This enables seamless integration of Azure capabilities into AI workflows through:

- MCP-compliant JSON schemas for service discovery and interaction
- Structured command and response patterns for AI agent consumption
- Context-aware parameter suggestions and auto-completion
- Standardized error handling and response formats

The server acts as a bridge, translating AI agent requests into Azure service operations while maintaining consistent interaction patterns defined by the MCP specification.

## Available MCP Tools

The Azure MCP Server provides tools for interacting with the following Azure services:

### Azure Cosmos DB

- List Cosmos DB accounts
- List and query databases
- Manage containers and items
- Execute SQL queries against containers

### Azure Storage

- List Storage accounts
- Manage blob containers and blobs
- List and query Storage tables
- Get container properties and metadata

### Azure Monitor (Log Analytics)

- List Log Analytics workspaces
- Query logs using KQL
- List available tables
- Configure monitoring options

### Azure App Configuration

- List App Configuration stores
- Manage key-value pairs
- Handle labeled configurations
- Lock/unlock configuration settings

### Azure Resource Groups

- List resource groups
- Resource group management operations

For detailed command documentation and examples, see [Azure MCP Commands](docs/azmcp-commands.md).

# Install Azure MCP Server in VS Code with GitHub Copilot

1. Install [VS Code Insiders](https://code.visualstudio.com/insiders/).
1. Install the pre-release versions of the [GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) and [GitHub Copilot Chat](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot-chat) extensions in VS Code Insiders.
1. Open VS Code Insiders in an empty folder.

## Temp Auth Setup for Pre-Release Version

### Install Script

Copy paste the contents of this install script to a powershell prompt or download and run the script:

https://github.com/Azure/azure-mcp/blob/main/eng/scripts/New-Npmrc.ps1 

### Or, manual steps

1. Run `npm install -g vsts-npm-auth`
1. Create file `.npmrc`

    ```npm
    registry=https://pkgs.dev.azure.com/azure-sdk/internal/_packaging/azure-sdk-for-js-pr/npm/registry/
    always-auth=true
    ```

1. Run `vsts-npm-auth -config .npmrc`

    > **IMPORTANT**  
    > WSL users need to run `vsts-npm-auth -config .npmrc -target ~/.npmrc`

## Manual Install:

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
            "@azure/mcp@latest",
            "server",
            "start"
          ]
        }
      }
    }
    ```

## Test the Azure MCP Server

1. Open GitHub Copilot and switch to Agent mode. You should see Azure MCP Server in the list of tools
1. Try a prompt that tells the agent to use the Azure MCP server, such as "List my Azure Storage containers."
1. The agent should be able to use the Azure MCP Server tools to complete your query.

# Using the Azure MCP Server in Cursor

Cursor support is not available at the moment - we are investigating usage, stay tuned.

## Contributing

We welcome contributions to Azure MCP! Whether you're fixing bugs, adding new features, or improving documentation, your contributions are welcome.

Please read our [Contributing Guide](https://github.com/Azure/azure-mcp/blob/main/CONTRIBUTING.md) for guidelines on:

- Setting up your development environment
- Adding new commands
- Code style and testing requirements
- Making pull requests
