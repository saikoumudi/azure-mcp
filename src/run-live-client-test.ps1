# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# run-live-client-test.ps1

# Step 1: Run the build script
Write-Host "Running build.ps1..."
& "$PSScriptRoot/build.ps1"

# Check if build succeeded
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed. Exiting."
    exit $LASTEXITCODE
}

# Step 2: Set MCP_PATH environment variable
$distPath = Join-Path $PSScriptRoot ".dist\azmcp.exe"
$env:AZURE_MCP_PATH = $distPath
Write-Host "AZURE_MCP_PATH set to: $env:AZURE_MCP_PATH"

# Step 3: Run dotnet test for a specific test class
# Replace 'AzureMCP.Tests.Commands.Client.McpClientTests' with your actual fully-qualified test class name
$testClassName = "AzureMCP.Tests.Commands.Client.LiveClientTests"

Write-Host "Running tests in class: $testClassName"

Start-Process -NoNewWindow -Wait -FilePath "dotnet" `
    -ArgumentList @(
        "test",
        "--filter", "FullyQualifiedName~$testClassName"
    ) `
    -Environment @{
        "AZURE_MCP_PATH" = $env:AZURE_MCP_PATH
    }
