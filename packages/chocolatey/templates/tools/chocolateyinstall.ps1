$ErrorActionPreference = 'Stop'

$packageName = '{{PACKAGE_NAME}}'
$toolsDir = "$( Split-Path -parent $MyInvocation.MyCommand.Definition )"
$url = 'https://github.com/{{PUBLISHER}}/{{PACKAGE_NAME}}/releases/download/v{{VERSION}}/{{PACKAGE_NAME}}_v{{VERSION}}_x86{{FILE_SUFFIX}}.zip'
$url64 = 'https://github.com/{{PUBLISHER}}/{{PACKAGE_NAME}}/releases/download/v{{VERSION}}/{{PACKAGE_NAME}}_v{{VERSION}}_x64{{FILE_SUFFIX}}.zip'
$urlArm64 = 'https://github.com/{{PUBLISHER}}/{{PACKAGE_NAME}}/releases/download/v{{VERSION}}/{{PACKAGE_NAME}}_v{{VERSION}}_arm64{{FILE_SUFFIX}}.zip'
$checksum = '{{CHECKSUM_X86}}'
$checksum64 = '{{CHECKSUM_X64}}'
$checksumArm64 = '{{CHECKSUM_ARM64}}'
$checksumType = 'sha256'

$packageArgs = @{
    packageName = $packageName
    unzipLocation = $toolsDir
    checksumType = $checksumType
}

# Detect architecture and set appropriate URL and checksum
if ($env:chocolateyForceX86 -eq 'true')
{
    Write-Host "Force using 32-bit version due to chocolateyForceX86 flag"
    $packageArgs['url'] = $url
    $packageArgs['checksum'] = $checksum
}
elseif ($env:PROCESSOR_ARCHITECTURE -eq 'ARM64' -or $env:PROCESSOR_ARCHITEW6432 -eq 'ARM64')
{
    Write-Host "Detected ARM64 architecture"
    $packageArgs['url'] = $urlArm64
    $packageArgs['checksum'] = $checksumArm64
}
elseif ([Environment]::Is64BitOperatingSystem)
{
    Write-Host "Detected 64-bit architecture"
    $packageArgs['url'] = $url64
    $packageArgs['checksum'] = $checksum64
}
else
{
    Write-Host "Detected 32-bit architecture"
    $packageArgs['url'] = $url
    $packageArgs['checksum'] = $checksum
}

# Check Windows version - ExplorerTabUtility requires Windows 11 (Build 22621 or later)
$osVersion = [System.Environment]::OSVersion.Version
$buildNumber = $osVersion.Build
if ($osVersion.Major -lt 10 -or ($osVersion.Major -eq 10 -and $buildNumber -lt 22621)) {
    Write-Warning "ExplorerTabUtility requires Windows 11 (Build 22621 or later). Current OS: Windows $($osVersion.Major).$($osVersion.Minor) Build $buildNumber"
    Write-Warning "Installation will continue, but the application may not function correctly."
}

Install-ChocolateyZipPackage @packageArgs

# Create Start Menu shortcut
$startMenuPath = [System.Environment]::GetFolderPath('CommonPrograms')
$startMenuShortcut = Join-Path $startMenuPath "$packageName.lnk"
$targetPath = Join-Path $toolsDir "$packageName\$packageName.exe"
Install-ChocolateyShortcut -ShortcutFilePath $startMenuShortcut -TargetPath $targetPath

# Start-Process -FilePath $targetPath

# Display installation success message
Write-Host ""
Write-Host "ExplorerTabUtility has been installed successfully!" -ForegroundColor Green
Write-Host "You can start it from the Start Menu." -ForegroundColor Green
