#
# Copyright (c) Microsoft Corporation.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.
#

<# Run Test cases Pre-Requisite: 
  1. After download the OngetGet DSC resources modules, it is expected the following are available under your current directory. For example,

    C:\Program Files\WindowsPowerShell\Modules\PackageManagement\
        
        DSCResources
#>

#Define the variables

$CurrentDirectory            = Split-Path -Path $MyInvocation.MyCommand.Path -Parent

$script:LocalRepositoryPath  = "$CurrentDirectory\LocalRepository"
$script:LocalRepositoryPath1 = "$CurrentDirectory\LocalRepository1"
$script:LocalRepositoryPath2 = "$CurrentDirectory\LocalRepository2"
$script:LocalRepositoryPath3 = "$CurrentDirectory\LocalRepository3"
$script:LocalRepository      = "LocalRepository"
$script:InstallationFolder   = $null
$script:DestinationPath      = $null
$script:Module               = $null



#A DSC configuration for installing Pester
configuration Sample_InstallPester
{
    <#
    .SYNOPSIS

    This is a DSC configution that install/uninstall the Pester tool from the nuget. 

    .PARAMETER DestinationPath
    Provides the file folder where the Pester to be installed.

    #>

    param
    (
        #Destination path for the package
        [Parameter(Mandatory)]
        [string]$DestinationPath       
    )

    Import-DscResource -Module PackageManagement -ModuleVersion 1.1.3.0

    Node "localhost"
    {
        
        #register package source       
        PackageManagementSource SourceRepository
        {

            Ensure      = "Present"
            Name        = "Mynuget"
            ProviderName= "Nuget" 
            SourceLocation   = "http://nuget.org/api/v2/"    
            InstallationPolicy ="Trusted"
        }   
        
        #Install a package from Nuget repository
		PackageManagement NugetPackage 
		{ 
			Ensure               = "Present"  
			Name                 = "Pester"
			AdditionalParameters = $DestinationPath
			DependsOn            = "[PackageManagementSource]SourceRepository" 
		}                       
    } 
}

Function InstallPester
{
    <#
    .SYNOPSIS

    This function downloads and installs the pester tool. 

    #>

    Write-Verbose -Message ("Calling function '$($MyInvocation.mycommand)'") -Verbose

    # Check if the Pester have installed already under Program Files\WindowsPowerShell\Modules\Pester
    $pester = Get-Module -Name "Pester" -ListAvailable | select -first 1

    if ($pester.count -ge 1)
    {
        Write-Verbose -Message "Pester has already installed under $($pester.ModuleBase)" -Verbose

        Import-module -Name "$($pester.ModuleBase)\Pester.psd1"          
    }
    else
    {
        # Get the module path where to be installed
        $module = Get-Module -Name "PackageManagementProviderResource" -ListAvailable

        # Compile it
        Sample_InstallPester -DestinationPath "$($module.ModuleBase)\test"

        # Run it
        Start-DscConfiguration -path .\Sample_InstallPester -wait -Verbose -force 

        $result = Get-DscConfiguration 
    
        #import the Pester tool. Note:$result.Name is something like 'Pester.3.3.5'
        Import-module -Name "$($module.ModuleBase)\test\$($result[1].Name)\tools\Pester.psd1"
    }
 }


Function SetupOneGetSourceTest
{
    <#
    .SYNOPSIS

    This is a helper function for a PackageManagementSource test

    #>
    Write-Verbose -Message ("Calling function '$($MyInvocation.mycommand)'") -Verbose

    Import-ModulesToSetupTest -ModuleChildPath  "MSFT_PackageManagementSource\MSFT_PackageManagementSource.psm1"

    UnRegisterAllSource

    # Install Pester and import it
    InstallPester 
}

