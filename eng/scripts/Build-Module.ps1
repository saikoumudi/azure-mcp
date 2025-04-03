#!/bin/env pwsh

[CmdletBinding(DefaultParameterSetName='none')]
param(
    [string] $OutputPath,
    [string] $Version,
    [switch] $SelfContained,
    [switch] $ReadyToRun,
    [Parameter(Mandatory=$true, ParameterSetName='Named')]
    [ValidateSet('windows','linux','macOS')]
    [string] $OperatingSystem,
    [Parameter(Mandatory=$true, ParameterSetName='Named')]
    [ValidateSet('x64','arm64')]
    [string] $Architecture
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$npmPackagePath = "$RepoRoot/eng/npm"
$projectFile = "$RepoRoot/src/AzureMCP.csproj"

if(!$Version) {
    $Version = & "$PSScriptRoot/Get-Version.ps1"
}

if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work"
}

Push-Location $RepoRoot
try {
    $runtime = $([System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier)
    $parts = $runtime.Split('-')
    if($OperatingSystem) {
        switch($OperatingSystem) {
            'windows' { $os = 'win' }
            'linux' { $os = 'linux' }
            'macos' { $os = 'osx' }
            default { Write-Error "Unsupported operating system: $OperatingSystem"; return }
        }
    } else {
        $os = $parts[0]
    }

    if($Architecture) {
        switch($Architecture) {
            'x64' { $arch = 'x64' }
            'arm64' { $arch = 'arm64' }
            default { Write-Error "Unsupported architecture: $Architecture"; return }
        }
    } else {
        $arch = $parts[1]
    }
    
    switch($os) {
        'win' { $node_os = 'win32'; $extension = '.exe' }
        'osx' { $node_os = 'darwin'; $extension = '' }
        default { $node_os = $os; $extension = '' }
    }

    $package = Get-Content "$npmPackagePath/package.json" -Raw | ConvertFrom-Json -AsHashtable

    Write-Host "Building version $Version, $os-$arch" -ForegroundColor Green

    $outputDir = "$OutputPath/$os-$arch"
    
    # Clear and recreate the package output directory
    Remove-Item -Path $outputDir -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

    Write-Host "Building azmcp for $os-$arch..." -ForegroundColor Green

    $command = "dotnet publish '$projectFile' --runtime '$os-$arch' --output '$outputDir' /p:Version=$Version" 
    
    if($SelfContained) {
        $command += " --self-contained"
    }

    if($ReadyToRun) {
        $command += " /p:PublishReadyToRun=true"
    }

    Invoke-LoggedCommand $command -GroupOutput

    # create a package.json in the output directory with a bin entry for the executable
    $platformPackageJson = [ordered]@{
        name        = "$($package.name)-$node_os-$arch"
        version     = $Version
        description = "$($package.description) for $node_os-$arch"
        repository  = $package.repository
        author      = $package.author
        bugs        = $package.bugs
        license     = $package.license
        bin         = @{ "azmcp-$node_os-$arch" = "azmcp$extension" }
        os          = @( $node_os )
        cpu         = @( $arch )
    }

    $platformPackageJson
    | ConvertTo-Json -Depth 10
    | Out-File -FilePath "$outputDir/package.json" -Encoding utf8

    Write-Host "Created package.json in $outputDir" -ForegroundColor Yellow

    Write-Host "`nBuild completed successfully!" -ForegroundColor Green
}
finally {
    Pop-Location
}
