# Script to add copyright headers to all files
$copyrightText = "Copyright (c) Microsoft Corporation."
$licenseText = "Licensed under the MIT License."

# Define comment style patterns
$commentStyles = @{
    'doubleslash' = @{
        extensions = @('.bicep', '.cpp', '.cs', '.dart', '.fs', '.glsl', '.go', '.groovy', 
                      '.java', '.js', '.kt', '.ll', '.mm', '.php', '.rs', '.scala', '.ts')
        style = @{
            prefix = "//"
            multi = $false
        }
    }
    'hash' = @{
        extensions = @('.cmake', '.coffee', '.jl', '.pl', '.ps1', '.py', '.r', '.rb', '.sh', 
                      '.yaml', '.yml', 'Makefile')
        style = @{
            prefix = "#"
            multi = $false
        }
    }
    'dash' = @{
        extensions = @('.lua', '.sql')
        style = @{
            prefix = "--"
            multi = $false
        }
    }
    'percent' = @{
        extensions = @('.matlab', '.tex')
        style = @{
            prefix = "%"
            multi = $false
        }
    }
    'semicolon' = @{
        extensions = @('.el')
        style = @{
            prefix = ";;"
            multi = $false
        }
    }
    'xml' = @{
        extensions = @('.html', '.md', '.xml')
        style = @{
            prefix = "<!--"
            suffix = "-->"
            multi = $true
        }
    }
    'c-style' = @{
        extensions = @('.c', '.h')
        style = @{
            prefix = "/*"
            middle = " *"
            suffix = " */"
            multi = $true
        }
    }
    'haskell' = @{
        extensions = @('.hs')
        style = @{
            prefix = "{-"
            suffix = "-}"
            multi = $true
        }
    }
    'ocaml' = @{
        extensions = @('.ml')
        style = @{
            prefix = "(*"
            suffix = "*)"
            multi = $true
        }
    }
    'batch' = @{
        extensions = @('.bat', '.cmd')
        style = @{
            prefix = "::"
            multi = $false
        }
    }
}

# Create extension to style lookup for faster access
$extensionStyles = @{}
foreach ($styleName in $commentStyles.Keys) {
    $style = $commentStyles[$styleName]
    foreach ($ext in $style.extensions) {
        $extensionStyles[$ext] = $style.style
    }
}

function Get-FileExtension {
    param (
        [string]$filePath
    )
    
    if ($filePath -match "Makefile$") {
        return "Makefile"
    }
    
    # Special handling for Matlab files
    if ($filePath -match "\.m$") {
        # Check if it's a Matlab file by looking for Matlab-specific keywords
        $content = Get-Content $filePath -Raw
        if ($content -match "function\s+|classdef\s+|^\s*%") {
            return ".matlab"
        }
        # Otherwise assume it's an Objective-C file
        return ".mm"  # Changed from .m to .mm for clarity
    }
    
    return [System.IO.Path]::GetExtension($filePath).ToLower()
}

function Get-CopyrightHeader {
    param (
        [string]$filePath
    )

    $ext = Get-FileExtension $filePath
    $style = $extensionStyles[$ext]

    if (-not $style) {
        Write-Warning "No comment style defined for extension: $ext"
        return $null
    }

    if ($style.multi) {
        if ($style.middle) {
            # C-style multi-line comment
            return @"
$($style.prefix)
$($style.middle) $copyrightText
$($style.middle) $licenseText
$($style.suffix)


"@
        }
        else {
            # HTML/XML-style or other multi-line comment
            return @"
$($style.prefix) $copyrightText
$($style.prefix) $licenseText $($style.suffix)


"@
        }
    }
    else {
        # Single-line comment style
        return @"
$($style.prefix) $copyrightText
$($style.prefix) $licenseText


"@
    }
}

Get-ChildItem -Path $PSScriptRoot\..\..\src, $PSScriptRoot\..\..\tests -Recurse -File | ForEach-Object {
    # Skip auto-generated files
    if ($_.FullName -like "*\obj\*" -or $_.FullName -like "*\bin\*") {
        Write-Host "Skipping generated file $($_.FullName)"
        return
    }

    $content = Get-Content $_.FullName -Raw
    if ([string]::IsNullOrEmpty($content)) {
        Write-Host "Skipping empty file $($_.FullName)"
        return
    }

    $header = Get-CopyrightHeader $_.FullName
    if (-not $header) {
        Write-Host "Skipping unsupported file type: $($_.FullName)"
        return
    }

    $ext = Get-FileExtension $_.FullName
    $style = $extensionStyles[$ext]
    
    # Check if file already has a copyright header
    $hasHeader = $false
    if ($style.multi) {
        $hasHeader = $content.TrimStart().StartsWith($style.prefix)
    }
    else {
        $hasHeader = $content.TrimStart().StartsWith("$($style.prefix) $copyrightText")
    }

    if ($hasHeader) {
        Write-Host "Copyright header already exists in $($_.FullName)"
        
        # Remove existing header and add new one to ensure consistency
        $lines = $content -split "`n"
        $nonHeaderStart = $lines | Where-Object { 
            $line = $_.TrimStart()
            -not [string]::IsNullOrWhiteSpace($line) -and
            -not $line.StartsWith($style.prefix) -and
            (-not $style.suffix -or -not $line.EndsWith($style.suffix))
        } | Select-Object -First 1
        
        if ($nonHeaderStart) {
            $startIndex = $content.IndexOf($nonHeaderStart)
            $content = $content.Substring($startIndex)
        }
    }

    $newContent = $header + $content
    Set-Content -Path $_.FullName -Value $newContent -NoNewline
    Write-Host "Updated copyright header in $($_.FullName)"
}