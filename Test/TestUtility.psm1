$script:appVeyorConstants =  @{ 
    AccountName = 'powershell'
    ApiUrl = 'https://ci.appveyor.com/api'
    PowerShellRepoName='powershell-f975h'  # daily run has this name
}


function Get-PowerShellCoreBuild {
    [CmdletBinding()]
    param(

        [string]$repoName = $script:appVeyorConstants.PowerShellRepoName,
        [string]$branch = 'master',
        [string]$downloadFolder = 'C:\projects'
    )

    $foundGood = $false
    $records = 20
    $lastBuildId = $null
    $project = $null

    while(!$foundGood)
    {
        $startBuildIdString = [string]::Empty
        if($lastBuildId)
        {
            $startBuildIdString = "&startBuildId=$lastBuildId"
        }


        $project = Invoke-RestMethod -Method Get -Uri "$($appVeyorConstants.ApiUrl)/projects/$($appVeyorConstants.AccountName)/$repoName/history?recordsNumber=$records$startBuildIdString&branch=$branch"

        foreach($build in $project.builds)
        {
            $version = $build.version
            $status = $build.status
            if($status -ieq 'success')
            {
                Write-Verbose "Using PowerShell Version: $version"

                $foundGood = $true

                Write-Verbose "Uri = $($appVeyorConstants.ApiUrl)/projects/$($appVeyorConstants.AccountName)/$repoName/build/$version"
                $project = Invoke-RestMethod -Method Get -Uri "$($appVeyorConstants.ApiUrl)/projects/$($appVeyorConstants.AccountName)/$repoName/build/$version" 
                break
            }
            else 
            {
                Write-Warning "There is a newer SDK build, $version, which is in status: $status"
            }
        }
    }

    # get project with last build details
    if (-not $project) {

        throw "Cannot find a good build for $branch"
    }

    # we assume here that build has a single job
    # get this job id

    $jobId = $project.build.jobs[0].jobId
    Write-Verbose "jobId=$jobId"
    
    Write-Verbose "$project.build.jobs[0]"

    $artifactsUrl = "$($appVeyorConstants.ApiUrl)/buildjobs/$jobId/artifacts"

    Write-Verbose "Uri=$artifactsUrl"
    $artifacts = Invoke-RestMethod -Method Get -Uri $artifactsUrl 

    if (-not $artifacts) {
        throw "Cannot find artifacts in $artifactsUrl"
    }

    # Get PowerShellCore.msi artifacts for Windows  
 
    $artifacts = $artifacts | where-object { $_.filename -like '*powershell*.msi'}
    $returnArtifactsLocation = @{}

    #download artifacts to a temp location

    foreach($artifact in $artifacts)
    {
        $artifactPath = $artifact[0].fileName
        $artifactFileName = Split-Path -Path $artifactPath -Leaf

        # artifact will be downloaded as 
        $tempLocalArtifactPath = "$downloadFolder\Temp-$artifactFileName-$jobId.msi"
        $localArtifactPath = "$downloadFolder\$artifactFileName-$jobId.msi"
        if(!(Test-Path $localArtifactPath))
        {
            # download artifact
            # -OutFile - is local file name where artifact will be downloaded into

            try 
            {
                $ProgressPreference = 'SilentlyContinue'
                Invoke-WebRequest -Method Get -Uri "$($appVeyorConstants.ApiUrl)/buildjobs/$jobId/artifacts/$artifactPath" `
                    -OutFile $tempLocalArtifactPath  -UseBasicParsing -DisableKeepAlive

                Move-Item -Path $tempLocalArtifactPath -Destination $localArtifactPath   
            } 
            finally
            {
                $ProgressPreference = 'Continue'
                if(test-path $tempLocalArtifactPath)
                {
                    remove-item $tempLocalArtifactPath
                }
            } 
        }

        #$lastDotIndex = $artifactFileName.LastIndexOf('.')
        #$returnArtifactsLocation.add($artifactFileName.Substring(0,$lastDotIndex),$localArtifactPath)

    }

    Write-Verbose $localArtifactPath
    return $localArtifactPath
}

function New-DirectoryIfNotExist {
    param(
        [string]$dir
    )

    if(-not (Test-Path $dir)){ 
        $null = New-Item -Path $dir -ItemType Directory -Force 
    }
}

function Setup-TestRepositoryPathVars {
    param(
        [string]$RepositoryRootDirectory
    )
    
    $script:LocalRepositoryPath  = "$RepositoryRootDirectory\LocalRepository"
    $script:LocalRepositoryPath1 = "$RepositoryRootDirectory\LocalRepository1"
    $script:LocalRepositoryPath2 = "$RepositoryRootDirectory\LocalRepository2"
    $script:LocalRepositoryPath3 = "$RepositoryRootDirectory\LocalRepository3"
    $script:LocalRepository      = "LocalRepository"
}

function New-TestRepositoryModules {
    # These values are shared between this and OneGetTestHelper.ps1 due to old test framework arch
    # Should do some work later to clean up the coupling
    
    if (($null -eq $script:LocalRepositoryPath) -or ("" -eq $script:LocalRepositoryPath)) {
        throw
    }

    New-DirectoryIfNotExist -dir $script:LocalRepositoryPath
    New-DirectoryIfNotExist -dir $script:LocalRepositoryPath1
    New-DirectoryIfNotExist -dir $script:LocalRepositoryPath2
    New-DirectoryIfNotExist -dir $script:LocalRepositoryPath3

    # Clear the directories from previous runs
    Remove-Item $script:LocalRepositoryPath\* -Recurse
    Remove-Item $script:LocalRepositoryPath1\* -Recurse
    Remove-Item $script:LocalRepositoryPath2\* -Recurse
    Remove-Item $script:LocalRepositoryPath3\* -Recurse

    # Ensure LocalRepository is registered while we're setting it up
    Register-Repository -Name $script:LocalRepository -InstallationPolicy Trusted -Ensure Present

    # Create module, publish module, remove module
    Write-Debug "PSScriptRoot: $PSScriptRoot"
    New-DirectoryIfNotExist -dir "$PSScriptRoot\ModuleTemp"
    New-EmptyModule -ModulePath "$PSScriptRoot\ModuleTemp" -ModuleVersion "12.0.1" -ModuleName "MyTestPackage"
    Publish-TestModule -ModulePath (Join-Path -Path "$PSScriptRoot\ModuleTemp" -ChildPath "MyTestPackage") -LocalRepository $script:LocalRepository
    Remove-Item (Join-Path -Path "$PSScriptRoot\ModuleTemp" -ChildPath "MyTestPackage") -Recurse

    New-EmptyModule -ModulePath "$PSScriptRoot\ModuleTemp" -ModuleVersion "12.0.1.1" -ModuleName "MyTestPackage"
    Publish-TestModule -ModulePath (Join-Path -Path "$PSScriptRoot\ModuleTemp" -ChildPath "MyTestPackage") -LocalRepository $script:LocalRepository
    Remove-Item (Join-Path -Path "$PSScriptRoot\ModuleTemp" -ChildPath "MyTestPackage") -Recurse
    
    New-EmptyModule -ModulePath "$PSScriptRoot\ModuleTemp" -ModuleVersion "15.2.1" -ModuleName "MyTestPackage"
    Publish-TestModule -ModulePath (Join-Path -Path "$PSScriptRoot\ModuleTemp" -ChildPath "MyTestPackage") -LocalRepository $script:LocalRepository
    Remove-Item (Join-Path -Path "$PSScriptRoot\ModuleTemp" -ChildPath "MyTestPackage") -Recurse

    New-EmptyModule -ModulePath "$PSScriptRoot\ModuleTemp" -ModuleVersion "1.1" -ModuleName "MyTestModule"
    Publish-TestModule -ModulePath (Join-Path -Path "$PSScriptRoot\ModuleTemp" -ChildPath "MyTestModule") -LocalRepository $script:LocalRepository
    Remove-Item (Join-Path -Path "$PSScriptRoot\ModuleTemp" -ChildPath "MyTestModule") -Recurse

    New-EmptyModule -ModulePath "$PSScriptRoot\ModuleTemp" -ModuleVersion "1.1.2" -ModuleName "MyTestModule"
    Publish-TestModule -ModulePath (Join-Path -Path "$PSScriptRoot\ModuleTemp" -ChildPath "MyTestModule") -LocalRepository $script:LocalRepository
    Remove-Item (Join-Path -Path "$PSScriptRoot\ModuleTemp" -ChildPath "MyTestModule") -Recurse

    New-EmptyModule -ModulePath "$PSScriptRoot\ModuleTemp" -ModuleVersion "3.2.1" -ModuleName "MyTestModule"
    Publish-TestModule -ModulePath (Join-Path -Path "$PSScriptRoot\ModuleTemp" -ChildPath "MyTestModule") -LocalRepository $script:LocalRepository
    Remove-Item (Join-Path -Path "$PSScriptRoot\ModuleTemp" -ChildPath "MyTestModule") -Recurse

    # Replicate
    Copy-Item -Path "$script:LocalRepositoryPath\*" -Destination $script:LocalRepositoryPath1 -Recurse -force -Verbose
    Copy-Item -Path "$script:LocalRepositoryPath\*" -Destination $script:LocalRepositoryPath2 -Recurse -force -Verbose
    Copy-Item -Path "$script:LocalRepositoryPath\*" -Destination $script:LocalRepositoryPath3 -Recurse -force -Verbose

    # Now that we're done setting it up, unregister before the tests start
    Register-Repository -Name $script:LocalRepository -InstallationPolicy Trusted -Ensure Absent
}

function New-EmptyModule {
    param(
        [string]$ModulePath,
        [string]$ModuleVersion,
        [string]$ModuleName
    )

    $fullModulePath = Join-Path -Path $ModulePath -ChildPath $ModuleName
    New-Item -Path $fullModulePath -ItemType Directory -Force

    $modulePSD1Path = "$fullModulePath\$ModuleName.psd1"

    # Create the module manifest
    Microsoft.PowerShell.Core\New-ModuleManifest -Path $modulePSD1Path -Description "$ModuleName" -ModuleVersion $ModuleVersion
}

function Publish-TestModule {
    param(
        [string]$ModulePath,
        [string]$LocalRepository
    )

    try
    {
        PowerShellGet\Publish-Module -Path $ModulePath -NuGetApiKey "Local-Repository-NuGet-ApiKey" -Repository $LocalRepository -Verbose -ErrorAction SilentlyContinue -Force 
    }
    catch
    { 
        # Ignore the particular error
        if ($_.FullyQualifiedErrorId -ine "ModuleVersionShouldBeGreaterThanGalleryVersion,Publish-Module")
        {
            throw
        }               
    }
}

function Register-Repository
{
    <#
    .SYNOPSIS

    This is a helper function to register/unregister the PowerShell repository

    .PARAMETER Name
    Provides the repository Name.

    .PARAMETER SourceLocation
    Provides the source location.

    .PARAMETER PublishLocation
    Provides the publish location.

    .PARAMETER InstallationPolicy
    Determines whether you trust the source repository.

    .PARAMETER Ensure
    Determines whether the repository to be registered or unregistered.
    #>

    param
    (
        [parameter(Mandatory = $true)]
        [System.String]
        $Name,

        [System.String]
        $SourceLocation=$script:LocalRepositoryPath,
   
        [System.String]
        $PublishLocation=$script:LocalRepositoryPath,

        [ValidateSet("Trusted","Untrusted")]
        [System.String]
        $InstallationPolicy="Trusted",

        [ValidateSet("Present","Absent")]
        [System.String]
        $Ensure="Present"
    )

    Write-Verbose -Message "Register-Repository called" -Verbose
    # Calling the following to trigger Bootstrap provider for the first time use PackageManagement
    Get-PackageSource -ProviderName Nuget -ForceBootstrap -WarningAction Ignore 

    $psrepositories = PowerShellGet\get-PSRepository
    $registeredRepository = $null
    $isRegistered = $false

    #Check if the repository has been registered already
    foreach ($repository in $psrepositories)
    {
        # The PSRepository is considered as "exists" if either the Name or Source Location are in used
        $isRegistered = ($repository.SourceLocation -ieq $SourceLocation) -or ($repository.Name -ieq $Name) 

        if ($isRegistered)
        {
            $registeredRepository = $repository
            break;
        }
    }

    if($Ensure -ieq "Present")
    {       
        # If the repository has already been registered, unregister it.
        if ($isRegistered -and ($null -ne $registeredRepository))
        {
            Unregister-PSRepository -Name $registeredRepository.Name
        }       

        PowerShellGet\Register-PSRepository -Name $Name -SourceLocation $SourceLocation -PublishLocation $PublishLocation -InstallationPolicy $InstallationPolicy
    }
    else
    {
        # The repository has already been registered
        if (-not $isRegistered)
        {
            return
        }

        PowerShellGet\UnRegister-PSRepository -Name $Name
    }            
}