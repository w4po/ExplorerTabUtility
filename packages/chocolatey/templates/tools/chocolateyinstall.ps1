$ErrorActionPreference = 'Stop'

$packageName = '{{PACKAGE_NAME}}'
$toolsDir = "$( Split-Path -parent $MyInvocation.MyCommand.Definition )"
$url = 'https://github.com/{{PUBLISHER}}/{{PACKAGE_NAME}}/releases/download/v{{VERSION}}/{{PACKAGE_NAME}}_v{{VERSION}}_Setup.exe'
$checksum = '{{CHECKSUM}}'
$checksumType = 'sha256'

# Check Windows version - ExplorerTabUtility requires Windows 11 (Build 22621 or later)
$osVersion = [System.Environment]::OSVersion.Version
$buildNumber = $osVersion.Build
if ($osVersion.Major -lt 10 -or ($osVersion.Major -eq 10 -and $buildNumber -lt 22621))
{
    Write-Warning "ExplorerTabUtility requires Windows 11 (Build 22621 or later). Current OS: Windows $( $osVersion.Major ).$( $osVersion.Minor ) Build $buildNumber"
    Write-Warning "Installation will continue, but the application may not function correctly."
}

$packageArgs = @{
    packageName = $packageName
    fileType = 'EXE'
    url = $url
    silentArgs = '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-'
    validExitCodes = @(0)
    checksum = $checksum
    checksumType = $checksumType
}

Install-ChocolateyPackage @packageArgs

# Display installation success message
Write-Host ""
Write-Host "ExplorerTabUtility has been installed successfully!" -ForegroundColor Green
Write-Host "You can start it from the Start Menu." -ForegroundColor Green
