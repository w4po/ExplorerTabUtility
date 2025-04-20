$ErrorActionPreference = 'Stop'

$packageName = '{{PACKAGE_NAME}}'

# Get the uninstaller path from the registry
$uninstallRegKey = Get-ItemProperty -Path @(
    'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*',
    'HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*',
    'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*'
) -ErrorAction SilentlyContinue | Where-Object { $_.DisplayName -like "$packageName*" }

if ($uninstallRegKey)
{
    $uninstallString = $uninstallRegKey.UninstallString

    # Stop any running instances
    Get-Process -Name $packageName -ErrorAction SilentlyContinue | Stop-Process -Force

    # Extract the path and add silent arguments
    if ($uninstallString)
    {
        $uninstallPath = $uninstallString.Replace('"', '')

        $silentArgs = '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-'
        $validExitCodes = @(0)

        # Run the uninstaller
        $packageArgs = @{
            packageName = $packageName
            fileType = 'EXE'
            silentArgs = $silentArgs
            validExitCodes = $validExitCodes
            file = $uninstallPath
        }

        Uninstall-ChocolateyPackage @packageArgs
    }
}
else
{
    Write-Warning "Could not find $packageName in the registry. The application may have been uninstalled manually."
}
