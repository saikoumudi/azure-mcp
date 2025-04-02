#!/bin/env pwsh
$currentDirectory = $PSScriptRoot
$projectFile = Join-Path $currentDirectory "AzureMCP.csproj"

# Create .dist directory if it doesn't exist
New-Item -ItemType Directory -Force -Path .dist | Out-Null

# Clean previous builds
Remove-Item -Path .dist/* -Recurse -Force -ErrorAction SilentlyContinue

$desc, $ext = if ($IsLinux) {
    "Linux"
} elseif ($IsMacOS) {
    "MacOS"
} elseif ($IsWindows) {
    "Windows", ".exe"
}

# Build the project
$runtime = $([System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier)
Write-Host "Building azmcp for $desc..." -ForegroundColor Green
dotnet publish "$projectFile" --runtime $runtime --self-contained --output .dist

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild completed successfully!" -ForegroundColor Green
    Write-Host "Binary location: $((Resolve-Path ".dist/azmcp$ext").Path)" -ForegroundColor Yellow
} else {
    Write-Host "`nBuild failed!" -ForegroundColor Red
    exit 1
}
