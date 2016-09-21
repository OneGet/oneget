#
# Script module for module 'PackageManagement'
#
Set-StrictMode -Version Latest

# Set up some helper variables to make it easier to work with the module
$PSModule = $ExecutionContext.SessionState.Module
$PSModuleRoot = $PSModule.ModuleBase

# Try to import the OneGet assemblies at the same directory regardless fullclr or coreclr
$OneGetModulePath = Join-Path -Path $PSModuleRoot -ChildPath 'Microsoft.PackageManagement.dll'
$binaryModuleRoot = $PSModuleRoot

if(-not (Test-Path -Path $OneGetModulePath))
{
    # Import the appropriate nested binary module based on the current PowerShell version
    $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath 'fullclr'

    if (($PSVersionTable.Keys -contains "PSEdition") -and ($PSVersionTable.PSEdition -ne 'Desktop')) {
        $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath 'coreclr'
    }
    
    $OneGetModulePath = Join-Path -Path $binaryModuleRoot -ChildPath 'Microsoft.PackageManagement.dll'
}

$OneGetModule = Import-Module -Name $OneGetModulePath -PassThru

$PSOneGetModulePath = Join-Path -Path $binaryModuleRoot -ChildPath 'Microsoft.PowerShell.PackageManagement.dll'
$PSOneGetModule = Import-Module -Name $PSOneGetModulePath -PassThru

# When the module is unloaded, remove the nested binary module that was loaded with it
if($OneGetModule)
{
    $PSModule.OnRemove = {
        Remove-Module -ModuleInfo $OneGetModule
    }
}

if($PSOneGetModule)
{
    $PSModule.OnRemove = {
        Remove-Module -ModuleInfo $PSOneGetModule
    }
}