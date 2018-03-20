<#
Create a NuGet package from a module location with up-to-update folder structure. The version of the package will
be the version of the PackageManagement.psd1.

DLLs ignored when packing:
- Microsoft.PackageManagement.NuGetProvider.dll
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]
    $OneGetModulePath,

    [Parameter(Mandatory = $false)]
    [string]
    $PackageId = "Microsoft.PowerShell.PackageManagement.Core",

    [Parameter(Mandatory = $false)]
    [string]
    $Authors = "Microsoft",

    [Parameter(Mandatory = $false)]
    [string]
    $Owners = "brywang",

    [Parameter(Mandatory = $false)]
    [string[]]
    $ReleaseNotes,

    [Parameter(Mandatory = $false)]
    [string]
    $ReleaseNotesFile,

    [Parameter(Mandatory = $false)]
    [switch]
    $IncludePdbs,

    [Parameter(Mandatory = $false)]
    [switch]
    $RefreshNuGetClient,

    [Parameter(Mandatory = $false)]
    [switch]
    $UseAssemblyVersionForPackageVersion,

    [Parameter(Mandatory = $false)]
    [string]
    $PackageOutDir,

    [Parameter(Mandatory = $false)]
    [string]
    $PrereleaseString
)

if (-not $PackageOutDir) {
    $PackageOutDir = $PSScriptRoot
}

$nugetCommand = Get-Command nuget -ErrorAction Ignore
if ((-not $nugetCommand) -or $RefreshNuGetClient) {
    Write-Verbose "Downloading latest NuGet.exe"
    $null = Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile (Join-Path -Path $PSScriptRoot -ChildPath nuget.exe)
    $nugetCommand = Get-Command nuget -ErrorAction Ignore
}

$ignoreFiles = @{
    'Microsoft.PackageManagement.NuGetProvider.dll' = $True 
}

Write-Verbose "Using NuGet version: $($nugetCommand.Version)"
$nugetPath = $nugetCommand.Source
$tempDir = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath ([System.IO.Path]::GetRandomFileName())
$null = New-Item -Path $tempDir -ItemType Directory
try {
    $frameworkPaths = @{
        'coreclr\netcoreapp2.0' = 'netcoreapp2.0'
        'coreclr\netstandard1.6' = 'netstandard1.6'
        'fullclr' = 'net45'
    }

    $version = $null
    foreach ($kvp in $frameworkPaths.GetEnumerator()) {
        $libDir = Join-Path -Path $tempDir -ChildPath 'lib' | Join-Path -ChildPath $kvp.Value
        if (-not (Test-Path -Path $libDir)) {
            $null = New-Item -Path $libDir -ItemType Directory
        }

        $dllRootDir = Join-Path -Path $OneGetModulePath -ChildPath $kvp.Name
        foreach ($item in (Get-ChildItem -Path (Join-Path -Path $dllRootDir -ChildPath "*.dll") -File)) {
            $fileName = Split-Path -Path $item.FullName -Leaf
            if ($ignoreFiles.ContainsKey($fileName)) {
                continue
            }

            Write-Verbose "Preparing file: $($item.FullName)"
            Copy-Item -Path $item.FullName -Destination $libDir
            if ($UseAssemblyVersionForPackageVersion -and (-not $version) -and ($fileName -eq 'Microsoft.PackageManagement.dll')) {
                $version = [System.Reflection.AssemblyName]::GetAssemblyName($item.FullName).Version.ToString()
            }
        }

        if ($IncludePdbs) {
            foreach ($item in (Get-ChildItem -Path (Join-Path -Path $dllRootDir -ChildPath "*.pdb") -File)) {
                Write-Verbose "Preparing file: $($item.FullName)"
                Copy-Item -Path $item.FullName -Destination $libDir
            }
        }
    }

    if (-not $UseAssemblyVersionForPackageVersion) {
        $psd1 = Get-ChildItem -Path (Join-Path -Path $OneGetModulePath -ChildPath "PackageManagement.psd1")
        if ($psd1) {
            $version = (Test-ModuleManifest -Path $psd1).Version.ToString()
        } else {
            Write-Warning "Couldn't find psd1 file in root of '$OneGetModulePath'. Is this the OneGet module directory you want to pack?"
        }
    }

    if (-not $version) {
        throw "Couldn't discover version of package - is Microsoft.PackageManagement.dll there?"
    }

    if ($PrereleaseString) {
        $version += "-$PrereleaseString"
    }

    $nuspecContents = Get-Content -Path (Join-Path -Path $PSScriptRoot -ChildPath 'OneGet.nuspec') | Out-String
    Write-Verbose "Setting Package.Id = $PackageId"
    $nuspecContents = $nuspecContents -replace "{name}",$PackageId
    Write-Verbose "Setting Package.Version = $version"
    $nuspecContents = $nuspecContents -replace "{version}",$version
    Write-Verbose "Setting Package.Authors = $Authors"
    $nuspecContents = $nuspecContents -replace "{authors}",$Authors
    Write-Verbose "Setting Package.Owners = $Owners"
    $nuspecContents = $nuspecContents -replace "{owners}",$Owners
    if ($ReleaseNotesFile) {
        if (-not (Test-Path -Path $ReleaseNotesFile)) {
            throw "Release notes file '$ReleaseNotesFile' does not exist"
        }

        $ReleaseNotes = Get-Content -Path $ReleaseNotesFile | Out-String
    } else {
        $ReleaseNotes = $ReleaseNotes | Out-String
    }

    Write-Verbose "Setting Package.ReleaseNotes = '$ReleaseNotes'"
    $nuspecContents = $nuspecContents -replace "{releaseNotes}",$ReleaseNotes
    $nuspecName = "temp.nuspec"
    $nuspecContents | Out-File -FilePath (Join-Path -Path $tempDir -ChildPath $nuspecName)
    Write-Verbose "Packing..."
    Push-Location -Path $tempDir
    & $nugetPath pack $nuspecName
    Pop-Location
    if ($LastExitCode -gt 0) {
        throw 'NuGet.exe pack failed. See previous error messages.'
    }

    Write-Verbose "Copying package to $PackageOutDir"
    Get-ChildItem -Path (Join-Path -Path $tempDir -ChildPath "*.nupkg") | Select-Object -First 1 | Select-Object -ExpandProperty FullName | Copy-Item -Destination $PackageOutDir
} finally {
    $tries = 3
    while ((Test-Path -Path $tempDir) -and ($tries -gt 0)) {
        try {
            $null = Remove-Item -Path $tempDir -Recurse -Force -ErrorAction Ignore
        } catch {
        }

        if ((Test-Path -Path $tempDir) -and ($tries -gt 0)) {
            Start-Sleep -Milliseconds (100 * $tries)
            $tries = $tries - 1
        }
    }

    if (Test-Path -path $tempDir) {
        Write-Error "Failed to remove temp directory: $tempDir"
    }
}

Write-Verbose "Done"