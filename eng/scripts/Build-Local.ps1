#!/bin/env pwsh

[CmdletBinding(DefaultParameterSetName='none')]
param(
    [switch] $SelfContained,
    [switch] $ReadyToRun,
    [switch] $UsePaths,
    [switch] $AllPlatforms
)

$RepoRoot = (Resolve-Path "$PSScriptRoot/../..").Path.Replace('\', '/')

$packagesPath = "$RepoRoot/.work/packages"
$distPath = "$RepoRoot/.dist"

function Build($os, $arch) {
    & "$RepoRoot/eng/scripts/Build-Module.ps1" `
        -OperatingSystem $os `
        -Architecture $arch `
        -SelfContained:$SelfContained `
        -ReadyToRun:$ReadyToRun `
        -OutputPath $packagesPath
}

Remove-Item -Path $packagesPath -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path $distPath -Recurse -Force -ErrorAction SilentlyContinue

if($AllPlatforms) {
    Build -os linux -arch x64
    Build -os windows -arch x64
    Build -os windows -arch arm64
    Build -os macos -arch x64
    Build -os macos -arch arm64
}
else {
    $runtime = $([System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier)
    $parts = $runtime.Split('-')
    $os = $parts[0]
    $arch = $parts[1]

    if($os -eq 'win') {
        $os = 'windows'
    } elseif($os -eq 'osx') {
        $os = 'macos'
    }

    Build -os $os -arch $arch
}

& "$RepoRoot/eng/scripts/Pack-Modules.ps1" `
    -ArtifactsPath $packagesPath `
    -UsePaths:$UsePaths `
    -OutputPath $distPath
