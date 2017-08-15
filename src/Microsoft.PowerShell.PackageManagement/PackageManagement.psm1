#
# Script module for module 'PackageManagement'
#
Set-StrictMode -Version Latest
Microsoft.PowerShell.Utility\Import-LocalizedData  LocalizedData -filename PackageManagement.Resources.psd1

# Summary: PackageManagement is supported on Windows PowerShell 3.0 or later, Nano Server and PowerShellCore
$isCore = ($PSVersionTable.Keys -contains "PSEdition") -and ($PSVersionTable.PSEdition -ne 'Desktop')
if ($isCore)
{
    # PackageManagement only works on versions of PowerShell Core built on .NETCoreApp 2.0 and above
    # The following API was added in PackageManagement as a dependency
    $loadFromMethod = [System.Reflection.Assembly].GetMethods() | Where-Object { $_.Name -eq 'LoadFrom' }
    if (-not $loadFromMethod)
    {
        if ($PSVersionTable.ContainsKey('GitCommitId')) {
            $psVersionString = $PSVersionTable.GitCommitId
        } else {
            $psVersionString = $PSVersionTable.PSVersion.ToString()
        }
        throw ($LocalizedData.OldPowerShellCoreVersion -f $psVersionString)
    }
}

# Set up some helper variables to make it easier to work with the module
$script:PSModule = $ExecutionContext.SessionState.Module
$script:PSModuleRoot = $script:PSModule.ModuleBase

$script:PkgMgmt = 'Microsoft.PackageManagement.dll'
$script:PSPkgMgmt = 'Microsoft.PowerShell.PackageManagement.dll'


# Try to import the OneGet assemblies at the same directory regardless fullclr or coreclr
$OneGetModulePath = Join-Path -Path $script:PSModuleRoot -ChildPath $script:PkgMgmt
$binaryModuleRoot = $script:PSModuleRoot


if(-not (Test-Path -Path $OneGetModulePath))
{
    # Import the appropriate nested binary module based on the current PowerShell version
    $binaryModuleRoot = Join-Path -Path $script:PSModuleRoot -ChildPath 'fullclr'

    if ($isCore) {
        $binaryModuleRoot = Join-Path -Path $script:PSModuleRoot -ChildPath 'coreclr'
    }
    
    $OneGetModulePath = Join-Path -Path $binaryModuleRoot -ChildPath $script:PkgMgmt
}

$PSOneGetModulePath = Join-Path -Path $binaryModuleRoot -ChildPath $script:PSPkgMgmt
$OneGetModule = Import-Module -Name $OneGetModulePath -PassThru
$PSOneGetModule = Import-Module -Name $PSOneGetModulePath -PassThru


# When the module is unloaded, remove the nested binary module that was loaded with it
if($OneGetModule)
{
    $script:PSModule.OnRemove = {
        Remove-Module -ModuleInfo $OneGetModule
    }
}

if($PSOneGetModule)
{
    $script:PSModule.OnRemove = {
        Remove-Module -ModuleInfo $PSOneGetModule
    }
}