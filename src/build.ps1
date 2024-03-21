param(
    [ValidateSet("net472")]
    [string]$Framework = "net472",

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
        Write-Host "Creating directory $destination"
        New-Item -ItemType Directory $destination -Force
    }
    foreach ($file in $itemsToCopy)
    {
        Write-Host $file
        if (Test-Path $file)
        {
            Write-Host "Copying $file to $destination"
            Copy-Item -Path $file -Destination (Join-Path $destination (Split-Path $file -Leaf)) -Verbose -Force
        }
        else {
            Write-Host "File $file does not exist"
        }
    }
}

Function CopyBinariesToDestinationDir($itemsToCopy, $destination, $framework, $configuration, $ext, $solutionDir)
{
    if (-not (Test-Path $destination))
    {
        Write-Host "Creating directory $destination"
        $null = New-Item -ItemType Directory $destination -Force
    }
    foreach ($file in $itemsToCopy)
    {
        Write-Host "Copying $file to $destination"
        $platform = 'Any CPU'
        $fullPath = Join-Path -Path $solutionDir -ChildPath $file | Join-Path -ChildPath 'bin' | Join-Path -ChildPath $configuration | Join-Path -ChildPath $framework | Join-Path -ChildPath "$file$ext"
        $fullPathWithPlatform = Join-Path -Path $solutionDir -ChildPath $file | Join-Path -ChildPath 'bin' | Join-Path -ChildPath $platform | Join-Path -ChildPath $configuration | Join-Path -ChildPath $framework | Join-Path -ChildPath "$file$ext"
		if (Test-Path $fullPath)
        {
            Write-Host "fullpath $fullPath exists"
            Copy-Item -Path $fullPath -Destination (Join-Path $destination "$file$ext") -Verbose -Force
        } elseif (Test-Path $fullPathWithPlatform) {
            Write-Host "fullPathWithPlatform $fullPathWithPlatform exists"
            Copy-Item -Path $fullPathWithPlatform -Destination (Join-Path $destination "$file$ext") -Verbose -Force
        } else {
            Write-Host "File $fullPath and $fullPathWithPlatform does not exist"
            return $false
        }
    }
    return $true
}

if ($Framework -eq "all")
{
    $frameworks = @('net472')
} else {
    $frameworks = @($Framework)
}

foreach ($currentFramework in $frameworks)
{
    $solutionPath = Split-Path $MyInvocation.InvocationName
    $solutionDir = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($solutionPath)

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
    $destinationDirBinaries = "$destinationDir/$packageFramework"
    $destinationDirDscResourcesBase = "$destinationDir/DSCResources"

    try
    {
        foreach ($assemblyName in $assemblyNames)
        {
            Write-Host "Generating resources file for $assemblyName"
            .\New-StronglyTypedCsFileForResx.ps1 -Project $assemblyName
            Push-Location $assemblyName
			if ($EmbedProviderManifest) {
				$env:EMBEDPROVIDERMANIFEST = 'true'
			} else {
				$env:EMBEDPROVIDERMANIFEST = ''
			}
            #Write-Host "Restoring package for $assemblyName"
            #dotnet restore
            #Write-Host "Building $assemblyName for $currentFramework"
            #dotnet build --framework $currentFramework --configuration $Configuration
            Write-Host "Clean package"
            dotnet clean 
            Write-Host "Publishing $assemblyName for $currentFramework"
            dotnet publish --framework $currentFramework --configuration $Configuration
            Write-Host "Completed restoring, building, and publishing"
            Pop-Location
        }
    }
    finally
    {
    }

    Get-ChildItem 
    CopyToDestinationDir $itemsToCopyCommon $destinationDir
    if (-not (CopyBinariesToDestinationDir $assemblyNames $destinationDirBinaries $currentFramework $Configuration '.dll' $solutionDir)) {
        throw 'Build failed'
    }
    CopyBinariesToDestinationDir $assemblyNames $destinationDirBinaries $currentFramework $Configuration '.pdb' $solutionDir
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

    Add-Type -assemblyname System.IO.Compression.FileSystem
    Write-Verbose "Zipping $sourcePath into $packageFileName" -verbose
    [System.IO.Compression.ZipFile]::CreateFromDirectory($sourcePath, $packageFileName) 
}