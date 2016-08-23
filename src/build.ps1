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

$solutionDir = Split-Path $MyInvocation.InvocationName
if (-not (Test-Path "$solutionDir/global.json"))
{
    throw "Not in solution root"
}

$assemblyNames = @(
    "Microsoft.PackageManagement",
    "Microsoft.PackageManagement.ArchiverProviders",
    "Microsoft.PackageManagement.CoreProviders",
    "Microsoft.PackageManagement.MetaProvider.PowerShell",
    "Microsoft.PackageManagement.MsiProvider",
    "Microsoft.PackageManagement.MsuProvider",
    "Microsoft.PowerShell.PackageManagement"
) 

$itemsToCopyBinaries = $assemblyNames | % { "$solutionDir\$_\bin\$Configuration\$Framework\$_.dll" }

$itemsToCopyCommon = @("$solutionDir\Microsoft.PowerShell.PackageManagement\PackageManagement.psd1",
    "$solutionDir\Microsoft.PowerShell.PackageManagement\PackageProviderFunctions.psm1",
    "$solutionDir\Microsoft.PowerShell.PackageManagement\PackageManagement.format.ps1xml")

$destinationDir = "$solutionDir/out/PackageManagement"
$destinationDirBinaries = "$destinationDir/$Framework"

# rename project.json.hidden to project.json
foreach ($assemblyName in $assemblyNames)
{
    if (Test-Path ("$solutionDir\$assemblyName\project.json.hidden"))
    {
        mv "$solutionDir\$assemblyName\project.json.hidden" "$solutionDir\$assemblyName\project.json"
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
        dotnet build --framework $Framework --configuration $Configuration
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
            mv "$solutionDir\$assemblyName\project.json" "$solutionDir\$assemblyName\project.json.hidden"
        }

        if (Test-Path ("$solutionDir\$assemblyName\project.lock.json"))
        {
            remove-item "$solutionDir\$assemblyName\project.lock.json"
        }
    }
}

CopyToDestinationDir $itemsToCopyCommon $destinationDir
CopyToDestinationDir $itemsToCopyBinaries $destinationDirBinaries