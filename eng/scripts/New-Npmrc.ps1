#!/bin/env pwsh
#Requires -Version 7

Write-Host "Installing authentication tool" -ForegroundColor Yellow
if($IsLinux) {
    sudo npm install -g vsts-npm-auth
} else {
    npm install -g vsts-npm-auth
}

Write-Host "Writing .npmrc file`n" -ForegroundColor Yellow
@"
registry=https://pkgs.dev.azure.com/azure-sdk/internal/_packaging/azure-sdk-for-js-pr/npm/registry/
always-auth=true
"@ | Set-Content -Path ".npmrc" -Force -Encoding utf8

Write-Host "Authenticating" -ForegroundColor Yellow
if($IsLinux) {
    vsts-npm-auth -config .npmrc -targetConfig ~/.npmrc -force
} else {
    vsts-npm-auth -config .npmrc -force
}