function SetupPackageManagementTest
{
    <#
    .SYNOPSIS

    This is a helper function for a PackageManagement test

    #>
    param([switch]$SetupPSModuleRepository)

    Write-Verbose -Message ("Calling function '$($MyInvocation.mycommand)'") -Verbose

    Import-ModulesToSetupTest -ModuleChildPath  "MSFT_PackageManagement\MSFT_PackageManagement.psm1"

    $script:DestinationPath = "$CurrentDirectory\TestResult\PackageManagementTest" 
    if ((Get-Variable -Name IsCoreCLR -ErrorAction Ignore) -and $IsCoreCLR) {
        # Assume the latest version is the version we're using (it'd be nice to have a better way to do this)
        $latestPsVersion = get-childitem "$Env:ProgramFiles\PowerShell" | where-object {$_.Name -match '[0-9]+[.][0-9]+[.][0-9]+[.][0-9]+'} | sort-object ($_.Name -as [Version]) -descending | select-object -first 1 | %{ $_.Name }
        Write-Verbose -Message "PSVersion: $latestPsVersion" -Verbose
        $script:PSModuleBase = "$Env:ProgramFiles\PowerShell\$latestPsVersion\modules"
        Write-Verbose -Message "Path $script:PSModuleBase" -Verbose
    } else {
        Write-Verbose -Message "Setting up test as Full CLR" -Verbose
        $script:PSModuleBase = "$env:ProgramFiles\windowspowershell\modules"
    }

    UnRegisterAllSource

    # Install Pester and import it
    InstallPester 

}

Function Import-ModulesToSetupTest
{
    <#
    .SYNOPSIS

    This is a helper function to import modules
    
    .PARAMETER ModuleChildPath
    Provides the child path of the module. The parent path should be the same as the DSC resource.
    #>

    param
    (
        [parameter(Mandatory = $true)]
        [System.String]
        $ModuleChildPath

    )
  
    Write-Verbose -Message ("Calling function '$($MyInvocation.mycommand)'") -Verbose

    $moduleChildPath="DSCResources\$($ModuleChildPath)"

    $script:Module = Get-LatestModuleByName -moduleName "PackageManagement"

    $modulePath = Microsoft.PowerShell.Management\Join-Path -Path $script:Module.ModuleBase -ChildPath $moduleChildPath

    # Using -Force to reload the module (while writing tests..it is common to change product code)
    Import-Module -Name "$($modulePath)"  -Force
    
    #c:\Program Files\WindowsPowerShell\Modules
    $script:InstallationFolder = "$($script:Module.ModuleBase)" 
 }

function RestoreRepository
{
    <#
    .SYNOPSIS

    This is a helper function to reset back your test environment.

    .PARAMETER RepositoryInfo
    Provides the hashtable containing the repository information used for regsitering the repositories.
    #>

    param
    (
        [parameter(Mandatory = $true)]
        [Hashtable]
        $RepositoryInfo
    )

    Write-Verbose -Message "RestoreRepository called"  -Verbose
       
    foreach ($repository in $RepositoryInfo.Keys)
    {
        try
        {
            $null = PowerShellGet\Register-PSRepository -Name $RepositoryInfo[$repository].Name `
                                            -SourceLocation $RepositoryInfo[$repository].SourceLocation `
                                            -PublishLocation $RepositoryInfo[$repository].PublishLocation `
                                            -InstallationPolicy $RepositoryInfo[$repository].InstallationPolicy `
                                            -ErrorAction SilentlyContinue 
        }
        #Ignore if the repository already registered
        catch
        {
            if ($_.FullyQualifiedErrorId -ine "PackageSourceExists")
            {
                throw
            }
        }                                    
    }   
}

