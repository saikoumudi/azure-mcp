# 🌟 Azure MCP Server

[![Install with NPX in VS Code](https://img.shields.io/badge/VS_Code-Install_Azure_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=Azure%20MCP%20Server&config=%7B%22command%22%3A%22npx%22%2C%22args%22%3A%5B%22-y%22%2C%22--registry%22%2C%22https%3A%2F%2Fpkgs.dev.azure.com%2Fazure-sdk%2Finternal%2F_packaging%2Fazure-sdk-for-js-pr%2Fnpm%2Fregistry%2F%22%2C%22%40azure%2Fmcp%40latest%22%2C%22server%22%2C%22start%22%5D%7D) [![Install with NPX in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Azure_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=Azure%20MCP%20Server&config=%7B%22command%22%3A%22npx%22%2C%22args%22%3A%5B%22-y%22%2C%22--registry%22%2C%22https%3A%2F%2Fpkgs.dev.azure.com%2Fazure-sdk%2Finternal%2F_packaging%2Fazure-sdk-for-js-pr%2Fnpm%2Fregistry%2F%22%2C%22%40azure%2Fmcp%40latest%22%2C%22server%22%2C%22start%22%5D%7D&quality=insiders)

## 🎯 Overview

Think of the Azure MCP Server as your AI-powered command center for Azure! It's a smart bridge that helps AI agents (like GitHub Copilot) understand and work with Azure services using simple, natural language conversations. No more remembering complex Azure CLI commands or digging through documentation! 🚀

### ✨ What Can You Do?

Just chat naturally with your AI assistant and let it handle the Azure magic! Here are some cool things you can try:

🔍 **Explore Your Azure Resources**
- "List my Azure storage accounts"
- "Show me all my Cosmos DB databases"
- "List my resource groups"

📊 **Query & Analyze**
- "Query my Log Analytics workspace"
- "Show me the tables in my Storage account"
- "List containers in my Cosmos DB database"

⚙️ **Manage Configuration**
- "List my App Configuration stores"
- "Show my key-value pairs in App Config"
- "Get details about my Storage container"

🔧 **Advanced Azure Operations**
- "List my CDN endpoints using Azure CLI"
- "Help me build an Azure application using Node.js"

### 🛠️ How It Works

The Azure MCP Server implements the [Model Context Protocol (MCP) specification](https://modelcontextprotocol.io) to create a seamless conversation between AI agents and Azure services through:

- 🔄 Smart JSON communication that AI agents understand
- 🏗️ Natural language commands that get translated to Azure operations
- 💡 Intelligent parameter suggestions and auto-completion
- ⚡ Consistent error handling that makes sense

Think of it as your AI assistant's Azure expertise toolkit - you ask questions in plain English, and it handles all the technical details behind the scenes!

## 🛠️ Available MCP Tools

The Azure MCP Server provides tools for interacting with the following Azure services:

### 📊 Azure Cosmos DB
- List Cosmos DB accounts
- List and query databases
- Manage containers and items
- Execute SQL queries against containers

### 💾 Azure Storage
- List Storage accounts
- Manage blob containers and blobs
- List and query Storage tables
- Get container properties and metadata

### 📈 Azure Monitor (Log Analytics)
- List Log Analytics workspaces
- Query logs using KQL
- List available tables
- Configure monitoring options

### ⚙️ Azure App Configuration
- List App Configuration stores
- Manage key-value pairs
- Handle labeled configurations
- Lock/unlock configuration settings

### 📦 Azure Resource Groups
- List resource groups
- Resource group management operations

### 🔧 Azure CLI Extension
- Execute Azure CLI commands directly
- Support for all Azure CLI functionality
- JSON output formatting
- Cross-platform compatibility

### 🚀 Azure Developer CLI (azd) Extension
- Execute Azure Developer CLI (azd) commands directly
- Support for template discovery, template initialization, provisioning and deployment
- Cross-platform compatibility

For detailed command documentation and examples, see [Azure MCP Commands](docs/azmcp-commands.md).

# 🔌 Getting Started

## Prerequisites
1. Install either the stable or Insiders release of VS Code:
   * 💫 [Stable release](https://code.visualstudio.com/download)
   * 🔮 [Insiders release](https://code.visualstudio.com/insiders)
2. Install [GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) and [GitHub Copilot Chat](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot-chat) extensions
3. Open VS Code in an empty folder

## 🚀 Installation

### ✨ Auto Install (The Fun Way!)

Why do things manually when you can do them automatically? Click one of these shiny buttons and let the magic happen:

[![Install with NPX in VS Code](https://img.shields.io/badge/VS_Code-Install_Azure_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=Azure%20MCP%20Server&config=%7B%22command%22%3A%22npx%22%2C%22args%22%3A%5B%22-y%22%2C%22--registry%22%2C%22https%3A%2F%2Fpkgs.dev.azure.com%2Fazure-sdk%2Finternal%2F_packaging%2Fazure-sdk-for-js-pr%2Fnpm%2Fregistry%2F%22%2C%22%40azure%2Fmcp%40latest%22%2C%22server%22%2C%22start%22%5D%7D) [![Install with NPX in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Azure_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=Azure%20MCP%20Server&config=%7B%22command%22%3A%22npx%22%2C%22args%22%3A%5B%22-y%22%2C%22--registry%22%2C%22https%3A%2F%2Fpkgs.dev.azure.com%2Fazure-sdk%2Finternal%2F_packaging%2Fazure-sdk-for-js-pr%2Fnpm%2Fregistry%2F%22%2C%22%40azure%2Fmcp%40latest%22%2C%22server%22%2C%22start%22%5D%7D&quality=insiders)

Just one click, and you're ready to go! 🎉

### 🔧 Manual Install (The Adventurous Way)

For those who like to take the scenic route or need more control over the installation process:

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

## 🔐 Temp Auth Setup for Pre-Release Version

### Windows Auth Setup
Copy paste the contents of this install script to a powershell prompt or download and run the script:

https://github.com/Azure/azure-mcp/blob/main/eng/scripts/New-Npmrc.ps1

### Linux Auth Setup (including Codespaces, Dev Containers)

>[!IMPORTANT]
>`vsts-npm-auth` only works on Windows, so you'll need to first generate the auth token on a Windows machine and then copy it to your Linux system.

1. On a Windows machine, follow the [Windows setup steps](#windows-auth-setup) above
2. Copy the contents from `%USERPROFILE%\.npmrc` on the Windows machine to `~/.npmrc` on your Linux system

### Manual Auth Setup
If you don't have a Windows machine, follow the manual steps listed in the "Other" section of the feed's [connection instructions for npm](https://dev.azure.com/azure-sdk/internal/_artifacts/feed/azure-sdk-for-js-pr/connect)

## 🧪 Test the Azure MCP Server

1. Open GitHub Copilot and [switch to Agent mode](https://code.visualstudio.com/docs/copilot/chat/chat-agent-mode)
2. You should see Azure MCP Server in the list of tools
3. Try a prompt that tells the agent to use the Azure MCP Server, such as "List my Azure Storage containers"
4. The agent should be able to use the Azure MCP Server tools to complete your query

## Troubleshooting

See [Troubleshooting guide](/TROUBLESHOOTING.md) for help with common issues and logging.

## 🔑 Authentication

Azure MCP Server seamlessly integrates with your host operating system's authentication mechanisms, making it super easy to get started! We use Azure Identity under the hood, which means you get all the goodness of DefaultAzureCredential.

### 🎭 How Authentication Works

The server automatically tries these authentication methods in order:

1. 🔐 **Environment Variables** (`EnvironmentCredential`) - Perfect for CI/CD pipelines
2. 👷 **Workload Identity** (`WorkloadIdentityCredential`) - Ideal for Kubernetes environments
3. 👤 **Managed Identity** (`ManagedIdentityCredential`) - Great for cloud-hosted scenarios
4. 🔄 **Shared Token Cache** (`SharedTokenCacheCredential`) - Uses cached tokens from other tools
5. 💫 **Visual Studio** (`VisualStudioCredential`) - Uses your Visual Studio credentials
6. 🌐 **Azure CLI** (`AzureCliCredential`) - Uses your existing Azure CLI login
7. 🔧 **Azure PowerShell** (`AzurePowerShellCredential`) - Uses your Az PowerShell login
8. 🚀 **Azure Developer CLI** (`AzureDeveloperCliCredential`) - Uses your azd login
9. 🎯 **Interactive Browser** (`InteractiveBrowserCredential`) - Falls back to browser-based login if needed

No special configuration needed - it just works! If you're already logged in through any of these methods, Azure MCP Server will automatically use those credentials. If not, it'll smoothly fall back to an interactive browser login.

### 🛡️ Security Note

Your credentials are always handled securely through the official Azure Identity SDK - we never store or manage tokens directly!

## 👥 Contributing

We welcome contributions to Azure MCP! Whether you're fixing bugs, adding new features, or improving documentation, your contributions are welcome.

Please read our [Contributing Guide](/CONTRIBUTING.md) for guidelines on:

- 🛠️ Setting up your development environment
- ✨ Adding new commands
- 📝 Code style and testing requirements
- 🔄 Making pull requests
