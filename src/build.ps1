param(
    [ValidateSet("net451", "netstandard1.6")]
    [string]$Framework = "netstandard1.6",

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

Function Test-DotNetRestore
{
    param(
        [string] $projectPath
    )
    Test-Path (Join-Path $projectPath 'project.lock.json')
}

Function CopyToDestinationDir($itemsToCopy, $destination)
{
    if (-not (Test-Path $destination))
    {
        New-Item -ItemType Directory $destination -Force
    }
    foreach ($file in $itemsToCopy)
    {
        if (Test-Path $file)
        {
            Copy-Item -Path $file -Destination (Join-Path $destination (Split-Path $file -Leaf)) -Verbose -Force
        }
    }
}

$solutionPath = Split-Path $MyInvocation.InvocationName
$solutionDir = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($solutionPath)
if (-not (Test-Path "$solutionDir/global.json"))
{
    throw "Not in solution root"
}

if ($Framework -eq "netstandard1.6")
{
    $packageFramework ="coreclr"
    $assemblyNames = @(
        "Microsoft.PackageManagement",
        "Microsoft.PackageManagement.ArchiverProviders",
        "Microsoft.PackageManagement.CoreProviders",
        "Microsoft.PackageManagement.MetaProvider.PowerShell",
        "Microsoft.PowerShell.PackageManagement",
        "Microsoft.PackageManagement.NuGetProvider"
        )
}
else
{
    $packageFramework ="fullclr"
    $assemblyNames = @(
        "Microsoft.PackageManagement",
        "Microsoft.PackageManagement.ArchiverProviders",
        "Microsoft.PackageManagement.CoreProviders",
        "Microsoft.PackageManagement.MetaProvider.PowerShell",
        "Microsoft.PackageManagement.MsiProvider",
        "Microsoft.PackageManagement.MsuProvider",
        "Microsoft.PowerShell.PackageManagement",
        "Microsoft.PackageManagement.NuGetProvider"
        )
}

$itemsToCopyBinaries = $assemblyNames | % { "$solutionDir\$_\bin\$Configuration\$Framework\$_.dll" }
$itemsToCopyPdbs = $assemblyNames | % { "$solutionDir\$_\bin\$Configuration\$Framework\$_.pdb" }

$itemsToCopyCommon = @("$solutionDir\Microsoft.PowerShell.PackageManagement\PackageManagement.psd1",
                       "$solutionDir\Microsoft.PowerShell.PackageManagement\PackageManagement.psm1",
                       "$solutionDir\Microsoft.PowerShell.PackageManagement\PackageProviderFunctions.psm1",
                       "$solutionDir\Microsoft.PowerShell.PackageManagement\PackageManagement.format.ps1xml")
					   
$dscResourceItemsCommon = @("$solutionDir\Microsoft.PackageManagement.DscResources\PackageManagementDscUtilities.psm1",
                      "$solutionDir\Microsoft.PackageManagement.DscResources\PackageManagementDscUtilities.strings.psd1")
			
$dscResourceItemsPackage = @("$solutionDir\Microsoft.PackageManagement.DscResources\MSFT_PackageManagement\MSFT_PackageManagement.psm1",
					  "$solutionDir\Microsoft.PackageManagement.DscResources\MSFT_PackageManagement\MSFT_PackageManagement.schema.mfl",
					  "$solutionDir\Microsoft.PackageManagement.DscResources\MSFT_PackageManagement\MSFT_PackageManagement.schema.mof",
					  "$solutionDir\Microsoft.PackageManagement.DscResources\MSFT_PackageManagement\MSFT_PackageManagement.strings.psd1")
					  
$dscResourceItemsPackageSource = @("$solutionDir\Microsoft.PackageManagement.DscResources\MSFT_PackageManagementSource\MSFT_PackageManagementSource.psm1",
					  "$solutionDir\Microsoft.PackageManagement.DscResources\MSFT_PackageManagementSource\MSFT_PackageManagementSource.schema.mfl",
					  "$solutionDir\Microsoft.PackageManagement.DscResources\MSFT_PackageManagementSource\MSFT_PackageManagementSource.schema.mof",
					  "$solutionDir\Microsoft.PackageManagement.DscResources\MSFT_PackageManagementSource\MSFT_PackageManagementSource.strings.psd1")

$destinationDir = "$solutionDir/out/PackageManagement"
$destinationDirBinaries = "$destinationDir/$packageFramework"
$destinationDirDscResourcesBase = "$destinationDir/DSCResources"

# rename project.json.hidden to project.json
foreach ($assemblyName in $assemblyNames)
{
    if (Test-Path ("$solutionDir\$assemblyName\project.json.hidden"))
    {
        Move-Item "$solutionDir\$assemblyName\project.json.hidden" "$solutionDir\$assemblyName\project.json" -Force
    }
}

try
{
    foreach ($assemblyName in $assemblyNames)
    {
        Write-Host "Generating resources file for $assemblyName"
        .\New-StronglyTypedCsFileForResx.ps1 -Project $assemblyName
        Push-Location $assemblyName
        Write-Host "Restoring package for $assemblyName"
        dotnet restore
        dotnet -v build --framework $Framework --configuration $Configuration
        Pop-Location
    }
}
finally
{
    # rename project.json to project.json.hidden
    foreach ($assemblyName in $assemblyNames)
    {
        if (Test-Path ("$solutionDir\$assemblyName\project.json"))
        {
            Move-Item "$solutionDir\$assemblyName\project.json" "$solutionDir\$assemblyName\project.json.hidden"
        }

        if (Test-Path ("$solutionDir\$assemblyName\project.lock.json"))
        {
            Remove-Item "$solutionDir\$assemblyName\project.lock.json"
        }
    }
}


CopyToDestinationDir $itemsToCopyCommon $destinationDir
CopyToDestinationDir $itemsToCopyBinaries $destinationDirBinaries
CopyToDestinationDir $itemsToCopyPdbs $destinationDirBinaries
CopyToDestinationDir $dscResourceItemsCommon $destinationDirDscResourcesBase
CopyToDestinationDir $dscResourceItemsPackage (Join-Path -Path $destinationDirDscResourcesBase -ChildPath "MSFT_PackageManagement")
CopyToDestinationDir $dscResourceItemsPackageSource (Join-Path -Path $destinationDirDscResourcesBase -ChildPath "MSFT_PackageManagementSource")

#Packing
$sourcePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($destinationDir)
$packagePath= Split-Path -Path $sourcePath
$zipFilePath = Join-Path $packagePath "OneGet.$packageFramework.zip"
$packageFileName = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($zipFilePath)

if(test-path $packageFileName)
{
    Remove-Item $packageFileName -force
}

if ($packageFramework -eq "fullclr")
{
  Add-Type -assemblyname System.IO.Compression.FileSystem
}
Write-Verbose "Zipping $sourcePath into $packageFileName" -verbose
[System.IO.Compression.ZipFile]::CreateFromDirectory($sourcePath, $packageFileName) 
