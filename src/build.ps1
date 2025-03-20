$currentDirectory = $PSScriptRoot
$projectFile = Join-Path $currentDirectory "AzureMCP"

# Create .dist directory if it doesn't exist
New-Item -ItemType Directory -Force -Path .dist | Out-Null

# Clean previous builds
Remove-Item -Path .dist\* -Recurse -Force -ErrorAction SilentlyContinue

# Build the project
Write-Host "Building azmcp for Windows..." -ForegroundColor Green
dotnet publish "$projectFile" `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output .dist `
    /p:PublishSingleFile=true `
    /p:PublishTrimmed=false `
    /p:PublishReadyToRun=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:DebugType=embedded

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild completed successfully!" -ForegroundColor Green
    Write-Host "Binary location: $((Resolve-Path '.dist\azmcp.exe').Path)" -ForegroundColor Yellow
} else {
    Write-Host "`nBuild failed!" -ForegroundColor Red
    exit 1
} 