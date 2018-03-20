param(
    [ValidateSet("net451", "netcoreapp2.0", "netstandard1.6", "all")]
    [string]$Framework = "netcoreapp2.0",

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
	
	[switch]$EmbedProviderManifest
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

Function CopyBinariesToDestinationDir($itemsToCopy, $destination, $framework, $configuration, $ext, $solutionDir)
{
    if (-not (Test-Path $destination))
    {
        $null = New-Item -ItemType Directory $destination -Force
    }
    foreach ($file in $itemsToCopy)
    {
        # Set by AppVeyor
        $platform = [System.Environment]::GetEnvironmentVariable('platform')
        if (-not $platform) {
            # If not set at all, try Any CPU
            $platform = 'Any CPU'
        }

        $fullPath = Join-Path -Path $solutionDir -ChildPath $file | Join-Path -ChildPath 'bin' | Join-Path -ChildPath $configuration | Join-Path -ChildPath $framework | Join-Path -ChildPath "$file$ext"
        $fullPathWithPlatform = Join-Path -Path $solutionDir -ChildPath $file | Join-Path -ChildPath 'bin' | Join-Path -ChildPath $platform | Join-Path -ChildPath $configuration | Join-Path -ChildPath $framework | Join-Path -ChildPath "$file$ext"
		if (Test-Path $fullPath)
        {
            Copy-Item -Path $fullPath -Destination (Join-Path $destination "$file$ext") -Verbose -Force
        } elseif (Test-Path $fullPathWithPlatform) {
            Copy-Item -Path $fullPathWithPlatform -Destination (Join-Path $destination "$file$ext") -Verbose -Force
        } else {
            return $false
        }
    }
    return $true
}

if ($Framework -eq "all")
{
    $frameworks = @('net451','netcoreapp2.0','netstandard1.6')
} else {
    $frameworks = @($Framework)
}

$providersFromPackages = @(
    @{
        'Name' = 'Microsoft.PowerShell.PackageManagement.NuGetProvider'
        'ProviderName' = 'NuGet'
        #'Location' = 'https://powershell.myget.org/F/powershellmodule/api/v2'
        'Location' = 'E:\Packages_bk\PackagesTesting'
        'Paths' = @('lib\{framework}\*.dll')
    }
)
$unregisterPackageSources = @()
foreach ($currentFramework in $frameworks)
{
    $solutionPath = Split-Path $MyInvocation.InvocationName
    $solutionDir = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($solutionPath)

    if (($currentFramework -eq "netcoreapp2.0") -or ($currentFramework -eq "netstandard1.6"))
    {
        $packageFramework ="coreclr"
        $assemblyNames = @(
            "Microsoft.PackageManagement",
            "Microsoft.PackageManagement.ArchiverProviders",
            "Microsoft.PackageManagement.CoreProviders",
            "Microsoft.PackageManagement.MetaProvider.PowerShell",
            "Microsoft.PowerShell.PackageManagement"
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
            "Microsoft.PowerShell.PackageManagement"
            )
    }

    $itemsToCopyBinaries = $assemblyNames | % { "$solutionDir\$_\bin\$Configuration\$currentFramework\$_.dll" }
    $itemsToCopyPdbs = $assemblyNames | % { "$solutionDir\$_\bin\$Configuration\$currentFramework\$_.pdb" }

    $itemsToCopyCommon = @("$solutionDir\Microsoft.PowerShell.PackageManagement\PackageManagement.psd1",
                        "$solutionDir\Microsoft.PowerShell.PackageManagement\PackageManagement.psm1",
                        "$solutionDir\Microsoft.PowerShell.PackageManagement\PackageManagement.Resources.psd1",
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
    if (($currentFramework -eq "netcoreapp2.0") -or ($currentFramework -eq "netstandard1.6"))
    {
        $destinationDirBinaries = "$destinationDir/$packageFramework/$currentFramework"
    } else 
    {
        $destinationDirBinaries = "$destinationDir/$packageFramework"
    }

    $destinationDirDscResourcesBase = "$destinationDir/DSCResources"

    try
    {
        foreach ($assemblyName in $assemblyNames)
        {
            Write-Host "Generating resources file for $assemblyName"
            .\New-StronglyTypedCsFileForResx.ps1 -Project $assemblyName
            Push-Location $assemblyName
            Write-Host "Restoring package for $assemblyName"
			if ($EmbedProviderManifest) {
				$env:EMBEDPROVIDERMANIFEST = 'true'
			} else {
				$env:EMBEDPROVIDERMANIFEST = ''
			}
            dotnet restore
            dotnet build --framework $currentFramework --configuration $Configuration
            dotnet publish --framework $currentFramework --configuration $Configuration
            Pop-Location
        }
    }
    finally
    {
    }


    CopyToDestinationDir $itemsToCopyCommon $destinationDir
    if (-not (CopyBinariesToDestinationDir $assemblyNames $destinationDirBinaries $currentFramework $Configuration '.dll' $solutionDir)) {
        throw 'Build failed'
    }
    CopyBinariesToDestinationDir $assemblyNames $destinationDirBinaries $currentFramework $Configuration '.pdb' $solutionDir
    CopyToDestinationDir $dscResourceItemsCommon $destinationDirDscResourcesBase
    CopyToDestinationDir $dscResourceItemsPackage (Join-Path -Path $destinationDirDscResourcesBase -ChildPath "MSFT_PackageManagement")
    CopyToDestinationDir $dscResourceItemsPackageSource (Join-Path -Path $destinationDirDscResourcesBase -ChildPath "MSFT_PackageManagementSource")

    # Download and copy providers from packages
    foreach ($providerPackageInfo in $providersFromPackages) {
        $packageSource = Get-PackageSource -Location $providerPackageInfo.Location -ProviderName $providerPackageInfo.ProviderName
        if (-not $packageSource) {
            $packageSource = Register-PackageSource -Name ([Guid]::NewGuid().ToString()) -Location $providerPackageInfo.Location -ProviderName $providerPackageInfo.ProviderName -Trusted
            $unregisterPackageSources += $packageSource
        }

        $package = Find-Package -Name $providerPackageInfo.Name -Source $packageSource.Name -ErrorAction Ignore
        if ($package) {
            $tempDir = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath ([System.IO.Path]::GetRandomFileName())
            $null = New-Item -Path $tempDir -ItemType Directory
            try {
                $null = $package | Save-Package -Path $tempDir -AllowPrereleaseVersions
                if ($providerPackageInfo.ProviderName -eq 'NuGet') {
                    $nupkgPath = Get-ChildItem -Path (Join-Path -Path $tempDir -ChildPath "$($providerPackageInfo.Name)*.nupkg") | Select-Object -First 1
                    if ($nupkgPath) {
                        $zipPath = Join-Path -Path $tempDir -ChildPath "temp.zip"
                        Copy-Item -Path $nupkgPath.FullName -Destination $zipPath
                        Expand-Archive -Path $zipPath -DestinationPath $tempDir
                        foreach ($relativePath in $providerPackageInfo.Paths) {
                            $frameworkToReplace = $currentFramework
                            $relativePath = $relativePath.Replace("{framework}", $frameworkToReplace)
                            foreach ($item in (Get-ChildItem -Path (Join-Path -Path $tempDir -ChildPath $relativePath))) {
                                Copy-Item -Path $item.FullName -Destination $destinationDirBinaries
                            }
                        }
                    }
                }
            } finally {
                if (Test-Path -Path $tempDir) {
                    $null = Remove-Item -Path $tempDir -Recurse -Force
                }
            }
        }
    }

    #Packing
    $sourcePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($destinationDir)
    $packagePath= Split-Path -Path $sourcePath
    $zipFilePath = Join-Path $packagePath "OneGet.$packageFramework.zip"
    $packageFileName = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($zipFilePath)

    if(test-path $packageFileName)
    {
        Remove-Item $packageFileName -force
    }

    Add-Type -assemblyname System.IO.Compression.FileSystem
    Write-Verbose "Zipping $sourcePath into $packageFileName" -verbose
    [System.IO.Compression.ZipFile]::CreateFromDirectory($sourcePath, $packageFileName) 
}

foreach ($unregisterPackageSource in $unregisterPackageSources) {
    $unregisterPackageSource | Unregister-PackageSource
}