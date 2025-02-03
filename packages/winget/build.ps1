# WinGet Manifest Builder
#
# This script downloads release artifacts and creates WinGet manifest files.
# It calculates SHA256 hashes for each architecture and generates the required YAML files.
#
# Requirements:
#   - Release artifacts must be ZIP files containing portable executables
#   - ZIP structure must be: 
#     MyApp.zip
#     └── MyApp/
#         └── MyApp.exe
#   - If your ZIP has a different structure, modify the NestedInstallerFiles section
#     in the PUBLISHER.PACKAGE_NAME.installer.yaml template
#
# Usage:
#   .\build.ps1 -Publisher "owner" -Name "repo" -Version "1.0.0" -Architectures @('x64','arm64')
#
# Required Parameters:
#   -Publisher     : The repository owner/publisher name
#   -Name         : The repository/package name
#   -Version      : The version to publish (without 'v' prefix)
#   -Architectures : Array of architectures to include (e.g., @('x64','arm64'))
#
# Optional Parameters:
#   -Token        : GitHub token for creating PR
#   -FileSuffix   : Added to artifact filenames (e.g., "_Net9.0_FrameworkDependent")
#   -Alias        : Command alias for the portable executable
#   -Description  : Package description for the manifest
#
# Example:
#   .\build.ps1 -Publisher "JohnDoe" -Name "CoolApp" -Version "1.0.0" `
#               -Architectures @('x64','arm64') -FileSuffix "_Net9.0" `
#               -Alias "coolapp" -Description "A cool application"

Param
(
    [parameter(Mandatory = $true)]
    [string]
    $Publisher,
    [parameter(Mandatory = $true)]
    [string]
    $Name,
    [parameter(Mandatory = $true)]
    [string]
    $Version,
    [parameter(Mandatory = $true)]
    [string[]]
    $Architectures,
    [parameter(Mandatory = $false)]
    [string]
    $Token,
    [parameter(Mandatory = $false)]
    [string]
    $FileSuffix,
    [parameter(Mandatory = $false)]
    [string]
    $Alias,
    [parameter(Mandatory = $false)]
    [string]
    $Description
)

function Get-ArtifactHashes {
    # Create temp directory
    $tempDir = Join-Path $env:TEMP "winget_artifacts_$([Guid]::NewGuid().ToString())"
    New-Item -ItemType Directory -Path $tempDir | Out-Null

    try {
        $hashes = @{}

        foreach ($arch in $Architectures) {
            $fileName = "$($Name)_v$($Version)_$($arch)$($FileSuffix).zip"
            $downloadUrl = "https://github.com/$Publisher/$Name/releases/download/v$Version/$fileName"
            $outputPath = Join-Path $tempDir $fileName

            # Download the file
            Write-Host "Downloading $arch artifact from: $downloadUrl"
            Invoke-WebRequest -Uri $downloadUrl -OutFile $outputPath -UseBasicParsing

            # Calculate SHA256
            $hash = (Get-FileHash -Algorithm SHA256 $outputPath).Hash
            $hashes[$arch] = $hash.ToLower()
        }

        return $hashes
    }
    finally {
        # Cleanup temp directory
        if (Test-Path $tempDir) {
            Remove-Item -Path $tempDir -Recurse -Force
        }
    }
}

function Write-MetaData {
    param (
        [parameter(Mandatory = $true)]
        [string]
        $FileName
    )
    $content = Get-Content $FileName -Raw

    # Basic replacements
    $content = $content.Replace('<VERSION>', $Version)
    $content = $content.Replace('<PUBLISHER>', $Publisher)
    $content = $content.Replace('<PACKAGE_NAME>', $Name)
    $content = $content.Replace('<FILE_SUFFIX>', $FileSuffix)
    $content = $content.Replace('<ALIAS>', $Alias)
    $content = $content.Replace('<DESCRIPTION>', $Description)
    $date = Get-Date -Format "yyyy-MM-dd"
    $content = $content.Replace('<DATE>', $date)

    # Handle installers and their hashes
    if ($FileName.EndsWith('installer.yaml')) {
        foreach ($arch in $Hashes.Keys) {
            $content += @"

- Architecture: $arch
  InstallerUrl: https://github.com/$Publisher/$Name/releases/download/v$Version/$($Name)_v$($Version)_$($arch)$($FileSuffix).zip
  InstallerSha256: $($Hashes[$arch])
"@
        }
    }

    $FileName = $FileName.Replace("PUBLISHER.PACKAGE_NAME", "$Publisher.$Name")
    $content | Out-File -Encoding 'UTF8' "./$Version/$FileName"
}

# clean version string
$Version = $Version.TrimStart('v')

#Alias
if (-not $Alias) {
    $Alias = $Name
}

#Description
if (-not $Description) {
    $Description = $Name
}

#FileSuffix
if (-not $FileSuffix) {
    $FileSuffix = ""
}

# Create the version folder
New-Item -Path $PWD -Name $Version -ItemType "directory"

# Download artifacts and calculate hashes
$Hashes = Get-ArtifactHashes

Get-ChildItem '*.yaml' | ForEach-Object -Process {
    Write-MetaData -FileName $_.Name
}

# ANSI color codes
$green = "`e[32m"
$cyan = "`e[36m"
$reset = "`e[0m"

Write-Output "`n${green}=== Generated manifest files ===${reset}`n"
Get-ChildItem ".\$Version\*.yaml" | ForEach-Object {
    Write-Output "${cyan}=== Contents of $($_.Name) ===${reset}`n"
    Get-Content $_.FullName
    Write-Output "`n${green}===============================${reset}`n"
}

if (-not $Token) {
    return
}

# Validate only for testing
#winget validate --manifest $Version
#return

# Create the PR
$prTitle = "[Automated] Add $Publisher.$Name version $Version"
wingetcreate submit --token $Token -p "$prTitle" $Version
