#!/bin/env pwsh

[CmdletBinding(DefaultParameterSetName='none')]
param(
    [switch] $Authenticate,
    [string] $Registry = 'https://pkgs.dev.azure.com/azure-sdk/internal/_packaging/azure-sdk-for-js-pr/npm/registry/'
)

. "$PSScriptRoot/../common/scripts/common.ps1"

if ($Authenticate) {
    Write-Host "Installing authentication tool" -ForegroundColor Yellow
    Invoke-LoggedCommand "npm install -g vsts-npm-auth"
}

$contents = @"
registry=$Registry
always-auth=true
"@

Write-Host "Writing .npmrc file`n" -ForegroundColor Yellow
$contents | Set-Content -Path ".npmrc" -Force -Encoding utf8

if ($Authenticate) {
    Write-Host "Authenticating" -ForegroundColor Yellow
    Invoke-LoggedCommand "vsts-npm-auth -config .npmrc"
}

Write-Host "Writing .npmrc file..." -ForegroundColor Green
