$ErrorActionPreference = 'Stop'

$packageName = '{{PACKAGE_NAME}}'
$toolsDir = "$( Split-Path -parent $MyInvocation.MyCommand.Definition )"
$url = 'https://github.com/{{PUBLISHER}}/{{PACKAGE_NAME}}/releases/download/v{{VERSION}}/{{PACKAGE_NAME}}_v{{VERSION}}_Setup.exe'
$checksum = '{{CHECKSUM}}'
$checksumType = 'sha256'

# Get package parameters
$pp = Get-PackageParameters

# Check for interactive mode - handle different parameter formats and case insensitivity
$interactive = $false
if ( $pp.ContainsKey('Interactive'))
{
    # If parameter exists with any value or no value
    $interactive = $true

    # If explicitly set to false, honor that
    if ($pp.Interactive -is [string] -and $pp.Interactive.ToLower() -eq 'false')
    {
        $interactive = $false
    }
}

# Check Windows version - ExplorerTabUtility requires Windows 11 (Build 22621 or later)
$osVersion = [System.Environment]::OSVersion.Version
$buildNumber = $osVersion.Build
if ($osVersion.Major -lt 10 -or ($osVersion.Major -eq 10 -and $buildNumber -lt 22621))
{
    Write-Warning "ExplorerTabUtility requires Windows 11 (Build 22621 or later). Current OS: Windows $( $osVersion.Major ).$( $osVersion.Minor ) Build $buildNumber"
    Write-Warning "Installation will continue, but the application may not function correctly."
}

# Set installer arguments based on interactive mode
if ($interactive)
{
    Write-Host "Running installer in interactive mode" -ForegroundColor Cyan
    $silentArgs = ''
}
else
{
    Write-Host "Running installer in silent mode" -ForegroundColor Cyan
    $silentArgs = '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-'
}

$packageArgs = @{
    packageName = $packageName
    fileType = 'EXE'
    url = $url
    silentArgs = $silentArgs
    validExitCodes = @(0)
    checksum = $checksum
    checksumType = $checksumType
}

Install-ChocolateyPackage @packageArgs

# Display installation success message
Write-Host ""
Write-Host "ExplorerTabUtility has been installed successfully!" -ForegroundColor Green
Write-Host "You can start it from the Start Menu." -ForegroundColor Green
