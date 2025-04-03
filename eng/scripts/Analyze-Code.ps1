#!/bin/env pwsh

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

Push-Location $RepoRoot
try {
    # source analysis steps here
}
finally {
    Pop-Location
}