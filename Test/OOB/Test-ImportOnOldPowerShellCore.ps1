[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [switch]
    $IsWindows,

    [Parameter(Mandatory=$true)]
    [string]
    $PackageManagementVersion,

    [Parameter(Mandatory=$true)]
    [string]
    $PathToPackageManagementModule
)

Import-Module "$PSScriptRoot\..\TestUtility.psm1" -Force

function Test-ImportOnOldPowerShellCore {
    [CmdletBinding()]
    param()
    Install-PackageProvider PSL -Force -verbose
    if ([Environment]::OSVersion.Version.Major -eq 6) {
        Write-Verbose "Assuming OS is Win 8.1 (includes Win Server 2012 R2)"
        $pslLocation = Join-Path -Path $PSScriptRoot -ChildPath .. | Join-Path -ChildPath "PSL\win81\PSL_6.0.0.14.json"
    } else {
        Write-Verbose "Assuming OS is Win 10"
        $pslLocation = Join-Path -Path $PSScriptRoot -ChildPath .. | Join-Path -ChildPath "PSL\win10\PSL_6.0.0.14.json"
    }

    $powershellCore = (Get-Package -provider msi -name PowerShell_6.0.0.14 -ErrorAction SilentlyContinue | Sort-Object -Property Version -Descending | Select-Object -First 1)
    if ($powershellCore) {
        Write-Warning ("PowerShell already installed" -f $powershellCore.Name)
    } else {
        $powershellCore = Install-PowerShellCore -PSLLocation $pslLocation
    }

    if (-not $powerShellCore)
    {
        Write-Error "PowerShell Core wasn't found or installed."
        return $false
    }

    $powershellVersion = $powershellCore.Version
    $powershellFolder = "$Env:ProgramFiles\PowerShell\$powershellVersion"
    $command = "& {Import-Module `$Env:ProgramFiles\$PathToPackageManagementModule}"
    $result = & "$powershellFolder\powershell" -command $command
    if (-not $result)
    {
        # No output is bad output
        Write-Error $result
        return $false
    }

    return $true
}

if ($IsWindows)
{
    Test-ImportOnOldPowerShellCore
} else {
    Write-Verbose -Message 'ImportOnOldPowerShellCore test disabled for non-Windows.'
    $true
}