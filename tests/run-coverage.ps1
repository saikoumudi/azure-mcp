# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#!/usr/bin/env pwsh

$ErrorActionPreference = 'Stop'

# Ensure reportgenerator tool is installed
if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
    Write-Host "Installing reportgenerator tool..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

# Clean previous results
if (Test-Path TestResults) {
    Remove-Item -Recurse -Force TestResults
}

# Run tests with coverage
Write-Host "Running tests with coverage..."
dotnet test AzureMcp.Tests.csproj `
    --collect:"XPlat Code Coverage" `
    --results-directory TestResults `
    --settings coverage.runsettings

# Find the coverage file
$coverageFile = Get-ChildItem -Recurse -Filter "coverage.cobertura.xml" TestResults | Select-Object -First 1

if (-not $coverageFile) {
    Write-Error "No coverage file found!"
    exit 1
}

# Generate reports
Write-Host "Generating coverage reports..."
reportgenerator `
    -reports:$coverageFile.FullName `
    -targetdir:TestResults/CoverageReport `
    "-reporttypes:Html;HtmlSummary;Cobertura" `
    -assemblyfilters:"+azmcp" `
    -classfilters:"-*Tests*;-*Program"

Write-Host "Coverage report generated at TestResults/CoverageReport/index.html"

# Open the report in default browser
$reportPath = Join-Path (Get-Location) "TestResults/CoverageReport/index.html"
if (Test-Path $reportPath) {
    Write-Host "Opening coverage report in browser..."
    Start-Process $reportPath
} else {
    Write-Error "Could not find coverage report at $reportPath"
}