function CleanupRepository
{
    <#
    .SYNOPSIS

    This is a helper function for the test setp. Sometimes tests require no other repositories
    are registered, this function helps to do so

    #>

    Write-Verbose -Message "CleanupRepository called" -Verbose

    $returnVal = @{}
    $psrepositories = PowerShellGet\get-PSRepository

    foreach ($repository in $psrepositories)
    {
        #Save the info for later restore process
        $repositoryInfo = @{"Name"=$repository.Name; `
                            "SourceLocation"=$repository.SourceLocation; `
                            "PublishLocation"=$repository.PublishLocation;`
                            "InstallationPolicy"=$repository.InstallationPolicy}

        $returnVal.Add($repository.Name, $repositoryInfo);

        try
        {
            $null = Unregister-PSRepository -Name $repository.Name -ErrorAction SilentlyContinue 
        }
        catch
        {
            if ($_.FullyQualifiedErrorId -ine "RepositoryCannotBeUnregistered")
            {
                throw
            }
        }         
    }   
    
    Return $returnVal   
}

function RegisterPackageSource
{
    <#
    .SYNOPSIS

    This is a helper function to register/unregister the package source

    .PARAMETER Name
    Provides the package source Name.

    .PARAMETER SourceUri
    Provides the source location.

    .PARAMETER PublishLocation
    Provides the publish location.

    .PARAMETER Credential
    Provides the access to the package on a remote source.

    .PARAMETER InstallationPolicy
    Determines whether you trust the source repository.

    .PARAMETER ProviderName
    Provides the package provider name.

    .PARAMETER Ensure
    Determines whether the package source to be registered or unregistered.
    #>

    param
    (
        [parameter(Mandatory = $true)]
        [System.String]
        $Name,

        #Source location. It can be source name or uri
        [System.String]
        $SourceUri,

        [System.Management.Automation.PSCredential]
        $Credential,
    
        [System.String]
        [ValidateSet("Trusted","Untrusted")]
        $InstallationPolicy ="Untrusted",

        [System.String]
        $ProviderName="Nuget",

        [ValidateSet("Present","Absent")]
        [System.String]
        $Ensure="Present"
    )

    Write-Verbose -Message "Calling RegisterPackageSource" -Verbose

    #import the OngetSource module
    Import-ModulesToSetupTest -ModuleChildPath  "MSFT_PackageManagementSource\MSFT_PackageManagementSource.psm1"
    
    if($Ensure -ieq "Present")
    {       
        # If the repository has already been registered, unregister it.
        UnRegisterSource -Name $Name -ProviderName $ProviderName -SourceUri $SourceUri       

        MSFT_PackageManagementSource\Set-TargetResource -Name $name `
                                             -providerName $ProviderName `
                                             -SourceLocation $SourceUri `
                                             -SourceCredential $Credential `
                                             -InstallationPolicy $InstallationPolicy `
                                             -Verbose `
                                             -Ensure Present
    }
    else
    {
        # The repository has already been registered
        UnRegisterSource -Name $Name -ProviderName $ProviderName -SourceUri $SourceUri
    } 
    
    # remove the OngetSource module, after we complete the register/unregister task
    Remove-Module -Name  "MSFT_PackageManagementSource"  -Force -ErrorAction SilentlyContinue         
}

Function UnRegisterSource
{
    <#
    .SYNOPSIS

    This is a helper function to unregister a particular package source

    .PARAMETER Name
    Provides the package source Name.

    .PARAMETER SourceUri
    Provides the source location.

    .PARAMETER ProviderName
    Provides the package provider name.
    #>

    param
    (
        [parameter(Mandatory = $true)]
        [System.String]
        $Name,

        [System.String]
        $SourceUri,

        [System.String]
        $ProviderName="Nuget"
    )

    Write-Verbose -Message ("Calling function '$($MyInvocation.mycommand)'") -Verbose

    $getResult = MSFT_PackageManagementSource\Get-TargetResource -Name $name -providerName $ProviderName -SourceLocation $SourceUri -Verbose

    if ($getResult.Ensure -ieq "Present")
    {
        #Unregister it
        MSFT_PackageManagementSource\Set-TargetResource -Name $name -providerName $ProviderName -SourceLocation $SourceUri -Verbose -Ensure Absent               
    }
}

Function UnRegisterAllSource
{
    <#
    .SYNOPSIS

    This is a helper function to unregister all the package source on the machine

    #>

    Write-Verbose -Message ("Calling function '$($MyInvocation.mycommand)'") -Verbose

    $sources = PackageManagement\Get-PackageSource

    foreach ($source in $sources)
    {
        try
        {
            #Unregister whatever can be unregistered
            PackageManagement\Unregister-PackageSource -Name $source.Name -providerName $source.ProviderName -ErrorAction SilentlyContinue  2>&1   
        }
        catch
        {
            if ($_.FullyQualifiedErrorId -ine "RepositoryCannotBeUnregistered")
            {
                throw
            }
        }         
    }
}

function CreateCredObject
{
    <#
    .SYNOPSIS

    This is a helper function for the cmdlets testing where requires PSCredential

    #>
        
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingConvertToSecureStringWithPlainText", "")]
    param(           
        [System.String]
        $Name,
        
        [System.String]
        $PSCode
        )


    Write-Verbose -Message ("Calling function '$($MyInvocation.mycommand)'") -Verbose

    $secCode = ConvertTo-SecureString -String $PSCode -AsPlainText -Force
    $cred = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList ($Name, $secCode)
    return $cred
}

function CreateTestModuleInLocalRepository
{
    <#
    .SYNOPSIS

    This is a helper function that generates test packages/modules and publishes them to a local repository.
    Please note that it only generates manifest files just for testing purpose.

    .PARAMETER ModuleName
    Provides the module Name to be generated.

    .PARAMETER ModuleVersion
    Provides the module version to be generated.

    .PARAMETER LocalRepository
    Provides the local repository Name.
    #>

    param(
        [System.String]
        $ModuleName, 

        [System.String]
        $ModuleVersion,

        [System.String]
        $LocalRepository
    )

    Write-Verbose -Message ("Calling function '$($MyInvocation.mycommand)'") -Verbose

    # Return if the package already exists
    $m = PowerShellGet\Find-Module -Name $ModuleName -Repository $LocalRepository  -RequiredVersion $ModuleVersion  -ErrorAction Ignore
    if($m)
    {
        return
    }

    # Get the parent 'PackageManagementProviderResource' module path
    $parentModulePath = Microsoft.PowerShell.Management\Split-Path -Path $script:Module.ModuleBase -Parent

    $modulePath = Microsoft.PowerShell.Management\Join-Path -Path $parentModulePath -ChildPath "$ModuleName"

    New-Item -Path $modulePath -ItemType Directory -Force

    $modulePSD1Path = "$modulePath\$ModuleName.psd1"

    # Create the module manifest
    Microsoft.PowerShell.Core\New-ModuleManifest -Path $modulePSD1Path -Description "$ModuleName" -ModuleVersion $ModuleVersion

    
    
    try
    {
        # Publish the module to your local repository
        PowerShellGet\Publish-Module -Path $modulePath -NuGetApiKey "Local-Repository-NuGet-ApiKey" -Repository $LocalRepository -Verbose -ErrorAction SilentlyContinue -Force 
    }
    catch
    { 
        # Ignore the particular error
        if ($_.FullyQualifiedErrorId -ine "ModuleVersionShouldBeGreaterThanGalleryVersion,Publish-Module")
        {
            throw
        }               
    }

    # Remove the module under modulepath once we published it to the local repository
    Microsoft.PowerShell.Management\Remove-item -Path $modulePath -Recurse -Force -ErrorAction SilentlyContinue
}

function ConvertHashtableToArryCimInstance
{
  <#
    .SYNOPSIS

    This helper function is mainly used to convert AdditionalParameters of PackageMangement DSC resource
    to Microsoft.Management.Infrastructure.CimInstance[]. This will enable writing DRTs for Get/Set/Test
    methods.

    #>
    [OutputType([Microsoft.Management.Infrastructure.CimInstance[]])]
    param([Hashtable] $AdditionalParameters = $(throw "AdditionalParameters cannot be null."))

    [Microsoft.Management.Infrastructure.CimInstance[]] $result = [Microsoft.Management.Infrastructure.CimInstance[]]::new($AdditionalParameters.Count)

    $index = 0
    $AdditionalParameters.Keys | % {
        $instance = New-CimInstance -ClassName MSFT_KeyValuePair -Namespace root/microsoft/Windows/DesiredStateConfiguration -Property @{
            Key = $_
            Value = $AdditionalParameters[$_]
            } -ClientOnly
        $result[$index] = $instance
        $index++
    }

    $result
}

function IsAdmin
{
    <#
    .SYNOPSIS
        Checks whether the current session is Elevated. Used for test suites which has this
        requirement   
    #>
    [OutputType([bool])]
    
    param()
        try {
        $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
        $principal = New-Object Security.Principal.WindowsPrincipal -ArgumentList $identity
        return $principal.IsInRole( [Security.Principal.WindowsBuiltInRole]::Administrator )
    } catch {
    }

    return $false
}

function Get-LatestModuleByName {
    param(
        [string]$moduleName
    )

    $allModulesSorted = Get-Module -Name $moduleName -ListAvailable | Sort-Object -Property Version -Descending
    $topVersion = $allModulesSorted[0].Version
    $topModules = $allModulesSorted | Where-Object {$_.Version -eq $topVersion}
    if ($topModules.Count -eq 1) {
        $topModules[0]
    } else {
        $topModules = $topModules | %{ @{Module=$_;Time=((Get-ItemProperty $_.Path).LastWriteTime)} } | Sort-Object { $_.Time -as [DateTime] } -Descending | %{ $_.Module }
        $topModules[0]
    }
}