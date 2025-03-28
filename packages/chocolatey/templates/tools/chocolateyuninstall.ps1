$ErrorActionPreference = 'Stop'

$packageName = '{{PACKAGE_NAME}}'
$toolsDir = "$( Split-Path -parent $MyInvocation.MyCommand.Definition )"
$targetPath = Join-Path $toolsDir "$packageName\$packageName.exe"

# Stop any running instances
$processName = [System.IO.Path]::GetFileNameWithoutExtension($targetPath)
Get-Process -Name $processName -ErrorAction SilentlyContinue | Stop-Process -Force

# Remove Start Menu shortcut
$startMenuPath = [System.Environment]::GetFolderPath('CommonPrograms')
$startMenuShortcut = Join-Path $startMenuPath "$packageName.lnk"
if (Test-Path $startMenuShortcut)
{
    Remove-Item $startMenuShortcut -Force
}
