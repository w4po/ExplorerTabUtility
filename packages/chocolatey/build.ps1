# Chocolatey Package Builder
#
# This script downloads the installer from GitHub releases and creates a Chocolatey package.
# It calculates the SHA256 hash for the installer and generates the required files.
#
# Requirements:
#   - Installer filename should follow the pattern: {name}_v{version}_Setup.exe
#
# Usage:
#   .\build.ps1 -Publisher "owner" -Name "repo" -Version "1.0.0"
#
# Required Parameters:
#   -Publisher    : The repository owner/publisher name
#   -Name         : The repository/package name
#   -Version      : The version to publish (without 'v' prefix)
#
# Optional Parameters:
#   -ApiKey       : Chocolatey API key for publishing
#   -Description  : Package description for the nuspec
#   -Summary      : Short summary for the nuspec

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
    [parameter(Mandatory = $false)]
    [string]
    $ApiKey,
    [parameter(Mandatory = $false)]
    [string]
    $Description,
    [parameter(Mandatory = $false)]
    [string]
    $Summary
)

function Get-ArtifactHash
{
    # Create temp directory
    $tempDir = Join-Path $env:TEMP "choco_artifacts_$([Guid]::NewGuid().ToString() )"
    New-Item -ItemType Directory -Path $tempDir | Out-Null

    try
    {
        $fileName = "$( $Name )_v$( $Version )_Setup.exe"
        $downloadUrl = "https://github.com/$Publisher/$Name/releases/download/v$Version/$fileName"
        $outputPath = Join-Path $tempDir $fileName

        # Download the file
        Write-Host "Downloading installer from: $downloadUrl"
        Invoke-WebRequest -Uri $downloadUrl -OutFile $outputPath -UseBasicParsing

        # Calculate SHA256
        $hash = (Get-FileHash -Algorithm SHA256 $outputPath).Hash
        return $hash.ToLower()
    }
    finally
    {
        # Cleanup temp directory
        if (Test-Path $tempDir)
        {
            Remove-Item -Path $tempDir -Recurse -Force
        }
    }
}

function Write-TemplateFile
{
    param (
        [parameter(Mandatory = $true)]
        [string]
        $SourceFile,
        [parameter(Mandatory = $true)]
        [string]
        $DestinationFile
    )
    $content = Get-Content $SourceFile -Raw

    # Basic replacements
    $content = $content.Replace('{{VERSION}}', $Version)
    $content = $content.Replace('{{PUBLISHER}}', $Publisher)
    $content = $content.Replace('{{PACKAGE_ID}}',$Name.ToLower())
    $content = $content.Replace('{{PACKAGE_NAME}}', $Name)
    $content = $content.Replace('{{DESCRIPTION}}', $Description)
    $content = $content.Replace('{{SUMMARY}}', $Summary)
    $content = $content.Replace('{{CHECKSUM}}', $Checksum)

    $content | Out-File -Encoding 'UTF8' $DestinationFile
}

# Clean version string
$Version = $Version.TrimStart('v')

#Description
if (-not $Description)
{
    $Description = $Name
}

# Summary
if (-not $Summary)
{
    $Summary = $Name
}

# Create the package directory
$packageDir = Join-Path $PWD "package"
if (Test-Path $packageDir)
{
    Remove-Item -Path $packageDir -Recurse -Force
}
New-Item -Path $PWD -Name "package" -ItemType "directory" | Out-Null
New-Item -Path $packageDir -Name "tools" -ItemType "directory" | Out-Null

# Download artifacts and calculate hashes
$Checksum = Get-ArtifactHash

# Process template files
Write-TemplateFile -SourceFile "templates\PACKAGE_NAME.nuspec" -DestinationFile "package\$Name.nuspec"
Write-TemplateFile -SourceFile "templates\tools\chocolateyinstall.ps1" -DestinationFile "package\tools\chocolateyinstall.ps1"
Write-TemplateFile -SourceFile "templates\tools\chocolateyuninstall.ps1" -DestinationFile "package\tools\chocolateyuninstall.ps1"

# ANSI color codes
$green = "`e[32m"
$cyan = "`e[36m"
$reset = "`e[0m"

Write-Output "`n${green}=== Generated Chocolatey package files ===${reset}`n"
Get-ChildItem ".\package\*.nuspec", ".\package\tools\*.ps1" | ForEach-Object {
    Write-Output "${cyan}=== Contents of $( $_.Name ) ===${reset}`n"
    Get-Content $_.FullName
    Write-Output "`n${green}===============================${reset}`n"
}

# Create the package
Write-Output "`n${green}=== Building Chocolatey package ===${reset}`n"
choco pack "package\$Name.nuspec" --out "."

# Push the package if API key is provided
if ($ApiKey)
{
    Write-Output "`n${green}=== Pushing package to Chocolatey ===${reset}`n"
    choco push "$Name.$Version.nupkg" --source=https://push.chocolatey.org/ --api-key=$ApiKey
}
