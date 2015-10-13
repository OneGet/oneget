
#########################################################################################
#
# Copyright (c) Microsoft Corporation. All rights reserved.
#
# PowerShellGet Module
#
#########################################################################################

Microsoft.PowerShell.Core\Set-StrictMode -Version Latest

#region script variables

# Check if this is nano server. [System.Runtime.Loader.AssemblyLoadContext] is only available on NanoServer
try {
    [System.Runtime.Loader.AssemblyLoadContext]
    $script:isNanoServer = $true
}
catch
{
    $script:isNanoServer = $false
}

try
{
    $script:MyDocumentsFolderPath = [Environment]::GetFolderPath("MyDocuments")
}
catch
{
    $script:MyDocumentsFolderPath = $null
}

$script:ProgramFilesPSPath = Microsoft.PowerShell.Management\Join-Path -Path $env:ProgramFiles -ChildPath "WindowsPowerShell"

$script:MyDocumentsPSPath = if($script:MyDocumentsFolderPath)
                            {
                                Microsoft.PowerShell.Management\Join-Path -Path $script:MyDocumentsFolderPath -ChildPath "WindowsPowerShell"
                            } 
                            else
                            {
                                Microsoft.PowerShell.Management\Join-Path -Path $env:USERPROFILE -ChildPath "Documents\WindowsPowerShell"
                            }

$script:ProgramFilesModulesPath = Microsoft.PowerShell.Management\Join-Path -Path $script:ProgramFilesPSPath -ChildPath "Modules"
$script:MyDocumentsModulesPath = Microsoft.PowerShell.Management\Join-Path -Path $script:MyDocumentsPSPath -ChildPath "Modules"

$script:ProgramFilesScriptsPath = Microsoft.PowerShell.Management\Join-Path -Path $script:ProgramFilesPSPath -ChildPath "Scripts"

$script:MyDocumentsScriptsPath = Microsoft.PowerShell.Management\Join-Path -Path $script:MyDocumentsPSPath -ChildPath "Scripts"

$script:TempPath = ([System.IO.DirectoryInfo]$env:TEMP).FullName
$script:PSGetItemInfoFileName = "PSGetModuleInfo.xml"
$script:PSGetAppLocalPath="$env:LOCALAPPDATA\Microsoft\Windows\PowerShell\PowerShellGet"
$script:PSGetModuleSourcesFilePath = Microsoft.PowerShell.Management\Join-Path -Path $script:PSGetAppLocalPath -ChildPath "PSRepositories.xml"
$script:PSGetModuleSources = $null
$script:PSGetInstalledModules = $null

$script:PSGetAppLocalScriptsPath = Microsoft.PowerShell.Management\Join-Path -Path $script:PSGetAppLocalPath -ChildPath 'Scripts'
if(-not (Microsoft.PowerShell.Management\Test-Path -Path $script:PSGetAppLocalScriptsPath))
{
    $null = Microsoft.PowerShell.Management\New-Item -Path $script:PSGetAppLocalScriptsPath `
                                                     -ItemType Directory `
                                                     -Force `
                                                     -Confirm:$false `
                                                     -WhatIf:$false
}

$script:PSGetProgramDataScriptsPath = Microsoft.PowerShell.Management\Join-Path -Path $env:ProgramData -ChildPath 'Microsoft\Windows\PowerShell\PowerShellGet\Scripts'
if(-not (Microsoft.PowerShell.Management\Test-Path -Path $script:PSGetProgramDataScriptsPath))
{
    $null = Microsoft.PowerShell.Management\New-Item -Path $script:PSGetProgramDataScriptsPath `
                                                     -ItemType Directory `
                                                     -Force `
                                                     -Confirm:$false `
                                                     -WhatIf:$false
}

$script:InstalledScriptInfoFileName = 'InstalledScriptInfo.xml'
$script:PSGetInstalledScripts = $null

# Public PSGallery module source name and location
$Script:PSGalleryModuleSource="PSGallery"
$Script:PSGallerySourceUri  = 'https://go.microsoft.com/fwlink/?LinkID=397631&clcid=0x409'
$Script:PSGalleryPublishUri = 'https://go.microsoft.com/fwlink/?LinkID=397527&clcid=0x409'
$Script:PSGalleryScriptSourceUri = 'https://go.microsoft.com/fwlink/?LinkID=622995&clcid=0x409'

# PSGallery V3 Source
$Script:PSGalleryV3SourceUri = 'https://go.microsoft.com/fwlink/?LinkId=528403&clcid=0x409'

$Script:PSGalleryV2ApiAvailable = $true
$Script:PSGalleryV3ApiAvailable = $false
$Script:PSGalleryApiChecked = $false

$Script:ResponseUri = "ResponseUri"
$Script:StatusCode = "StatusCode"
$Script:Exception = "Exception"

$script:PSModuleProviderName = "PSModule"
$script:PackageManagementProviderParam  = "PackageManagementProvider"
$script:PublishLocation = "PublishLocation"
$script:ScriptSourceLocation = 'ScriptSourceLocation'
$script:ScriptPublishLocation = 'ScriptPublishLocation'

$script:NuGetProviderName = "NuGet"

$script:SupportsPSModulesFeatureName="supports-powershell-modules"
$script:FastPackRefHastable = @{}
$script:NuGetBinaryProgramDataPath="$env:ProgramFiles\PackageManagement\ProviderAssemblies"
$script:NuGetBinaryLocalAppDataPath="$env:LOCALAPPDATA\PackageManagement\ProviderAssemblies"
$script:NuGetClient = $null
# PowerShellGetFormatVersion will be incremented when we change the .nupkg format structure. 
# PowerShellGetFormatVersion is in the form of Major.Minor.  
# Minor is incremented for the backward compatible format change.
# Major is incremented for the breaking change.
$script:CurrentPSGetFormatVersion = "1.0"
$script:PSGetFormatVersion = "PowerShellGetFormatVersion"
$script:SupportedPSGetFormatVersionMajors = @("1")
$script:ModuleReferences = 'Module References'
$script:AllVersions = "AllVersions"
$script:Filter      = "Filter"
$script:IncludeValidSet = @("DscResource","Cmdlet","Function", 'Workflow')
$script:DscResource = "PSDscResource"
$script:Command     = "PSCommand"
$script:Cmdlet      = "PSCmdlet"
$script:Function    = "PSFunction"
$script:Workflow    = "PSWorkflow"
$script:Includes    = "PSIncludes"
$script:Tag         = "Tag"
$script:NotSpecified= '_NotSpecified_'
$script:PSGetModuleName = 'PowerShellGet'
$script:FindByCanonicalId = 'FindByCanonicalId'
$script:InstalledLocation = 'InstalledLocation'
$script:PSArtifactType = 'Type'
$script:PSArtifactTypeModule = 'Module'
$script:PSArtifactTypeScript = 'Script'
$script:All = 'All'

$script:Name = 'Name'
$script:Version = 'Version'
$script:Path = 'Path'
$script:ScriptBase = 'ScriptBase'
$script:Description = 'Description'
$script:Author = 'Author'
$script:CompanyName = 'CompanyName'
$script:Copyright = 'Copyright'
$script:Tags = 'Tags'
$script:LicenseUri = 'LicenseUri'
$script:ProjectUri = 'ProjectUri'
$script:IconUri = 'IconUri'
$script:RequiredModules = 'RequiredModules'
$script:ExternalModuleDependencies = 'ExternalModuleDependencies'
$script:ReleaseNotes = 'ReleaseNotes'
$script:RequiredScripts = 'RequiredScripts'
$script:ExternalScriptDependencies = 'ExternalScriptDependencies'
$script:ExportedCommands  = 'ExportedCommands'
$script:ExportedFunctions = 'ExportedFunctions'
$script:ExportedWorkflows = 'ExportedWorkflows'
$script:TextInfo = (Get-Culture).TextInfo

$script:PSScriptInfoProperties = @($script:Name
                                   $script:Version,
                                   $script:Path,
                                   $script:ScriptBase,
                                   $script:Description,
                                   $script:Author,
                                   $script:CompanyName,
                                   $script:Copyright,
                                   $script:Tags,
                                   $script:ReleaseNotes,
                                   $script:RequiredModules,
                                   $script:ExternalModuleDependencies,
                                   $script:RequiredScripts,
                                   $script:ExternalScriptDependencies,
                                   $script:LicenseUri,
                                   $script:ProjectUri,
                                   $script:IconUri,
                                   $script:ExportedCommands,
                                   $script:ExportedFunctions,
                                   $script:ExportedWorkflows
                                   )

# Wildcard pattern matching configuration.
$script:wildcardOptions = [System.Management.Automation.WildcardOptions]::CultureInvariant -bor `
                          [System.Management.Automation.WildcardOptions]::IgnoreCase

$script:DynamicOptionTypeMap = @{
                                    0 = [string];       # String
                                    1 = [string[]];     # StringArray
                                    2 = [int];          # Int
                                    3 = [switch];       # Switch
                                    4 = [string];       # Folder
                                    5 = [string];       # File
                                    6 = [string];       # Path
                                    7 = [Uri];          # Uri
                                    8 = [SecureString]; #SecureString
                                }
#endregion script variables

#region Module message resolvers
$script:PackageManagementMessageResolverScriptBlock =  {
                                                param($i, $Message)
                                                return (PackageManagementMessageResolver -MsgId $i, -Message $Message)			
                                            }		

$script:PackageManagementSaveModuleMessageResolverScriptBlock =  {
                                                param($i, $Message)
                                                $PackageTarget = $LocalizedData.InstallModulewhatIfMessage
                                                $QuerySaveUntrustedPackage = $LocalizedData.QuerySaveUntrustedPackage

                                                switch ($i)
                                                {
                                                    'ActionInstallPackage' { return "Save-Module" }
                                                    'QueryInstallUntrustedPackage' {return $QuerySaveUntrustedPackage}
                                                    'TargetPackage' { return $PackageTarget }
                                                     Default {
                                                        $Message = $Message -creplace "Install", "Download"
                                                        $Message = $Message -creplace "install", "download"
                                                        return (PackageManagementMessageResolver -MsgId $i, -Message $Message)
                                                     }
                                                }                                                
                                            }

$script:PackageManagementInstallModuleMessageResolverScriptBlock =  {
                                                param($i, $Message)
                                                $PackageTarget = $LocalizedData.InstallModulewhatIfMessage

                                                switch ($i)
                                                {
                                                    'ActionInstallPackage' { return "Install-Module" }
                                                    'TargetPackage' { return $PackageTarget }
                                                     Default {
                                                        return (PackageManagementMessageResolver -MsgId $i, -Message $Message)
                                                     }
                                                }                                                
                                            }		

$script:PackageManagementUnInstallModuleMessageResolverScriptBlock =  {
                                                param($i, $Message)
                                                $PackageTarget = $LocalizedData.InstallModulewhatIfMessage
                                                switch ($i)
                                                {
                                                    'ActionUninstallPackage' { return "Uninstall-Module" }              
                                                    'TargetPackageVersion' { return $PackageTarget }
                                                     Default {
                                                        return (PackageManagementMessageResolver -MsgId $i, -Message $Message)
                                                     }
                                                }                                                
                                            }		

$script:PackageManagementUpdateModuleMessageResolverScriptBlock =  {
                                                param($i, $Message)
                                                $PackageTarget = ($LocalizedData.UpdateModulewhatIfMessage -replace "__OLDVERSION__",$($psgetItemInfo.Version))                                                
                                                switch ($i)
                                                {
                                                    'ActionInstallPackage' { return "Update-Module" }              
                                                    'TargetPackage' { return $PackageTarget }
                                                     Default {
                                                        return (PackageManagementMessageResolver -MsgId $i, -Message $Message)
                                                     }
                                                }                                     
                                            }
                                            
function PackageManagementMessageResolver($MsgID, $Message) {    
              	$NoMatchFound = $LocalizedData.NoMatchFound
              	$SourceNotFound = $LocalizedData.SourceNotFound              
                $ModuleIsNotTrusted = $LocalizedData.ModuleIsNotTrusted
                $RepositoryIsNotTrusted = $LocalizedData.RepositoryIsNotTrusted
                $QueryInstallUntrustedPackage = $LocalizedData.QueryInstallUntrustedPackage

                switch ($MsgID)
                {
                   'NoMatchFound' { return $NoMatchFound }
                   'SourceNotFound' { return $SourceNotFound }
                   'CaptionPackageNotTrusted' { return $ModuleIsNotTrusted }
                   'CaptionSourceNotTrusted' { return $RepositoryIsNotTrusted }
                   'QueryInstallUntrustedPackage' {return $QueryInstallUntrustedPackage}
                    Default {
                        if($Message)
                        {
                            $tempMessage = $Message     -creplace "Package", "Module"
                            $tempMessage = $tempMessage -creplace "package", "module"
                            $tempMessage = $tempMessage -creplace "Sources", "Repositories"
                            $tempMessage = $tempMessage -creplace "sources", "repositories"
                            $tempMessage = $tempMessage -creplace "Source", "Repository"
                            $tempMessage = $tempMessage -creplace "source", "repository"

                            return $tempMessage
                        }
                    }
                }    
}                                    		

#endregion Module message resolvers

#region Script message resolvers
$script:PackageManagementMessageResolverScriptBlockForScriptCmdlets =  {
                                                param($i, $Message)
                                                return (PackageManagementMessageResolverForScripts -MsgId $i, -Message $Message)			
                                            }		

$script:PackageManagementSaveScriptMessageResolverScriptBlock =  {
                                                param($i, $Message)
                                                $PackageTarget = $LocalizedData.InstallScriptwhatIfMessage
                                                $QuerySaveUntrustedPackage = $LocalizedData.QuerySaveUntrustedScriptPackage

                                                switch ($i)
                                                {
                                                    'ActionInstallPackage' { return "Save-Script" }
                                                    'QueryInstallUntrustedPackage' {return $QuerySaveUntrustedPackage}
                                                    'TargetPackage' { return $PackageTarget }
                                                     Default {
                                                        $Message = $Message -creplace "Install", "Download"
                                                        $Message = $Message -creplace "install", "download"
                                                        return (PackageManagementMessageResolverForScripts -MsgId $i, -Message $Message)
                                                     }
                                                }                                                
                                            }

$script:PackageManagementInstallScriptMessageResolverScriptBlock =  {
                                                param($i, $Message)
                                                $PackageTarget = $LocalizedData.InstallScriptwhatIfMessage

                                                switch ($i)
                                                {
                                                    'ActionInstallPackage' { return "Install-Script" }
                                                    'TargetPackage' { return $PackageTarget }
                                                     Default {
                                                        return (PackageManagementMessageResolverForScripts -MsgId $i, -Message $Message)
                                                     }
                                                }                                                
                                            }		

$script:PackageManagementUnInstallScriptMessageResolverScriptBlock =  {
                                                param($i, $Message)
                                                $PackageTarget = $LocalizedData.InstallScriptwhatIfMessage
                                                switch ($i)
                                                {
                                                    'ActionUninstallPackage' { return "Uninstall-Script" }              
                                                    'TargetPackageVersion' { return $PackageTarget }
                                                     Default {
                                                        return (PackageManagementMessageResolverForScripts -MsgId $i, -Message $Message)
                                                     }
                                                }                                                
                                            }		

$script:PackageManagementUpdateScriptMessageResolverScriptBlock =  {
                                                param($i, $Message)
                                                $PackageTarget = ($LocalizedData.UpdateScriptwhatIfMessage -replace "__OLDVERSION__",$($psgetItemInfo.Version))                                                
                                                switch ($i)
                                                {
                                                    'ActionInstallPackage' { return "Update-Script" }              
                                                    'TargetPackage' { return $PackageTarget }
                                                     Default {
                                                        return (PackageManagementMessageResolverForScripts -MsgId $i, -Message $Message)
                                                     }
                                                }                                     
                                            }
                                            
function PackageManagementMessageResolverForScripts($MsgID, $Message) {    
              	$NoMatchFound = $LocalizedData.NoMatchFoundForScriptName
              	$SourceNotFound = $LocalizedData.SourceNotFound              
                $ScriptIsNotTrusted = $LocalizedData.ScriptIsNotTrusted
                $RepositoryIsNotTrusted = $LocalizedData.RepositoryIsNotTrusted
                $QueryInstallUntrustedPackage = $LocalizedData.QueryInstallUntrustedScriptPackage

                switch ($MsgID)
                {
                   'NoMatchFound' { return $NoMatchFound }
                   'SourceNotFound' { return $SourceNotFound }
                   'CaptionPackageNotTrusted' { return $ScriptIsNotTrusted }
                   'CaptionSourceNotTrusted' { return $RepositoryIsNotTrusted }
                   'QueryInstallUntrustedPackage' {return $QueryInstallUntrustedPackage}
                    Default {
                        if($Message)
                        {
                            $tempMessage = $Message     -creplace "Package", "Script"
                            $tempMessage = $tempMessage -creplace "package", "script"
                            $tempMessage = $tempMessage -creplace "Sources", "Repositories"
                            $tempMessage = $tempMessage -creplace "sources", "repositories"
                            $tempMessage = $tempMessage -creplace "Source", "Repository"
                            $tempMessage = $tempMessage -creplace "source", "repository"

                            return $tempMessage
                        }
                    }
                }    
}                                    		

#endregion Script message resolvers

Microsoft.PowerShell.Utility\Import-LocalizedData  LocalizedData -filename PSGet.Resource.psd1

#region Add .Net type for Telemetry APIs

# This code is required to add a .Net type and call the Telemetry APIs 
# This is required since PowerShell does not support generation of .Net Anonymous types
#
$requiredAssembly = ( 
    "system.management.automation, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"    
    ) 

$source = @" 
using System; 
using System.Management.Automation;

namespace Microsoft.PowerShell.Get 
{ 
    public static class Telemetry  
    { 
        public static void TraceMessageArtifactsNotFound(string[] artifactsNotFound, string operationName) 
        { 
            Microsoft.PowerShell.Telemetry.Internal.TelemetryAPI.TraceMessage(operationName, new { ArtifactsNotFound = artifactsNotFound });
        }         
        
    } 
} 
"@ 

# Telemetry is turned off by default.
$script:TelemetryEnabled = $false

try
{
    Add-Type -ReferencedAssemblies $requiredAssembly -TypeDefinition $source -Language CSharp -ErrorAction SilentlyContinue

    if (([Microsoft.PowerShell.Get.Telemetry] | Get-Member -Static).Name.Contains("TraceMessageArtifactsNotFound"))
    {
        # Turn ON Telemetry if the infrastructure is present on the machine
        $script:TelemetryEnabled = $true
    }
}
catch
{
    # Disable Telemetry if there are any issues finding/loading the Telemetry infrastructure
    $script:TelemetryEnabled = $false
}


#endregion

#region *-Module cmdlets
function Publish-Module
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(SupportsShouldProcess=$true,
                   PositionalBinding=$false,
                   HelpUri='http://go.microsoft.com/fwlink/?LinkID=398575',
                   DefaultParameterSetName="ModuleNameParameterSet")]
    Param
    (
        [Parameter(Mandatory=$true, 
                   ParameterSetName="ModuleNameParameterSet",
                   ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Name,

        [Parameter(Mandatory=$true, 
                   ParameterSetName="ModulePathParameterSet",
                   ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Path,

        [Parameter(ParameterSetName="ModuleNameParameterSet")]
        [ValidateNotNullOrEmpty()]
        [Version]
        $RequiredVersion,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $NuGetApiKey,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $Repository = $Script:PSGalleryModuleSource,

        [Parameter()] 
        [ValidateSet("1.0")]
        [Version]
        $FormatVersion,

        [Parameter()]
        [string[]]
        $ReleaseNotes,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Tags,
        
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $LicenseUri,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $IconUri,
        
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $ProjectUri
    )

    Begin
    {
        if($script:isNanoServer) {
            $message = $LocalizedData.PublishPSArtifactUnsupportedOnNano -f "Module"
            ThrowError -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $message `
                        -ErrorId "UnsupportedOperation" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ExceptionObject $PSCmdlet `
                        -ErrorCategory InvalidOperation
        }

        Get-PSGalleryApiAvailability -Repository $Repository
        
        if($LicenseUri -and -not (Test-WebUri -uri $LicenseUri))
        {
            $message = $LocalizedData.InvalidWebUri -f ($LicenseUri, "LicenseUri")
            ThrowError -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $message `
                        -ErrorId "InvalidWebUri" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ErrorCategory InvalidArgument `
                        -ExceptionObject $LicenseUri
        }

        if($IconUri -and -not (Test-WebUri -uri $IconUri))
        {
            $message = $LocalizedData.InvalidWebUri -f ($IconUri, "IconUri")
            ThrowError -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $message `
                        -ErrorId "InvalidWebUri" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ErrorCategory InvalidArgument `
                        -ExceptionObject $IconUri
        }

        if($ProjectUri -and -not (Test-WebUri -uri $ProjectUri))
        {
            $message = $LocalizedData.InvalidWebUri -f ($ProjectUri, "ProjectUri")
            ThrowError -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $message `
                        -ErrorId "InvalidWebUri" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ErrorCategory InvalidArgument `
                        -ExceptionObject $ProjectUri
        }
        
        #If users are providing tags using -Tags while running PS 5.0, will show warning messages
        if($Tags)
        {
            $message = $LocalizedData.TagsShouldBeIncludedInManifestFile -f ($Path)
            Write-Warning $message 
        }

        if($ReleaseNotes)
        {
            $message = $LocalizedData.ReleaseNotesShouldBeIncludedInManifestFile -f ($Path)
            Write-Warning $message 
        }

        if($LicenseUri)
        {
            $message = $LocalizedData.LicenseUriShouldBeIncludedInManifestFile -f ($Path)
            Write-Warning $message
        }

        if($IconUri)
        {
            $message = $LocalizedData.IconUriShouldBeIncludedInManifestFile -f ($Path)
            Write-Warning $message
        }

        if($ProjectUri)
        {
            $message = $LocalizedData.ProjectUriShouldBeIncludedInManifestFile -f ($Path)
            Write-Warning $message
        }

        Install-NuGetClientBinaries
    }

    Process
    {
        $ev = $null
        $moduleSource = Get-PSRepository -Name $Repository -ErrorVariable ev
        if($ev) { return }

        $DestinationLocation = $moduleSource.PublishLocation
                
        if(-not $DestinationLocation -or
           (-not (Microsoft.PowerShell.Management\Test-Path $DestinationLocation) -and 
           -not (Test-WebUri -uri $DestinationLocation)))

        {
            $message = $LocalizedData.PSGalleryPublishLocationIsMissing -f ($Repository, $Repository)
            ThrowError -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $message `
                        -ErrorId "PSGalleryPublishLocationIsMissing" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ErrorCategory InvalidArgument `
                        -ExceptionObject $Repository
        }

        $providerName = Get-ProviderName -PSCustomObject $moduleSource
        if($providerName -ne $script:NuGetProviderName)
        {
            $message = $LocalizedData.PublishModuleSupportsOnlyNuGetBasedPublishLocations -f ($moduleSource.PublishLocation, $Repository, $Repository)
            ThrowError -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $message `
                        -ErrorId "PublishModuleSupportsOnlyNuGetBasedPublishLocations" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ErrorCategory InvalidArgument `
                        -ExceptionObject $Repository
        }

        if($Name)
        {
            $module = Microsoft.PowerShell.Core\Get-Module -ListAvailable -Name $Name -Verbose:$false | 
                          Microsoft.PowerShell.Core\Where-Object {-not $RequiredVersion -or ($RequiredVersion -eq $_.Version)} 

            if(-not $module)
            {
                if($RequiredVersion)
                {
                    $message = $LocalizedData.ModuleWithRequiredVersionNotAvailableLocally -f ($Name, $RequiredVersion)
                }
                else
                {
                    $message = $LocalizedData.ModuleNotAvailableLocally -f ($Name)
                }

                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage $message `
                           -ErrorId "ModuleNotAvailableLocallyToPublish" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument `
                           -ExceptionObject $Name

            }
            elseif($module.GetType().ToString() -ne "System.Management.Automation.PSModuleInfo")
            {
                $message = $LocalizedData.AmbiguousModuleName -f ($Name)
                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage $message `
                           -ErrorId "AmbiguousModuleNameToPublish" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument `
                           -ExceptionObject $Name
            }

            $Path = $module.ModuleBase
        }
        else
        {
            if(-not (Microsoft.PowerShell.Management\Test-Path -path $Path -PathType Container))
            {                
                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage ($LocalizedData.PathIsNotADirectory -f ($Path)) `
                           -ErrorId "PathIsNotADirectory" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument `
                           -ExceptionObject $Path
            }
        }

        $moduleName = Microsoft.PowerShell.Management\Split-Path $Path -Leaf
        
        # if the Leaf of the $Path is a version, use its parent folder name as the module name
        $ModuleVersion = New-Object System.Version
        if([System.Version]::TryParse($moduleName, ([ref]$ModuleVersion)))
        {
            $moduleName = Microsoft.PowerShell.Management\Split-Path -Path (Microsoft.PowerShell.Management\Split-Path $Path -Parent) -Leaf
        }

        $message = $LocalizedData.PublishModuleLocation -f ($moduleName, $Path)
        Write-Verbose -Message $message

        # Copy the source module to temp location to publish
        $tempModulePath = Microsoft.PowerShell.Management\Join-Path -Path $script:TempPath -ChildPath "$(Microsoft.PowerShell.Utility\Get-Random)\$moduleName"
        if(-not $FormatVersion)
        {
            $tempModulePathForFormatVersion = $tempModulePath
        }
        elseif ($FormatVersion -eq "1.0")
        {
            $tempModulePathForFormatVersion = Microsoft.PowerShell.Management\Join-Path $tempModulePath "Content\Deployment\$script:ModuleReferences\$moduleName"
        }

        $null = Microsoft.PowerShell.Management\New-Item -Path $tempModulePathForFormatVersion -ItemType Directory -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
        Microsoft.PowerShell.Management\Copy-Item -Path "$Path\*" -Destination $tempModulePathForFormatVersion -Force -Recurse -Confirm:$false -WhatIf:$false

        try
        {
            $manifestPath = Microsoft.PowerShell.Management\Join-Path $tempModulePathForFormatVersion "$moduleName.psd1"
        
            if(-not (Microsoft.PowerShell.Management\Test-Path $manifestPath))
            {
                $message = $LocalizedData.InvalidModuleToPublish -f ($moduleName)
                ThrowError -ExceptionName "System.InvalidOperationException" `
                           -ExceptionMessage $message `
                           -ErrorId "InvalidModuleToPublish" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidOperation `
                           -ExceptionObject $moduleName
            }

            $moduleInfo = Microsoft.PowerShell.Core\Test-ModuleManifest -Path $manifestPath `
                                                                        -Verbose:$VerbosePreference

            if(-not $moduleInfo -or 
               -not $moduleInfo.Author -or 
               -not $moduleInfo.Description)
            {
                $message = $LocalizedData.MissingRequiredManifestKeys -f ($moduleName)
                ThrowError -ExceptionName "System.InvalidOperationException" `
                           -ExceptionMessage $message `
                           -ErrorId "MissingRequiredModuleManifestKeys" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidOperation `
                           -ExceptionObject $moduleName
            }

            # Check if the specified module name is already used for a script on the specified repository
            # Use Find-Script to check if that name is already used as scriptname
            $scriptPSGetItemInfo = Find-Script -Name $moduleName `
                                               -Repository $Repository `
                                               -Tag 'PSScript' `
                                               -Verbose:$VerbosePreference `
                                               -ErrorAction SilentlyContinue `
                                               -WarningAction SilentlyContinue `
                                               -Debug:$DebugPreference | 
                                        Microsoft.PowerShell.Core\Where-Object {$_.Name -eq $moduleName} | 
                                            Microsoft.PowerShell.Utility\Select-Object -Last 1
            if($scriptPSGetItemInfo)
            {
                $message = $LocalizedData.SpecifiedNameIsAlearyUsed -f ($moduleName, $Repository)
                ThrowError -ExceptionName "System.InvalidOperationException" `
                           -ExceptionMessage $message `
                           -ErrorId "SpecifiedNameIsAlearyUsed" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidOperation `
                           -ExceptionObject $moduleName
            }

            $currentPSGetItemInfo = Find-Module -Name $moduleInfo.Name `
                                                -Repository $Repository `
                                                -Verbose:$VerbosePreference `
                                                -ErrorAction SilentlyContinue `
                                                -WarningAction SilentlyContinue `
                                                -Debug:$DebugPreference | 
                                        Microsoft.PowerShell.Core\Where-Object {$_.Name -eq $moduleInfo.Name} | 
                                            Microsoft.PowerShell.Utility\Select-Object -Last 1

            if($currentPSGetItemInfo -and $currentPSGetItemInfo.Version -ge $moduleInfo.Version)
            {
                $message = $LocalizedData.ModuleVersionShouldBeGreaterThanGalleryVersion -f ($moduleInfo.Name, $moduleInfo.Version, $currentPSGetItemInfo.Version, $currentPSGetItemInfo.RepositorySourceLocation)
                ThrowError -ExceptionName "System.InvalidOperationException" `
                            -ExceptionMessage $message `
                            -ErrorId "ModuleVersionShouldBeGreaterThanGalleryVersion" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ErrorCategory InvalidOperation
            }

            $shouldProcessMessage = $LocalizedData.PublishModulewhatIfMessage -f ($moduleInfo.Version, $moduleInfo.Name)
            if($PSCmdlet.ShouldProcess($shouldProcessMessage, "Publish-Module"))
            {
                Publish-PSArtifactUtility -PSModuleInfo $moduleInfo `
                                          -ManifestPath $manifestPath `
                                          -NugetApiKey $NuGetApiKey `
                                          -Destination $DestinationLocation `
                                          -Repository $Repository `
                                          -NugetPackageRoot $tempModulePath `
                                          -FormatVersion $FormatVersion `
                                          -ReleaseNotes $($ReleaseNotes -join "`n") `
                                          -Tags $Tags `
                                          -LicenseUri $LicenseUri `
                                          -IconUri $IconUri `
                                          -ProjectUri $ProjectUri `
                                          -Verbose:$VerbosePreference `
                                          -WarningAction $WarningPreference `
                                          -ErrorAction $ErrorActionPreference `
                                          -Debug:$DebugPreference
            }
        }
        finally
        {
            Microsoft.PowerShell.Management\Remove-Item $tempModulePath -Force -Recurse -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
        }
    }
}

function Find-Module
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(HelpUri='http://go.microsoft.com/fwlink/?LinkID=398574')]
    [outputtype("PSCustomObject[]")]
    Param
    (
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Name,

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Alias("Version")]
        [Version]
        $MinimumVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $MaximumVersion,
        
        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $RequiredVersion,

        [Parameter()]
        [switch]
        $AllVersions,

        [Parameter()]
        [switch]
        $IncludeDependencies,

        [Parameter()]
        [ValidateNotNull()]
        [string]
        $Filter,
        
        [Parameter()]
        [ValidateNotNull()]
        [string[]]
        $Tag,

        [Parameter()]
        [ValidateNotNull()]
        [ValidateSet("DscResource","Cmdlet","Function")]
        [string[]]
        $Includes,

        [Parameter()]
        [ValidateNotNull()]
        [string[]]
        $DscResource,

        [Parameter()]
        [ValidateNotNull()]
        [string[]]
        $Command,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Repository
    )

    Begin
    {
        Get-PSGalleryApiAvailability -Repository $Repository
        
        Install-NuGetClientBinaries
    }

    Process
    {
        $ValidationResult = Validate-VersionParameters -CallerPSCmdlet $PSCmdlet `
                                                       -Name $Name `
                                                       -MinimumVersion $MinimumVersion `
                                                       -MaximumVersion $MaximumVersion `
                                                       -RequiredVersion $RequiredVersion

        if(-not $ValidationResult)
        {
            # Validate-VersionParameters throws the error. 
            # returning to avoid further execution when different values are specified for -ErrorAction parameter
            return
        }

        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters[$script:PSArtifactType] = $script:PSArtifactTypeModule
                
        if($PSBoundParameters.ContainsKey("Repository"))
        {
            $PSBoundParameters["Source"] = $Repository
            $null = $PSBoundParameters.Remove("Repository")
            
            $ev = $null
            $null = Get-PSRepository -Name $Repository -ErrorVariable ev
            if($ev) { return }
        }
        
        $PSBoundParameters["MessageResolver"] = $script:PackageManagementMessageResolverScriptBlock

        $modulesFoundInPSGallery = @()

        # No Telemetry must be performed if PSGallery is not in the supplied list of Repositories
        $isRepositoryNullOrPSGallerySpecified = $false
        if ((-not $Repository) -or ($Repository -and ($Repository -Contains $Script:PSGalleryModuleSource)))        
        {
            $isRepositoryNullOrPSGallerySpecified = $true
        }

        PackageManagement\Find-Package @PSBoundParameters | Microsoft.PowerShell.Core\ForEach-Object {

                                                        $psgetItemInfo = New-PSGetItemInfo -SoftwareIdenties $_ -Type $script:PSArtifactTypeModule 
                                                        
                                                        $psgetItemInfo

                                                        if ($psgetItemInfo -and 
                                                            $isRepositoryNullOrPSGallerySpecified -and 
                                                            $script:TelemetryEnabled -and 
                                                            ($psgetItemInfo.Repository -eq $Script:PSGalleryModuleSource))
                                                        { 
                                                            $modulesFoundInPSGallery += $psgetItemInfo.Name 
                                                        }
                                                 }

        # Perform Telemetry if Repository is not supplied or Repository contains PSGallery
        # We are only interested in finding modules not in PSGallery
        if ($isRepositoryNullOrPSGallerySpecified)
        {
            Log-ArtifactNotFoundInPSGallery -SearchedName $Name -FoundName $modulesFoundInPSGallery -operationName 'PSGET_FIND_MODULE'
        }
    }
}

function Save-Module
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(DefaultParameterSetName='NameAndPathParameterSet',
                   HelpUri='http://go.microsoft.com/fwlink/?LinkId=531351',
                   SupportsShouldProcess=$true)]
    Param
    (
        [Parameter(Mandatory=$true, 
                   ValueFromPipelineByPropertyName=$true,
                   Position=0,
                   ParameterSetName='NameAndPathParameterSet')]
        [Parameter(Mandatory=$true, 
                   ValueFromPipelineByPropertyName=$true,
                   Position=0,
                   ParameterSetName='NameAndLiteralPathParameterSet')]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Name,

        [Parameter(Mandatory=$true, 
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0,
                   ParameterSetName='InputOjectAndPathParameterSet')]
        [Parameter(Mandatory=$true, 
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0,
                   ParameterSetName='InputOjectAndLiteralPathParameterSet')]
        [ValidateNotNull()]
        [PSCustomObject[]]
        $InputObject,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndPathParameterSet')]
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndLiteralPathParameterSet')]
        [Alias("Version")]
        [ValidateNotNull()]
        [Version]
        $MinimumVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndPathParameterSet')]
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndLiteralPathParameterSet')]
        [ValidateNotNull()]
        [Version]
        $MaximumVersion,
        
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndPathParameterSet')]
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndLiteralPathParameterSet')]
        [ValidateNotNull()]
        [Version]
        $RequiredVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndPathParameterSet')]
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndLiteralPathParameterSet')]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Repository,

        [Parameter(Mandatory=$true, ParameterSetName='NameAndPathParameterSet')]
        [Parameter(Mandatory=$true, ParameterSetName='InputOjectAndPathParameterSet')]
        [string]
        $Path,

        [Parameter(Mandatory=$true, ParameterSetName='NameAndLiteralPathParameterSet')]
        [Parameter(Mandatory=$true, ParameterSetName='InputOjectAndLiteralPathParameterSet')]
        [string]
        $LiteralPath,

        [Parameter()]
        [switch]
        $Force
    )

    Begin
    {
        Get-PSGalleryApiAvailability -Repository $Repository
                
        Install-NuGetClientBinaries

        # Module names already tried in the current pipeline for InputObject parameterset
        $moduleNamesInPipeline = @()
    }

    Process
    {
        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters["MessageResolver"] = $script:PackageManagementSaveModuleMessageResolverScriptBlock
        $PSBoundParameters[$script:PSArtifactType] = $script:PSArtifactTypeModule

        if($Path)
        {
            $null = $PSBoundParameters.Remove("Path")
            $destinationPath = Resolve-PathHelper -Path $Path -CallerPSCmdlet $PSCmdlet | Microsoft.PowerShell.Utility\Select-Object -First 1

            if(-not $destinationPath -or -not (Microsoft.PowerShell.Management\Test-path $destinationPath))
            {
                $errorMessage = ($LocalizedData.PathNotFound -f $Path)
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage $errorMessage `
                            -ErrorId "PathNotFound" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ExceptionObject $Path `
                            -ErrorCategory InvalidArgument
            }
        }
        else
        {
            $null = $PSBoundParameters.Remove("LiteralPath")
            $destinationPath = Resolve-PathHelper -Path $LiteralPath -IsLiteralPath -CallerPSCmdlet $PSCmdlet | Microsoft.PowerShell.Utility\Select-Object -First 1

            if(-not $destinationPath -or -not (Microsoft.PowerShell.Management\Test-Path -LiteralPath $destinationPath))
            {
                $errorMessage = ($LocalizedData.PathNotFound -f $LiteralPath)
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage $errorMessage `
                            -ErrorId "PathNotFound" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ExceptionObject $LiteralPath `
                            -ErrorCategory InvalidArgument
            }
        }

        $PSBoundParameters["DestinationPath"] = $destinationPath

        if($Name)
        {
            $ValidationResult = Validate-VersionParameters -CallerPSCmdlet $PSCmdlet `
                                                           -Name $Name `
                                                           -MinimumVersion $MinimumVersion `
                                                           -MaximumVersion $MaximumVersion `
                                                           -RequiredVersion $RequiredVersion

            if(-not $ValidationResult)
            {
                # Validate-VersionParameters throws the error. 
                # returning to avoid further execution when different values are specified for -ErrorAction parameter
                return
            }

            if($PSBoundParameters.ContainsKey("Repository"))
            {
                $PSBoundParameters["Source"] = $Repository
                $null = $PSBoundParameters.Remove("Repository")

                $ev = $null
                $null = Get-PSRepository -Name $Repository -ErrorVariable ev
                if($ev) { return }
            }

            if($PSBoundParameters.ContainsKey("Version"))
            {
                $null = $PSBoundParameters.Remove("Version")
                $PSBoundParameters["MinimumVersion"] = $MinimumVersion
            }

            $null = PackageManagement\Install-Package @PSBoundParameters
        }
        elseif($InputObject)
        {
            $null = $PSBoundParameters.Remove("InputObject")

            foreach($inputValue in $InputObject)
            {
                if (($inputValue.PSTypeNames -notcontains "Microsoft.PowerShell.Commands.PSRepositoryItemInfo") -and
                    ($inputValue.PSTypeNames -notcontains "Deserialized.Microsoft.PowerShell.Commands.PSRepositoryItemInfo") -and
                    ($inputValue.PSTypeNames -notcontains "Microsoft.PowerShell.Commands.PSGetDscResourceInfo") -and
                    ($inputValue.PSTypeNames -notcontains "Deserialized.Microsoft.PowerShell.Commands.PSGetDscResourceInfo"))
                {
                    ThrowError -ExceptionName "System.ArgumentException" `
                                -ExceptionMessage $LocalizedData.InvalidInputObjectValue `
                                -ErrorId "InvalidInputObjectValue" `
                                -CallerPSCmdlet $PSCmdlet `
                                -ErrorCategory InvalidArgument `
                                -ExceptionObject $inputValue
                }
                
                if( ($inputValue.PSTypeNames -contains "Microsoft.PowerShell.Commands.PSGetDscResourceInfo") -or
                    ($inputValue.PSTypeNames -contains "Deserialized.Microsoft.PowerShell.Commands.PSGetDscResourceInfo"))
                {
                    $psgetModuleInfo = $inputValue.PSGetModuleInfo
                }
                else
                {
                    $psgetModuleInfo = $inputValue                    
                }

                # Skip the module name if it is already tried in the current pipeline
                if($moduleNamesInPipeline -contains $psgetModuleInfo.Name)
                {
                    continue
                }

                $moduleNamesInPipeline += $psgetModuleInfo.Name

                if ($psgetModuleInfo.PowerShellGetFormatVersion -and 
                    ($script:SupportedPSGetFormatVersionMajors -notcontains $psgetModuleInfo.PowerShellGetFormatVersion.Major))
                {
                    $message = $LocalizedData.NotSupportedPowerShellGetFormatVersion -f ($psgetModuleInfo.Name, $psgetModuleInfo.PowerShellGetFormatVersion, $psgetModuleInfo.Name)
                    Write-Error -Message $message -ErrorId "NotSupportedPowerShellGetFormatVersion" -Category InvalidOperation
                    continue
                }

                $PSBoundParameters["Name"] = $psgetModuleInfo.Name
                $PSBoundParameters["RequiredVersion"] = $psgetModuleInfo.Version
                $PSBoundParameters["Location"] = $psgetModuleInfo.RepositorySourceLocation
                $PSBoundParameters["PackageManagementProvider"] = (Get-ProviderName -PSCustomObject $psgetModuleInfo)

                $null = PackageManagement\Install-Package @PSBoundParameters
            }
        }
    }
}

function Install-Module
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(DefaultParameterSetName='NameParameterSet',
                   HelpUri='http://go.microsoft.com/fwlink/?LinkID=398573',
                   SupportsShouldProcess=$true)]
    Param
    (
        [Parameter(Mandatory=$true, 
                   ValueFromPipelineByPropertyName=$true,
                   Position=0,
                   ParameterSetName='NameParameterSet')]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Name,

        [Parameter(Mandatory=$true, 
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0,
                   ParameterSetName='InputObject')]
        [ValidateNotNull()]
        [PSCustomObject[]]
        $InputObject,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameParameterSet')]
        [Alias("Version")]
        [ValidateNotNull()]
        [Version]
        $MinimumVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameParameterSet')]
        [ValidateNotNull()]
        [Version]
        $MaximumVersion,
        
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameParameterSet')]
        [ValidateNotNull()]
        [Version]
        $RequiredVersion,

        [Parameter(ParameterSetName='NameParameterSet')]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Repository,

        [Parameter()] 
        [ValidateSet("CurrentUser","AllUsers")]
        [string]
        $Scope = "AllUsers",

        [Parameter()]
        [switch]
        $Force
    )

    Begin
    {
        Get-PSGalleryApiAvailability -Repository $Repository
        
        if(-not (Test-RunningAsElevated) -and ($Scope -ne "CurrentUser"))
        {
            # Throw an error when Install-Module is used as a non-admin user and '-Scope CurrentUser' is not specified
            $message = $LocalizedData.InstallModuleNeedsCurrentUserScopeParameterForNonAdminUser -f @($script:programFilesModulesPath, $script:MyDocumentsModulesPath)

            ThrowError -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $message `
                        -ErrorId "InstallModuleNeedsCurrentUserScopeParameterForNonAdminUser" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ErrorCategory InvalidArgument
        }

        Install-NuGetClientBinaries

        # Module names already tried in the current pipeline for InputObject parameterset
        $moduleNamesInPipeline = @()
        $YesToAll = $false
        $NoToAll = $false
        $SourceSGrantedTrust = @()
        $SourcesDeniedTrust = @()
    }

    Process
    {
        $RepositoryIsNotTrusted = $LocalizedData.RepositoryIsNotTrusted
        $QueryInstallUntrustedPackage = $LocalizedData.QueryInstallUntrustedPackage
        $PackageTarget = $LocalizedData.InstallModulewhatIfMessage
        	
        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters["MessageResolver"] = $script:PackageManagementInstallModuleMessageResolverScriptBlock
        $PSBoundParameters[$script:PSArtifactType] = $script:PSArtifactTypeModule
        $PSBoundParameters['Scope'] = $Scope

        if($PSCmdlet.ParameterSetName -eq "NameParameterSet")
        {
            $ValidationResult = Validate-VersionParameters -CallerPSCmdlet $PSCmdlet `
                                                           -Name $Name `
                                                           -MinimumVersion $MinimumVersion `
                                                           -MaximumVersion $MaximumVersion `
                                                           -RequiredVersion $RequiredVersion

            if(-not $ValidationResult)
            {
                # Validate-VersionParameters throws the error. 
                # returning to avoid further execution when different values are specified for -ErrorAction parameter
                return
            }

            if($PSBoundParameters.ContainsKey("Repository"))
            {
                $PSBoundParameters["Source"] = $Repository
                $null = $PSBoundParameters.Remove("Repository")

                $ev = $null
                $null = Get-PSRepository -Name $Repository -ErrorVariable ev
                if($ev) { return }
            }

            if($PSBoundParameters.ContainsKey("Version"))
            {
                $null = $PSBoundParameters.Remove("Version")
                $PSBoundParameters["MinimumVersion"] = $MinimumVersion
            }

            $null = PackageManagement\Install-Package @PSBoundParameters
        }
        elseif($PSCmdlet.ParameterSetName -eq "InputObject")
        {
            $null = $PSBoundParameters.Remove("InputObject")

            foreach($inputValue in $InputObject)
            {

                if (($inputValue.PSTypeNames -notcontains "Microsoft.PowerShell.Commands.PSRepositoryItemInfo") -and
                    ($inputValue.PSTypeNames -notcontains "Deserialized.Microsoft.PowerShell.Commands.PSRepositoryItemInfo") -and
                    ($inputValue.PSTypeNames -notcontains "Microsoft.PowerShell.Commands.PSGetDscResourceInfo") -and
                    ($inputValue.PSTypeNames -notcontains "Deserialized.Microsoft.PowerShell.Commands.PSGetDscResourceInfo"))
                {
                    ThrowError -ExceptionName "System.ArgumentException" `
                                -ExceptionMessage $LocalizedData.InvalidInputObjectValue `
                                -ErrorId "InvalidInputObjectValue" `
                                -CallerPSCmdlet $PSCmdlet `
                                -ErrorCategory InvalidArgument `
                                -ExceptionObject $inputValue
                }
                
                if( ($inputValue.PSTypeNames -contains "Microsoft.PowerShell.Commands.PSGetDscResourceInfo") -or
                    ($inputValue.PSTypeNames -contains "Deserialized.Microsoft.PowerShell.Commands.PSGetDscResourceInfo"))
                {
                    $psgetModuleInfo = $inputValue.PSGetModuleInfo
                }
                else
                {
                    $psgetModuleInfo = $inputValue                    
                }

                # Skip the module name if it is already tried in the current pipeline
                if($moduleNamesInPipeline -contains $psgetModuleInfo.Name)
                {
                    continue
                }

                $moduleNamesInPipeline += $psgetModuleInfo.Name

                if ($psgetModuleInfo.PowerShellGetFormatVersion -and 
                    ($script:SupportedPSGetFormatVersionMajors -notcontains $psgetModuleInfo.PowerShellGetFormatVersion.Major))
                {
                    $message = $LocalizedData.NotSupportedPowerShellGetFormatVersion -f ($psgetModuleInfo.Name, $psgetModuleInfo.PowerShellGetFormatVersion, $psgetModuleInfo.Name)
                    Write-Error -Message $message -ErrorId "NotSupportedPowerShellGetFormatVersion" -Category InvalidOperation
                    continue
                }

                $PSBoundParameters["Name"] = $psgetModuleInfo.Name
                $PSBoundParameters["RequiredVersion"] = $psgetModuleInfo.Version
                $PSBoundParameters["Location"] = $psgetModuleInfo.RepositorySourceLocation
                $PSBoundParameters["PackageManagementProvider"] = (Get-ProviderName -PSCustomObject $psgetModuleInfo)

                #Check if module is already installed
                $InstalledModuleInfo = Test-ModuleInstalled -Name $psgetModuleInfo.Name -RequiredVersion  $psgetModuleInfo.Version                 
                if(-not $Force -and $InstalledModuleInfo -ne $null)
                {
                    $message = $LocalizedData.ModuleAlreadyInstalledVerbose -f ($InstalledModuleInfo.Version, $InstalledModuleInfo.Name, $InstalledModuleInfo.ModuleBase)
                    Write-Verbose -Message $message
                }
                else
                {
                    $source =  $psgetModuleInfo.Repository
                    $installationPolicy = (Get-PSRepository -Name $source).InstallationPolicy                
                    $ShouldProcessMessage = $PackageTarget -f ($psgetModuleInfo.Name, $psgetModuleInfo.Version)
                
                    if($psCmdlet.ShouldProcess($ShouldProcessMessage))
                    {
                        if($installationPolicy.Equals("Untrusted", [StringComparison]::OrdinalIgnoreCase))
                        {
	                    if(-not($YesToAll -or $NoToAll -or $SourceSGrantedTrust.Contains($source) -or $sourcesDeniedTrust.Contains($source) -or $Force))   
                            {
	                        $message = $QueryInstallUntrustedPackage -f ($psgetModuleInfo.Name, $psgetModuleInfo.RepositorySourceLocation)
                                if($PSVersionTable.PSVersion -ge [Version]"5.0")
                                {
                                     $sourceTrusted = $psCmdlet.ShouldContinue("$message", "$RepositoryIsNotTrusted",$true, [ref]$YesToAll, [ref]$NoToAll)
                                }
                                else
                                {
                                    $sourceTrusted = $psCmdlet.ShouldContinue("$message", "$RepositoryIsNotTrusted", [ref]$YesToAll, [ref]$NoToAll)
                                }                               

                                if($sourceTrusted)
                                {
                                    $SourceSGrantedTrust+=$source
                                }
                                else
                                {
                                    $SourcesDeniedTrust+=$source
                                }
                            }
                        }
                        if($installationPolicy.Equals("trusted", [StringComparison]::OrdinalIgnoreCase) -or $SourceSGrantedTrust.Contains($source) -or $YesToAll -or $Force)
                        {
                            $PSBoundParameters["Force"] = $true                        
	                        $null = PackageManagement\Install-Package @PSBoundParameters
                        }                                  
                    }
                }
            }
        }
    }
}

function Update-Module
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(SupportsShouldProcess=$true,
                   HelpUri='http://go.microsoft.com/fwlink/?LinkID=398576')]
    Param
    (
        [Parameter(ValueFromPipelineByPropertyName=$true, 
                   Position=0)]
        [ValidateNotNullOrEmpty()]
        [String[]]
        $Name, 

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $RequiredVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $MaximumVersion,

        [Parameter()]
        [Switch]
        $Force
    )

    Begin
    {
        Install-NuGetClientBinaries

        # Module names already tried in the current pipeline
        $moduleNamesInPipeline = @()
    }

    Process
    {
        $moduleBasesToUpdate = @()

        $ValidationResult = Validate-VersionParameters -CallerPSCmdlet $PSCmdlet `
                                                       -Name $Name `
                                                       -MaximumVersion $MaximumVersion `
                                                       -RequiredVersion $RequiredVersion

        if(-not $ValidationResult)
        {
            # Validate-VersionParameters throws the error. 
            # returning to avoid further execution when different values are specified for -ErrorAction parameter
            return
        }

        if($Name)
        {
            if(($Name.Count -eq 1) -and ($Name -eq $script:PSGetModuleName))
            {
                Update-PowerShellGetModule -CallerPSCmdlet $PSCmdlet
                return
            }

            foreach($moduleName in $Name)
            {
                $availableModules = Get-Module -ListAvailable $moduleName -Verbose:$false | Microsoft.PowerShell.Utility\Select-Object -Unique
        
                if(-not $availableModules -and -not (Test-WildcardPattern -Name $moduleName))
                {                    
                    $message = $LocalizedData.ModuleNotInstalledOnThisMachine -f ($moduleName)
                    Write-Error -Message $message -ErrorId "ModuleNotInstalledOnThisMachine" -Category InvalidOperation -TargetObject $moduleName
                    continue
                }

                foreach($mod in $availableModules)
                {
                    # Check if this module got installed with PSGet and user has required permissions
                    $PSGetItemInfoPath = Microsoft.PowerShell.Management\Join-Path $mod.ModuleBase $script:PSGetItemInfoFileName
                    if (Microsoft.PowerShell.Management\Test-path $PSGetItemInfoPath)
                    {
                        if(-not (Test-RunningAsElevated) -and $mod.ModuleBase.StartsWith($script:programFilesModulesPath, [System.StringComparison]::OrdinalIgnoreCase))
                        {                            
                            if(-not (Test-WildcardPattern -Name $moduleName))
                            {
                                $message = $LocalizedData.AdminPrivilegesRequiredForUpdate -f ($mod.Name, $mod.ModuleBase)
                                Write-Error -Message $message -ErrorId "AdminPrivilegesAreRequiredForUpdate" -Category InvalidOperation -TargetObject $moduleName
                            }
                            continue
                        }

                        $moduleBasesToUpdate += $mod.ModuleBase
                    }
                    else
                    {
                        if(-not (Test-WildcardPattern -Name $moduleName))
                        {
                            $message = $LocalizedData.ModuleNotInstalledUsingPowerShellGet -f ($mod.Name)
                            Write-Error -Message $message -ErrorId "ModuleNotInstalledUsingInstallModuleCmdlet" -Category InvalidOperation -TargetObject $moduleName
                        }
                        continue
                    }
                }
            }
        }
        else
        {            
            $modulePaths = @()
            $modulePaths += $script:MyDocumentsModulesPath

            if((Test-RunningAsElevated))
            {
                $modulePaths += $script:programFilesModulesPath
            }
        
            foreach ($location in $modulePaths)
            {
                # find all modules installed using PSGet
                $moduleBases = Microsoft.PowerShell.Management\Get-ChildItem $location -Recurse `
                                                                             -Attributes Hidden -Filter $script:PSGetItemInfoFileName `
                                                                             -ErrorAction SilentlyContinue `
                                                                             -WarningAction SilentlyContinue `
                                                                             | Microsoft.PowerShell.Core\Foreach-Object { $_.Directory }
                foreach ($moduleBase in $moduleBases)
                {
                    $PSGetItemInfoPath = Microsoft.PowerShell.Management\Join-Path $moduleBase.FullName $script:PSGetItemInfoFileName

                    # Check if this module got installed using PSGet, read its contents and compare with current version
                    if (Microsoft.PowerShell.Management\Test-Path $PSGetItemInfoPath)
                    {
                        $moduleBasesToUpdate += $moduleBase
                    }
                }
            }
        }

        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters[$script:PSArtifactType] = $script:PSArtifactTypeModule

        foreach($moduleBase in $moduleBasesToUpdate)
        {
            $PSGetItemInfoPath = Microsoft.PowerShell.Management\Join-Path $moduleBase $script:PSGetItemInfoFileName

            $psgetItemInfo = DeSerialize-PSObject -Path $PSGetItemInfoPath
            
            # Skip the module name if it is already tried in the current pipeline
            if($moduleNamesInPipeline -contains $psgetItemInfo.Name)
            {
                continue
            }

            $moduleNamesInPipeline += $psgetItemInfo.Name

            $message = $LocalizedData.CheckingForModuleUpdate -f ($psgetItemInfo.Name)
            Write-Verbose -Message $message

            $providerName = Get-ProviderName -PSCustomObject $psgetItemInfo
            if(-not $providerName)
            {
                $providerName = $script:NuGetProviderName
            }

            $PSBoundParameters["Name"] = $psgetItemInfo.Name
            $PSBoundParameters["Location"] = $psgetItemInfo.RepositorySourceLocation

            Get-PSGalleryApiAvailability -Repository (Get-SourceName -Location $psgetItemInfo.RepositorySourceLocation)

            $PSBoundParameters["PackageManagementProvider"] = $providerName 
            $PSBoundParameters["InstallUpdate"] = $true

            if($moduleBase.ToString().StartsWith($script:MyDocumentsModulesPath, [System.StringComparison]::OrdinalIgnoreCase))
            {
                $PSBoundParameters["Scope"] = "CurrentUser"
            }

            $PSBoundParameters["MessageResolver"] = $script:PackageManagementUpdateModuleMessageResolverScriptBlock
            $sid = PackageManagement\Install-Package @PSBoundParameters
        }
    }
}

function Uninstall-Module
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(DefaultParameterSetName='NameParameterSet',
                   SupportsShouldProcess=$true,
                   HelpUri='http://go.microsoft.com/fwlink/?LinkId=526864')]
    Param
    (
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   Mandatory=$true, 
                   Position=0,
                   ParameterSetName='NameParameterSet')]
        [ValidateNotNullOrEmpty()]
        [String[]]
        $Name, 

        [Parameter(Mandatory=$true, 
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0,
                   ParameterSetName='InputObject')]
        [ValidateNotNull()]
        [PSCustomObject[]]
        $InputObject,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameParameterSet')]
        [ValidateNotNull()]
        [Version]
        $MinimumVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameParameterSet')]
        [ValidateNotNull()]
        [Version]
        $RequiredVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameParameterSet')]
        [ValidateNotNull()]
        [Version]
        $MaximumVersion,

        [Parameter()]
        [Switch]
        $Force
    )

    Process
    {
        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters["MessageResolver"] = $script:PackageManagementUnInstallModuleMessageResolverScriptBlock 
        $PSBoundParameters[$script:PSArtifactType] = $script:PSArtifactTypeModule

        if($PSCmdlet.ParameterSetName -eq "InputObject")
        {
            $null = $PSBoundParameters.Remove("InputObject")
        
            foreach($inputValue in $InputObject)
            {
                if (($inputValue.PSTypeNames -notcontains "Microsoft.PowerShell.Commands.PSRepositoryItemInfo") -and
                    ($inputValue.PSTypeNames -notcontains "Deserialized.Microsoft.PowerShell.Commands.PSRepositoryItemInfo"))
                {
                    ThrowError -ExceptionName "System.ArgumentException" `
                                -ExceptionMessage $LocalizedData.InvalidInputObjectValue `
                                -ErrorId "InvalidInputObjectValue" `
                                -CallerPSCmdlet $PSCmdlet `
                                -ErrorCategory InvalidArgument `
                                -ExceptionObject $inputValue
                }

                $PSBoundParameters["Name"] = $inputValue.Name
                $PSBoundParameters["RequiredVersion"] = $inputValue.Version

                $null = PackageManagement\Uninstall-Package @PSBoundParameters
            }
        }
        else
        {
            $ValidationResult = Validate-VersionParameters -CallerPSCmdlet $PSCmdlet `
                                                           -Name $Name `
                                                           -MinimumVersion $MinimumVersion `
                                                           -MaximumVersion $MaximumVersion `
                                                           -RequiredVersion $RequiredVersion

            if(-not $ValidationResult)
            {
                # Validate-VersionParameters throws the error. 
                # returning to avoid further execution when different values are specified for -ErrorAction parameter
                return
            }

            $null = PackageManagement\Uninstall-Package @PSBoundParameters
        }
    }
}

function Get-InstalledModule
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(HelpUri='http://go.microsoft.com/fwlink/?LinkId=526863')]
    Param
    (
        [Parameter(ValueFromPipelineByPropertyName=$true, 
                   Position=0)]
        [ValidateNotNullOrEmpty()]
        [String[]]
        $Name, 

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $MinimumVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $RequiredVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $MaximumVersion
    )

    Process
    {
        $ValidationResult = Validate-VersionParameters -CallerPSCmdlet $PSCmdlet `
                                                       -Name $Name `
                                                       -MinimumVersion $MinimumVersion `
                                                       -MaximumVersion $MaximumVersion `
                                                       -RequiredVersion $RequiredVersion

        if(-not $ValidationResult)
        {
            # Validate-VersionParameters throws the error. 
            # returning to avoid further execution when different values are specified for -ErrorAction parameter
            return
        }

        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters["MessageResolver"] = $script:PackageManagementMessageResolverScriptBlock
        $PSBoundParameters[$script:PSArtifactType] = $script:PSArtifactTypeModule

        PackageManagement\Get-Package @PSBoundParameters | Microsoft.PowerShell.Core\ForEach-Object {New-PSGetItemInfo -SoftwareIdenties $_ -Type $script:PSArtifactTypeModule}  
    }
}

#endregion *-Module cmdlets

#region *-PSRepositoryItem cmdlets

function Find-PSRepositoryItem
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(HelpUri='http://go.microsoft.com/fwlink/?LinkId=619783')]
    [outputtype("PSCustomObject[]")]
    Param
    (
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Name,

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Alias("Version")]
        [Version]
        $MinimumVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $MaximumVersion,
        
        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $RequiredVersion,

        [Parameter()]
        [switch]
        $AllVersions,

        [Parameter()]
        [switch]
        $IncludeDependencies,

        [Parameter()]
        [ValidateNotNull()]
        [string]
        $Filter,
        
        [Parameter()]
        [ValidateNotNull()]
        [string[]]
        $Tag,

        [Parameter()]
        [ValidateNotNull()]
        [ValidateSet("DscResource","Function","Workflow")]
        [string[]]
        $Includes,

        [Parameter()]
        [ValidateNotNull()]
        [string[]]
        $DscResource,

        [Parameter()]
        [ValidateNotNull()]
        [string[]]
        $Command,

        [Parameter()]
        [ValidateNotNull()]
        [ValidateSet('Module','Script','All')]
        [string[]]
        $Type = 'All',

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Repository
    )

    Begin
    {
        Get-PSGalleryApiAvailability -Repository $Repository
        
        Install-NuGetClientBinaries
    }

    Process
    {
        $ValidationResult = Validate-VersionParameters -CallerPSCmdlet $PSCmdlet `
                                                       -Name $Name `
                                                       -MinimumVersion $MinimumVersion `
                                                       -MaximumVersion $MaximumVersion `
                                                       -RequiredVersion $RequiredVersion

        if(-not $ValidationResult)
        {
            # Validate-VersionParameters throws the error. 
            # returning to avoid further execution when different values are specified for -ErrorAction parameter
            return
        }

        $null = $PSBoundParameters.Remove($script:PSArtifactType)
        
        if(($Type -contains $script:PSArtifactTypeModule) -or ($Type -contains $script:All))
        {
            Find-Module @PSBoundParameters -ErrorAction SilentlyContinue
        }

        if(($Type -contains $script:PSArtifactTypeScript) -or ($Type -contains $script:All))
        {
            Find-Script @PSBoundParameters -ErrorAction SilentlyContinue
        }
    }
}

#endregion *-PSRepositoryItem cmdlets

#region *-Script cmdlets
function Publish-Script
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(SupportsShouldProcess=$true,
                   PositionalBinding=$false,
                   DefaultParameterSetName='PathParameterSet',
                   HelpUri='http://go.microsoft.com/fwlink/?LinkId=619788')]
    Param
    (
        [Parameter(Mandatory=$true, 
                   ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='PathParameterSet')]
        [ValidateNotNullOrEmpty()]
        [string]
        $Path,

        [Parameter(Mandatory=$true, 
                   ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='LiteralPathParameterSet')]
        [ValidateNotNullOrEmpty()]
        [string]
        $LiteralPath,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $NuGetApiKey,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $Repository = $Script:PSGalleryModuleSource
    )

    Begin
    {
        if($script:isNanoServer) {
            $message = $LocalizedData.PublishPSArtifactUnsupportedOnNano -f "Script"
            ThrowError -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $message `
                        -ErrorId "UnsupportedOperation" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ExceptionObject $PSCmdlet `
                        -ErrorCategory InvalidOperation
        }

        Get-PSGalleryApiAvailability -Repository $Repository        

        Install-NuGetClientBinaries
    }

    Process
    {
        $scriptFilePath = $null
        if($Path)
        {
            $scriptFilePath = Resolve-PathHelper -Path $Path -CallerPSCmdlet $PSCmdlet | 
                                  Microsoft.PowerShell.Utility\Select-Object -First 1
            
            if(-not $scriptFilePath -or 
               -not (Microsoft.PowerShell.Management\Test-Path -Path $scriptFilePath -PathType Leaf))
            {
                $errorMessage = ($LocalizedData.PathNotFound -f $Path)
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage $errorMessage `
                            -ErrorId "PathNotFound" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ExceptionObject $Path `
                            -ErrorCategory InvalidArgument
            }
        }
        else
        {
            $scriptFilePath = Resolve-PathHelper -Path $LiteralPath -IsLiteralPath -CallerPSCmdlet $PSCmdlet | 
                                  Microsoft.PowerShell.Utility\Select-Object -First 1

            if(-not $scriptFilePath -or 
               -not (Microsoft.PowerShell.Management\Test-Path -LiteralPath $scriptFilePath -PathType Leaf))
            {
                $errorMessage = ($LocalizedData.PathNotFound -f $LiteralPath)
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage $errorMessage `
                            -ErrorId "PathNotFound" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ExceptionObject $LiteralPath `
                            -ErrorCategory InvalidArgument
            }
        }

        if(-not $scriptFilePath.EndsWith('.ps1', [System.StringComparison]::OrdinalIgnoreCase))
        {
            $errorMessage = ($LocalizedData.InvalidScriptFilePath -f $scriptFilePath)
            ThrowError  -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $errorMessage `
                        -ErrorId "InvalidScriptFilePath" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ExceptionObject $scriptFilePath `
                        -ErrorCategory InvalidArgument
            return
        }

        $ev = $null
        $repo = Get-PSRepository -Name $Repository -ErrorVariable ev
        if($ev) { return }

        $DestinationLocation = $repo.ScriptPublishLocation
                
        if(-not $DestinationLocation -or
           (-not (Microsoft.PowerShell.Management\Test-Path -Path $DestinationLocation) -and 
           -not (Test-WebUri -uri $DestinationLocation)))

        {
            $message = $LocalizedData.PSRepositoryScriptPublishLocationIsMissing -f ($Repository, $Repository)
            ThrowError -ExceptionName "System.ArgumentException" `
                       -ExceptionMessage $message `
                       -ErrorId "PSRepositoryScriptPublishLocationIsMissing" `
                       -CallerPSCmdlet $PSCmdlet `
                       -ErrorCategory InvalidArgument `
                       -ExceptionObject $Repository
        }

        if(-not $NuGetApiKey)
        {
            if(Microsoft.PowerShell.Management\Test-Path -Path $DestinationLocation)
            {
                $NuGetApiKey = "$(Get-Random)"
            }
            else
            {
                $message = $LocalizedData.NuGetApiKeyIsRequiredForNuGetBasedGalleryService -f ($Repository, $DestinationLocation)
                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage $message `
                           -ErrorId "NuGetApiKeyIsRequiredForNuGetBasedGalleryService" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument
            }
        }

        $providerName = Get-ProviderName -PSCustomObject $repo
        if($providerName -ne $script:NuGetProviderName)
        {
            $message = $LocalizedData.PublishScriptSupportsOnlyNuGetBasedPublishLocations -f ($DestinationLocation, $Repository, $Repository)
            ThrowError -ExceptionName "System.ArgumentException" `
                       -ExceptionMessage $message `
                       -ErrorId "PublishScriptSupportsOnlyNuGetBasedPublishLocations" `
                       -CallerPSCmdlet $PSCmdlet `
                       -ErrorCategory InvalidArgument `
                       -ExceptionObject $Repository
        }

        if($Path)
        {
            $PSScriptInfo = Test-ScriptFile -Path $scriptFilePath
        }
        else
        {
            $PSScriptInfo = Test-ScriptFile -LiteralPath $scriptFilePath
        }
       
        if(-not $PSScriptInfo)
        {
            # Test-ScriptFile throws the actual error
            return
        }

        $scriptName = $PSScriptInfo.Name

        # Copy the source script file to temp location to publish
        $tempScriptPath = Microsoft.PowerShell.Management\Join-Path -Path $script:TempPath `
                              -ChildPath "$(Microsoft.PowerShell.Utility\Get-Random)\$scriptName"

        $null = Microsoft.PowerShell.Management\New-Item -Path $tempScriptPath -ItemType Directory -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
        if($Path)
        {
            Microsoft.PowerShell.Management\Copy-Item -Path $scriptFilePath -Destination $tempScriptPath -Force -Recurse -Confirm:$false -WhatIf:$false
        }
        else
        {
            Microsoft.PowerShell.Management\Copy-Item -LiteralPath $scriptFilePath -Destination $tempScriptPath -Force -Recurse -Confirm:$false -WhatIf:$false
        }

        try
        {
            # Check if the specified script name is already used for a module on the specified repository
            # Use Find-Module to check if that name is already used as module name
            $modulePSGetItemInfo = Find-Module -Name $scriptName `
                                               -Repository $Repository `
                                               -Tag 'PSModule' `
                                               -Verbose:$VerbosePreference `
                                               -ErrorAction SilentlyContinue `
                                               -WarningAction SilentlyContinue `
                                               -Debug:$DebugPreference | 
                                        Microsoft.PowerShell.Core\Where-Object {$_.Name -eq $scriptName} | 
                                            Microsoft.PowerShell.Utility\Select-Object -Last 1
            if($modulePSGetItemInfo)
            {
                $message = $LocalizedData.SpecifiedNameIsAlearyUsed -f ($scriptName, $Repository)
                ThrowError -ExceptionName "System.InvalidOperationException" `
                           -ExceptionMessage $message `
                           -ErrorId "SpecifiedNameIsAlearyUsed" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidOperation `
                           -ExceptionObject $scriptName
            }

            $currentPSGetItemInfo = $null
            $currentPSGetItemInfo = Find-Script -Name $scriptName `
                                                -Repository $Repository `
                                                -Verbose:$VerbosePreference `
                                                -ErrorAction SilentlyContinue `
                                                -WarningAction SilentlyContinue `
                                                -Debug:$DebugPreference | 
                                        Microsoft.PowerShell.Core\Where-Object {$_.Name -eq $scriptName} | 
                                            Microsoft.PowerShell.Utility\Select-Object -Last 1

            if($currentPSGetItemInfo -and $currentPSGetItemInfo.Version -ge $PSScriptInfo.Version)
            {
                $message = $LocalizedData.ScriptVersionShouldBeGreaterThanGalleryVersion -f ($scriptName,
                                                                                             $PSScriptInfo.Version,
                                                                                             $currentPSGetItemInfo.Version,
                                                                                             $currentPSGetItemInfo.RepositorySourceLocation)
                ThrowError -ExceptionName "System.InvalidOperationException" `
                           -ExceptionMessage $message `
                           -ErrorId "ScriptVersionShouldBeGreaterThanGalleryVersion" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidOperation
            }

            $shouldProcessMessage = $LocalizedData.PublishScriptwhatIfMessage -f ($PSScriptInfo.Version, $scriptName)
            if($PSCmdlet.ShouldProcess($shouldProcessMessage, "Publish-Script"))
            {
                Publish-PSArtifactUtility -PSScriptInfo $PSScriptInfo `
                                          -NugetApiKey $NuGetApiKey `
                                          -Destination $DestinationLocation `
                                          -Repository $Repository `
                                          -NugetPackageRoot $tempScriptPath `
                                          -Verbose:$VerbosePreference `
                                          -WarningAction $WarningPreference `
                                          -ErrorAction $ErrorActionPreference `
                                          -Debug:$DebugPreference
            }
        }
        finally
        {
            Microsoft.PowerShell.Management\Remove-Item $tempScriptPath -Force -Recurse -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
        }
    }
}

function Find-Script
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(HelpUri='http://go.microsoft.com/fwlink/?LinkId=619785')]
    [outputtype("PSCustomObject[]")]
    Param
    (
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Name,

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Alias("Version")]
        [Version]
        $MinimumVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $MaximumVersion,
        
        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $RequiredVersion,

        [Parameter()]
        [switch]
        $AllVersions,

        [Parameter()]
        [switch]
        $IncludeDependencies,

        [Parameter()]
        [ValidateNotNull()]
        [string]
        $Filter,
        
        [Parameter()]
        [ValidateNotNull()]
        [string[]]
        $Tag,

        [Parameter()]
        [ValidateNotNull()]
        [ValidateSet('Function','Workflow')]
        [string[]]
        $Includes,

        [Parameter()]
        [ValidateNotNull()]
        [string[]]
        $Command,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Repository
    )

    Begin
    {
        Get-PSGalleryApiAvailability -Repository $Repository
        
        Install-NuGetClientBinaries
    }

    Process
    {
        $ValidationResult = Validate-VersionParameters -CallerPSCmdlet $PSCmdlet `
                                                       -Name $Name `
                                                       -MinimumVersion $MinimumVersion `
                                                       -MaximumVersion $MaximumVersion `
                                                       -RequiredVersion $RequiredVersion

        if(-not $ValidationResult)
        {
            # Validate-VersionParameters throws the error. 
            # returning to avoid further execution when different values are specified for -ErrorAction parameter
            return
        }

        $PSBoundParameters['Provider'] = $script:PSModuleProviderName
        $PSBoundParameters[$script:PSArtifactType] = $script:PSArtifactTypeScript
                
        if($PSBoundParameters.ContainsKey("Repository"))
        {
            $PSBoundParameters["Source"] = $Repository
            $null = $PSBoundParameters.Remove("Repository")

            $ev = $null
            $null = Get-PSRepository -Name $Repository -ErrorVariable ev
            if($ev) { return }
        }
        
        $PSBoundParameters["MessageResolver"] = $script:PackageManagementMessageResolverScriptBlockForScriptCmdlets

        $scriptsFoundInPSGallery = @()

        # No Telemetry must be performed if PSGallery is not in the supplied list of Repositories
        $isRepositoryNullOrPSGallerySpecified = $false
        if ((-not $Repository) -or ($Repository -and ($Repository -Contains $Script:PSGalleryModuleSource)))        
        {
            $isRepositoryNullOrPSGallerySpecified = $true
        }

        PackageManagement\Find-Package @PSBoundParameters | Microsoft.PowerShell.Core\ForEach-Object {

                                                        $psgetItemInfo = New-PSGetItemInfo -SoftwareIdenties $_ -Type $script:PSArtifactTypeScript 
                                                        
                                                        $psgetItemInfo

                                                        if ($psgetItemInfo -and 
                                                            $isRepositoryNullOrPSGallerySpecified -and 
                                                            $script:TelemetryEnabled -and 
                                                            ($psgetItemInfo.Repository -eq $Script:PSGalleryModuleSource))
                                                        { 
                                                            $scriptsFoundInPSGallery += $psgetItemInfo.Name 
                                                        }
                                                 }

        # Perform Telemetry if Repository is not supplied or Repository contains PSGallery
        # We are only interested in finding artifacts not in PSGallery
        if ($isRepositoryNullOrPSGallerySpecified)
        {
            Log-ArtifactNotFoundInPSGallery -SearchedName $Name -FoundName $scriptsFoundInPSGallery -operationName PSGET_FIND_SCRIPT
        }
    }
}

function Save-Script
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(DefaultParameterSetName='NameAndPathParameterSet',
                   HelpUri='http://go.microsoft.com/fwlink/?LinkId=619786',
                   SupportsShouldProcess=$true)]
    Param
    (
        [Parameter(Mandatory=$true, 
                   ValueFromPipelineByPropertyName=$true,
                   Position=0,
                   ParameterSetName='NameAndPathParameterSet')]
        [Parameter(Mandatory=$true, 
                   ValueFromPipelineByPropertyName=$true,
                   Position=0,
                   ParameterSetName='NameAndLiteralPathParameterSet')]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Name,

        [Parameter(Mandatory=$true, 
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0,
                   ParameterSetName='InputOjectAndPathParameterSet')]
        [Parameter(Mandatory=$true, 
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0,
                   ParameterSetName='InputOjectAndLiteralPathParameterSet')]
        [ValidateNotNull()]
        [PSCustomObject[]]
        $InputObject,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndPathParameterSet')]
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndLiteralPathParameterSet')]
        [Alias("Version")]
        [ValidateNotNull()]
        [Version]
        $MinimumVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndPathParameterSet')]
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndLiteralPathParameterSet')]
        [ValidateNotNull()]
        [Version]
        $MaximumVersion,
        
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndPathParameterSet')]
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndLiteralPathParameterSet')]
        [ValidateNotNull()]
        [Version]
        $RequiredVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndPathParameterSet')]
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndLiteralPathParameterSet')]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Repository,

        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndPathParameterSet')]

        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='InputOjectAndPathParameterSet')]
        [string]
        $Path,

        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameAndLiteralPathParameterSet')]

        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='InputOjectAndLiteralPathParameterSet')]
        [string]
        $LiteralPath,

        [Parameter()]
        [switch]
        $Force
    )

    Begin
    {
        Get-PSGalleryApiAvailability -Repository $Repository
                
        Install-NuGetClientBinaries

        # Script names already tried in the current pipeline for InputObject parameterset
        $scriptNamesInPipeline = @()
    }

    Process
    {
        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters["MessageResolver"] = $script:PackageManagementSaveScriptMessageResolverScriptBlock
        $PSBoundParameters[$script:PSArtifactType] = $script:PSArtifactTypeScript

        if($Path)
        {
            $null = $PSBoundParameters.Remove("Path")
            $destinationPath = Resolve-PathHelper -Path $Path -CallerPSCmdlet $PSCmdlet | 
                                   Microsoft.PowerShell.Utility\Select-Object -First 1

            if(-not $destinationPath -or -not (Microsoft.PowerShell.Management\Test-path $destinationPath))
            {
                $errorMessage = ($LocalizedData.PathNotFound -f $Path)
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage $errorMessage `
                            -ErrorId "PathNotFound" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ExceptionObject $Path `
                            -ErrorCategory InvalidArgument
            }
        }
        else
        {
            $null = $PSBoundParameters.Remove("LiteralPath")
            $destinationPath = Resolve-PathHelper -Path $LiteralPath -IsLiteralPath -CallerPSCmdlet $PSCmdlet | 
                                   Microsoft.PowerShell.Utility\Select-Object -First 1

            if(-not $destinationPath -or -not (Microsoft.PowerShell.Management\Test-Path -LiteralPath $destinationPath))
            {
                $errorMessage = ($LocalizedData.PathNotFound -f $LiteralPath)
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage $errorMessage `
                            -ErrorId "PathNotFound" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ExceptionObject $LiteralPath `
                            -ErrorCategory InvalidArgument
            }
        }

        $PSBoundParameters["DestinationPath"] = $destinationPath

        if($Name)
        {
            $ValidationResult = Validate-VersionParameters -CallerPSCmdlet $PSCmdlet `
                                                           -Name $Name `
                                                           -MinimumVersion $MinimumVersion `
                                                           -MaximumVersion $MaximumVersion `
                                                           -RequiredVersion $RequiredVersion

            if(-not $ValidationResult)
            {
                # Validate-VersionParameters throws the error. 
                # returning to avoid further execution when different values are specified for -ErrorAction parameter
                return
            }

            if($PSBoundParameters.ContainsKey("Repository"))
            {
                $PSBoundParameters["Source"] = $Repository
                $null = $PSBoundParameters.Remove("Repository")

                $ev = $null
                $null = Get-PSRepository -Name $Repository -ErrorVariable ev
                if($ev) { return }
            }

            if($PSBoundParameters.ContainsKey("Version"))
            {
                $null = $PSBoundParameters.Remove("Version")
                $PSBoundParameters["MinimumVersion"] = $MinimumVersion
            }

            $null = PackageManagement\Install-Package @PSBoundParameters
        }
        elseif($InputObject)
        {
            $null = $PSBoundParameters.Remove("InputObject")

            foreach($inputValue in $InputObject)
            {
                if (($inputValue.PSTypeNames -notcontains "Microsoft.PowerShell.Commands.PSRepositoryItemInfo") -and
                    ($inputValue.PSTypeNames -notcontains "Deserialized.Microsoft.PowerShell.Commands.PSRepositoryItemInfo"))
                {
                    ThrowError -ExceptionName "System.ArgumentException" `
                                -ExceptionMessage $LocalizedData.InvalidInputObjectValue `
                                -ErrorId "InvalidInputObjectValue" `
                                -CallerPSCmdlet $PSCmdlet `
                                -ErrorCategory InvalidArgument `
                                -ExceptionObject $inputValue
                }
                
                $psRepositoryItemInfo = $inputValue

                # Skip the script name if it is already tried in the current pipeline
                if($scriptNamesInPipeline -contains $psRepositoryItemInfo.Name)
                {
                    continue
                }

                $scriptNamesInPipeline += $psRepositoryItemInfo.Name

                if ($psRepositoryItemInfo.PowerShellGetFormatVersion -and 
                    ($script:SupportedPSGetFormatVersionMajors -notcontains $psRepositoryItemInfo.PowerShellGetFormatVersion.Major))
                {
                    $message = $LocalizedData.NotSupportedPowerShellGetFormatVersionScripts -f ($psRepositoryItemInfo.Name, $psRepositoryItemInfo.PowerShellGetFormatVersion, $psRepositoryItemInfo.Name)
                    Write-Error -Message $message -ErrorId "NotSupportedPowerShellGetFormatVersion" -Category InvalidOperation
                    continue
                }

                $PSBoundParameters["Name"] = $psRepositoryItemInfo.Name
                $PSBoundParameters["RequiredVersion"] = $psRepositoryItemInfo.Version
                $PSBoundParameters["Location"] = $psRepositoryItemInfo.RepositorySourceLocation
                $PSBoundParameters["PackageManagementProvider"] = (Get-ProviderName -PSCustomObject $psRepositoryItemInfo)

                $null = PackageManagement\Install-Package @PSBoundParameters
            }
        }
    }
}

function Install-Script
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(DefaultParameterSetName='NameParameterSet',
                   HelpUri='http://go.microsoft.com/fwlink/?LinkId=619784',
                   SupportsShouldProcess=$true)]
    Param
    (
        [Parameter(Mandatory=$true, 
                   ValueFromPipelineByPropertyName=$true,
                   Position=0,
                   ParameterSetName='NameParameterSet')]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Name,

        [Parameter(Mandatory=$true, 
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0,
                   ParameterSetName='InputObject')]
        [ValidateNotNull()]
        [PSCustomObject[]]
        $InputObject,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameParameterSet')]
        [Alias("Version")]
        [ValidateNotNull()]
        [Version]
        $MinimumVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameParameterSet')]
        [ValidateNotNull()]
        [Version]
        $MaximumVersion,
        
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameParameterSet')]
        [ValidateNotNull()]
        [Version]
        $RequiredVersion,

        [Parameter(ParameterSetName='NameParameterSet')]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Repository,

        [Parameter()]
        [ValidateSet("CurrentUser","AllUsers")]
        [string]
        $Scope = "CurrentUser",

        [Parameter()]
        [switch]
        $Force
    )

    Begin
    {
        Get-PSGalleryApiAvailability -Repository $Repository
        
        if(-not (Test-RunningAsElevated) -and ($Scope -ne "CurrentUser"))
        {
            # Throw an error when Install-Script is used as a non-admin user and '-Scope CurrentUser' is not specified
            $AdminPreviligeErrorMessage = $LocalizedData.InstallScriptNeedsCurrentUserScopeParameterForNonAdminUser -f @($script:ProgramFilesScriptsPath, $script:MyDocumentsScriptsPath)
            $AdminPreviligeErrorId = 'InstallScriptNeedsCurrentUserScopeParameterForNonAdminUser'

            ThrowError -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $AdminPreviligeErrorMessage `
                        -ErrorId $AdminPreviligeErrorId `
                        -CallerPSCmdlet $PSCmdlet `
                        -ErrorCategory InvalidArgument
        }

        # Check and add the scope path to PATH environment variable
        if($Scope -eq 'AllUsers')
        {
            $scopePath = $script:ProgramFilesScriptsPath
            $envVariableTarget = 'Machine'
        }
        else
        {
            $scopePath = $script:MyDocumentsScriptsPath
            $envVariableTarget = 'User'
        }

        # Check and add the $scopePath to $env:Path value
        if(($env:PATH -split ';') -notcontains $scopePath)
        {
            $currentPATHValue = [Environment]::GetEnvironmentVariable('PATH', $envVariableTarget)
            if(($currentPATHValue -split ';') -notcontains $scopePath)
            {
                # To ensure that the installed script is immediately usable, 
                # we need to add the scope path to the PATH enviroment variable.
                [Environment]::SetEnvironmentVariable('PATH', "$currentPATHValue;$scopePath", $envVariableTarget)
            }

            # Check and add the $scopePath to $env:Path value for the current session 
            # so that installed scripts can be used in the current sesssion.
            $env:Path = "$env:PATH;$scopePath"
        }

        Install-NuGetClientBinaries
        
        # Script names already tried in the current pipeline for InputObject parameterset
        $scriptNamesInPipeline = @()

        $YesToAll = $false
        $NoToAll = $false
        $SourceSGrantedTrust = @()
        $SourcesDeniedTrust = @()
    }

    Process
    {
        $RepositoryIsNotTrusted = $LocalizedData.RepositoryIsNotTrusted
        $QueryInstallUntrustedPackage = $LocalizedData.QueryInstallUntrustedScriptPackage
        $PackageTarget = $LocalizedData.InstallScriptwhatIfMessage
        	
        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters["MessageResolver"] = $script:PackageManagementInstallScriptMessageResolverScriptBlock
        $PSBoundParameters[$script:PSArtifactType] = $script:PSArtifactTypeScript
        $PSBoundParameters['Scope'] = $Scope

        if($PSCmdlet.ParameterSetName -eq "NameParameterSet")
        {
            $ValidationResult = Validate-VersionParameters -CallerPSCmdlet $PSCmdlet `
                                                           -Name $Name `
                                                           -MinimumVersion $MinimumVersion `
                                                           -MaximumVersion $MaximumVersion `
                                                           -RequiredVersion $RequiredVersion

            if(-not $ValidationResult)
            {
                # Validate-VersionParameters throws the error. 
                # returning to avoid further execution when different values are specified for -ErrorAction parameter
                return
            }

            if($PSBoundParameters.ContainsKey("Repository"))
            {
                $PSBoundParameters["Source"] = $Repository
                $null = $PSBoundParameters.Remove("Repository")

                $ev = $null
                $null = Get-PSRepository -Name $Repository -ErrorVariable ev
                if($ev) { return }
            }

            if($PSBoundParameters.ContainsKey("Version"))
            {
                $null = $PSBoundParameters.Remove("Version")
                $PSBoundParameters["MinimumVersion"] = $MinimumVersion
            }

            $null = PackageManagement\Install-Package @PSBoundParameters
        }
        elseif($PSCmdlet.ParameterSetName -eq "InputObject")
        {
            $null = $PSBoundParameters.Remove("InputObject")

            foreach($inputValue in $InputObject)
            {

                if (($inputValue.PSTypeNames -notcontains "Microsoft.PowerShell.Commands.PSRepositoryItemInfo") -and
                    ($inputValue.PSTypeNames -notcontains "Deserialized.Microsoft.PowerShell.Commands.PSRepositoryItemInfo"))
                {
                    ThrowError -ExceptionName "System.ArgumentException" `
                                -ExceptionMessage $LocalizedData.InvalidInputObjectValue `
                                -ErrorId "InvalidInputObjectValue" `
                                -CallerPSCmdlet $PSCmdlet `
                                -ErrorCategory InvalidArgument `
                                -ExceptionObject $inputValue
                }

                $psRepositoryItemInfo = $inputValue

                # Skip the script name if it is already tried in the current pipeline
                if($scriptNamesInPipeline -contains $psRepositoryItemInfo.Name)
                {
                    continue
                }

                $scriptNamesInPipeline += $psRepositoryItemInfo.Name

                if ($psRepositoryItemInfo.PowerShellGetFormatVersion -and 
                    ($script:SupportedPSGetFormatVersionMajors -notcontains $psRepositoryItemInfo.PowerShellGetFormatVersion.Major))
                {
                    $message = $LocalizedData.NotSupportedPowerShellGetFormatVersionScripts -f ($psRepositoryItemInfo.Name, $psRepositoryItemInfo.PowerShellGetFormatVersion, $psRepositoryItemInfo.Name)
                    Write-Error -Message $message -ErrorId "NotSupportedPowerShellGetFormatVersion" -Category InvalidOperation
                    continue
                }

                $PSBoundParameters["Name"] = $psRepositoryItemInfo.Name
                $PSBoundParameters["RequiredVersion"] = $psRepositoryItemInfo.Version
                $PSBoundParameters["Location"] = $psRepositoryItemInfo.RepositorySourceLocation
                $PSBoundParameters["PackageManagementProvider"] = (Get-ProviderName -PSCustomObject $psRepositoryItemInfo)
                
                $InstalledScriptInfo = Test-ScriptInstalled -Name $psRepositoryItemInfo.Name                 
                if(-not $Force -and $InstalledScriptInfo)
                {
                    $message = $LocalizedData.ScriptAlreadyInstalledVerbose -f ($InstalledScriptInfo.Version, $InstalledScriptInfo.Name, $InstalledScriptInfo.ScriptBase)
                    Write-Verbose -Message $message
                }
                else
                {
                    $source =  $psRepositoryItemInfo.Repository
                    $installationPolicy = (Get-PSRepository -Name $source).InstallationPolicy                
                    $ShouldProcessMessage = $PackageTarget -f ($psRepositoryItemInfo.Name, $psRepositoryItemInfo.Version)
                
                    if($psCmdlet.ShouldProcess($ShouldProcessMessage))
                    {
                        if($installationPolicy.Equals("Untrusted", [StringComparison]::OrdinalIgnoreCase))
                        {
                            if(-not($YesToAll -or $NoToAll -or $SourceSGrantedTrust.Contains($source) -or $sourcesDeniedTrust.Contains($source) -or $Force))
                            {
                                $message = $QueryInstallUntrustedPackage -f ($psRepositoryItemInfo.Name, $psRepositoryItemInfo.RepositorySourceLocation)
                            
                                if($PSVersionTable.PSVersion -ge [Version]"5.0")
                                {
                                    $sourceTrusted = $psCmdlet.ShouldContinue("$message", "$RepositoryIsNotTrusted",$true, [ref]$YesToAll, [ref]$NoToAll)
                                }
                                else
                                {
                                    $sourceTrusted = $psCmdlet.ShouldContinue("$message", "$RepositoryIsNotTrusted", [ref]$YesToAll, [ref]$NoToAll)
                                }

                                if($sourceTrusted)
                                {
                                    $SourcesGrantedTrust+=$source
                                }
                                else
                                {
                                    $SourcesDeniedTrust+=$source
                                }
                            }
                         }
                     }
                     if($installationPolicy.Equals("trusted", [StringComparison]::OrdinalIgnoreCase) -or $SourcesGrantedTrust.Contains($source) -or $YesToAll -or $Force)
                     {
                        $PSBoundParameters["Force"] = $true                        
                        $null = PackageManagement\Install-Package @PSBoundParameters                        
                     }                                  
                }                   
            }
        }
    }
}

function Update-Script
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(SupportsShouldProcess=$true,
                   HelpUri='http://go.microsoft.com/fwlink/?LinkId=619787')]
    Param
    (
        [Parameter(ValueFromPipelineByPropertyName=$true, 
                   Position=0)]
        [ValidateNotNullOrEmpty()]
        [String[]]
        $Name, 

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $RequiredVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $MaximumVersion,

        [Parameter()]
        [Switch]
        $Force
    )

    Begin
    {
        Install-NuGetClientBinaries

        # Script names already tried in the current pipeline
        $scriptNamesInPipeline = @()
    }

    Process
    {
        $scriptFilePathsToUpdate = @()

        $ValidationResult = Validate-VersionParameters -CallerPSCmdlet $PSCmdlet `
                                                       -Name $Name `
                                                       -MaximumVersion $MaximumVersion `
                                                       -RequiredVersion $RequiredVersion

        if(-not $ValidationResult)
        {
            # Validate-VersionParameters throws the error. 
            # returning to avoid further execution when different values are specified for -ErrorAction parameter
            return
        }

        if($Name)
        {
            foreach($scriptName in $Name)
            {
                $availableScriptPaths = Get-AvailableScriptFilePath -Name $scriptName -Verbose:$false
        
                if(-not $availableScriptPaths -and -not (Test-WildcardPattern -Name $scriptName))
                {                    
                    $message = $LocalizedData.ScriptNotInstalledOnThisMachine -f ($scriptName, $script:MyDocumentsScriptsPath, $script:ProgramFilesScriptsPath)
                    Write-Error -Message $message -ErrorId "ScriptNotInstalledOnThisMachine" -Category InvalidOperation -TargetObject $scriptName
                    continue
                }

                foreach($scriptFilePath in $availableScriptPaths)
                {
                    $installedScriptFilePath = Get-InstalledScriptFilePath -Name ([System.IO.Path]::GetFileNameWithoutExtension($scriptFilePath)) | 
                                                   Microsoft.PowerShell.Core\Where-Object {$_ -eq $scriptFilePath }

                    # Check if this script got installed with PowerShellGet and user has required permissions
                    if ($installedScriptFilePath)
                    {
                        if(-not (Test-RunningAsElevated) -and $installedScriptFilePath.StartsWith($script:ProgramFilesScriptsPath, [System.StringComparison]::OrdinalIgnoreCase))
                        {                            
                            if(-not (Test-WildcardPattern -Name $scriptName))
                            {
                                $message = $LocalizedData.AdminPrivilegesRequiredForScriptUpdate -f ($scriptName, $installedScriptFilePath)
                                Write-Error -Message $message -ErrorId "AdminPrivilegesAreRequiredForUpdate" -Category InvalidOperation -TargetObject $scriptName
                            }
                            continue
                        }

                        $scriptFilePathsToUpdate += $installedScriptFilePath
                    }
                    else
                    {
                        if(-not (Test-WildcardPattern -Name $scriptName))
                        {
                            $message = $LocalizedData.ScriptNotInstalledUsingPowerShellGet -f ($scriptName)
                            Write-Error -Message $message -ErrorId "ScriptNotInstalledUsingPowerShellGet" -Category InvalidOperation -TargetObject $scriptName
                        }
                        continue
                    }
                }
            }
        }
        else
        {
            $isRunningAsElevated = Test-RunningAsElevated
            $installedScriptFilePaths = Get-InstalledScriptFilePath

            if($isRunningAsElevated)
            {
                $scriptFilePathsToUpdate = $installedScriptFilePaths
            }
            else
            {
                # Update the scripts installed under 
                $scriptFilePathsToUpdate = $installedScriptFilePaths | Microsoft.PowerShell.Core\Where-Object {
                                                $_.StartsWith($script:MyDocumentsScriptsPath, [System.StringComparison]::OrdinalIgnoreCase)}
            }
        }

        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters[$script:PSArtifactType] = $script:PSArtifactTypeScript
        $PSBoundParameters["MessageResolver"] = $script:PackageManagementUpdateScriptMessageResolverScriptBlock
        $PSBoundParameters["InstallUpdate"] = $true

        foreach($scriptFilePath in $scriptFilePathsToUpdate)
        {
            $scriptName = [System.IO.Path]::GetFileNameWithoutExtension($scriptFilePath)

            $installedScriptInfoFilePath = $null
            $installedScriptInfoFileName = "$($scriptName)_$script:InstalledScriptInfoFileName"

            if($scriptFilePath.ToString().StartsWith($script:MyDocumentsScriptsPath, [System.StringComparison]::OrdinalIgnoreCase))
            {
                $PSBoundParameters["Scope"] = "CurrentUser"
                $installedScriptInfoFilePath = Microsoft.PowerShell.Management\Join-Path -Path $script:PSGetAppLocalScriptsPath `
                                                                                         -ChildPath $installedScriptInfoFileName
            }
            elseif($scriptFilePath.ToString().StartsWith($script:ProgramFilesScriptsPath, [System.StringComparison]::OrdinalIgnoreCase))
            {
                $PSBoundParameters["Scope"] = "AllUsers"
                $installedScriptInfoFilePath = Microsoft.PowerShell.Management\Join-Path -Path $script:PSGetProgramDataScriptsPath `
                                                                                         -ChildPath $installedScriptInfoFileName

            }

            $psgetItemInfo = $null
            if($installedScriptInfoFilePath -and (Microsoft.PowerShell.Management\Test-Path -Path $installedScriptInfoFilePath -PathType Leaf))
            {
                $psgetItemInfo = DeSerialize-PSObject -Path $installedScriptInfoFilePath
            }
            
            # Skip the script name if it is already tried in the current pipeline
            if(-not $psgetItemInfo -or ($scriptNamesInPipeline -contains $psgetItemInfo.Name))
            {
                continue
            }


            $scriptFilePath = Microsoft.PowerShell.Management\Join-Path -Path $psgetItemInfo.InstalledLocation `
                                                                        -ChildPath "$($psgetItemInfo.Name).ps1"

            # Remove the InstalledScriptInfo.xml file if the actual script file was manually uninstalled by the user
            if(-not (Microsoft.PowerShell.Management\Test-Path -Path $scriptFilePath -PathType Leaf))
            {
                Microsoft.PowerShell.Management\Remove-Item -Path $installedScriptInfoFilePath -Force -ErrorAction SilentlyContinue

                continue
            }

            $scriptNamesInPipeline += $psgetItemInfo.Name

            $message = $LocalizedData.CheckingForScriptUpdate -f ($psgetItemInfo.Name)
            Write-Verbose -Message $message

            $providerName = Get-ProviderName -PSCustomObject $psgetItemInfo
            if(-not $providerName)
            {
                $providerName = $script:NuGetProviderName
            }

            $PSBoundParameters["PackageManagementProvider"] = $providerName 
            $PSBoundParameters["Name"] = $psgetItemInfo.Name
            $PSBoundParameters["Location"] = $psgetItemInfo.RepositorySourceLocation

            Get-PSGalleryApiAvailability -Repository (Get-SourceName -Location $psgetItemInfo.RepositorySourceLocation)

            $sid = PackageManagement\Install-Package @PSBoundParameters
        }
    }
}

function Uninstall-Script
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(DefaultParameterSetName='NameParameterSet',
                   SupportsShouldProcess=$true,
                   HelpUri='http://go.microsoft.com/fwlink/?LinkId=619789')]
    Param
    (
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   Mandatory=$true, 
                   Position=0,
                   ParameterSetName='NameParameterSet')]
        [ValidateNotNullOrEmpty()]
        [String[]]
        $Name, 

        [Parameter(Mandatory=$true, 
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0,
                   ParameterSetName='InputObject')]
        [ValidateNotNull()]
        [PSCustomObject[]]
        $InputObject,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameParameterSet')]
        [ValidateNotNull()]
        [Version]
        $MinimumVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameParameterSet')]
        [ValidateNotNull()]
        [Version]
        $RequiredVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='NameParameterSet')]
        [ValidateNotNull()]
        [Version]
        $MaximumVersion,

        [Parameter()]
        [Switch]
        $Force
    )

    Process
    {
        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters["MessageResolver"] = $script:PackageManagementUnInstallScriptMessageResolverScriptBlock
        $PSBoundParameters[$script:PSArtifactType] = $script:PSArtifactTypeScript

        if($PSCmdlet.ParameterSetName -eq "InputObject")
        {
            $null = $PSBoundParameters.Remove("InputObject")
        
            foreach($inputValue in $InputObject)
            {
                if (($inputValue.PSTypeNames -notcontains "Microsoft.PowerShell.Commands.PSRepositoryItemInfo") -and
                    ($inputValue.PSTypeNames -notcontains "Deserialized.Microsoft.PowerShell.Commands.PSRepositoryItemInfo"))
                {
                    ThrowError -ExceptionName "System.ArgumentException" `
                                -ExceptionMessage $LocalizedData.InvalidInputObjectValue `
                                -ErrorId "InvalidInputObjectValue" `
                                -CallerPSCmdlet $PSCmdlet `
                                -ErrorCategory InvalidArgument `
                                -ExceptionObject $inputValue
                }

                $PSBoundParameters["Name"] = $inputValue.Name
                $PSBoundParameters["RequiredVersion"] = $inputValue.Version

                $null = PackageManagement\Uninstall-Package @PSBoundParameters
            }
        }
        else
        {
            $ValidationResult = Validate-VersionParameters -CallerPSCmdlet $PSCmdlet `
                                                           -Name $Name `
                                                           -MinimumVersion $MinimumVersion `
                                                           -MaximumVersion $MaximumVersion `
                                                           -RequiredVersion $RequiredVersion

            if(-not $ValidationResult)
            {
                # Validate-VersionParameters throws the error. 
                # returning to avoid further execution when different values are specified for -ErrorAction parameter
                return
            }

            $null = PackageManagement\Uninstall-Package @PSBoundParameters
        }
    }
}

function Get-InstalledScript
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(HelpUri='http://go.microsoft.com/fwlink/?LinkId=619790')]
    Param
    (
        [Parameter(ValueFromPipelineByPropertyName=$true, 
                   Position=0)]
        [ValidateNotNullOrEmpty()]
        [String[]]
        $Name, 

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $MinimumVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $RequiredVersion,

        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNull()]
        [Version]
        $MaximumVersion
    )

    Process
    {
        $ValidationResult = Validate-VersionParameters -CallerPSCmdlet $PSCmdlet `
                                                       -Name $Name `
                                                       -MinimumVersion $MinimumVersion `
                                                       -MaximumVersion $MaximumVersion `
                                                       -RequiredVersion $RequiredVersion

        if(-not $ValidationResult)
        {
            # Validate-VersionParameters throws the error. 
            # returning to avoid further execution when different values are specified for -ErrorAction parameter
            return
        }

        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters["MessageResolver"] = $script:PackageManagementMessageResolverScriptBlockForScriptCmdlets
        $PSBoundParameters[$script:PSArtifactType] = $script:PSArtifactTypeScript

        PackageManagement\Get-Package @PSBoundParameters | Microsoft.PowerShell.Core\ForEach-Object {New-PSGetItemInfo -SoftwareIdenties $_ -Type $script:PSArtifactTypeScript}
    }
}

#endregion *-Script cmdlets

#region *-PSRepository cmdlets

function Register-PSRepository
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(PositionalBinding=$false,
                   HelpUri='http://go.microsoft.com/fwlink/?LinkID=517129')]
    Param 
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Name,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $SourceLocation,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $PublishLocation,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $ScriptSourceLocation,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $ScriptPublishLocation,

        [Parameter()]
        [ValidateSet('Trusted','Untrusted')]
        [string]
        $InstallationPolicy = 'Untrusted',

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $PackageManagementProvider        
    )

    DynamicParam
    {
        if (Get-Variable -Name SourceLocation -ErrorAction SilentlyContinue)
        {
            Set-Variable -Name selctedProviderName -value $null -Scope 1

            if(Get-Variable -Name PackageManagementProvider -ErrorAction SilentlyContinue)
            {
                $selctedProviderName = $PackageManagementProvider
                $null = Get-DynamicParameters -Location $SourceLocation -PackageManagementProvider ([REF]$selctedProviderName)
            }
            else
            {
                $dynamicParameters = Get-DynamicParameters -Location $SourceLocation -PackageManagementProvider ([REF]$selctedProviderName)
                Set-Variable -Name PackageManagementProvider -Value $selctedProviderName -Scope 1
                $null = $dynamicParameters
            }
        }
    }

    Begin
    {
        if($PackageManagementProvider)
        {
            $providers = PackageManagement\Get-PackageProvider | Where-Object { $_.Name -ne $script:PSModuleProviderName -and $_.Features.ContainsKey($script:SupportsPSModulesFeatureName) }

            if ($providers.Name -notcontains $PackageManagementProvider)
            {
                $message = $LocalizedData.InvalidPackageManagementProviderValue -f ($PackageManagementProvider, ($providers.Name -join ','), $script:NuGetProviderName)
                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage $message `
                           -ErrorId "InvalidPackageManagementProviderValue" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument `
                           -ExceptionObject $PackageManagementProvider
                return
            }
        }

        Get-PSGalleryApiAvailability -Repository $Name
        
        Install-NuGetClientBinaries
    }

    Process
    {
        # Ping and resolve the specified location
        $SourceLocation = Resolve-Location -Location (Get-LocationString -LocationUri $SourceLocation) `
                                           -LocationParameterName 'SourceLocation' `
                                           -CallerPSCmdlet $PSCmdlet
        if(-not $SourceLocation)
        {
            # Above Resolve-Location function throws an error when it is not able to resolve a location
            return
        }

        if($InstallationPolicy -eq "Trusted")
        {
            $PSBoundParameters.Add("Trusted", $true)
        }

        $providerName = $null

        if($PackageManagementProvider)
        {            
            $providerName = $PackageManagementProvider
        }
        elseif($selctedProviderName)
        {
            $providerName = $selctedProviderName
        }
        else
        {
            $providerName = Get-PackageManagementProviderName -Location $SourceLocation
        }

        if($providerName)
        {
            $PSBoundParameters[$script:PackageManagementProviderParam] = $providerName
        }

        if($PublishLocation)
        {
            $PSBoundParameters[$script:PublishLocation] = Get-LocationString -LocationUri $PublishLocation
        }

        if($ScriptPublishLocation)
        {
            $PSBoundParameters[$script:ScriptPublishLocation] = Get-LocationString -LocationUri $ScriptPublishLocation
        }

        if($ScriptSourceLocation)
        {
            $PSBoundParameters[$script:ScriptSourceLocation] = Get-LocationString -LocationUri $ScriptSourceLocation
        }

        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        
        $PSBoundParameters["Location"] = Get-LocationString -LocationUri $SourceLocation
        $null = $PSBoundParameters.Remove("SourceLocation")
        $null = $PSBoundParameters.Remove("InstallationPolicy")

        $PSBoundParameters["MessageResolver"] = $script:PackageManagementMessageResolverScriptBlock

        $null = PackageManagement\Register-PackageSource @PSBoundParameters
    }
}

function Set-PSRepository
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(PositionalBinding=$false,
                   HelpUri='http://go.microsoft.com/fwlink/?LinkID=517128')]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Name,
        
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $SourceLocation,


        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $PublishLocation,        

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $ScriptSourceLocation,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $ScriptPublishLocation,

        [Parameter()]
        [ValidateSet('Trusted','Untrusted')]
        [string]
        $InstallationPolicy,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $PackageManagementProvider
    )

    DynamicParam
    {
        if (Get-Variable -Name Name -ErrorAction SilentlyContinue)
        {
            $moduleSource = Get-PSRepository -Name $Name -ErrorAction SilentlyContinue -WarningAction SilentlyContinue

            if($moduleSource)
            {
                $providerName = (Get-ProviderName -PSCustomObject $moduleSource)
            
                $loc = $moduleSource.SourceLocation
            
                if(Get-Variable -Name SourceLocation -ErrorAction SilentlyContinue)
                {
                    $loc = $SourceLocation
                }

                if(Get-Variable -Name PackageManagementProvider -ErrorAction SilentlyContinue)
                {
                    $providerName = $PackageManagementProvider
                }

                $null = Get-DynamicParameters -Location $loc -PackageManagementProvider ([REF]$providerName)
            }
        }
    }

    Begin
    {
        if($PackageManagementProvider)
        {
            $providers = PackageManagement\Get-PackageProvider | Where-Object { $_.Name -ne $script:PSModuleProviderName -and $_.Features.ContainsKey($script:SupportsPSModulesFeatureName) }

            if ($providers.Name -notcontains $PackageManagementProvider)
            {
                $message = $LocalizedData.InvalidPackageManagementProviderValue -f ($PackageManagementProvider, ($providers.Name -join ','), $script:NuGetProviderName)
                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage $message `
                           -ErrorId "InvalidPackageManagementProviderValue" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument `
                           -ExceptionObject $PackageManagementProvider
                return
            }
        }

        Get-PSGalleryApiAvailability -Repository $Name
        
        Install-NuGetClientBinaries
    }

    Process
    {
        # Ping and resolve the specified location
        if($SourceLocation)
        {
            # Ping and resolve the specified location
            $SourceLocation = Resolve-Location -Location (Get-LocationString -LocationUri $SourceLocation) `
                                               -LocationParameterName 'SourceLocation' `
                                               -CallerPSCmdlet $PSCmdlet
            if(-not $SourceLocation)
            {
                # Above Resolve-Location function throws an error when it is not able to resolve a location
                return
            }
        }

        $ModuleSource = Get-PSRepository -Name $Name -ErrorAction SilentlyContinue -WarningAction SilentlyContinue

        if(-not $ModuleSource)
        {
            $message = $LocalizedData.RepositoryNotFound -f ($Name)

            ThrowError -ExceptionName "System.InvalidOperationException" `
                       -ExceptionMessage $message `
                       -ErrorId "RepositoryNotFound" `
                       -CallerPSCmdlet $PSCmdlet `
                       -ErrorCategory InvalidOperation `
                       -ExceptionObject $Name
        }

        if (-not $PackageManagementProvider)
        {
            $PackageManagementProvider = (Get-ProviderName -PSCustomObject $ModuleSource)
        }

        $Trusted = $ModuleSource.Trusted
        if($InstallationPolicy)
        {
            if($InstallationPolicy -eq "Trusted")
            {
                $Trusted = $true
            }
            else
            {
                $Trusted = $false
            }

            $null = $PSBoundParameters.Remove("InstallationPolicy")
        }

        if($PublishLocation)
        {
            $PSBoundParameters[$script:PublishLocation] = Get-LocationString -LocationUri $PublishLocation
        }

        if($ScriptPublishLocation)
        {
            $PSBoundParameters[$script:ScriptPublishLocation] = Get-LocationString -LocationUri $ScriptPublishLocation
        }

        if($ScriptSourceLocation)
        {
            $PSBoundParameters[$script:ScriptSourceLocation] = Get-LocationString -LocationUri $ScriptSourceLocation
        }

        if($SourceLocation)
        {
            $PSBoundParameters["NewLocation"] = Get-LocationString -LocationUri $SourceLocation

            $null = $PSBoundParameters.Remove("SourceLocation")
        }

        $PSBoundParameters[$script:PackageManagementProviderParam] = $PackageManagementProvider
        $PSBoundParameters.Add("Trusted", $Trusted)        
        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters["MessageResolver"] = $script:PackageManagementMessageResolverScriptBlock

        $null = PackageManagement\Set-PackageSource @PSBoundParameters
    }
}

function Unregister-PSRepository
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(HelpUri='http://go.microsoft.com/fwlink/?LinkID=517130')]
    Param
    (
        [Parameter(ValueFromPipelineByPropertyName=$true,
                   Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Name
    )
    
    Begin
    {
        Get-PSGalleryApiAvailability -Repository $Name
    }

    Process
    {
        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters["MessageResolver"] = $script:PackageManagementMessageResolverScriptBlock

        $null = $PSBoundParameters.Remove("Name")

        foreach ($moduleSourceName in $Name)
        {
            # Check if $moduleSourceName contains any wildcards
            if(Test-WildcardPattern $moduleSourceName)
            {
                $message = $LocalizedData.RepositoryNameContainsWildCards -f ($moduleSourceName)
                Write-Error -Message $message -ErrorId "RepositoryNameContainsWildCards" -Category InvalidOperation
                continue
            }

            $PSBoundParameters["Source"] = $moduleSourceName

            $null = PackageManagement\Unregister-PackageSource @PSBoundParameters
        }
    }
}

function Get-PSRepository
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(HelpUri='http://go.microsoft.com/fwlink/?LinkID=517127')]
    Param
    (
        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Name
    )
    
    Begin
    {
        Get-PSGalleryApiAvailability -Repository $Name
    }

    Process
    {
        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters["MessageResolver"] = $script:PackageManagementMessageResolverScriptBlock

        if($Name)
        {
            foreach($sourceName in $Name)
            {
                $PSBoundParameters["Name"] = $sourceName
                
                $packageSources = PackageManagement\Get-PackageSource @PSBoundParameters

                $packageSources | Microsoft.PowerShell.Core\ForEach-Object { New-ModuleSourceFromPackageSource -PackageSource $_ }
            }
        }
        else
        {
            $packageSources = PackageManagement\Get-PackageSource @PSBoundParameters

            $packageSources | Microsoft.PowerShell.Core\ForEach-Object { New-ModuleSourceFromPackageSource -PackageSource $_ }
        }
    }
}

#endregion *-PSRepository cmdlets

#region *-ScriptFile cmdlets

# Below is the sample PSScriptInfo in a script file.
<#PSScriptInfo

.VERSION 1.0

.AUTHOR manikb

.COMPANYNAME Microsoft Corporation

.COPYRIGHT (c) 2015 Microsoft Corporation. All rights reserved.

.TAGS Tag1 Tag2 Tag3

.LICENSEURI https://contoso.com/License

.PROJECTURI https://contoso.com/

.ICONURI https://contoso.com/Icon

.EXTERNALMODULEDEPENDENCIES ExternalModule1

.REQUIREDSCRIPTS Start-WFContosoServer,Stop-ContosoServerScript

.EXTERNALSCRIPTDEPENDENCIES Stop-ContosoServerScript

.RELEASENOTES
contoso script now supports following features
Feature 1
Feature 2
Feature 3
Feature 4
Feature 5

#>

<# #Requires -Module statements #>

<# 

.DESCRIPTION 
 Description goes here. 

#> 


#
function Test-ScriptFile
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(PositionalBinding=$false,
                   DefaultParameterSetName='PathParameterSet',
                   HelpUri='http://go.microsoft.com/fwlink/?LinkId=619791')]
    Param
    (
        [Parameter(Mandatory=$true,
                   Position=0,
                   ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='PathParameterSet')]
        [ValidateNotNullOrEmpty()]
        [string]
        $Path,

        [Parameter(Mandatory=$true, 
                   ValueFromPipelineByPropertyName=$true,
                   ParameterSetName='LiteralPathParameterSet')]
        [ValidateNotNullOrEmpty()]
        [string]
        $LiteralPath
    )

    Process
    {
        $scriptFilePath = $null
        if($Path)
        {
            $scriptFilePath = Resolve-PathHelper -Path $Path -CallerPSCmdlet $PSCmdlet | Microsoft.PowerShell.Utility\Select-Object -First 1
            
            if(-not $scriptFilePath -or -not (Microsoft.PowerShell.Management\Test-Path -Path $scriptFilePath -PathType Leaf))
            {
                $errorMessage = ($LocalizedData.PathNotFound -f $Path)
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage $errorMessage `
                            -ErrorId "PathNotFound" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ExceptionObject $Path `
                            -ErrorCategory InvalidArgument
                return
            }
        }
        else
        {
            $scriptFilePath = Resolve-PathHelper -Path $LiteralPath -IsLiteralPath -CallerPSCmdlet $PSCmdlet | Microsoft.PowerShell.Utility\Select-Object -First 1

            if(-not $scriptFilePath -or -not (Microsoft.PowerShell.Management\Test-Path -LiteralPath $scriptFilePath -PathType Leaf))
            {
                $errorMessage = ($LocalizedData.PathNotFound -f $LiteralPath)
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage $errorMessage `
                            -ErrorId "PathNotFound" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ExceptionObject $LiteralPath `
                            -ErrorCategory InvalidArgument
                return
            }
        }

        if(-not $scriptFilePath.EndsWith('.ps1', [System.StringComparison]::OrdinalIgnoreCase))
        {
            $errorMessage = ($LocalizedData.InvalidScriptFilePath -f $scriptFilePath)
            ThrowError  -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $errorMessage `
                        -ErrorId "InvalidScriptFilePath" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ExceptionObject $scriptFilePath `
                        -ErrorCategory InvalidArgument
            return
        }

        $PSScriptInfo = Microsoft.PowerShell.Utility\New-Object PSCustomObject -Property ([ordered]@{})
        $script:PSScriptInfoProperties | Microsoft.PowerShell.Core\ForEach-Object {
                                                Microsoft.PowerShell.Utility\Add-Member -InputObject $PSScriptInfo `
                                                                                        -MemberType NoteProperty `
                                                                                        -Name $_ `
                                                                                        -Value $null
                                            }

        ValidateAndAdd-PSScriptInfoEntry -PSScriptInfo $PSScriptInfo `
                                         -PropertyName $script:Name `
                                         -PropertyValue ([System.IO.Path]::GetFileNameWithoutExtension($scriptFilePath)) `
                                         -CallerPSCmdlet $PSCmdlet

        ValidateAndAdd-PSScriptInfoEntry -PSScriptInfo $PSScriptInfo `
                                         -PropertyName $script:Path `
                                         -PropertyValue $scriptFilePath `
                                         -CallerPSCmdlet $PSCmdlet

        ValidateAndAdd-PSScriptInfoEntry -PSScriptInfo $PSScriptInfo `
                                         -PropertyName $script:ScriptBase `
                                         -PropertyValue (Microsoft.PowerShell.Management\Split-Path -Path $scriptFilePath -Parent) `
                                         -CallerPSCmdlet $PSCmdlet

        [System.Management.Automation.Language.Token[]]$tokens = $null;
        [System.Management.Automation.Language.ParseError[]]$errors = $null;
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptFilePath, ([ref]$tokens), ([ref]$errors))
        
        if($errors)
        {
            $errorMessage = ($LocalizedData.ScriptParseError -f $scriptFilePath)
            ThrowError  -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $errorMessage `
                        -ErrorId "ScriptParseError" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ExceptionObject $errors `
                        -ErrorCategory InvalidArgument
            return
        }

        if($ast)
        {
            # Get the block/group comment begining with <#PSScriptInfo
            $CommentTokens = $tokens | Microsoft.PowerShell.Core\Where-Object {$_.Kind -eq 'Comment'}

            $psscriptInfoComments = $CommentTokens | 
                                        Microsoft.PowerShell.Core\Where-Object { $_.Extent.Text -match "<#PSScriptInfo" } | 
                                            Microsoft.PowerShell.Utility\Select-Object -First 1

            if(-not $psscriptInfoComments)
            {
                $errorMessage = ($LocalizedData.MissingPSScriptInfo -f $scriptFilePath)
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage $errorMessage `
                            -ErrorId "MissingPSScriptInfo" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ExceptionObject $scriptFilePath `
                            -ErrorCategory InvalidArgument
                return
            }

            # $psscriptInfoComments.Text will have the multiline PSScriptInfo comment, 
            # split them into multiple lines to parse for the PSScriptInfo metadata properties.
            $commentLines = $psscriptInfoComments.Text -split "`n" | % {$_.Trim()}

            $KeyName = $null
            $Value = ""

            # PSScriptInfo comment will be in following format:
                <#PSScriptInfo

                .VERSION 1.0

                .AUTHOR manikb

                .COMPANYNAME Microsoft Corporation

                .COPYRIGHT (c) 2015 Microsoft Corporation. All rights reserved.

                .TAGS Tag1 Tag2 Tag3

                .LICENSEURI https://contoso.com/License

                .PROJECTURI https://contoso.com/

                .ICONURI https://contoso.com/Icon

                .EXTERNALMODULEDEPENDENCIES ExternalModule1

                .REQUIREDSCRIPTS Start-WFContosoServer,Stop-ContosoServerScript

                .EXTERNALSCRIPTDEPENDENCIES Stop-ContosoServerScript

                .RELEASENOTES
                contoso script now supports following features
                Feature 1
                Feature 2
                Feature 3
                Feature 4
                Feature 5

                #>
            # If comment line count is not more than two, it doesn't have the any metadata property
            # First line is <#PSScriptInfo
            # Last line #>
            #
            if($commentLines.Count -gt 2)
            {
                for($i = 1; $i -lt ($commentLines.count - 1); $i++)
                {
                    $line = $commentLines[$i]

                    if(-not $line)
                    {
                        continue
                    }

                    # A line is starting with . conveys a new metadata property
                    # __NEWLINE__ is used for replacing the value lines while adding the value to $PSScriptInfo object
                    #
                    if($line.StartsWith('.'))
                    {
                        $parts = $line -split '[.\s+]',3 | Microsoft.PowerShell.Core\Where-Object {$_}

                        if($KeyName -and $Value)
                        {
                            $Value = $Value -split '__NEWLINE__'  | Microsoft.PowerShell.Core\Where-Object {$_}

                            ValidateAndAdd-PSScriptInfoEntry -PSScriptInfo $PSScriptInfo `
                                                             -PropertyName $KeyName `
                                                             -PropertyValue $Value `
                                                             -CallerPSCmdlet $PSCmdlet
                        }

                        $KeyName = $null
                        $Value = ""

                        if($parts.GetType().ToString() -eq "System.String")
                        {
                            $KeyName = $parts
                        } 
                        else
                        {
                            $KeyName = $parts[0]; 
                            $Value = $parts[1]
                        }
                    }                    
                    else
                    {
                        if($Value)
                        {
                            # __NEWLINE__ is used for replacing the value lines while adding the value to $PSScriptInfo object
                            $Value += '__NEWLINE__'
                        }

                        $Value += $line
                    }
                }

                if($KeyName -and $Value)
                {
                    $Value = $Value -split '__NEWLINE__'  | Microsoft.PowerShell.Core\Where-Object {$_}

                    ValidateAndAdd-PSScriptInfoEntry -PSScriptInfo $PSScriptInfo `
                                                     -PropertyName $KeyName `
                                                     -PropertyValue $Value `
                                                     -CallerPSCmdlet $PSCmdlet

                    $KeyName = $null
                    $Value = ""
                }
            }

            $helpContent = $ast.GetHelpContent()
            if($helpContent -and $helpContent.Description)
            {
                ValidateAndAdd-PSScriptInfoEntry -PSScriptInfo $PSScriptInfo `
                                                 -PropertyName $script:DESCRIPTION `
                                                 -PropertyValue $helpContent.Description.Trim() `
                                                 -CallerPSCmdlet $PSCmdlet

            }

            # Handle RequiredModules
            if((Microsoft.PowerShell.Utility\Get-Member -InputObject $ast -Name 'ScriptRequirements') -and 
               $ast.ScriptRequirements -and
               (Microsoft.PowerShell.Utility\Get-Member -InputObject $ast.ScriptRequirements -Name 'RequiredModules') -and
               $ast.ScriptRequirements.RequiredModules)
            {
                ValidateAndAdd-PSScriptInfoEntry -PSScriptInfo $PSScriptInfo `
                                                 -PropertyName $script:RequiredModules `
                                                 -PropertyValue $ast.ScriptRequirements.RequiredModules `
                                                 -CallerPSCmdlet $PSCmdlet
            }

            # Get all defined functions and populate ExportedCommands, ExportedFunctions and ExportedWorkflows
            $allCommands = $ast.FindAll({param($i) return ($i.GetType().Name -eq 'FunctionDefinitionAst')}, $true)

            if($allCommands)
            {
                $allCommandNames = $allCommands | ForEach-Object {$_.Name} | Select-Object -Unique
                ValidateAndAdd-PSScriptInfoEntry -PSScriptInfo $PSScriptInfo `
                                        -PropertyName $script:ExportedCommands `
                                        -PropertyValue $allCommandNames `
                                        -CallerPSCmdlet $PSCmdlet            

                $allFunctionNames = $allCommands | Where-Object {-not $_.IsWorkflow}  | ForEach-Object {$_.Name} | Select-Object -Unique
                ValidateAndAdd-PSScriptInfoEntry -PSScriptInfo $PSScriptInfo `
                                        -PropertyName $script:ExportedFunctions `
                                        -PropertyValue $allFunctionNames `
                                        -CallerPSCmdlet $PSCmdlet


                $allWorkflowNames = $allCommands | Where-Object {$_.IsWorkflow} | ForEach-Object {$_.Name} | Select-Object -Unique 
                ValidateAndAdd-PSScriptInfoEntry -PSScriptInfo $PSScriptInfo `
                                        -PropertyName $script:ExportedWorkflows `
                                        -PropertyValue $allWorkflowNames `
                                        -CallerPSCmdlet $PSCmdlet
            }
        }

        # Ensure that the script file has the required metadata properties. 
        if(-not $PSScriptInfo.Version -or -not $PSScriptInfo.Author -or -not $PSScriptInfo.Description)
        {
            $errorMessage = ($LocalizedData.MissingRequiredPSScriptInfoProperties -f $scriptFilePath)
            ThrowError  -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $errorMessage `
                        -ErrorId "MissingRequiredPSScriptInfoProperties" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ExceptionObject $Path `
                        -ErrorCategory InvalidArgument
            return
        }

        $PSScriptInfo.PSTypeNames.Insert(0, "Microsoft.PowerShell.Commands.PSScriptInfo")

        return $PSScriptInfo
    }
}

function New-ScriptFile
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(PositionalBinding=$false,
                   SupportsShouldProcess=$true,
                   HelpUri='http://go.microsoft.com/fwlink/?LinkId=619792')]
    Param
    (
        [Parameter(Mandatory=$true,
                   Position=0,
                   ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Path,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [Version]
        $Version,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Author,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Description,

        [Parameter()] 
        [ValidateNotNullOrEmpty()]
        [String]
        $CompanyName,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $Copyright,
        
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Object[]]
        $RequiredModules,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [String[]]
        $ExternalModuleDependencies,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $RequiredScripts,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [String[]]
        $ExternalScriptDependencies,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Tags,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $ProjectUri,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $LicenseUri,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $IconUri,

        [Parameter()]
        [string[]]
        $ReleaseNotes,
                
        [Parameter()]
        [switch]
        $PassThru,

        [Parameter()]
        [switch]
        $Force
    )

    Process
    {
        if(-not $Path.EndsWith('.ps1', [System.StringComparison]::OrdinalIgnoreCase))
        {
            $errorMessage = ($LocalizedData.InvalidScriptFilePath -f $Path)
            ThrowError  -ExceptionName 'System.ArgumentException' `
                        -ExceptionMessage $errorMessage `
                        -ErrorId 'InvalidScriptFilePath' `
                        -CallerPSCmdlet $PSCmdlet `
                        -ExceptionObject $Path `
                        -ErrorCategory InvalidArgument
            return
        }

        if(-not $Force -and (Microsoft.PowerShell.Management\Test-Path -Path $Path))
        {
            $errorMessage = ($LocalizedData.ScriptFileExist -f $Path)
            ThrowError  -ExceptionName 'System.ArgumentException' `
                        -ExceptionMessage $errorMessage `
                        -ErrorId 'ScriptFileExist' `
                        -CallerPSCmdlet $PSCmdlet `
                        -ExceptionObject $Path `
                        -ErrorCategory InvalidArgument
            return
        }

        $PSScriptInfoString = Get-PSScriptInfoString -Version $Version `
                                                     -Author $Author `
                                                     -CompanyName $CompanyName `
                                                     -Copyright $Copyright `
                                                     -ExternalModuleDependencies $ExternalModuleDependencies `
                                                     -RequiredScripts $RequiredScripts `
                                                     -ExternalScriptDependencies $ExternalScriptDependencies `
                                                     -Tags $Tags `
                                                     -ProjectUri $ProjectUri `
                                                     -LicenseUri $LicenseUri `
                                                     -IconUri $IconUri `
                                                     -ReleaseNotes $ReleaseNotes
                                                     
        $requiresStrings = Get-RequiresString -RequiredModules $RequiredModules

        $ScriptCommentHelpInfoString = Get-ScriptCommentHelpInfoString -Description $Description

        $ScriptMetadataString = $PSScriptInfoString
        $ScriptMetadataString += "`n"

        if("$requiresStrings".Trim())
        {
            $ScriptMetadataString += "`n"
            $ScriptMetadataString += $requiresStrings -join "`n"
            $ScriptMetadataString += "`n"
        }

        $ScriptMetadataString += "`n"
        $ScriptMetadataString += $ScriptCommentHelpInfoString        
        $ScriptMetadataString += "Param()`n`n"

        $tempScriptFilePath = Microsoft.PowerShell.Management\Join-Path -Path $env:TEMP -ChildPath "$(Get-Random).ps1"
        
        try
        {
            Microsoft.PowerShell.Management\Set-Content -Value $ScriptMetadataString -Path $tempScriptFilePath -Force -WhatIf:$false -Confirm:$false

            $scriptInfo = Test-ScriptFile -Path $tempScriptFilePath

            if(-not $scriptInfo)
            {
                # Above Test-ScriptFile cmdlet writes the errors
                return
            }

    	    if($Force -or $PSCmdlet.ShouldProcess($Path, ($LocalizedData.NewScriptFilewhatIfMessage -f $Path) ))
    	    {
                Microsoft.PowerShell.Management\Copy-Item -Path $tempScriptFilePath -Destination $Path -Force -WhatIf:$false -Confirm:$false

                if($PassThru)
                {
                    $ScriptMetadataString
                }
            }
        }
        finally
        {
            Microsoft.PowerShell.Management\Remove-Item -Path $tempScriptFilePath -Force -WhatIf:$false -Confirm:$false -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
        }
    }
}

function Update-ScriptFile
{
    <#
    .ExternalHelp PSModule.psm1-help.xml
    #>
    [CmdletBinding(PositionalBinding=$false,
                   DefaultParameterSetName='PathParameterSet',
                   SupportsShouldProcess=$true,
                   HelpUri='http://go.microsoft.com/fwlink/?LinkId=619793')]
    Param
    (
        [Parameter(Mandatory=$true,
                   Position=0,
                   ParameterSetName='PathParameterSet',
                   ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Path,

        [Parameter(Mandatory=$true,
                   Position=0,
                   ParameterSetName='LiteralPathParameterSet',
                   ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $LiteralPath,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Version]
        $Version,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $Author,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $Description,

        [Parameter()] 
        [ValidateNotNullOrEmpty()]
        [String]
        $CompanyName,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $Copyright,
        
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Object[]]
        $RequiredModules,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [String[]]
        $ExternalModuleDependencies,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $RequiredScripts,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [String[]]
        $ExternalScriptDependencies,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Tags,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $ProjectUri,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $LicenseUri,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $IconUri,

        [Parameter()]
        [string[]]
        $ReleaseNotes,
                
        [Parameter()]
        [switch]
        $PassThru,

        [Parameter()]
        [switch]
        $Force
    )

    Process
    {
        $scriptFilePath = $null
        if($Path)
        {
            $scriptFilePath = Resolve-PathHelper -Path $Path -CallerPSCmdlet $PSCmdlet | 
                                  Microsoft.PowerShell.Utility\Select-Object -First 1
            
            if(-not $scriptFilePath -or 
               -not (Microsoft.PowerShell.Management\Test-Path -Path $scriptFilePath -PathType Leaf))
            {
                $errorMessage = ($LocalizedData.PathNotFound -f $Path)
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage $errorMessage `
                            -ErrorId "PathNotFound" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ExceptionObject $Path `
                            -ErrorCategory InvalidArgument
            }
        }
        else
        {
            $scriptFilePath = Resolve-PathHelper -Path $LiteralPath -IsLiteralPath -CallerPSCmdlet $PSCmdlet | 
                                  Microsoft.PowerShell.Utility\Select-Object -First 1

            if(-not $scriptFilePath -or 
               -not (Microsoft.PowerShell.Management\Test-Path -LiteralPath $scriptFilePath -PathType Leaf))
            {
                $errorMessage = ($LocalizedData.PathNotFound -f $LiteralPath)
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage $errorMessage `
                            -ErrorId "PathNotFound" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ExceptionObject $LiteralPath `
                            -ErrorCategory InvalidArgument
            }
        }

        if(-not $scriptFilePath.EndsWith('.ps1', [System.StringComparison]::OrdinalIgnoreCase))
        {
            $errorMessage = ($LocalizedData.InvalidScriptFilePath -f $scriptFilePath)
            ThrowError  -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $errorMessage `
                        -ErrorId "InvalidScriptFilePath" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ExceptionObject $scriptFilePath `
                        -ErrorCategory InvalidArgument
            return
        }

        $psscriptInfo = Test-ScriptFile -Path $scriptFilePath
        if(-not $psscriptInfo)
        {
            # Test-ScriptFile cmdlet throws the actual errors
            return
        }
        
        # Use existing values if any of the parameters are not specified during Update-ScriptFile
        if(-not $Version)
        {
            $Version = $psscriptInfo.Version
        }

        if(-not $Author)
        {
            $Author = $psscriptInfo.Author
        }

        if(-not $CompanyName)
        {
            $CompanyName = $psscriptInfo.CompanyName
        }

        if(-not $Copyright)
        {
            $Copyright = $psscriptInfo.Copyright
        }

        if(-not $RequiredModules)
        {
            $RequiredModules = $psscriptInfo.RequiredModules
        }

        if(-not $ExternalModuleDependencies)
        {
            $ExternalModuleDependencies = $psscriptInfo.ExternalModuleDependencies
        }

        if(-not $RequiredScripts)
        {
            $RequiredScripts = $psscriptInfo.RequiredScripts
        }

        if(-not $ExternalScriptDependencies)
        {
            $ExternalScriptDependencies = $psscriptInfo.ExternalScriptDependencies
        }

        if(-not $Tags)
        {
            $Tags = $psscriptInfo.Tags
        }

        if(-not $ProjectUri)
        {
            $ProjectUri = $psscriptInfo.ProjectUri
        }

        if(-not $LicenseUri)
        {
            $LicenseUri = $psscriptInfo.LicenseUri
        }

        if(-not $IconUri)
        {
            $IconUri = $psscriptInfo.IconUri
        }

        if(-not $ReleaseNotes)
        {
            $ReleaseNotes = $psscriptInfo.ReleaseNotes
        }

        $PSScriptInfoString = Get-PSScriptInfoString -Version $Version `
                                                     -Author $Author `
                                                     -CompanyName $CompanyName `
                                                     -Copyright $Copyright `
                                                     -ExternalModuleDependencies $ExternalModuleDependencies `
                                                     -RequiredScripts $RequiredScripts `
                                                     -ExternalScriptDependencies $ExternalScriptDependencies `
                                                     -Tags $Tags `
                                                     -ProjectUri $ProjectUri `
                                                     -LicenseUri $LicenseUri `
                                                     -IconUri $IconUri `
                                                     -ReleaseNotes $ReleaseNotes
                                                     
        $requiresStrings = Get-RequiresString -RequiredModules $RequiredModules
        
        $DescriptionValue = if($Description) {$Description} else {$psscriptInfo.Description}
        $ScriptCommentHelpInfoString = Get-ScriptCommentHelpInfoString -Description $DescriptionValue

        $ScriptMetadataString = $PSScriptInfoString
        $ScriptMetadataString += "`n"

        if("$requiresStrings".Trim())
        {
            $ScriptMetadataString += "`n"
            $ScriptMetadataString += $requiresStrings -join "`n"
            $ScriptMetadataString += "`n"
        }

        $ScriptMetadataString += "`n"
        $ScriptMetadataString += $ScriptCommentHelpInfoString
        $ScriptMetadataString += "`nParam()`n`n"
        if(-not $ScriptMetadataString)
        {
            return
        }
        
        $tempScriptFilePath = Microsoft.PowerShell.Management\Join-Path -Path $env:TEMP -ChildPath "$(Get-Random).ps1"
        
        try
        {
            # First create a new script file with new script metadata to ensure that updated values are valid.
            Microsoft.PowerShell.Management\Set-Content -Value $ScriptMetadataString -Path $tempScriptFilePath -Force -WhatIf:$false -Confirm:$false

            $scriptInfo = Test-ScriptFile -Path $tempScriptFilePath

            if(-not $scriptInfo)
            {
                # Above Test-ScriptFile cmdlet writes the error
                return
            }

            [System.Management.Automation.Language.Token[]]$tokens = $null;
            [System.Management.Automation.Language.ParseError[]]$errors = $null;
            $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptFilePath, ([ref]$tokens), ([ref]$errors))

            # Update PSScriptInfo and #Requires
            $CommentTokens = $tokens | Microsoft.PowerShell.Core\Where-Object {$_.Kind -eq 'Comment'}

            $psscriptInfoComments = $CommentTokens | 
                                        Microsoft.PowerShell.Core\Where-Object { $_.Extent.Text -match "<#PSScriptInfo" } | 
                                            Microsoft.PowerShell.Utility\Select-Object -First 1

            if(-not $psscriptInfoComments)
            {
                $errorMessage = ($LocalizedData.MissingPSScriptInfo -f $scriptFilePath)
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage $errorMessage `
                            -ErrorId "MissingPSScriptInfo" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ExceptionObject $scriptFilePath `
                            -ErrorCategory InvalidArgument
                return
            }

            # Ensure that metadata is replaced at the correct location and should not corrupt the existing script file.

            # Remove the lines between below lines and add the new PSScriptInfo and new #Requires statements
            # ($psscriptInfoComments.Extent.StartLineNumber - 1)
            # ($psscriptInfoComments.Extent.EndLineNumber - 1)
            $tempContents = @()
            $IsNewPScriptInfoAdded = $false
            $scriptFileContents = Microsoft.PowerShell.Management\Get-Content -Path $scriptFilePath

            for($i = 0; $i -lt $scriptFileContents.Count; $i++)
            {
               $line = $scriptFileContents[$i]
               if(($i -ge ($psscriptInfoComments.Extent.StartLineNumber - 1)) -and
                  ($i -le ($psscriptInfoComments.Extent.EndLineNumber - 1)))
               {
                   if(-not $IsNewPScriptInfoAdded)
                   {
                       $PSScriptInfoString = $PSScriptInfoString.TrimStart()
                       $requiresStrings = $requiresStrings.TrimEnd()

                       $tempContents += "$PSScriptInfoString `n`n$($requiresStrings -join "`n")"
                       $IsNewPScriptInfoAdded = $true
                   }
               }
               elseif($line -notmatch "\s*#Requires\s+-Module")
               {
                   # Add the existing lines if they are not part of PSScriptInfo comment or not containing #Requires -Module statements.
                   $tempContents += $line
               }
            }

            Microsoft.PowerShell.Management\Set-Content -Value $tempContents -Path $tempScriptFilePath -Force -WhatIf:$false -Confirm:$false

            $scriptInfo = Test-ScriptFile -Path $tempScriptFilePath

            if(-not $scriptInfo)
            {
                # Above Test-ScriptFile cmdlet writes the error
                return
            }
            
            # Now update the Description value if a new is specified.
            if($Description)
            {
                $tempContents = @()
                $IsDescriptionAdded = $false
                
                $IsDescriptionBeginFound = $false
                $scriptFileContents = Microsoft.PowerShell.Management\Get-Content -Path $tempScriptFilePath

                for($i = 0; $i -lt $scriptFileContents.Count; $i++)
                {
                   $line = $scriptFileContents[$i]

                   if(-not $IsDescriptionAdded)
                   {
                        if(-not $IsDescriptionBeginFound)
                        {
                            if($line.Trim().StartsWith(".DESCRIPTION", [System.StringComparison]::OrdinalIgnoreCase))
                            {
                               $IsDescriptionBeginFound = $true
                            }
                            else
                            {
                                $tempContents += $line
                            }
                        }
                        else
                        {
                            # Description begin has found
                            # Skip the old description lines until description end is found

                            if($line.Trim().StartsWith("#>", [System.StringComparison]::OrdinalIgnoreCase) -or 
                               $line.Trim().StartsWith(".", [System.StringComparison]::OrdinalIgnoreCase))
                            {
                               $tempContents += ".DESCRIPTION `n$($Description -join "`n")`n"
                               $IsDescriptionAdded = $true
                               $tempContents += $line
                            }      
                        }
                   }
                   else
                   {
                       $tempContents += $line
                   }
                }

                Microsoft.PowerShell.Management\Set-Content -Value $tempContents -Path $tempScriptFilePath -Force -WhatIf:$false -Confirm:$false

                $scriptInfo = Test-ScriptFile -Path $tempScriptFilePath

                if(-not $scriptInfo)
                {
                    # Above Test-ScriptFile cmdlet writes the error
                    return
                }
            }

            if($Force -or $PSCmdlet.ShouldProcess($Path, ($LocalizedData.UpdateScriptFilewhatIfMessage -f $Path) ))
    	    {
                Microsoft.PowerShell.Management\Copy-Item -Path $tempScriptFilePath -Destination $Path -Force -WhatIf:$false -Confirm:$false

                if($PassThru)
                {
                    $ScriptMetadataString
                }
            }
        }
        finally
        {
            Microsoft.PowerShell.Management\Remove-Item -Path $tempScriptFilePath -Force -WhatIf:$false -Confirm:$false -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
        }
    }
}

function Get-RequiresString
{
    [CmdletBinding()]
    Param
    (
        [Parameter()]
        [Object[]]
        $RequiredModules
    )

    Process
    {
        if($RequiredModules)
        {
            $RequiredModuleStrings = @()

            foreach($requiredModuleObject in $RequiredModules)
            {
                if($requiredModuleObject.GetType().ToString() -eq 'System.Collections.Hashtable')
                {
                    if(($requiredModuleObject.Keys.Count -eq 1) -and 
                        (Microsoft.PowerShell.Utility\Get-Member -InputObject $requiredModuleObject -Name 'ModuleName'))
                    {
                        $RequiredModuleStrings += $requiredModuleObject['ModuleName'].ToString()
                    }
                    else
                    {
                        $moduleSpec = [Microsoft.PowerShell.Commands.ModuleSpecification]::new($requiredModuleObject)
                        if (-not (Microsoft.PowerShell.Utility\Get-Variable -Name moduleSpec -ErrorAction SilentlyContinue))
                        {
                            return
                        }

                        $keyvalueStrings = $requiredModuleObject.Keys | Microsoft.PowerShell.Core\ForEach-Object {"$_ = '$( $requiredModuleObject[$_])'"}
                        $RequiredModuleStrings += "@{$($keyvalueStrings -join '; ')}"
                    }
                }
                else
                {
                    $RequiredModuleStrings += $requiredModuleObject.ToString()
                }
            }

            $hashRequiresStrings = $RequiredModuleStrings | 
                                       Microsoft.PowerShell.Core\ForEach-Object { "#Requires -Module $_" }
        
            return $hashRequiresStrings
        }
    }
}

function Get-PSScriptInfoString
{
    [CmdletBinding(PositionalBinding=$false)]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [Version]
        $Version,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Author,

        [Parameter()] 
        [String]
        $CompanyName,

        [Parameter()]
        [string]
        $Copyright,

        [Parameter()]
        [String[]]
        $ExternalModuleDependencies,

        [Parameter()]
        [string[]]
        $RequiredScripts,

        [Parameter()]
        [String[]]
        $ExternalScriptDependencies,

        [Parameter()]
        [string[]]
        $Tags,

        [Parameter()]
        [Uri]
        $ProjectUri,

        [Parameter()]
        [Uri]
        $LicenseUri,

        [Parameter()]
        [Uri]
        $IconUri,

        [Parameter()]
        [string[]]
        $ReleaseNotes
    )

    Process
    {
        $PSScriptInfoString = @"

<#PSScriptInfo

.VERSION $Version

.AUTHOR $Author

.COMPANYNAME $CompanyName

.COPYRIGHT $Copyright

.TAGS $Tags

.LICENSEURI $LicenseUri

.PROJECTURI $ProjectUri

.ICONURI $IconUri

.EXTERNALMODULEDEPENDENCIES $($ExternalModuleDependencies -join ',')

.REQUIREDSCRIPTS $($RequiredScripts -join ',')

.EXTERNALSCRIPTDEPENDENCIES $($ExternalScriptDependencies -join ',')

.RELEASENOTES
$($ReleaseNotes -join "`n")

#>
"@
        return $PSScriptInfoString
    }
}

function Get-ScriptCommentHelpInfoString
{
    [CmdletBinding(PositionalBinding=$false)]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Description,

        [Parameter()]
        [string]
        $Synopsis,

        [Parameter()]
        [string[]]
        $Example,

        [Parameter()]
        [string[]]
        $Inputs,

        [Parameter()]
        [string[]]
        $Outputs,

        [Parameter()]
        [string[]]
        $Notes,

        [Parameter()]
        [string[]]
        $Link,

        [Parameter()]
        [string]
        $Component,

        [Parameter()]
        [string]
        $Role,

        [Parameter()]
        [string]
        $Functionality
    )

    Process
    {
        $ScriptCommentHelpInfoString = "<# `n`n.DESCRIPTION `n $Description `n`n"

        if("$Synopsis".Trim())
        {
            $ScriptCommentHelpInfoString += ".SYNOPSIS `n$Synopsis `n`n"
        }

        if("$Example".Trim())
        {
            $Example | ForEach-Object {
                           if($_)
                           {
                               $ScriptCommentHelpInfoString += ".EXAMPLE `n$_ `n`n"
                           }
                       } 
        }

        if("$Inputs".Trim())
        {
            $Inputs |  ForEach-Object {
                           if($_)
                           {
                               $ScriptCommentHelpInfoString += ".INPUTS `n$_ `n`n"
                           }
                       } 
        }

        if("$Outputs".Trim())
        {
            $Outputs |  ForEach-Object {
                           if($_)
                           {
                               $ScriptCommentHelpInfoString += ".OUTPUTS `n$_ `n`n"
                           }
                       } 
        }

        if("$Notes".Trim())
        {
            $ScriptCommentHelpInfoString += ".NOTES `n$($Notes -join "`n") `n`n"
        }

        if("$Link".Trim())
        {
            $Link |  ForEach-Object {
                         if($_)
                         {
                              $ScriptCommentHelpInfoString += ".LINK `n$_ `n`n"
                         }
                     } 
        }

        if("$Component".Trim())
        {
            $ScriptCommentHelpInfoString += ".COMPONENT `n$($Component -join "`n") `n`n"
        }

        if("$Role".Trim())
        {
            $ScriptCommentHelpInfoString += ".ROLE `n$($Role -join "`n") `n`n"
        }

        if("$Functionality".Trim())
        {
            $ScriptCommentHelpInfoString += ".FUNCTIONALITY `n$($Functionality -join "`n") `n`n"
        }

        $ScriptCommentHelpInfoString += "#> `n"

        return $ScriptCommentHelpInfoString
    }
}

#endregion *-ScriptFile cmdlets

#region Utility functions
function ToUpper
{
    param([string]$str)
    return $script:TextInfo.ToUpper($str)
}

function Resolve-PathHelper
{
    param 
    (
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $path,

        [Parameter()]
        [switch]
        $isLiteralPath,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.PSCmdlet]
        $callerPSCmdlet
    )
    
    $resolvedPaths =@()

    foreach($currentPath in $path)
    {
        try
        {
            if($isLiteralPath)
            {
                $currentResolvedPaths = Microsoft.PowerShell.Management\Resolve-Path -LiteralPath $currentPath -ErrorAction Stop
            }
            else
            {
                $currentResolvedPaths = Microsoft.PowerShell.Management\Resolve-Path -Path $currentPath -ErrorAction Stop
            }
        }
        catch
        {
            $errorMessage = ($LocalizedData.PathNotFound -f $currentPath)
            ThrowError  -ExceptionName "System.InvalidOperationException" `
                        -ExceptionMessage $errorMessage `
                        -ErrorId "PathNotFound" `
                        -CallerPSCmdlet $callerPSCmdlet `
                        -ErrorCategory InvalidOperation
        }

        foreach($currentResolvedPath in $currentResolvedPaths)
        {
            $resolvedPaths += $currentResolvedPath.ProviderPath
        }
    }

    $resolvedPaths
}

function Check-PSGalleryApiAvailability
{
    param
    (
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $PSGalleryV2ApiUri,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $PSGalleryV3ApiUri
    )   

    # check internet availability first
    $connected = $false
    if(Get-Command Microsoft.PowerShell.Management\Test-Connection -ErrorAction SilentlyContinue)
    {        
        $connected = Microsoft.PowerShell.Management\Test-Connection -ComputerName "www.microsoft.com" -Count 1 -Quiet
    }
    else
    {
        $connected = NetTCPIP\Test-NetConnection -ComputerName "www.microsoft.com" -InformationLevel Quiet
    }
    if ( -not $connected)
    {
        return
    }

    $statusCode_v2 = $null
    $resolvedUri_v2 = $null
    $statusCode_v3 = $null
    $resolvedUri_v3 = $null

    # ping V2
    $res_v2 = Ping-Endpoint -Endpoint $PSGalleryV2ApiUri 
    if ($res_v2.ContainsKey($Script:ResponseUri))
    {
        $resolvedUri_v2 = $res_v2[$Script:ResponseUri]
    }
    if ($res_v2.ContainsKey($Script:StatusCode))
    {
        $statusCode_v2 = $res_v2[$Script:StatusCode]
    } 
    

    # ping V3
    $res_v3 = Ping-Endpoint -Endpoint $PSGalleryV3ApiUri
    if ($res_v3.ContainsKey($Script:ResponseUri))
    {
        $resolvedUri_v3 = $res_v3[$Script:ResponseUri]
    }
    if ($res_v3.ContainsKey($Script:StatusCode))
    {
        $statusCode_v3 = $res_v3[$Script:StatusCode]
    } 
    

    $Script:PSGalleryV2ApiAvailable = (($statusCode_v2 -eq 200) -and ($resolvedUri_v2))
    $Script:PSGalleryV3ApiAvailable = (($statusCode_v3 -eq 200) -and ($resolvedUri_v3))
    $Script:PSGalleryApiChecked = $true
}

function Get-PSGalleryApiAvailability
{
    param
    (
        [Parameter()]
        [string[]]
        $Repository
    )

    # skip if repository is null or not PSGallery
    if ( -not $Repository)
    {
        return
    }

    if ($Repository -notcontains $Script:PSGalleryModuleSource )
    {
        return
    }

    # run check only once 
    if( -not $Script:PSGalleryApiChecked)
    {
        $null = Check-PSGalleryApiAvailability -PSGalleryV2ApiUri $Script:PSGallerySourceUri -PSGalleryV3ApiUri $Script:PSGalleryV3SourceUri
    }

    if ( -not $Script:PSGalleryV2ApiAvailable )
    {
        if ($Script:PSGalleryV3ApiAvailable)
        {
            ThrowError -ExceptionName "System.InvalidOperationException" `
                       -ExceptionMessage $LocalizedData.PSGalleryApiV2Discontinued `
                       -ErrorId "PSGalleryApiV2Discontinued" `
                       -CallerPSCmdlet $PSCmdlet `
                       -ErrorCategory InvalidOperation
        }
        else 
        {
            # both APIs are down, throw error
            ThrowError -ExceptionName "System.InvalidOperationException" `
                       -ExceptionMessage $LocalizedData.PowerShellGalleryUnavailable `
                       -ErrorId "PowerShellGalleryUnavailable" `
                       -CallerPSCmdlet $PSCmdlet `
                       -ErrorCategory InvalidOperation
        }

    }
    else 
    {
        if ($Script:PSGalleryV3ApiAvailable)
        {
            Write-Warning -Message $LocalizedData.PSGalleryApiV2Deprecated
            return
        }
    }

    # if V2 is available and V3 is not available, do nothing  
}

function WebRequestApisAvailable
{
    $webRequestApiAvailable = $false
    try 
    {
        [System.Net.WebRequest]
        $webRequestApiAvailable = $true
    } 
    catch 
    {
    }
    return $webRequestApiAvailable
}

function Ping-Endpoint
{
    param
    (
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Endpoint
    )   
        
    $results = @{}

    if(WebRequestApisAvailable)
    {
        $iss = [System.Management.Automation.Runspaces.InitialSessionState]::Create()
        $iss.types.clear()
        $iss.formats.clear()
        $iss.LanguageMode = "FullLanguage"

        $WebRequestcmd =  @'
            try
            {{
                $request = [System.Net.WebRequest]::Create("{0}")
                $request.Method = 'GET'
                $request.Timeout = 30000
                $response = [System.Net.HttpWebResponse]$request.GetResponse()             
                $response
                $response.Close()
            }}
            catch [System.Net.WebException]
            {{
                "Error:System.Net.WebException"
            }} 
'@ -f $EndPoint

        $ps = [powershell]::Create($iss).AddScript($WebRequestcmd)
        $response = $ps.Invoke()
        $ps.dispose()

        if ($response -ne "Error:System.Net.WebException")
        {
            $results.Add($Script:ResponseUri,$response.ResponseUri.ToString())
            $results.Add($Script:StatusCode,$response.StatusCode.value__)
        }        
    }
    else
    {
        $response = $null
        try
        {
            $httpClient = New-Object 'System.Net.Http.HttpClient'
            $response = $httpclient.GetAsync($endpoint)          
        }
        catch
        {            
        } 

        if ($response -ne $null -and $response.result -ne $null)
        {        
            $results.Add($Script:ResponseUri,$response.Result.RequestMessage.RequestUri.AbsoluteUri.ToString())
            $results.Add($Script:StatusCode,$response.result.StatusCode.value__)            
        }
    }
    return $results
}

function Validate-VersionParameters
{
    Param(
        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.PSCmdlet]
        $CallerPSCmdlet,

        [Parameter()]
        [String[]]
        $Name,

        [Parameter()]
        [Version]
        $MinimumVersion,

        [Parameter()]
        [Version]
        $RequiredVersion,

        [Parameter()]
        [Version]
        $MaximumVersion
    )

    if($RequiredVersion -and ($MinimumVersion -or $MaximumVersion))
    {
        ThrowError -ExceptionName "System.ArgumentException" `
                   -ExceptionMessage $LocalizedData.VersionRangeAndRequiredVersionCannotBeSpecifiedTogether `
                   -ErrorId "VersionRangeAndRequiredVersionCannotBeSpecifiedTogether" `
                   -CallerPSCmdlet $CallerPSCmdlet `
                   -ErrorCategory InvalidArgument
    }

    if($RequiredVersion -or $MinimumVersion -or $MaximumVersion)
    {
        if(-not $Name -or $Name.Count -ne 1 -or (Test-WildcardPattern -Name $Name[0]))
        {
            ThrowError -ExceptionName "System.ArgumentException" `
                       -ExceptionMessage $LocalizedData.VersionParametersAreAllowedOnlyWithSingleName `
                       -ErrorId "VersionParametersAreAllowedOnlyWithSingleName" `
                       -CallerPSCmdlet $CallerPSCmdlet `
                       -ErrorCategory InvalidArgument
        }
    }

    if($MinimumVersion -and $MaximumVersion -and ($MinimumVersion -gt $MaximumVersion))
    {
        $Message = $LocalizedData.MinimumVersionIsGreaterThanMaximumVersion -f ($MinimumVersion, $MaximumVersion)
        ThrowError -ExceptionName "System.ArgumentException" `
                    -ExceptionMessage $Message `
                    -ErrorId "MinimumVersionIsGreaterThanMaximumVersion" `
                    -CallerPSCmdlet $CallerPSCmdlet `
                    -ErrorCategory InvalidArgument
    }

    return $true
}

function Set-ModuleSourcesVariable
{
    [CmdletBinding()]
    param([switch]$Force)

    if(-not $script:PSGetModuleSources -or $Force)
    {
        $isPersistRequired = $false
        if(Microsoft.PowerShell.Management\Test-Path $script:PSGetModuleSourcesFilePath)
        {
            $script:PSGetModuleSources = DeSerialize-PSObject -Path $script:PSGetModuleSourcesFilePath
        }
        else
        {
            $script:PSGetModuleSources = [ordered]@{}
        }

        if(-not $script:PSGetModuleSources.Contains($Script:PSGalleryModuleSource))
        {
            $psgalleryLocation = Resolve-Location -Location $Script:PSGallerySourceUri `
                                                  -LocationParameterName 'SourceLocation' `
                                                  -ErrorAction SilentlyContinue `
                                                  -WarningAction SilentlyContinue

            $scriptSourceLocation = Resolve-Location -Location $Script:PSGalleryScriptSourceUri `
                                                     -LocationParameterName 'ScriptSourceLocation' `
                                                     -ErrorAction SilentlyContinue `
                                                     -WarningAction SilentlyContinue
            if($psgalleryLocation)
            {
                $moduleSource = Microsoft.PowerShell.Utility\New-Object PSCustomObject -Property ([ordered]@{
                        Name = $Script:PSGalleryModuleSource
                        SourceLocation =  $psgalleryLocation
                        PublishLocation = $Script:PSGalleryPublishUri
                        ScriptSourceLocation = $scriptSourceLocation
                        ScriptPublishLocation = $Script:PSGalleryPublishUri
                        Trusted=$false
                        Registered=$true
                        InstallationPolicy = 'Untrusted'
                        PackageManagementProvider=$script:NuGetProviderName
                        ProviderOptions = @{}
                    })

                $moduleSource.PSTypeNames.Insert(0, "Microsoft.PowerShell.Commands.PSRepository")
                $script:PSGetModuleSources.Add($Script:PSGalleryModuleSource, $moduleSource)
            }
        }

        # Already registed repositories may not have the ScriptSourceLocation property, try to populate it from the existing SourceLocation
        # Also populate the PublishLocation and ScriptPublishLocation from the SourceLocation if PublishLocation is empty/null.
        # 
        $script:PSGetModuleSources.Keys | Microsoft.PowerShell.Core\ForEach-Object { 
                                              $moduleSource = $script:PSGetModuleSources[$_]

                                              if(-not (Get-Member -InputObject $moduleSource -Name $script:ScriptSourceLocation))
                                              {
                                                  $scriptSourceLocation = Get-ScriptSourceLocation -Location $moduleSource.SourceLocation

                                                  Microsoft.PowerShell.Utility\Add-Member -InputObject $script:PSGetModuleSources[$_] `
                                                                                          -MemberType NoteProperty `
                                                                                          -Name $script:ScriptSourceLocation `
                                                                                          -Value $scriptSourceLocation

                                                  if(Get-Member -InputObject $moduleSource -Name $script:PublishLocation)
                                                  {
                                                      if(-not $moduleSource.PublishLocation)
                                                      {
                                                          $script:PSGetModuleSources[$_].PublishLocation = Get-PublishLocation -Location $moduleSource.SourceLocation
                                                      }

                                                      Microsoft.PowerShell.Utility\Add-Member -InputObject $script:PSGetModuleSources[$_] `
                                                                                              -MemberType NoteProperty `
                                                                                              -Name $script:ScriptPublishLocation `
                                                                                              -Value $moduleSource.PublishLocation
                                                  }

                                                  $isPersistRequired = $true
                                              }
                                          }
        
        if($isPersistRequired)
        {
            Save-ModuleSources
        }
    }   
}

function Get-PackageManagementProviderName
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $Location
    )

    $PackageManagementProviderName = $null
    $loc = Get-LocationString -LocationUri $Location

    $providers = PackageManagement\Get-PackageProvider | Where-Object { $_.Features.ContainsKey($script:SupportsPSModulesFeatureName) }

    foreach($provider in $providers)
    {
        # Skip the PSModule provider
        if($provider.ProviderName -eq $script:PSModuleProviderName)
        {
            continue
        }

        $packageSource = Get-PackageSource -Location $loc -Provider $provider.ProviderName  -ErrorAction SilentlyContinue 
                    
        if($packageSource)
        {
            $PackageManagementProviderName = $provider.ProviderName
            break
        }
    }

    return $PackageManagementProviderName
}

function Get-ProviderName
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true)]
        [PSCustomObject]
        $PSCustomObject
    )

    $providerName = $script:NuGetProviderName

    if((Get-Member -InputObject $PSCustomObject -Name PackageManagementProvider))
    {
        $providerName = $PSCustomObject.PackageManagementProvider
    }

    return $providerName
}

function Get-DynamicParameters
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $Location,

        [Parameter(Mandatory=$true)]
        [REF]
        $PackageManagementProvider
    )

    $paramDictionary = New-Object System.Management.Automation.RuntimeDefinedParameterDictionary
    $dynamicOptions = $null

    $loc = Get-LocationString -LocationUri $Location

    if(-not $loc)
    {
        return $paramDictionary
    }

    # Ping and resolve the specified location
    $loc = Resolve-Location -Location $loc `
                            -LocationParameterName 'Location' `
                            -ErrorAction SilentlyContinue `
                            -WarningAction SilentlyContinue
    if(-not $loc)
    {
        return $paramDictionary
    }

    $providers = PackageManagement\Get-PackageProvider | Where-Object { $_.Features.ContainsKey($script:SupportsPSModulesFeatureName) }
            
    if ($PackageManagementProvider.Value)
    {
        # Skip the PSModule provider
        if($PackageManagementProvider.Value -ne $script:PSModuleProviderName)
        {
            $SelectedProvider = $providers | Where-Object {$_.ProviderName -eq $PackageManagementProvider.Value}

            if($SelectedProvider)
            {
                $res = Get-PackageSource -Location $loc -Provider $PackageManagementProvider.Value -ErrorAction SilentlyContinue 
            
                if($res)
                {
                    $dynamicOptions = $SelectedProvider.DynamicOptions
                }
            }
        }
    }
    else
    {
        $PackageManagementProvider.Value = Get-PackageManagementProviderName -Location $Location
        if($PackageManagementProvider.Value)
        {
            $provider = $providers | Where-Object {$_.ProviderName -eq $PackageManagementProvider.Value}
            $dynamicOptions = $provider.DynamicOptions
        }
    }

    foreach ($option in $dynamicOptions)
    {
        # Skip the Destination parameter
        if( $option.IsRequired -and 
            ($option.Name -eq "Destination") )
        {
            continue
        }

        $paramAttribute = New-Object System.Management.Automation.ParameterAttribute
        $paramAttribute.Mandatory = $option.IsRequired

        $message = $LocalizedData.DynamicParameterHelpMessage -f ($option.Name, $PackageManagementProvider.Value, $loc, $option.Name)
        $paramAttribute.HelpMessage = $message

        $attributeCollection = new-object System.Collections.ObjectModel.Collection[System.Attribute]
        $attributeCollection.Add($paramAttribute)

        $ageParam = New-Object System.Management.Automation.RuntimeDefinedParameter($option.Name,
                                                                                    $script:DynamicOptionTypeMap[$option.Type.value__],
                                                                                    $attributeCollection)
        $paramDictionary.Add($option.Name, $ageParam)
    }

    return $paramDictionary
}

function New-PSGetItemInfo
{
    param
    (
        [Parameter(Mandatory=$true)]
        $SoftwareIdenties,

        [Parameter()]
        $PackageManagementProviderName,

        [Parameter()]
        [string]
        $SourceLocation,

        [Parameter(Mandatory=$true)]
        [string]
        $Type,

        [Parameter()]
        [string]
        $InstalledLocation
    )

    foreach($swid in $SoftwareIdenties)
    {

        if($SourceLocation)
        {
            $sourceName = (Get-SourceName -Location $SourceLocation)
        }
        else
        {
            # First get the source name from the Metadata
            # if not exists, get the source name from $swid.Source
            # otherwise default to $swid.Source  
            $sourceName = (Get-First $swid.Metadata["SourceName"])

            if(-not $sourceName)
            {
                $sourceName = (Get-SourceName -Location $swid.Source)
            }

            if(-not $sourceName)
            {
                $sourceName = $swid.Source
            }

            $SourceLocation = $swid.Source
        }

        $published = (Get-First $swid.Metadata["published"])
        $PublishedDate = New-Object System.DateTime

        $tags = (Get-First $swid.Metadata["tags"]) -split " "
        $userTags = @()
        $exportedDscResources = @()
        $exportedCommands = @()
        $exportedCmdlets = @()
        $exportedFunctions = @()
        $exportedWorkflows = @()
        $PSGetFormatVersion = $null

        ForEach($tag in $tags)
        {
            if(-not $tag.Trim())
            {
                continue
            }

            $parts = $tag -split "_",2
            if($parts.Count -ne 2)
            {
                $userTags += $tag
                continue
            }

            Switch($parts[0])
            {
                $script:Command            { $exportedCommands += $parts[1]; break }
                $script:DscResource        { $exportedDscResources += $parts[1]; break }
                $script:Cmdlet             { $exportedCmdlets += $parts[1]; break }
                $script:Function           { $exportedFunctions += $parts[1]; break }
                $script:Workflow           { $exportedWorkflows += $parts[1]; break }
                $script:PSGetFormatVersion { $PSGetFormatVersion = $parts[1]; break }
                $script:Includes           { break }
                Default                    { $userTags += $tag; break }
            }
        }

        $ArtifactDependencies = @()
        Foreach ($dependencyString in $swid.Dependencies)
        {
            [Uri]$packageId = $null
            if([Uri]::TryCreate($dependencyString, [System.UriKind]::Absolute, ([ref]$packageId)))
            {
                $segments = $packageId.Segments
                $Version = $null
                $DependencyName = $null
                if ($segments)   
                {
                    $DependencyName = [Uri]::UnescapeDataString($segments[0].Trim('/', '\'))
                    $Version = if($segments.Count -gt 1){[Uri]::UnescapeDataString($segments[1])}
                }

                $dep = [ordered]@{
                            Name=$DependencyName
                        }

                if($Version)
                {
                    # Required/exact version is represented in NuGet as "[2.0]"
                    if ($Version -match "\[+[0-9.]+\]")
                    {
                        $dep["RequiredVersion"] = $Version.Trim('[', ']')
                    }
                    else
                    {
                        $dep['MinimumVersion'] = $Version
                    }
                }
                
                $dep["CanonicalId"]=$dependencyString

                $ArtifactDependencies += $dep
            }
        }

        if($userTags -contains 'PSModule')
        {
            $Type = $script:PSArtifactTypeModule
        }
        elseif($userTags -contains 'PSScript')
        {
            $Type = $script:PSArtifactTypeScript
        }

        $PSGetItemInfo = Microsoft.PowerShell.Utility\New-Object PSCustomObject -Property ([ordered]@{
                Name = $swid.Name
                Version = [Version]$swid.Version
                Type = $Type    
                Description = (Get-First $swid.Metadata["description"])
                Author = (Get-EntityName -SoftwareIdentity $swid -Role "author")
                CompanyName = (Get-EntityName -SoftwareIdentity $swid -Role "owner")
                Copyright = (Get-First $swid.Metadata["copyright"])
                PublishedDate = if([System.DateTime]::TryParse($published, ([ref]$PublishedDate))){$PublishedDate};
                LicenseUri = (Get-UrlFromSwid -SoftwareIdentity $swid -UrlName "license")
                ProjectUri = (Get-UrlFromSwid -SoftwareIdentity $swid -UrlName "project")
                IconUri = (Get-UrlFromSwid -SoftwareIdentity $swid -UrlName "icon")
                Tags = $userTags

                Includes = @{
                                DscResource = $exportedDscResources
                                Command     = $exportedCommands
                                Cmdlet      = $exportedCmdlets
                                Function    = $exportedFunctions
                                Workflow    = $exportedWorkflows
                            }

                PowerShellGetFormatVersion=[Version]$PSGetFormatVersion

                ReleaseNotes = (Get-First $swid.Metadata["releaseNotes"])

                Dependencies = $ArtifactDependencies

                RepositorySourceLocation = $SourceLocation
                Repository = $sourceName
                PackageManagementProvider = if($PackageManagementProviderName) { $PackageManagementProviderName } else { (Get-First $swid.Metadata["PackageManagementProvider"]) }
            })

        if(-not $InstalledLocation)
        {
            $InstalledLocation = (Get-First $swid.Metadata[$script:InstalledLocation])
        }

        if($InstalledLocation)
        {
            Microsoft.PowerShell.Utility\Add-Member -InputObject $PSGetItemInfo -MemberType NoteProperty -Name $script:InstalledLocation -Value $InstalledLocation
        }

        $PSGetItemInfo.PSTypeNames.Insert(0, "Microsoft.PowerShell.Commands.PSRepositoryItemInfo")
        $PSGetItemInfo
    }
}

function Get-SourceName
{
    [CmdletBinding()]
    [OutputType("string")]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Location
    )

    Set-ModuleSourcesVariable

    foreach($psModuleSource in $script:PSGetModuleSources.Values)
    {
        if(($psModuleSource.SourceLocation -eq $Location) -or
           ($psModuleSource.ScriptSourceLocation -eq $Location))
        {
            return $psModuleSource.Name
        }
    }
}

function Get-UrlFromSwid
{
    param
    (
        [Parameter(Mandatory=$true)]
        $SoftwareIdentity,

        [Parameter(Mandatory=$true)]
        $UrlName
    )
    
    foreach($link in $SoftwareIdentity.Links)
    {
        if( $link.Relationship -eq $UrlName)
        {
            return $link.HRef
        }
    }

    return $null
}

function Get-EntityName
{
    param
    (
        [Parameter(Mandatory=$true)]
        $SoftwareIdentity,

        [Parameter(Mandatory=$true)]
        $Role
    )

    foreach( $entity in $SoftwareIdentity.Entities )
    {
        if( $entity.Role -eq $Role)
        {
            $entity.Name
        }
    }
}

function Install-NuGetClientBinaries
{
    [CmdletBinding()]
    param()

    if($script:NuGetClient -and (Microsoft.PowerShell.Management\Test-Path $script:NuGetClient))
    {
        return
    }

    # Bootstrap NuGet provider if it is not available
    $nugetProvider = PackageManagement\Get-PackageProvider -ErrorAction SilentlyContinue -WarningAction SilentlyContinue | Microsoft.PowerShell.Core\Where-Object {$_.Name -eq $script:NuGetProviderName}

    [bool]$needToBootStrap = $true

    if($nugetProvider)
    {
        # on nano we don't need the exe
        if ($script:isNanoServer) {
            $needToBootStrap = $false
        }
        elseif ($nugetProvider.Features.Exe -and 
            (Microsoft.PowerShell.Management\Test-Path $nugetProvider.Features.Exe)) {
            $script:NuGetClient = $nugetProvider.Features.Exe
            $needToBootStrap = $false
        }
    }

    if ($needToBootStrap)
    {
        $ShouldContinueQueryMessage = $LocalizedData.InstallNuGetBinariesShouldContinueQuery -f @($script:NuGetBinaryProgramDataPath,$script:NuGetBinaryLocalAppDataPath)

        if($PSCmdlet.ShouldContinue($ShouldContinueQueryMessage, $LocalizedData.InstallNuGetBinariesShouldContinueCaption))
        {
            Write-Verbose -Message $LocalizedData.DownloadingNugetBinaries

            # Bootstrap the NuGet provider
            $nugetProvider = PackageManagement\Get-PackageProvider -Name NuGet -Force

            if($nugetProvider -and 
                $nugetProvider.Features.Exe -and 
                (Microsoft.PowerShell.Management\Test-Path $nugetProvider.Features.Exe))
            {
                $script:NuGetClient = $nugetProvider.Features.Exe
            }
        }
    }

    # for nano server, we only need the nuget provider
    if ($script:isNanoServer -and $nugetProvider) {
        return
    }

    if(-not $script:NuGetClient -or 
       -not (Microsoft.PowerShell.Management\Test-Path $script:NuGetClient))
    {
        $message = $LocalizedData.CouldNotInstallNuGetBinaries -f @($script:NuGetBinaryProgramDataPath,$script:NuGetBinaryLocalAppDataPath)
        ThrowError -ExceptionName "System.InvalidOperationException" `
                    -ExceptionMessage $message `
                    -ErrorId "CouldNotInstallNuGetBinaries" `
                    -CallerPSCmdlet $PSCmdlet `
                    -ErrorCategory InvalidOperation
    }
}

# Check if current user is running with elevated privileges
function Test-RunningAsElevated
{
    [CmdletBinding()]
    [OutputType([bool])]
    Param()

    $wid=[System.Security.Principal.WindowsIdentity]::GetCurrent()
    $prp=new-object System.Security.Principal.WindowsPrincipal($wid)
    $adm=[System.Security.Principal.WindowsBuiltInRole]::Administrator
    return $prp.IsInRole($adm)
}

function Get-EscapedString
{
    [CmdletBinding()]
    [OutputType([String])]
    Param
    (
        [Parameter()]
        [string]
        $ElementValue
    )

    return [System.Security.SecurityElement]::Escape($ElementValue)
}

function ValidateAndGet-ScriptDependencies
{
    param(
        [Parameter(Mandatory=$true)]
        [string]
        $Repository,

        [Parameter(Mandatory=$true)]
        [PSCustomObject]
        $DependentScriptInfo,

        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.PSCmdlet]
        $CallerPSCmdlet
    )

    $DependenciesDetails = @()

    # Validate dependent modules
    $RequiredModuleSpecification = $DependentScriptInfo.RequiredModules
    if($RequiredModuleSpecification)
    {
        ForEach($moduleSpecification in $RequiredModuleSpecification)
        {
            $ModuleName = $moduleSpecification.Name

            $FindModuleArguments = @{
                                        Repository = $Repository
                                        Verbose = $VerbosePreference
                                        ErrorAction = 'SilentlyContinue'
                                        WarningAction = 'SilentlyContinue'
                                        Debug = $DebugPreference
                                    }

            if($DependentScriptInfo.ExternalModuleDependencies -contains $ModuleName)
            {
                Write-Verbose -Message ($LocalizedData.SkippedModuleDependency -f $ModuleName)

                continue
            }

            $FindModuleArguments['Name'] = $ModuleName
            $ReqModuleInfo = @{}
            $ReqModuleInfo['Name'] = $ModuleName

            if($moduleSpecification.Version)
            {
                $FindModuleArguments['MinimumVersion'] = $moduleSpecification.Version
                $ReqModuleInfo['MinimumVersion'] = $moduleSpecification.Version
            }
            elseif((Get-Member -InputObject $moduleSpecification -Name RequiredVersion) -and $moduleSpecification.RequiredVersion)
            {            
                $FindModuleArguments['RequiredVersion'] = $moduleSpecification.RequiredVersion
                $ReqModuleInfo['RequiredVersion'] = $moduleSpecification.RequiredVersion
            }

            $psgetItemInfo = Find-Module @FindModuleArguments  | 
                                        Microsoft.PowerShell.Core\Where-Object {$_.Name -eq $ModuleName} | 
                                            Microsoft.PowerShell.Utility\Select-Object -Last 1

            if(-not $psgetItemInfo)
            {
                $message = $LocalizedData.UnableToResolveScriptDependency -f ('module', $ModuleName, $DependentScriptInfo.Name, $Repository, 'ExternalModuleDependencies')
                ThrowError -ExceptionName "System.InvalidOperationException" `
                            -ExceptionMessage $message `
                            -ErrorId "UnableToResolveScriptDependency" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ErrorCategory InvalidOperation
            }

            $DependenciesDetails += $ReqModuleInfo
        }
    }

    # Validate dependent scrips
    $RequiredScripts = $DependentScriptInfo.RequiredScripts
    if($RequiredScripts)
    {
        ForEach($requiredScript in $RequiredScripts)
        {
            $FindScriptArguments = @{
                                        Repository = $Repository
                                        Verbose = $VerbosePreference
                                        ErrorAction = 'SilentlyContinue'
                                        WarningAction = 'SilentlyContinue'
                                        Debug = $DebugPreference
                                    }

            if($DependentScriptInfo.ExternalScriptDependencies -contains $requiredScript)
            {
                Write-Verbose -Message ($LocalizedData.SkippedScriptDependency -f $requiredScript)

                continue
            }

            $FindScriptArguments['Name'] = $requiredScript
            $ReqScriptInfo = @{}
            $ReqScriptInfo['Name'] = $requiredScript

            $psgetItemInfo = Find-Script @FindScriptArguments  | 
                                        Microsoft.PowerShell.Core\Where-Object {$_.Name -eq $requiredScript} | 
                                            Microsoft.PowerShell.Utility\Select-Object -Last 1

            if(-not $psgetItemInfo)
            {
                $message = $LocalizedData.UnableToResolveScriptDependency -f ('script', $requiredScript, $DependentScriptInfo.Name, $Repository, 'ExternalScriptDependencies')
                ThrowError -ExceptionName "System.InvalidOperationException" `
                            -ExceptionMessage $message `
                            -ErrorId "UnableToResolveScriptDependency" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ErrorCategory InvalidOperation
            }

            $DependenciesDetails += $ReqScriptInfo
        }
    }

    return $DependenciesDetails
}

function ValidateAndGet-RequiredModuleDetails
{
    param(
        [Parameter()]
        $ModuleManifestRequiredModules,

        [Parameter()]
        [PSModuleInfo[]]
        $RequiredPSModuleInfos,

        [Parameter(Mandatory=$true)]
        [string]
        $Repository,

        [Parameter(Mandatory=$true)]
        [PSModuleInfo]
        $DependentModuleInfo,

        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.PSCmdlet]
        $CallerPSCmdlet
    )

    $RequiredModuleDetails = @()

    if(-not $RequiredPSModuleInfos)
    {
        return $RequiredModuleDetails
    }

    if($ModuleManifestRequiredModules)
    {
        ForEach($RequiredModule in $ModuleManifestRequiredModules)
        {
            $ModuleName = $null
            $VersionString = $null

            $ReqModuleInfo = @{}

            $FindModuleArguments = @{
                                        Repository = $Repository
                                        Verbose = $VerbosePreference
                                        ErrorAction = 'SilentlyContinue'
                                        WarningAction = 'SilentlyContinue'
                                        Debug = $DebugPreference
                                    }

            # ModuleSpecification case
            if($RequiredModule.GetType().ToString() -eq 'System.Collections.Hashtable')
            {
                $ModuleName = $RequiredModule.ModuleName

                # Version format in NuSpec:
                # "[2.0]" --> (== 2.0) Required Version
                # "2.0" --> (>= 2.0) Minimum Version
                if($RequiredModule.Keys -Contains "RequiredVersion")
                {
                    $FindModuleArguments['RequiredVersion'] = $RequiredModule.RequiredVersion
                    $ReqModuleInfo['RequiredVersion'] = $RequiredModule.RequiredVersion
                }
                elseif($RequiredModule.Keys -Contains "ModuleVersion")
                {
                    $FindModuleArguments['MinimumVersion'] = $RequiredModule.ModuleVersion
                    $ReqModuleInfo['MinimumVersion'] = $RequiredModule.ModuleVersion
                }
            }
            else
            {
                # Just module name was specified
                $ModuleName = $RequiredModule.ToString()
            }
            
            if((Get-ExternalModuleDependencies -PSModuleInfo $DependentModuleInfo) -contains $ModuleName)
            {
                Write-Verbose -Message ($LocalizedData.SkippedModuleDependency -f $ModuleName)

                continue
            }      

            # Skip this module name if it's name is not in $RequiredPSModuleInfos.
            # This is required when a ModuleName is part of the NestedModules list of the actual module.
            # $ModuleName is packaged as part of the actual module When $RequiredPSModuleInfos doesn't contain it's name.
            if($RequiredPSModuleInfos.Name -notcontains $ModuleName)
            {
                continue
            }

            $ReqModuleInfo['Name'] = $ModuleName

            # Add the dependency only if the module is available on the gallery
            # Otherwise Module installation will fail as all required modules need to be available on 
            # the same Repository
            $FindModuleArguments['Name'] = $ModuleName

            $psgetItemInfo = Find-Module @FindModuleArguments  | 
                                        Microsoft.PowerShell.Core\Where-Object {$_.Name -eq $ModuleName} | 
                                            Microsoft.PowerShell.Utility\Select-Object -Last 1

            if(-not $psgetItemInfo)
            {
                $message = $LocalizedData.UnableToResolveModuleDependency -f ($ModuleName, $DependentModuleInfo.Name, $Repository, $ModuleName, $Repository, $ModuleName, $ModuleName)
                ThrowError -ExceptionName "System.InvalidOperationException" `
                            -ExceptionMessage $message `
                            -ErrorId "UnableToResolveModuleDependency" `
                            -CallerPSCmdlet $CallerPSCmdlet `
                            -ErrorCategory InvalidOperation
            }

            $RequiredModuleDetails += $ReqModuleInfo
        }
    }
    else
    {
        # If Import-LocalizedData cmdlet was failed to read the .psd1 contents 
        # use provided $RequiredPSModuleInfos (PSModuleInfo.RequiredModules or PSModuleInfo.NestedModules of the actual dependent module)

        $FindModuleArguments = @{
                                    Repository = $Repository
                                    Verbose = $VerbosePreference
                                    ErrorAction = 'SilentlyContinue'
                                    WarningAction = 'SilentlyContinue'
                                    Debug = $DebugPreference
                                }

        ForEach($RequiredModuleInfo in $RequiredPSModuleInfos)
        {
            $ModuleName = $requiredModuleInfo.Name

            if((Get-ExternalModuleDependencies -PSModuleInfo $DependentModuleInfo) -contains $ModuleName)
            {
                Write-Verbose -Message ($LocalizedData.SkippedModuleDependency -f $ModuleName)

                continue
            }

            $FindModuleArguments['Name'] = $ModuleName
            $FindModuleArguments['MinimumVersion'] = $requiredModuleInfo.Version

            $psgetItemInfo = Find-Module @FindModuleArguments  | 
                                        Microsoft.PowerShell.Core\Where-Object {$_.Name -eq $ModuleName} | 
                                            Microsoft.PowerShell.Utility\Select-Object -Last 1

            if(-not $psgetItemInfo)
            {
                $message = $LocalizedData.UnableToResolveModuleDependency -f ($ModuleName, $DependentModuleInfo.Name, $Repository, $ModuleName, $Repository, $ModuleName, $ModuleName)
                ThrowError -ExceptionName "System.InvalidOperationException" `
                            -ExceptionMessage $message `
                            -ErrorId "UnableToResolveModuleDependency" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ErrorCategory InvalidOperation
            }

            $RequiredModuleDetails += @{
                                            Name=$_.Name
                                            MinimumVersion=$_.Version
                                       }
        }
    }

    return $RequiredModuleDetails
}

function Get-ExternalModuleDependencies
{
    Param (
        [Parameter(Mandatory=$true)]
        [PSModuleInfo]
        $PSModuleInfo
    )

    if($PSModuleInfo.PrivateData -and 
       ($PSModuleInfo.PrivateData.GetType().ToString() -eq "System.Collections.Hashtable") -and 
       $PSModuleInfo.PrivateData["PSData"] -and
       ($PSModuleInfo.PrivateData["PSData"].GetType().ToString() -eq "System.Collections.Hashtable") -and
       $PSModuleInfo.PrivateData.PSData['ExternalModuleDependencies'] -and
       ($PSModuleInfo.PrivateData.PSData['ExternalModuleDependencies'].GetType().ToString() -eq "System.Object[]")
    )
    {
        return $PSModuleInfo.PrivateData.PSData.ExternalModuleDependencies        
    }
}

function Get-ModuleDependencies
{
    Param (
        [Parameter(Mandatory=$true)]
        [PSModuleInfo]
        $PSModuleInfo,

        [Parameter(Mandatory=$true)]
        [string]
        $Repository,

        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.PSCmdlet]
        $CallerPSCmdlet
    )

    $DependentModuleDetails = @()

    if($PSModuleInfo.RequiredModules -or $PSModuleInfo.NestedModules)
    {
        # PSModuleInfo.RequiredModules doesn't provide the RequiredVersion info from the ModuleSpecification
        # Reading the contents of module manifest file using Import-LocalizedData cmdlet
        # to get the RequiredVersion details.
        Import-LocalizedData -BindingVariable ModuleManifestHashTable `
                             -FileName (Microsoft.PowerShell.Management\Split-Path $PSModuleInfo.Path -Leaf) `
                             -BaseDirectory $PSModuleInfo.ModuleBase `
                             -ErrorAction SilentlyContinue `
                             -WarningAction SilentlyContinue                                

        if($PSModuleInfo.RequiredModules)
        {
            $ModuleManifestRequiredModules = $null

            if($ModuleManifestHashTable)
            {
                $ModuleManifestRequiredModules = $ModuleManifestHashTable.RequiredModules
            }
           

            $DependentModuleDetails += ValidateAndGet-RequiredModuleDetails -ModuleManifestRequiredModules $ModuleManifestRequiredModules `
                                                                            -RequiredPSModuleInfos $PSModuleInfo.RequiredModules `
                                                                            -Repository $Repository `
                                                                            -DependentModuleInfo $PSModuleInfo `
                                                                            -CallerPSCmdlet $CallerPSCmdlet `
                                                                            -Verbose:$VerbosePreference `
                                                                            -Debug:$DebugPreference 
        }

        if($PSModuleInfo.NestedModules)
        {
            $ModuleManifestRequiredModules = $null

            if($ModuleManifestHashTable)
            {
                $ModuleManifestRequiredModules = $ModuleManifestHashTable.NestedModules
            }
           
            # A nested module is a required module if it's ModuleBase is not starting with the current module's ModuleBase.
            $RequiredPSModuleInfos = $PSModuleInfo.NestedModules | Microsoft.PowerShell.Core\Where-Object {-not $_.ModuleBase.StartsWith($PSModuleInfo.ModuleBase, [System.StringComparison]::OrdinalIgnoreCase)}

            $DependentModuleDetails += ValidateAndGet-RequiredModuleDetails -ModuleManifestRequiredModules $ModuleManifestRequiredModules `
                                                                            -RequiredPSModuleInfos $RequiredPSModuleInfos `
                                                                            -Repository $Repository `
                                                                            -DependentModuleInfo $PSModuleInfo `
                                                                            -CallerPSCmdlet $CallerPSCmdlet `
                                                                            -Verbose:$VerbosePreference `
                                                                            -Debug:$DebugPreference 
        }
    }

    return $DependentModuleDetails
}

function Publish-PSArtifactUtility
{
    [CmdletBinding(PositionalBinding=$false)]
    Param
    (
        [Parameter(Mandatory=$true, ParameterSetName='PublishModule')]
        [ValidateNotNullOrEmpty()]
        [PSModuleInfo]
        $PSModuleInfo,

        [Parameter(Mandatory=$true, ParameterSetName='PublishScript')]
        [ValidateNotNullOrEmpty()]
        [PSCustomObject]
        $PSScriptInfo,

        [Parameter(Mandatory=$true, ParameterSetName='PublishModule')]
        [ValidateNotNullOrEmpty()]
        [string]
        $ManifestPath,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Destination,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Repository,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $NugetApiKey,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $NugetPackageRoot,

        [Parameter(ParameterSetName='PublishModule')] 
        [Version]
        $FormatVersion,

        [Parameter(ParameterSetName='PublishModule')]
        [string]
        $ReleaseNotes,

        [Parameter(ParameterSetName='PublishModule')]
        [string[]]
        $Tags,
        
        [Parameter(ParameterSetName='PublishModule')]
        [Uri]
        $LicenseUri,

        [Parameter(ParameterSetName='PublishModule')]
        [Uri]
        $IconUri,
        
        [Parameter(ParameterSetName='PublishModule')]
        [Uri]
        $ProjectUri
    )

    if(-not (Microsoft.PowerShell.Management\Test-Path $script:NuGetClient))
    {
        Install-NuGetClientBinaries
    }

    $PSArtifactType = $script:PSArtifactTypeModule
    $Name = $null
    $Description = $null
    $Version = $null
    $Author = $null
    $CompanyName = $null
    $Copyright = $null

    if($PSModuleInfo)
    {
        $Name = $PSModuleInfo.Name
        $Description = $PSModuleInfo.Description
        $Version = $PSModuleInfo.Version
        $Author = $PSModuleInfo.Author
        $CompanyName = $PSModuleInfo.CompanyName
        $Copyright = $PSModuleInfo.Copyright

        if($PSModuleInfo.PrivateData -and 
           ($PSModuleInfo.PrivateData.GetType().ToString() -eq "System.Collections.Hashtable") -and 
           $PSModuleInfo.PrivateData["PSData"] -and
           ($PSModuleInfo.PrivateData["PSData"].GetType().ToString() -eq "System.Collections.Hashtable")
           )
        {
            if( -not $Tags -and $PSModuleInfo.PrivateData.PSData["Tags"])
            { 
                $Tags = $PSModuleInfo.PrivateData.PSData.Tags
            }

            if( -not $ReleaseNotes -and $PSModuleInfo.PrivateData.PSData["ReleaseNotes"])
            { 
                $ReleaseNotes = $PSModuleInfo.PrivateData.PSData.ReleaseNotes
            }

            if( -not $LicenseUri -and $PSModuleInfo.PrivateData.PSData["LicenseUri"])
            { 
                $LicenseUri = $PSModuleInfo.PrivateData.PSData.LicenseUri
            }

            if( -not $IconUri -and $PSModuleInfo.PrivateData.PSData["IconUri"])
            { 
                $IconUri = $PSModuleInfo.PrivateData.PSData.IconUri
            }

            if( -not $ProjectUri -and $PSModuleInfo.PrivateData.PSData["ProjectUri"])
            { 
                $ProjectUri = $PSModuleInfo.PrivateData.PSData.ProjectUri
            }
        }
    }
    else
    {
        $PSArtifactType = $script:PSArtifactTypeScript

        $Name = $PSScriptInfo.Name
        $Description = $PSScriptInfo.Description
        $Version = $PSScriptInfo.Version        
        $Author = $PSScriptInfo.Author
        $CompanyName = $PSScriptInfo.CompanyName
        $Copyright = $PSScriptInfo.Copyright

        if($PSScriptInfo.'Tags')
        { 
            $Tags = $PSScriptInfo.Tags
        }

        if($PSScriptInfo.'ReleaseNotes')
        { 
            $ReleaseNotes = $PSScriptInfo.ReleaseNotes
        }

        if($PSScriptInfo.'LicenseUri')
        { 
            $LicenseUri = $PSScriptInfo.LicenseUri
        }

        if($PSScriptInfo.'IconUri')
        { 
            $IconUri = $PSScriptInfo.IconUri
        }

        if($PSScriptInfo.'ProjectUri')
        { 
            $ProjectUri = $PSScriptInfo.ProjectUri
        }
    }


    # Add PSModule and PSGet format version tags
    if(-not $Tags)
    {
        $Tags = @()
    }
    
    if($FormatVersion)
    {
        $Tags += "$($script:PSGetFormatVersion)_$FormatVersion"
    }

    $DependentModuleDetails = @()

    if($PSScriptInfo)
    {        
        $Tags += "PSScript"

        if($PSScriptInfo.ExportedCommands)
        {
            if($PSScriptInfo.ExportedFunctions)
            {
                $Tags += "$($script:Includes)_Function"
                $Tags += $PSScriptInfo.ExportedFunctions | Microsoft.PowerShell.Core\ForEach-Object { "$($script:Function)_$_" }
            }

            if($PSScriptInfo.ExportedWorkflows)
            {
                $Tags += "$($script:Includes)_Workflow"
                $Tags += $PSScriptInfo.ExportedWorkflows | Microsoft.PowerShell.Core\ForEach-Object { "$($script:Workflow)_$_" }
            }

            $Tags += $PSScriptInfo.ExportedCommands | Microsoft.PowerShell.Core\ForEach-Object { "$($script:Command)_$_" }
        }

        # Populate the dependencies elements from RequiredModules and RequiredScripts
        # 
        $DependentModuleDetails += ValidateAndGet-ScriptDependencies -Repository $Repository `
                                                                     -DependentScriptInfo $PSScriptInfo `
                                                                     -CallerPSCmdlet $PSCmdlet `
                                                                     -Verbose:$VerbosePreference `
                                                                     -Debug:$DebugPreference
    }
    else
    {
        $Tags += "PSModule"

        Import-LocalizedData -BindingVariable ModuleManifestHashTable `
                             -FileName (Microsoft.PowerShell.Management\Split-Path $ManifestPath -Leaf) `
                             -BaseDirectory (Microsoft.PowerShell.Management\Split-Path $ManifestPath -Parent) `
                             -ErrorAction SilentlyContinue `
                             -WarningAction SilentlyContinue


        if($PSModuleInfo.ExportedCommands.Count)
        {
            if($PSModuleInfo.ExportedCmdlets.Count)
            {
                $Tags += "$($script:Includes)_Cmdlet"
                $Tags += $PSModuleInfo.ExportedCmdlets.Keys | Microsoft.PowerShell.Core\ForEach-Object { "$($script:Cmdlet)_$_" }

                #if CmdletsToExport field in manifest file is "*", we suggest the user to include all those cmdlets for best practice
                if($ModuleManifestHashTable -and ($ModuleManifestHashTable.CmdletsToExport -eq "*"))
                {
                    $WarningMessage = $LocalizedData.ShouldIncludeCmdletsToExport -f ($ManifestPath)
                    Write-Warning -Message $WarningMessage
                }
            }

            if($PSModuleInfo.ExportedFunctions.Count)
            {
                $Tags += "$($script:Includes)_Function"
                $Tags += $PSModuleInfo.ExportedFunctions.Keys | Microsoft.PowerShell.Core\ForEach-Object { "$($script:Function)_$_" }

                if($ModuleManifestHashTable -and ($ModuleManifestHashTable.FunctionsToExport -eq "*"))
                {
                    $WarningMessage = $LocalizedData.ShouldIncludeFunctionsToExport -f ($ManifestPath)
                    Write-Warning -Message $WarningMessage
                }
            }

            $Tags += $PSModuleInfo.ExportedCommands.Keys | Microsoft.PowerShell.Core\ForEach-Object { "$($script:Command)_$_" }
        }

        $dscResourceNames = Get-ExportedDscResources -PSModuleInfo $PSModuleInfo 
        if($dscResourceNames)
        {
            $Tags += "$($script:Includes)_DscResource"

            $Tags += $dscResourceNames | Microsoft.PowerShell.Core\ForEach-Object { "$($script:DscResource)_$_" }

            #If DscResourcesToExport is commented out or "*" is used, we will write-warning
            if($ModuleManifestHashTable -and 
                ($ModuleManifestHashTable.ContainsKey("DscResourcesToExport") -and 
                $ModuleManifestHashTable.DscResourcesToExport -eq "*") -or 
                -not $ModuleManifestHashTable.ContainsKey("DscResourcesToExport"))
            {
                $WarningMessage = $LocalizedData.ShouldIncludeDscResourcesToExport
                Write-Warning -Message $WarningMessage
            }
        }

        # Populate the module dependencies elements from RequiredModules and 
        # NestedModules properties of the current PSModuleInfo
        $DependentModuleDetails = Get-ModuleDependencies -PSModuleInfo $PSModuleInfo `
                                                         -Repository $Repository `
                                                         -CallerPSCmdlet $PSCmdlet `
                                                         -Verbose:$VerbosePreference `
                                                         -Debug:$DebugPreference 
    }
    
    $dependencies = @()
    ForEach($Dependency in $DependentModuleDetails)
    {    
        $ModuleName = $Dependency.Name
        $VersionString = $null

        # Version format in NuSpec:
        # "[2.0]" --> (== 2.0) Required Version
        # "2.0" --> (>= 2.0) Minimum Version
        if($Dependency.Keys -Contains "RequiredVersion")
        {
            $VersionString = "[$($Dependency.RequiredVersion)]"
        }
        elseif($Dependency.Keys -Contains "MinimumVersion")
        {
            $VersionString = "$($Dependency.MinimumVersion)"
        }

        $dependencies += "<dependency id='$($ModuleName)' version='$($VersionString)' />"
    }
    
    # Populate the nuspec elements
    $nuspec = @"
<?xml version="1.0"?>
<package >
    <metadata>
        <id>$(Get-EscapedString -ElementValue $Name)</id>
        <version>$($Version)</version>
        <authors>$(Get-EscapedString -ElementValue $Author)</authors>
        <owners>$(Get-EscapedString -ElementValue $CompanyName)</owners>
        <description>$(Get-EscapedString -ElementValue $Description)</description>
        <releaseNotes>$(Get-EscapedString -ElementValue $ReleaseNotes)</releaseNotes>
        <copyright>$(Get-EscapedString -ElementValue $Copyright)</copyright>
        <tags>$(if($Tags){ Get-EscapedString -ElementValue ($Tags -join ' ')})</tags>
        $(if($LicenseUri){
        "<licenseUrl>$(Get-EscapedString -ElementValue $LicenseUri)</licenseUrl>
        <requireLicenseAcceptance>true</requireLicenseAcceptance>"
        })
        $(if($ProjectUri){
        "<projectUrl>$(Get-EscapedString -ElementValue $ProjectUri)</projectUrl>"
        })
        $(if($IconUri){
        "<iconUrl>$(Get-EscapedString -ElementValue $IconUri)</iconUrl>"
        })
        <dependencies>
            $dependencies
        </dependencies>
    </metadata>
</package>
"@

    try
    {        
        
        $NupkgPath = "$NugetPackageRoot\$Name.$($Version.ToString()).nupkg"
        $NuspecPath = "$NugetPackageRoot\$Name.nuspec"

        # Remove existing nuspec and nupkg files
        Microsoft.PowerShell.Management\Remove-Item $NupkgPath  -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
        Microsoft.PowerShell.Management\Remove-Item $NuspecPath -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
            
        Microsoft.PowerShell.Management\Set-Content -Value $nuspec -Path $NuspecPath

        # Create .nupkg file
        $output = & $script:NuGetClient pack $NuspecPath -OutputDirectory $NugetPackageRoot
        if($LASTEXITCODE)
        {
            if($PSArtifactType -eq $script:PSArtifactTypeModule)
            {
                $message = $LocalizedData.FailedToCreateCompressedModule -f ($output)
                $errorId = "FailedToCreateCompressedModule"
            }
            else
            {
                $message = $LocalizedData.FailedToCreateCompressedScript -f ($output)
                $errorId = "FailedToCreateCompressedScript"
            }

            Write-Error -Message $message -ErrorId $errorId -Category InvalidOperation
            return
        }

        # Publish the .nupkg to gallery
        $output = & $script:NuGetClient push $NupkgPath  -source $Destination -NonInteractive -ApiKey $NugetApiKey 
        if($LASTEXITCODE)
        {
            if($PSArtifactType -eq $script:PSArtifactTypeModule)
            {
                $message = $LocalizedData.FailedToPublish -f ($output)
                $errorId = "FailedToPublishTheModule"
            }
            else
            {
                $message = $LocalizedData.FailedToPublishScript -f ($output)
                $errorId = "FailedToPublishTheScript"
            }

            Write-Error -Message $message -ErrorId $errorId -Category InvalidOperation
        }
        else
        {
            if($PSArtifactType -eq $script:PSArtifactTypeModule)
            {
                $message = $LocalizedData.PublishedSuccessfully -f ($Name, $Destination, $Name)
            }
            else
            {
                $message = $LocalizedData.PublishedScriptSuccessfully -f ($Name, $Destination, $Name)
            }

            Write-Verbose -Message $message
        }
    }
    finally
    {
        Microsoft.PowerShell.Management\Remove-Item $NupkgPath  -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
        Microsoft.PowerShell.Management\Remove-Item $NuspecPath -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
    }
}

function ValidateAndAdd-PSScriptInfoEntry
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true)]
        [PSCustomObject]
        $PSScriptInfo,
        
        [Parameter(Mandatory=$true)]
        [string]
        $PropertyName,

        [Parameter()]
        $PropertyValue,

        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.PSCmdlet]
        $CallerPSCmdlet
    )
    
    $Value = $PropertyValue
    $KeyName = $PropertyName

    # return if $KeyName value is not null in $PSScriptInfo
    if(-not $value -or -not $KeyName -or (Get-Member -InputObject $PSScriptInfo -Name $KeyName) -and $PSScriptInfo."$KeyName")
    {
        return
    }

    switch($PropertyName)
    {
        # Validate the property value and also use proper key name as users can specify the property name in any case.
        $script:Version {
                            $KeyName = $script:Version

                            [Version]$Version = $null
                            if([System.Version]::TryParse($Value, ([ref]$Version)))
                            {
                                $Value = $Version                            
                            }
                            else
                            {
                                $message = $LocalizedData.InvalidVersion -f ($value)
                                ThrowError -ExceptionName "System.ArgumentException" `
                                            -ExceptionMessage $message `
                                            -ErrorId "InvalidVersion" `
                                            -CallerPSCmdlet $CallerPSCmdlet `
                                            -ErrorCategory InvalidArgument `
                                            -ExceptionObject $Value
                                return
                            }
                            break
                        }

        $script:Author  { $KeyName = $script:Author }

        $script:Description { $KeyName = $script:Description }

        $script:CompanyName { $KeyName = $script:CompanyName }

        $script:Copyright { $KeyName = $script:Copyright }

        $script:Tags {
                        $KeyName = $script:Tags
                        $Value = $Value -split '[,\s+]' | Microsoft.PowerShell.Core\Where-Object {$_}
                        break
                     }

        $script:LicenseUri {
                                $KeyName = $script:LicenseUri

                                if(-not (Test-WebUri -Uri $value))
                                {
                                    $message = $LocalizedData.InvalidWebUri -f ($LicenseUri, "LicenseUri")
                                    ThrowError -ExceptionName "System.ArgumentException" `
                                                -ExceptionMessage $message `
                                                -ErrorId "InvalidWebUri" `
                                                -CallerPSCmdlet $CallerPSCmdlet `
                                                -ErrorCategory InvalidArgument `
                                                -ExceptionObject $Value
                                    return
                                }

                                $Value = [Uri]$Value
                           }

        $script:ProjectUri {
                                $KeyName = $script:ProjectUri

                                if(-not (Test-WebUri -Uri $value))
                                {
                                    $message = $LocalizedData.InvalidWebUri -f ($ProjectUri, "ProjectUri")
                                    ThrowError -ExceptionName "System.ArgumentException" `
                                                -ExceptionMessage $message `
                                                -ErrorId "InvalidWebUri" `
                                                -CallerPSCmdlet $CallerPSCmdlet `
                                                -ErrorCategory InvalidArgument `
                                                -ExceptionObject $Value
                                    return
                                }

                                $Value = [Uri]$Value
                           }

        $script:IconUri {
                            $KeyName = $script:IconUri

                            if(-not (Test-WebUri -Uri $value))
                            {
                                $message = $LocalizedData.InvalidWebUri -f ($IconUri, "IconUri")
                                ThrowError -ExceptionName "System.ArgumentException" `
                                            -ExceptionMessage $message `
                                            -ErrorId "InvalidWebUri" `
                                            -CallerPSCmdlet $CallerPSCmdlet `
                                            -ErrorCategory InvalidArgument `
                                            -ExceptionObject $Value
                                return
                            }

                            $Value = [Uri]$Value
                        }

        $script:ExternalModuleDependencies {
                                               $KeyName = $script:ExternalModuleDependencies
                                               $Value = $Value -split '[,\s+]' | Microsoft.PowerShell.Core\Where-Object {$_}
                                           }

        $script:ReleaseNotes { $KeyName = $script:ReleaseNotes }

        $script:RequiredModules { $KeyName = $script:RequiredModules }

        $script:RequiredScripts { 
                                    $KeyName = $script:RequiredScripts
                                    $Value = $Value -split '[,\s+]' | Microsoft.PowerShell.Core\Where-Object {$_}
                                }

        $script:ExternalScriptDependencies { 
                                               $KeyName = $script:ExternalScriptDependencies
                                               $Value = $Value -split '[,\s+]' | Microsoft.PowerShell.Core\Where-Object {$_}
                                           }

        $script:ExportedCommands  { $KeyName = $script:ExportedCommands }

        $script:ExportedFunctions { $KeyName = $script:ExportedFunctions }

        $script:ExportedWorkflows { $KeyName = $script:ExportedWorkflows }
    }

    Microsoft.PowerShell.Utility\Add-Member -InputObject $PSScriptInfo `
                                            -MemberType NoteProperty `
                                            -Name $KeyName `
                                            -Value $Value `
                                            -Force
}

function Get-ExportedDscResources
{
    [CmdletBinding(PositionalBinding=$false)]
    Param 
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [PSModuleInfo]
        $PSModuleInfo
    )

    $dscResources = @()

    if(Get-Command -Name Get-DscResource -Module PSDesiredStateConfiguration -ErrorAction SilentlyContinue)
    {
        $OldPSModulePath = $env:PSModulePath

        try
        {
            $env:PSModulePath = Join-Path -Path $PSHOME -ChildPath "Modules"
            $env:PSModulePath = "$env:PSModulePath;$(Split-Path -Path $PSModuleInfo.ModuleBase -Parent)"

            $dscResources = PSDesiredStateConfiguration\Get-DscResource -ErrorAction SilentlyContinue -WarningAction SilentlyContinue | 
                                Microsoft.PowerShell.Core\ForEach-Object {
                                    if($_.Module -and ($_.Module.Name -eq $PSModuleInfo.Name))
                                    {
                                        $_.Name
                                    }
                                }
        }
        finally
        {
            $env:PSModulePath = $OldPSModulePath
        }
    }
    else
    {
        $dscResourcesDir = Microsoft.PowerShell.Management\Join-Path -Path $PSModuleInfo.ModuleBase -ChildPath "DscResources"
        if(Microsoft.PowerShell.Management\Test-Path $dscResourcesDir)
        {
            $dscResources = Microsoft.PowerShell.Management\Get-ChildItem -Path $dscResourcesDir -Directory -Name
        }
    }

    return $dscResources
}

function Get-LocationString
{
    [CmdletBinding(PositionalBinding=$false)]
    Param 
    (
        [Parameter()]
        [Uri]
        $LocationUri
    )

    $LocationString = $null

    if($LocationUri)
    {
        if($LocationUri.Scheme -eq 'file')
        {
            $LocationString = $LocationUri.OriginalString
        }
        elseif($LocationUri.AbsoluteUri)
        {
            $LocationString = $LocationUri.AbsoluteUri
        }
        else
        {
            $LocationString = $LocationUri.ToString()
        }
    }

    return $LocationString
}

function Update-PowerShellGetModule
{
    [CmdletBinding()]
    Param 
    (
        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.PSCmdlet]
        $CallerPSCmdlet
    )

    $Name = $script:PSGetModuleName

    if($PSVersionTable.PSVersion -lt [Version]"5.0")
    {
        $message = $LocalizedData.PowerShellGetUpdateIsNotSupportedOnLowerPSVersions
        ThrowError -ExceptionName "System.InvalidOperationException" `
                   -ExceptionMessage $message `
                   -ErrorId "PowerShellGetModuleUpdateIsNotSupportedOnLowerPSVersions" `
                   -CallerPSCmdlet $CallerPSCmdlet `
                   -ErrorCategory InvalidOperation `
                   -ExceptionObject $Name
        return
    }

    $PSGetModuleInfo = Microsoft.PowerShell.Core\Get-Module -ListAvailable -Name $Name | Microsoft.PowerShell.Utility\Select-Object -Unique
                
    if(-not $PSGetModuleInfo -or 
        ($PSGetModuleInfo.GetType().ToString() -ne "System.Management.Automation.PSModuleInfo") -or
        -not $PSGetModuleInfo.ModuleBase.StartsWith($script:programFilesModulesPath, [System.StringComparison]::OrdinalIgnoreCase))
    {
        $message = $LocalizedData.PowerShellGetModuleIsNotInstalledProperly -f $script:programFilesModulesPath
        ThrowError -ExceptionName "System.InvalidOperationException" `
                   -ExceptionMessage $message `
                   -ErrorId "PowerShellGetModuleIsNotInstalledProperly" `
                   -CallerPSCmdlet $CallerPSCmdlet `
                   -ErrorCategory InvalidOperation `
                   -ExceptionObject $Name
        return
    }

    if(-not (Test-RunningAsElevated))
    {                            
        $message = $LocalizedData.AdminPrivilegesRequiredForUpdate -f ($PSGetModuleInfo.Name, $PSGetModuleInfo.ModuleBase)
        ThrowError -ExceptionName "System.InvalidOperationException" `
                   -ExceptionMessage $message `
                   -ErrorId "AdminPrivilegesAreRequiredForUpdate" `
                   -CallerPSCmdlet $CallerPSCmdlet `
                   -ErrorCategory InvalidOperation `
                   -ExceptionObject $Name
        return
    }

    
    $sourceLocation = $Script:PSGallerySourceUri
    
    $findPackageInfo = PackageManagement\Find-Package -Name $Name `
                                                      -Source $sourceLocation `
                                                      -ProviderName $script:NuGetProviderName `
                                                      -ErrorAction SilentlyContinue `
                                                      -WarningAction SilentlyContinue

    if($findPackageInfo -and 
       ($findPackageInfo.Name -eq $PSGetModuleInfo.Name) -and 
       ($findPackageInfo.Version -gt $PSGetModuleInfo.Version))
    {
        $tempDestination = Microsoft.PowerShell.Management\Join-Path -Path $script:TempPath -ChildPath "$(Microsoft.PowerShell.Utility\Get-Random)"

        $null = Microsoft.PowerShell.Management\New-Item -Path $tempDestination `
                                                         -ItemType Directory `
                                                         -Force `
                                                         -Confirm:$false `
                                                         -WhatIf:$false



        try
        {
            $modules = PackageManagement\Install-Package -Name $Name `
                                                         -Source $sourceLocation `
                                                         -Destination $tempDestination `
                                                         -ProviderName $script:NuGetProviderName `
                                                         -ExcludeVersion `
                                                         -Force
            foreach($module in $modules)
            {
                $tempModulePath = Microsoft.PowerShell.Management\Join-Path -Path $tempDestination -ChildPath $module.Name

                if(Microsoft.PowerShell.Management\Test-Path -Path $tempModulePath)
                {
                    # Remove the *.nupkg file
                    if(Microsoft.PowerShell.Management\Test-Path -Path "$tempModulePath\$($module.Name).nupkg")
                    {
                        Microsoft.PowerShell.Management\Remove-Item -Path "$tempModulePath\$($module.Name).nupkg" `
                                                                    -Force `
                                                                    -ErrorAction SilentlyContinue `
                                                                    -WarningAction SilentlyContinue `
                                                                    -Confirm:$false `
                                                                    -WhatIf:$false
                    }

                    # Validate the module
                    $newModuleInfo = Test-ValidManifestModule -ModuleBasePath $tempModulePath
                    if(-not $newModuleInfo)
                    {
                        $message = $LocalizedData.InvalidPSModule -f ($module.Name)
                        ThrowError -ExceptionName "System.InvalidOperationException" `
                                    -ExceptionMessage $message `
                                    -ErrorId "InvalidManifestModule" `
                                    -CallerPSCmdlet $CallerPSCmdlet `
                                    -ErrorCategory InvalidOperation `
                                    -ExceptionObject $module.Name
                        return
                    }

                    $currentModuleInfo = Microsoft.PowerShell.Core\Get-Module -ListAvailable -Name $module.Name | 
                                            Microsoft.PowerShell.Utility\Select-Object -Unique

                    if($currentModuleInfo -and ($currentModuleInfo.Version -ge $newModuleInfo.Version))
                    {
                        Continue
                    }

                    # Check the authenticode signature
                    $latestModuleFiles = Microsoft.PowerShell.Management\Get-ChildItem -Path $tempModulePath -Recurse -File

                    foreach($file in $latestModuleFiles)
                    {
                        $newSignature = Microsoft.PowerShell.Security\Get-AuthenticodeSignature -FilePath $file.FullName

                        if($newSignature.Status -ne "Valid" -or
                           ($newSignature.SignerCertificate -and
                            $newSignature.SignerCertificate.DnsNameList -and
                            $newSignature.SignerCertificate.DnsNameList.UniCode.ToString() -notmatch "Microsoft"))
                        {
                            $message = $LocalizedData.InvalidAuthenticodeSignature -f ($module.Name, $file.Name)
                            ThrowError -ExceptionName "System.InvalidOperationException" `
                                       -ExceptionMessage $message `
                                       -ErrorId "InvalidAuthenticodeSignature" `
                                       -CallerPSCmdlet $CallerPSCmdlet `
                                       -ErrorCategory InvalidOperation `
                                       -ExceptionObject $module.Name
                            return
                        }
                    }                

                    # Copy the module
                    $DestinationPath = "$script:programFilesModulesPath\$($module.Name)\$($newModuleInfo.Version)"

                    if(-not (Microsoft.PowerShell.Management\Test-Path -Path $DestinationPath))
                    {
                        $null = Microsoft.PowerShell.Management\New-Item -Path $DestinationPath `
                                                                         -ItemType Directory `
                                                                         -Force `
                                                                         -Confirm:$false `
                                                                         -WhatIf:$false
                    }

                    Microsoft.PowerShell.Management\Copy-Item -Path "$tempModulePath\*" `
                                                              -Destination $DestinationPath `
                                                              -Force `
                                                              -Recurse `
                                                              -Confirm:$false `
                                                              -WhatIf:$false

                    # Update the PowerShellGet module under ${env:ProgramFiles(x86)} or $env:ProgramW6432
                    # depending on the current process's architecture
                    if($env:ProgramW6432 -and ${env:ProgramFiles(x86)})
                    {
                        if($env:ProgramFiles -eq $env:ProgramW6432)
                        {                                
                            $DestinationPath = "${env:ProgramFiles(x86)}\WindowsPowerShell\Modules\$($module.Name)\$($newModuleInfo.Version)"
                        }
                        else
                        {
                            $DestinationPath = "$env:ProgramW6432\WindowsPowerShell\Modules\$($module.Name)\$($newModuleInfo.Version)"
                        }

                        if(-not (Microsoft.PowerShell.Management\Test-Path -Path $DestinationPath))
                        {
                            $null = Microsoft.PowerShell.Management\New-Item -Path $DestinationPath `
                                                                             -ItemType Directory `
                                                                             -Force `
                                                                             -Confirm:$false `
                                                                             -WhatIf:$false
                        }

                        Microsoft.PowerShell.Management\Copy-Item -Path "$tempModulePath\*" `
                                                                  -Destination $DestinationPath `
                                                                  -Force `
                                                                  -Recurse `
                                                                  -Confirm:$false `
                                                                  -WhatIf:$false
                    }
                
                    Write-Verbose -Message ($LocalizedData.ModuleGotUpdated -f $module.Name)
                }
            }
        }
        finally
        {
            Microsoft.PowerShell.Management\Remove-Item -Path $tempDestination `
                                                        -Force `
                                                        -Recurse `
                                                        -ErrorAction SilentlyContinue `
                                                        -WarningAction SilentlyContinue `
                                                        -Confirm:$false `
                                                        -WhatIf:$false
        }
    }
    else
    {
        Write-Verbose -Message ($LocalizedData.NoUpdateAvailable -f $Name)
    }

}
#endregion Utility functions

#region PSModule Provider APIs Implementation
function Get-PackageProviderName
{ 
    return $script:PSModuleProviderName
}

function Get-Feature
{
    Write-Debug ($LocalizedData.ProviderApiDebugMessage -f ('Get-Feature'))
    Write-Output -InputObject (New-Feature $script:SupportsPSModulesFeatureName )
}

function Initialize-Provider
{
    Write-Debug ($LocalizedData.ProviderApiDebugMessage -f ('Initialize-Provider'))
}

function Get-DynamicOptions
{
    param
    (
        [Microsoft.PackageManagement.MetaProvider.PowerShell.OptionCategory] 
        $category
    )

    Write-Debug ($LocalizedData.ProviderApiDebugMessage -f ('Get-DynamicOptions'))

    Write-Output -InputObject (New-DynamicOption -Category $category -Name $script:PackageManagementProviderParam -ExpectedType String -IsRequired $false)

    Write-Output -InputObject (New-DynamicOption -Category $category `
                                                 -Name $script:PSArtifactType `
                                                 -ExpectedType String `
                                                 -IsRequired $false `
                                                 -PermittedValues @($script:PSArtifactTypeModule,$script:PSArtifactTypeScript, $script:All))

    switch($category)
    {
        Package {
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name $script:AllVersions -ExpectedType Switch -IsRequired $false)
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name $script:Filter -ExpectedType String -IsRequired $false)
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name $script:Tag -ExpectedType StringArray -IsRequired $false)
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name Includes -ExpectedType StringArray -IsRequired $false -PermittedValues $script:IncludeValidSet)
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name DscResource -ExpectedType StringArray -IsRequired $false)
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name Command -ExpectedType StringArray -IsRequired $false)
                }

        Source  {
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name $script:PublishLocation -ExpectedType String -IsRequired $false)
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name $script:ScriptSourceLocation -ExpectedType String -IsRequired $false)
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name $script:ScriptPublishLocation -ExpectedType String -IsRequired $false)
                }

        Install 
                {
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name "Scope" -ExpectedType String -IsRequired $false -PermittedValues @("CurrentUser","AllUsers"))
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name "Location" -ExpectedType String -IsRequired $false)
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name "InstallUpdate" -ExpectedType Switch -IsRequired $false)
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name "InstallationPolicy" -ExpectedType String -IsRequired $false)
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name "DestinationPath" -ExpectedType String -IsRequired $false)
                }
    }
}

function Add-PackageSource
{
    [CmdletBinding()]
    param
    (
        [string]
        $Name,
         
        [string]
        $Location,

        [bool]
        $Trusted
    )
     
    Write-Debug ($LocalizedData.ProviderApiDebugMessage -f ('Add-PackageSource'))

    Set-ModuleSourcesVariable -Force

    $IsNewModuleSource = $false
    $Options = $request.Options

    foreach( $o in $Options.Keys )
    {
        Write-Debug ( "OPTION: {0} => {1}" -f ($o, $Options[$o]) )
    }

    if($Options.ContainsKey('IsNewModuleSource'))
    {
        $IsNewModuleSource = $Options['IsNewModuleSource']
    }

    $IsUpdatePackageSource = $false
    if($Options.ContainsKey('IsUpdatePackageSource'))
    {
        $IsUpdatePackageSource = $Options['IsUpdatePackageSource']
    }

    $PublishLocation = $null
    if($Options.ContainsKey($script:PublishLocation))
    {
        $PublishLocation = $Options[$script:PublishLocation]

        if(($Name -ne $Script:PSGalleryModuleSource) -and 
           -not (Microsoft.PowerShell.Management\Test-Path $PublishLocation) -and
           -not (Test-WebUri -uri $PublishLocation)
          )
        {
            $PublishLocationUri = [Uri]$PublishLocation
            if($PublishLocationUri.Scheme -eq 'file')
            {
                $message = $LocalizedData.PathNotFound -f ($PublishLocation)
                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage $message `
                           -ErrorId "PathNotFound" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument `
                           -ExceptionObject $PublishLocation
            }
            else
            {
                $message = $LocalizedData.InvalidWebUri -f ($PublishLocation, "PublishLocation")
                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage $message `
                           -ErrorId "InvalidWebUri" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument `
                           -ExceptionObject $PublishLocation
            }            
        }
    }

    $ScriptSourceLocation = $null
    if($Options.ContainsKey($script:ScriptSourceLocation))
    {
        $ScriptSourceLocation = $Options[$script:ScriptSourceLocation]

        if(($Name -ne $Script:PSGalleryModuleSource) -and 
           -not (Microsoft.PowerShell.Management\Test-Path $ScriptSourceLocation) -and
           -not (Test-WebUri -uri $ScriptSourceLocation)
          )
        {
            $ScriptSourceLocationUri = [Uri]$ScriptSourceLocation
            if($ScriptSourceLocationUri.Scheme -eq 'file')
            {
                $message = $LocalizedData.PathNotFound -f ($ScriptSourceLocation)
                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage $message `
                           -ErrorId "PathNotFound" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument `
                           -ExceptionObject $ScriptSourceLocation
            }
            else
            {
                $message = $LocalizedData.InvalidWebUri -f ($ScriptSourceLocation, "ScriptSourceLocation")
                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage $message `
                           -ErrorId "InvalidWebUri" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument `
                           -ExceptionObject $ScriptSourceLocation
            }            
        }
    }

    $ScriptPublishLocation = $null
    if($Options.ContainsKey($script:ScriptPublishLocation))
    {
        $ScriptPublishLocation = $Options[$script:ScriptPublishLocation]

        if(($Name -ne $Script:PSGalleryModuleSource) -and 
           -not (Microsoft.PowerShell.Management\Test-Path $ScriptPublishLocation) -and
           -not (Test-WebUri -uri $ScriptPublishLocation)
          )
        {
            $ScriptPublishLocationUri = [Uri]$ScriptPublishLocation
            if($ScriptPublishLocationUri.Scheme -eq 'file')
            {
                $message = $LocalizedData.PathNotFound -f ($ScriptPublishLocation)
                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage $message `
                           -ErrorId "PathNotFound" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument `
                           -ExceptionObject $ScriptPublishLocation
            }
            else
            {
                $message = $LocalizedData.InvalidWebUri -f ($ScriptPublishLocation, "ScriptPublishLocation")
                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage $message `
                           -ErrorId "InvalidWebUri" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument `
                           -ExceptionObject $ScriptPublishLocation
            }            
        }
    }

    # Ping and resolve the specified location
    $Location = Resolve-Location -Location $Location `
                                 -LocationParameterName 'Location' `
                                 -CallerPSCmdlet $PSCmdlet
    if(-not $Location)
    {
        # Above Resolve-Location function throws an error when it is not able to resolve a location
        return
    }

    if(($Name -ne $Script:PSGalleryModuleSource) -and 
       -not (Microsoft.PowerShell.Management\Test-Path -Path $Location) -and
       -not (Test-WebUri -uri $Location)
      )
    {
        $LocationUri = [Uri]$Location
        if($LocationUri.Scheme -eq 'file')
        {
            $message = $LocalizedData.PathNotFound -f ($Location)
            ThrowError -ExceptionName "System.ArgumentException" `
                       -ExceptionMessage $message `
                       -ErrorId "PathNotFound" `
                       -CallerPSCmdlet $PSCmdlet `
                       -ErrorCategory InvalidArgument `
                       -ExceptionObject $Location
        }
        else
        {
            $message = $LocalizedData.InvalidWebUri -f ($Location, "Location")
            ThrowError -ExceptionName "System.ArgumentException" `
                       -ExceptionMessage $message `
                       -ErrorId "InvalidWebUri" `
                       -CallerPSCmdlet $PSCmdlet `
                       -ErrorCategory InvalidArgument `
                       -ExceptionObject $Location
        }
    }

    if(Test-WildcardPattern $Name)
    {
        $message = $LocalizedData.RepositoryNameContainsWildCards -f ($Name)
        ThrowError -ExceptionName "System.ArgumentException" `
                    -ExceptionMessage $message `
                    -ErrorId "RepositoryNameContainsWildCards" `
                    -CallerPSCmdlet $PSCmdlet `
                    -ErrorCategory InvalidArgument `
                    -ExceptionObject $Name
    }

    $LocationString = Get-ValidModuleLocation -LocationString $Location -ParameterName "Location"

    if($LocationString -ne $script:PSGetModuleSources[$Script:PSGalleryModuleSource].SourceLocation)
    {
        if($Name -eq $Script:PSGalleryModuleSource)
        {
            ThrowError -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $LocalizedData.SourceLocationValueForPSGalleryCannotBeChanged `
                        -ErrorId "LocationValueForPSGalleryCannotBeChanged" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ErrorCategory InvalidArgument
        }
    }

    if($PublishLocation -and 
       ($Name -eq $Script:PSGalleryModuleSource) -and
       ($PublishLocation -ne $script:PSGetModuleSources[$Script:PSGalleryModuleSource].PublishLocation))
    {
        ThrowError -ExceptionName "System.ArgumentException" `
                    -ExceptionMessage $LocalizedData.PublishLocationValueForPSGalleryCannotBeChanged `
                    -ErrorId "PublishLocationValueForPSGalleryCannotBeChanged" `
                    -CallerPSCmdlet $PSCmdlet `
                    -ErrorCategory InvalidArgument
    }

    # Check if Location is already registered with another Name
    $existingSourceName = Get-SourceName -Location $LocationString

    if($existingSourceName -and 
       ($Name -ne $existingSourceName) -and
       -not $IsNewModuleSource)
    {
        $message = $LocalizedData.RepositoryAlreadyRegistered -f ($existingSourceName, $Location, $Name)
        ThrowError -ExceptionName "System.ArgumentException" `
                   -ExceptionMessage $message `
                   -ErrorId "RepositoryAlreadyRegistered" `
                   -CallerPSCmdlet $PSCmdlet `
                   -ErrorCategory InvalidArgument
    }
    
    $currentSourceObject = $null

    # Check if Name is already registered
    if($script:PSGetModuleSources.Contains($Name))
    {
        $currentSourceObject = $script:PSGetModuleSources[$Name]
        $null = $script:PSGetModuleSources.Remove($Name)
    }
    
    if(-not $PublishLocation -and $currentSourceObject -and $currentSourceObject.PublishLocation)
    {
        $PublishLocation = $currentSourceObject.PublishLocation
    }

    if(-not $ScriptPublishLocation -and $currentSourceObject -and $currentSourceObject.ScriptPublishLocation)
    {
        $ScriptPublishLocation = $currentSourceObject.ScriptPublishLocation
    }

    if(-not $ScriptSourceLocation -and $currentSourceObject -and $currentSourceObject.ScriptSourceLocation)
    {
        $ScriptSourceLocation = $currentSourceObject.ScriptSourceLocation
    }

    $IsProviderSpecified = $false;
    if ($Options.ContainsKey($script:PackageManagementProviderParam))
    {
        $SpecifiedProviderName = $Options[$script:PackageManagementProviderParam] 

        $IsProviderSpecified = $true

        Write-Verbose ($LocalizedData.SpecifiedProviderName -f $SpecifiedProviderName)
        if ($SpecifiedProviderName -eq $script:PSModuleProviderName)
        {
            $message = $LocalizedData.InvalidPackageManagementProviderValue -f ($SpecifiedProviderName, $script:NuGetProviderName, $script:NuGetProviderName)
            ThrowError -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $message `
                        -ErrorId "InvalidPackageManagementProviderValue" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ErrorCategory InvalidArgument `
                        -ExceptionObject $SpecifiedProviderName
            return
        }
    }
    else
    {
        $SpecifiedProviderName = $script:NuGetProviderName
        Write-Verbose ($LocalizedData.ProviderNameNotSpecified -f $SpecifiedProviderName)
    }

    $packageSource = $null
        
    $selProviders = $request.SelectProvider($SpecifiedProviderName)

    if(-not $selProviders -and $IsProviderSpecified)
    {
        $message = $LocalizedData.SpecifiedProviderNotAvailable -f $SpecifiedProviderName
        ThrowError -ExceptionName "System.InvalidOperationException" `
                    -ExceptionMessage $message `
                    -ErrorId "SpecifiedProviderNotAvailable" `
                    -CallerPSCmdlet $PSCmdlet `
                    -ErrorCategory InvalidOperation `
                    -ExceptionObject $SpecifiedProviderName
    }

    # Try with user specified provider or NuGet provider
    foreach($SelectedProvider in $selProviders)
    {
        if($request.IsCanceled)
        {
            return
        }

        if($SelectedProvider -and $SelectedProvider.Features.ContainsKey($script:SupportsPSModulesFeatureName))
        {
            $packageSource = $SelectedProvider.ResolvePackageSources( (New-Request -Sources @($LocationString)) )
        }
        else
        {
            $message = $LocalizedData.SpecifiedProviderDoesnotSupportPSModules -f $SelectedProvider.ProviderName
            ThrowError -ExceptionName "System.InvalidOperationException" `
                        -ExceptionMessage $message `
                        -ErrorId "SpecifiedProviderDoesnotSupportPSModules" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ErrorCategory InvalidOperation `
                        -ExceptionObject $SelectedProvider.ProviderName
        }

        if($packageSource)
        {
            break
        }
    }

    # Poll other package provider when NuGet provider doesn't resolves the specified location
    if(-not $packageSource -and -not $IsProviderSpecified)
    {
        Write-Verbose ($LocalizedData.PollingPackageManagementProvidersForLocation -f $LocationString)

        $moduleProviders = $request.SelectProvidersWithFeature($script:SupportsPSModulesFeatureName)
        
        foreach($provider in $moduleProviders)
        {
            if($request.IsCanceled)
            {
                return
            }

            # Skip already tried $SpecifiedProviderName and PSModule provider
            if($provider.ProviderName -eq $SpecifiedProviderName -or 
               $provider.ProviderName -eq $script:PSModuleProviderName)
            {
                continue
            }

            Write-Verbose ($LocalizedData.PollingSingleProviderForLocation -f ($LocationString, $provider.ProviderName))
            $packageSource = $provider.ResolvePackageSources((New-Request -Option @{} -Sources @($LocationString))) 

            if($packageSource)
            {
                Write-Verbose ($LocalizedData.FoundProviderForLocation -f ($provider.ProviderName, $Location))
                $SelectedProvider = $provider
                break
            }
        }
    }

    if(-not $packageSource)
    {
        $message = $LocalizedData.SpecifiedLocationCannotBeRegistered -f $Location
        ThrowError -ExceptionName "System.InvalidOperationException" `
                    -ExceptionMessage $message `
                    -ErrorId "SpecifiedLocationCannotBeRegistered" `
                    -CallerPSCmdlet $PSCmdlet `
                    -ErrorCategory InvalidOperation `
                    -ExceptionObject $Location
    }

    $ProviderOptions = @{}

    $SelectedProvider.DynamicOptions | Microsoft.PowerShell.Core\ForEach-Object { 
                                            if($options.ContainsKey($_.Name) ) 
                                            { 
                                                $ProviderOptions[$_.Name] = $options[$_.Name]
                                            }
                                       }

    # Keep the existing provider options if not specified in Set-PSRepository
    if($currentSourceObject)
    {
        $currentSourceObject.ProviderOptions.GetEnumerator() | Microsoft.PowerShell.Core\ForEach-Object {
                                                                   if (-not $ProviderOptions.ContainsKey($_.Key) )
                                                                   {
                                                                       $ProviderOptions[$_.Key] = $_.Value
                                                                   }
                                                               }
    }
    
    if(-not $PublishLocation)
    {
        $PublishLocation = Get-PublishLocation -Location $LocationString
    }
    
    # Use the PublishLocation for the scripts when ScriptPublishLocation is not specified by the user
    if(-not $ScriptPublishLocation)
    {
        $ScriptPublishLocation = $PublishLocation

        # ScriptPublishLocation and PublishLocation should be equal in case of SMB Share or Local directory paths
        if($Options.ContainsKey($script:ScriptPublishLocation) -and
           (Microsoft.PowerShell.Management\Test-Path -Path $ScriptPublishLocation))
        {
            if($ScriptPublishLocation -ne $PublishLocation)
            {
                $message = $LocalizedData.PublishLocationPathsForModulesAndScriptsShouldBeEqual -f ($LocationString, $ScriptSourceLocation)
                ThrowError -ExceptionName "System.InvalidOperationException" `
                            -ExceptionMessage $message `
                            -ErrorId "PublishLocationPathsForModulesAndScriptsShouldBeEqual" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ErrorCategory InvalidOperation `
                            -ExceptionObject $Location
            }
        }
    }

    if(-not $ScriptSourceLocation)
    {
        $ScriptSourceLocation = Get-ScriptSourceLocation -Location $LocationString
    }
    elseif($Options.ContainsKey($script:ScriptSourceLocation))
    {
        # ScriptSourceLocation and SourceLocation cannot be same for they are URLs
        # Both should be equal in case of SMB Share or Local directory paths
        if(Microsoft.PowerShell.Management\Test-Path -Path $ScriptSourceLocation)
        {
            if($ScriptSourceLocation -ne $LocationString)
            {
                $message = $LocalizedData.SourceLocationPathsForModulesAndScriptsShouldBeEqual -f ($LocationString, $ScriptSourceLocation)
                ThrowError -ExceptionName "System.InvalidOperationException" `
                            -ExceptionMessage $message `
                            -ErrorId "SourceLocationPathsForModulesAndScriptsShouldBeEqual" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ErrorCategory InvalidOperation `
                            -ExceptionObject $Location
            }
        }
        else
        {
            if($ScriptSourceLocation -eq $LocationString)
            {
                $message = $LocalizedData.SourceLocationUrisForModulesAndScriptsShouldBeDifferent -f ($LocationString, $ScriptSourceLocation)
                ThrowError -ExceptionName "System.InvalidOperationException" `
                            -ExceptionMessage $message `
                            -ErrorId "SourceLocationUrisForModulesAndScriptsShouldBeDifferent" `
                            -CallerPSCmdlet $PSCmdlet `
                            -ErrorCategory InvalidOperation `
                            -ExceptionObject $Location
            }
        }
    }    

    # Add new module source
    $moduleSource = Microsoft.PowerShell.Utility\New-Object PSCustomObject -Property ([ordered]@{
            Name = $Name
            SourceLocation = $LocationString            
            PublishLocation = $PublishLocation
            ScriptSourceLocation = $ScriptSourceLocation
            ScriptPublishLocation = $ScriptPublishLocation
            Trusted=$Trusted
            Registered= (-not $IsNewModuleSource)
            InstallationPolicy = if($Trusted) {'Trusted'} else {'Untrusted'}
            PackageManagementProvider = $SelectedProvider.ProviderName
            ProviderOptions = $ProviderOptions
        })

    $moduleSource.PSTypeNames.Insert(0, "Microsoft.PowerShell.Commands.PSRepository")

    # Persist the repositories only when Register-PSRepository cmdlet is used
    if(-not $IsNewModuleSource)
    {
        $script:PSGetModuleSources.Add($Name, $moduleSource)            

        $message = $LocalizedData.RepositoryRegistered -f ($Name, $LocationString)
        Write-Verbose $message

        # Persist the module sources
        Save-ModuleSources
    }

    # return the package source object.
    Write-Output -InputObject (New-PackageSourceFromModuleSource -ModuleSource $moduleSource)
}

function Resolve-PackageSource
{ 
    Write-Debug ($LocalizedData.ProviderApiDebugMessage -f ('Resolve-PackageSource'))

    Set-ModuleSourcesVariable

    $SourceName = $request.PackageSources

    if(-not $SourceName)
    {
        $SourceName = "*"
    }

    foreach($moduleSourceName in $SourceName)
    {
        if($request.IsCanceled)
        {
            return
        }

        $wildcardPattern = New-Object System.Management.Automation.WildcardPattern $moduleSourceName,$script:wildcardOptions
        $moduleSourceFound = $false

        $script:PSGetModuleSources.GetEnumerator() | 
            Microsoft.PowerShell.Core\Where-Object {$wildcardPattern.IsMatch($_.Key)} | 
                Microsoft.PowerShell.Core\ForEach-Object {

                    $moduleSource = $script:PSGetModuleSources[$_.Key]

                    $packageSource = New-PackageSourceFromModuleSource -ModuleSource $moduleSource

                    Write-Output -InputObject $packageSource

                    $moduleSourceFound = $true
                }

        if(-not $moduleSourceFound)
        {
            $sourceName  = Get-SourceName -Location $moduleSourceName

            if($sourceName)
            {
                $moduleSource = $script:PSGetModuleSources[$sourceName]

                $packageSource = New-PackageSourceFromModuleSource -ModuleSource $moduleSource

                Write-Output -InputObject $packageSource
            }
            elseif( -not (Test-WildcardPattern $moduleSourceName))
            {
                $message = $LocalizedData.RepositoryNotFound -f ($moduleSourceName)

                Write-Error -Message $message -ErrorId "RepositoryNotFound" -Category InvalidOperation -TargetObject $moduleSourceName
            }
        }
    }
}

function Remove-PackageSource
{ 
    param
    (
        [string]
        $Name
    )

    Write-Debug ($LocalizedData.ProviderApiDebugMessage -f ('Remove-PackageSource'))

    Set-ModuleSourcesVariable -Force

    $ModuleSourcesToBeRemoved = @()

    foreach ($moduleSourceName in $Name)
    {
        if($request.IsCanceled)
        {
            return
        }

        # PSGallery module source cannot be unregistered
        if($moduleSourceName -eq $Script:PSGalleryModuleSource)
        {
            $message = $LocalizedData.RepositoryCannotBeUnregistered -f ($moduleSourceName)
            Write-Error -Message $message -ErrorId "RepositoryCannotBeUnregistered" -Category InvalidOperation -TargetObject $moduleSourceName
            continue
        }

        # Check if $Name contains any wildcards
        if(Test-WildcardPattern $moduleSourceName)
        {
            $message = $LocalizedData.RepositoryNameContainsWildCards -f ($moduleSourceName)
            Write-Error -Message $message -ErrorId "RepositoryNameContainsWildCards" -Category InvalidOperation -TargetObject $moduleSourceName
            continue
        }

        # Check if the specified module source name is in the registered module sources
        if(-not $script:PSGetModuleSources.Contains($moduleSourceName))
        {
            $message = $LocalizedData.RepositoryNotFound -f ($moduleSourceName)
            Write-Error -Message $message -ErrorId "RepositoryNotFound" -Category InvalidOperation -TargetObject $moduleSourceName
            continue
        }

        $ModuleSourcesToBeRemoved += $moduleSourceName
        $message = $LocalizedData.RepositoryUnregistered -f ($moduleSourceName)
        Write-Verbose $message
    }

    # Remove the module source
    $ModuleSourcesToBeRemoved | Microsoft.PowerShell.Core\ForEach-Object { $null = $script:PSGetModuleSources.Remove($_) }

    # Persist the module sources
    Save-ModuleSources
}

function Find-Package
{ 
    [CmdletBinding()]
    param
    (
        [string[]]
        $names,

        [string]
        $requiredVersion,

        [string]
        $minimumVersion,

        [string]
        $maximumVersion
    )

    Write-Debug ($LocalizedData.ProviderApiDebugMessage -f ('Find-Package'))

    Set-ModuleSourcesVariable

    if($RequiredVersion -and $MinimumVersion)
    {
        ThrowError -ExceptionName "System.ArgumentException" `
                   -ExceptionMessage $LocalizedData.VersionRangeAndRequiredVersionCannotBeSpecifiedTogether `
                   -ErrorId "VersionRangeAndRequiredVersionCannotBeSpecifiedTogether" `
                   -CallerPSCmdlet $PSCmdlet `
                   -ErrorCategory InvalidArgument
    }

    if($RequiredVersion -or $MinimumVersion)
    {
        if(-not $names -or $names.Count -ne 1 -or (Test-WildcardPattern -Name $names[0]))
        {
            ThrowError -ExceptionName "System.ArgumentException" `
                       -ExceptionMessage $LocalizedData.VersionParametersAreAllowedOnlyWithSingleName `
                       -ErrorId "VersionParametersAreAllowedOnlyWithSingleName" `
                       -CallerPSCmdlet $PSCmdlet `
                       -ErrorCategory InvalidArgument
        }
    }

    $options = $request.Options

    foreach( $o in $options.Keys )
    {
        Write-Debug ( "OPTION: {0} => {1}" -f ($o, $options[$o]) )
    }

    $LocationOGPHashtable = [ordered]@{}
    if($options -and $options.ContainsKey('Source'))
    {
        $SourceNames = $($options['Source'])

        Write-Verbose ($LocalizedData.SpecifiedSourceName -f ($SourceNames))

        foreach($sourceName in $SourceNames)
        {
            if($script:PSGetModuleSources.Contains($sourceName))
            {
                $ModuleSource = $script:PSGetModuleSources[$sourceName]
                $LocationOGPHashtable[$ModuleSource.SourceLocation] = (Get-ProviderName -PSCustomObject $ModuleSource)
            }
            else
            {
                $sourceByLocation = Get-SourceName -Location $sourceName

                if ($sourceByLocation)
                {
                    $ModuleSource = $script:PSGetModuleSources[$sourceByLocation]
                    $LocationOGPHashtable[$ModuleSource.SourceLocation] = (Get-ProviderName -PSCustomObject $ModuleSource)
                }
                else
                {
                    $message = $LocalizedData.RepositoryNotFound -f ($sourceName)
                    ThrowError -ExceptionName "System.ArgumentException" `
                               -ExceptionMessage $message `
                               -ErrorId "RepositoryNotFound" `
                               -CallerPSCmdlet $PSCmdlet `
                               -ErrorCategory InvalidArgument `
                               -ExceptionObject $sourceName
                }
            }
        }
    }
    elseif($options -and 
           $options.ContainsKey($script:PackageManagementProviderParam) -and 
           $options.ContainsKey('Location'))
    {
        $Location = $options['Location']
        $PackageManagementProvider = $options['PackageManagementProvider']

        Write-Verbose ($LocalizedData.SpecifiedLocationAndOGP -f ($Location, $PackageManagementProvider))

        $LocationOGPHashtable[$Location] = $PackageManagementProvider
    }
    else
    {
        Write-Verbose $LocalizedData.NoSourceNameIsSpecified

        $script:PSGetModuleSources.Values | Microsoft.PowerShell.Core\ForEach-Object { $LocationOGPHashtable[$_.SourceLocation] = (Get-ProviderName -PSCustomObject $_) }
    }

    $artifactTypes = $script:PSArtifactTypeModule
    if($options.ContainsKey($script:PSArtifactType))
    {
        $artifactTypes = $options[$script:PSArtifactType]
    }

    if($artifactTypes -eq $script:All)
    {
        $artifactTypes = @($script:PSArtifactTypeModule,$script:PSArtifactTypeScript)
    }

    $providerOptions = @{}

    if($options.ContainsKey($script:AllVersions))
    {
        $providerOptions[$script:AllVersions] = $options[$script:AllVersions]
    }

    if($options.ContainsKey($script:Filter))
    {
        $Filter = $options[$script:Filter]
        $providerOptions['Contains'] = $Filter
    }

    if($options.ContainsKey($script:Tag))
    {
        $userSpecifiedTags = $options[$script:Tag] | Microsoft.PowerShell.Utility\Select-Object -Unique        
    }
    else
    {
        $userSpecifiedTags = @($script:NotSpecified)
    }

    $specifiedDscResources = @()
    if($options.ContainsKey('DscResource'))
    {
        $specifiedDscResources = $options['DscResource'] | 
                                    Microsoft.PowerShell.Utility\Select-Object -Unique | 
                                        Microsoft.PowerShell.Core\ForEach-Object {"$($script:DscResource)_$_"}
    }

    $specifiedCommands = @()
    if($options.ContainsKey('Command'))
    {
        $specifiedCommands = $options['Command'] | 
                                Microsoft.PowerShell.Utility\Select-Object -Unique |
                                    Microsoft.PowerShell.Core\ForEach-Object {"$($script:Command)_$_"}
    }

    $specifiedIncludes = @()
    if($options.ContainsKey('Includes'))
    {
        $includes = $options['Includes'] | 
                        Microsoft.PowerShell.Utility\Select-Object -Unique | 
                            Microsoft.PowerShell.Core\ForEach-Object {"$($script:Includes)_$_"}
        
        # Add PSIncludes_DscResource to $specifiedIncludes iff -DscResource names are not specified
        # Add PSIncludes_Cmdlet or PSIncludes_Function to $specifiedIncludes iff -Command names are not specified
        # otherwise $script:NotSpecified will be added to $specifiedIncludes
        if($includes)
        {   
            if(-not $specifiedDscResources -and ($includes -contains "$($script:Includes)_DscResource") )
            {
               $specifiedIncludes += "$($script:Includes)_DscResource"
            }

            if(-not $specifiedCommands)
            {
               if($includes -contains "$($script:Includes)_Cmdlet")
               {
                   $specifiedIncludes += "$($script:Includes)_Cmdlet"
               }

               if($includes -contains "$($script:Includes)_Function")
               {
                   $specifiedIncludes += "$($script:Includes)_Function"
               }

               if($includes -contains "$($script:Includes)_Workflow")
               {
                   $specifiedIncludes += "$($script:Includes)_Workflow"
               }
            }
        }
    }

    if(-not $specifiedDscResources)
    {
        $specifiedDscResources += $script:NotSpecified
    }

    if(-not $specifiedCommands)
    {
        $specifiedCommands += $script:NotSpecified
    }

    if(-not $specifiedIncludes)
    {
        $specifiedIncludes += $script:NotSpecified
    }
    
    $providerSearchTags = @{}

    foreach($tag in $userSpecifiedTags)
    {
        foreach($include in $specifiedIncludes)
        {
            foreach($command in $specifiedCommands)
            {
                foreach($resource in $specifiedDscResources)
                {
                    $providerTags = @()
                    if($resource -ne $script:NotSpecified)
                    {
                        $providerTags += $resource
                    }

                    if($command -ne $script:NotSpecified)
                    {
                        $providerTags += $command
                    }
                    
                    if($include -ne $script:NotSpecified)
                    {
                        $providerTags += $include
                    }

                    if($tag -ne $script:NotSpecified)
                    {
                        $providerTags += $tag
                    }

                    if($providerTags)
                    {
                        $providerSearchTags["$tag $resource $command $include"] = $providerTags
                    }
                }
            }
        }
    }

    $InstallationPolicy = "Untrusted"
    if($options.ContainsKey('InstallationPolicy'))
    {
        $InstallationPolicy = $options['InstallationPolicy']
    }

    $streamedResults = @()

    foreach($artifactType in $artifactTypes)
    {
        foreach($kvPair in $LocationOGPHashtable.GetEnumerator())
        {
            if($request.IsCanceled)
            {
                return
            }

            $Location = $kvPair.Key
            if($artifactType -eq $script:PSArtifactTypeScript)
            {
                $sourceName = Get-SourceName -Location $Location

                if($SourceName)
                {
                    $ModuleSource = $script:PSGetModuleSources[$SourceName]

                    # Skip source if no ScriptSourceLocation is available.
                    if(-not $ModuleSource.ScriptSourceLocation)
                    {
                        continue
                    }

                    $Location = $ModuleSource.ScriptSourceLocation
                }
            }

            $ProviderName = $kvPair.Value

            Write-Verbose ($LocalizedData.GettingPackageManagementProviderObject -f ($ProviderName))

	        $provider = $request.SelectProvider($ProviderName)

            if(-not $provider)
            {
                Write-Error -Message ($LocalizedData.PackageManagementProviderIsNotAvailable -f $ProviderName)

                Continue
            }

            Write-Verbose ($LocalizedData.SpecifiedLocationAndOGP -f ($Location, $provider.ProviderName))	

            if($providerSearchTags.Values.Count)
            {
                $tagList = $providerSearchTags.Values
            }
            else
            {
                $tagList = @($script:NotSpecified)
            }

            $namesParameterEmpty = ($names.Count -eq 1) -and ($names[0] -eq '')
        
            foreach($providerTag in $tagList)
            {
                if($request.IsCanceled)
                {
                    return
                }

                $FilterOnTag = @()

                if($providerTag -ne $script:NotSpecified)
                {
                    $FilterOnTag = $providerTag
                }

                if(Microsoft.PowerShell.Management\Test-Path -Path $Location)
                {
                    if($artifactType -eq $script:PSArtifactTypeScript)
                    {
                        $FilterOnTag += 'PSScript'
                    }
                    elseif($artifactType -eq $script:PSArtifactTypeModule)
                    {
                        $FilterOnTag += 'PSModule'
                    }
                }

                if($FilterOnTag)
                {
                    $providerOptions["FilterOnTag"] = $FilterOnTag
                }

                if($request.Options.ContainsKey($script:FindByCanonicalId))
                {
                    $providerOptions[$script:FindByCanonicalId] = $request.Options[$script:FindByCanonicalId]
                }

                $pkgs = $provider.FindPackages($names, 
                                               $requiredVersion, 
                                               $minimumVersion, 
                                               $maximumVersion,
                                               (New-Request -Sources @($Location) -Options $providerOptions) )

                foreach($pkg in  $pkgs)
                {
                    if($request.IsCanceled)
                    {
                        return
                    }

                    # $pkg.Name has to match any of the supplied names, using PowerShell wildcards
                    if ($namesParameterEmpty -or ($names | % { if ($pkg.Name -like $_){return $true; break} } -End {return $false}))
                    {
                        $fastPackageReference = New-FastPackageReference -ProviderName $provider.ProviderName `
                                                                         -PackageName $pkg.Name `
                                                                         -Version $pkg.Version `
                                                                         -Source $Location `
                                                                         -ArtifactType $artifactType

                        if($streamedResults -notcontains $fastPackageReference)
                        {
                            $streamedResults += $fastPackageReference

                            $FromTrustedSource = $false

                            $ModuleSourceName = Get-SourceName -Location $Location

                            if($ModuleSourceName)
                            {
                                $FromTrustedSource = $script:PSGetModuleSources[$ModuleSourceName].Trusted
                            }
                            elseif($InstallationPolicy -eq "Trusted")
                            {
                                $FromTrustedSource = $true
                            }

                            $sid = New-SoftwareIdentityFromPackage -Package $pkg `
                                                                   -PackageManagementProviderName $provider.ProviderName `
                                                                   -SourceLocation $Location `
                                                                   -IsFromTrustedSource:$FromTrustedSource `
                                                                   -Type $artifactType `
                                                                   -request $request
            
                            $script:FastPackRefHastable[$fastPackageReference] = $pkg

                            Write-Output -InputObject $sid
                        }
                    }
                }
            }
        }
    }
}

function Install-Package
{ 
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $fastPackageReference
    )

    Set-ModuleSourcesVariable

    Write-Debug ($LocalizedData.ProviderApiDebugMessage -f ('Install-Package'))

    Write-Debug ($LocalizedData.FastPackageReference -f $fastPackageReference)     
    
    $Force = $false
    $MinimumVersion = $null
    $RequiredVersion = $null
    $IsSavePackage = $false
    $Scope = $null

    # take the fastPackageReference and get the package object again.
    $parts = $fastPackageReference -Split '[|]'

    if( $parts.Length -eq 5 )
    {
        $providerName = $parts[0]
        $packageName = $parts[1]
        $version = $parts[2]
        $sourceLocation= $parts[3]
        $artfactType = $parts[4]

        # The default destination location for Modules is ProgramFiles path and for script MyDocuments path
        if($artfactType -eq $script:PSArtifactTypeScript)
        {
            $scriptDestination = $script:MyDocumentsScriptsPath
            $moduleDestination = $script:MyDocumentsModulesPath
            $AdminPreviligeErrorMessage = $LocalizedData.InstallScriptNeedsCurrentUserScopeParameterForNonAdminUser -f @($script:ProgramFilesScriptsPath, $script:MyDocumentsScriptsPath)
            $AdminPreviligeErrorId = 'InstallScriptNeedsCurrentUserScopeParameterForNonAdminUser'
        }
        else
        {
            $scriptDestination = $script:MyDocumentsScriptsPath
            $moduleDestination = $script:programFilesModulesPath
            $AdminPreviligeErrorMessage = $LocalizedData.InstallModuleNeedsCurrentUserScopeParameterForNonAdminUser -f @($script:programFilesModulesPath, $script:MyDocumentsModulesPath)
            $AdminPreviligeErrorId = 'InstallModuleNeedsCurrentUserScopeParameterForNonAdminUser'
        }

        $installUpdate = $false

        $options = $request.Options

        if($options)
        {
            foreach( $o in $options.Keys )
            {
                Write-Debug ("OPTION: {0} => {1}" -f ($o, $request.Options[$o]) )
            }

            if($options.ContainsKey('Scope'))
            {
                $Scope = $options['Scope']
                Write-Verbose ($LocalizedData.SpecifiedInstallationScope -f $Scope)
        
                if($Scope -eq "CurrentUser")
                {
                    $scriptDestination = $script:MyDocumentsScriptsPath
                    $moduleDestination = $script:MyDocumentsModulesPath
                }
                elseif($Scope -eq "AllUsers")
                {
                    $scriptDestination = $script:ProgramFilesScriptsPath
                    $moduleDestination = $script:programFilesModulesPath

                    if(-not (Test-RunningAsElevated))
                    {
                        # Throw an error when Install-Module/Script is used as a non-admin user and '-Scope CurrentUser' is not specified
                        ThrowError -ExceptionName "System.ArgumentException" `
                                    -ExceptionMessage $AdminPreviligeErrorMessage `
                                    -ErrorId $AdminPreviligeErrorId `
                                    -CallerPSCmdlet $PSCmdlet `
                                    -ErrorCategory InvalidArgument
                    }
                }
            }
            elseif($options.ContainsKey('DestinationPath'))
            {
                $IsSavePackage = $true
                $scriptDestination = $options['DestinationPath']
                $moduleDestination = $options['DestinationPath']
            }
            # if no scope and no destination path and not elevated, then raised error
            elseif($artfactType -eq $script:PSArtifactTypeModule -and -not (Test-RunningAsElevated))
            {
                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage $AdminPreviligeErrorMessage `
                           -ErrorId $AdminPreviligeErrorId `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument
            }

            if($Scope -and $artfactType -eq $script:PSArtifactTypeScript)
            {
                # Check and add the scope path to PATH environment variable
                if($Scope -eq 'AllUsers')
                {
                    $envVariableTarget = 'Machine'
                }
                else
                {
                    $envVariableTarget = 'User'
                }

                $currentPATHValue = [Environment]::GetEnvironmentVariable('PATH', $envVariableTarget)
                if(($currentPATHValue -split ';') -notcontains $scriptDestination)
                {
                    [Environment]::SetEnvironmentVariable('PATH', "$currentPATHValue;$scriptDestination", $envVariableTarget)
                }
            }
            
            if($artfactType -eq $script:PSArtifactTypeModule)
            {
                $message = $LocalizedData.ModuleDestination -f @($moduleDestination)
            }
            else
            {
                $message = $LocalizedData.ScriptDestination -f @($scriptDestination, $moduleDestination)
            }            
            Write-Verbose $message

            if($options.ContainsKey('Force'))
            {
                $Force = $options['Force']
            }
            
            if($options.ContainsKey('MinimumVersion'))
            {
                $MinimumVersion = $options['MinimumVersion']
            }

            if($options.ContainsKey('RequiredVersion'))
            {
                $RequiredVersion = $options['RequiredVersion']
            }
                        
            if($options.ContainsKey('InstallUpdate'))
            {
                $installUpdate = $options['InstallUpdate']
            }            
        }

        Write-Debug "ArtfactType is $artfactType"

        if($artfactType -eq $script:PSArtifactTypeModule)
        {
            # Test if module is already installed
            $InstalledModuleInfo = if(-not $IsSavePackage){ Test-ModuleInstalled -Name $packageName -RequiredVersion $RequiredVersion }

            if(-not $Force -and $InstalledModuleInfo)
            {
                if($RequiredVersion -and (Test-ModuleSxSVersionSupport))
                {
                    # Check if the module with the required version is already installed otherwise proceed to install/update.
                    if($InstalledModuleInfo)
                    {
                        $message = $LocalizedData.ModuleWithRequiredVersionAlreadyInstalled -f ($InstalledModuleInfo.Version, $InstalledModuleInfo.Name, $InstalledModuleInfo.ModuleBase, $InstalledModuleInfo.Version)
                        Write-Error -Message $message -ErrorId "ModuleWithRequiredVersionAlreadyInstalled" -Category InvalidOperation

                        return
                    }
                }
                else
                {
                    if(-not $installUpdate)
                    {
                        if( (-not $MinimumVersion -and ($version -ne $InstalledModuleInfo.Version)) -or 
                            ($MinimumVersion -and ($MinimumVersion -gt $InstalledModuleInfo.Version)))
                        {
                            if($PSVersionTable.PSVersion -ge [Version]"5.0")
                            {
                                $message = $LocalizedData.ModuleAlreadyInstalledSxS -f ($InstalledModuleInfo.Version, $InstalledModuleInfo.Name, $InstalledModuleInfo.ModuleBase, $version, $InstalledModuleInfo.Version, $version)                            
                            }
                            else
                            {
                                $message = $LocalizedData.ModuleAlreadyInstalled -f ($InstalledModuleInfo.Version, $InstalledModuleInfo.Name, $InstalledModuleInfo.ModuleBase, $InstalledModuleInfo.Version, $version)
                            }
                            Write-Error -Message $message -ErrorId "ModuleAlreadyInstalled" -Category InvalidOperation
                        }
                        else
                        {
                            $message = $LocalizedData.ModuleAlreadyInstalledVerbose -f ($InstalledModuleInfo.Version, $InstalledModuleInfo.Name, $InstalledModuleInfo.ModuleBase)
                            Write-Verbose $message                
                        }

                        return
                    }
                    else
                    {
                        if($InstalledModuleInfo.Version -lt $version)
                        {
                            $message = $LocalizedData.FoundModuleUpdate -f ($InstalledModuleInfo.Name, $version)
                            Write-Verbose $message    
                        }
                        else
                        {
                            $message = $LocalizedData.NoUpdateAvailable -f ($InstalledModuleInfo.Name)
                            Write-Verbose $message
                            return
                        }
                    }
                }
            }
        }

        if($artfactType -eq $script:PSArtifactTypeScript)
        {
            # Test if script is already installed
            $InstalledScriptInfo = if(-not $IsSavePackage){ Test-ScriptInstalled -Name $packageName }

            Write-Debug "InstalledScriptInfo is $InstalledScriptInfo"

            if(-not $Force -and $InstalledScriptInfo)
            {
                if(-not $installUpdate)
                {
                    if( (-not $MinimumVersion -and ($version -ne $InstalledScriptInfo.Version)) -or 
                        ($MinimumVersion -and ($MinimumVersion -gt $InstalledScriptInfo.Version)))
                    {
                        $message = $LocalizedData.ScriptAlreadyInstalled -f ($InstalledScriptInfo.Version, $InstalledScriptInfo.Name, $InstalledScriptInfo.ScriptBase, $InstalledScriptInfo.Version, $version)
                        Write-Error -Message $message -ErrorId "ScriptAlreadyInstalled" -Category InvalidOperation
                    }
                    else
                    {
                        $message = $LocalizedData.ScriptAlreadyInstalledVerbose -f ($InstalledScriptInfo.Version, $InstalledScriptInfo.Name, $InstalledScriptInfo.ScriptBase)
                        Write-Verbose $message                
                    }

                    return
                }
                else
                {
                    if($InstalledScriptInfo.Version -lt $version)
                    {
                        $message = $LocalizedData.FoundScriptUpdate -f ($InstalledScriptInfo.Name, $version)
                        Write-Verbose $message
                    }
                    else
                    {
                        $message = $LocalizedData.NoScriptUpdateAvailable -f ($InstalledScriptInfo.Name)
                        Write-Verbose $message
                        return
                    }
                }
            }
        }

        # create a temp folder and download the module
        $tempDestination = Microsoft.PowerShell.Management\Join-Path -Path $script:TempPath -ChildPath "$(Microsoft.PowerShell.Utility\Get-Random)"
        $null = Microsoft.PowerShell.Management\New-Item -Path $tempDestination -ItemType Directory -Force -Confirm:$false -WhatIf:$false

        try
        {
            $provider = $request.SelectProvider($providerName)
            if(-not $provider)
            {
                Write-Error -Message ($LocalizedData.PackageManagementProviderIsNotAvailable -f $providerName)

                return
            }

            if($request.IsCanceled)
            {
                return
            }

            Write-Verbose ($LocalizedData.SpecifiedLocationAndOGP -f ($provider.ProviderName, $providerName))
		
            $newRequest = New-Request -Options @{Destination=$tempDestination;
                                                 ExcludeVersion=$true} `
                                      -Sources @($SourceLocation)

            if($artfactType -eq $script:PSArtifactTypeModule)
            {
                $message = $LocalizedData.DownloadingModuleFromGallery -f ($packageName, $version, $sourceLocation)
            }
            else
            {
                $message = $LocalizedData.DownloadingScriptFromGallery -f ($packageName, $version, $sourceLocation)
            }
            Write-Verbose $message

            $installedPkgs = $provider.InstallPackage($script:FastPackRefHastable[$fastPackageReference], $newRequest)

            foreach($pkg in $installedPkgs)
            {
                if($request.IsCanceled)
                {
                    return
                }

                $destinationModulePath = Microsoft.PowerShell.Management\Join-Path -Path $moduleDestination -ChildPath $pkg.Name

                # Side-by-Side module version is avialable on PowerShell 5.0 or later versions only
                # By default, PowerShell module versions will be installed/updated Side-by-Side.
                if(Test-ModuleSxSVersionSupport)
                {
                    $destinationModulePath = Microsoft.PowerShell.Management\Join-Path -Path $destinationModulePath -ChildPath $pkg.Version
                }

                $destinationscriptPath = $scriptDestination

                # Get actual artifact type from the package
                $packageType = $script:PSArtifactTypeModule
                $installLocation = $destinationModulePath
                $tempPackagePath = Microsoft.PowerShell.Management\Join-Path -Path $tempDestination -ChildPath $pkg.Name
                if(Microsoft.PowerShell.Management\Test-Path -Path $tempPackagePath)
                {
                    $packageFiles = Microsoft.PowerShell.Management\Get-ChildItem -Path $tempPackagePath -Recurse -Exclude "*.nupkg","*.nuspec"

                    if($packageFiles -and $packageFiles.GetType().ToString() -eq 'System.IO.FileInfo' -and $packageFiles.Name -eq "$($pkg.Name).ps1")
                    {
                        $packageType = $script:PSArtifactTypeScript
                        $installLocation = $destinationscriptPath
                    }
                }
                
                $sid = New-SoftwareIdentityFromPackage -Package $pkg `
                                                       -SourceLocation $sourceLocation `
                                                       -PackageManagementProviderName $provider.ProviderName `
                                                       -Request $request `
                                                       -Type $packageType `
                                                       -InstalledLocation $installLocation

                # construct the PSGetItemInfo from SoftwareIdentity and persist it
                $psgItemInfo = New-PSGetItemInfo -SoftwareIdenties $pkg `
                                                 -PackageManagementProviderName $provider.ProviderName `
                                                 -SourceLocation $sourceLocation `
                                                 -Type $packageType `
                                                 -InstalledLocation $installLocation

                if($packageType -eq $script:PSArtifactTypeModule)
                {
                    if ($psgItemInfo.PowerShellGetFormatVersion -and 
                        ($script:SupportedPSGetFormatVersionMajors -notcontains $psgItemInfo.PowerShellGetFormatVersion.Major))
                    {
                        $message = $LocalizedData.NotSupportedPowerShellGetFormatVersion -f ($psgItemInfo.Name, $psgItemInfo.PowerShellGetFormatVersion, $psgItemInfo.Name)
                        Write-Error -Message $message -ErrorId "NotSupportedPowerShellGetFormatVersion" -Category InvalidOperation
                        continue
                    }
                
                    if(-not $psgItemInfo.PowerShellGetFormatVersion)
                    {
                        $sourceModulePath = Microsoft.PowerShell.Management\Join-Path $tempDestination $pkg.Name
                    }
                    else
                    {
                        $sourceModulePath = Microsoft.PowerShell.Management\Join-Path $tempDestination "$($pkg.Name)\Content\*\$script:ModuleReferences\$($pkg.Name)"
                    }

                    # Validate the module
                    $CurrentModuleInfo = Test-ValidManifestModule -ModuleBasePath $sourceModulePath
                    if(-not $IsSavePackage -and -not $CurrentModuleInfo)
                    {
                        $message = $LocalizedData.InvalidPSModule -f ($pkg.Name)
                        Write-Error -Message $message -ErrorId "InvalidManifestModule" -Category InvalidOperation
                        continue
                    }

                    # Test if module is already installed
                    $InstalledModuleInfo2 = if(-not $IsSavePackage){ Test-ModuleInstalled -Name $pkg.Name -RequiredVersion $pkg.Version }

                    if($pkg.Name -ne $packageName)
                    {
                        if(-not $Force -and $InstalledModuleInfo2)
                        {
                            if(Test-ModuleSxSVersionSupport)
                            {
                                if($pkg.version -eq $InstalledModuleInfo2.Version)
                                {
                                    if(-not $installUpdate)
                                    {
                                        $message = $LocalizedData.ModuleWithRequiredVersionAlreadyInstalled -f ($InstalledModuleInfo2.Version, $InstalledModuleInfo2.Name, $InstalledModuleInfo2.ModuleBase, $InstalledModuleInfo2.Version)
                                    }
                                    else
                                    {
                                        $message = $LocalizedData.NoUpdateAvailable -f ($pkg.Name)
                                    }

                                    Write-Verbose $message
                                    Continue
                                }
                            }
                            else
                            {
                                if(-not $installUpdate)
                                {
                                    $message = $LocalizedData.ModuleAlreadyInstalledVerbose -f ($InstalledModuleInfo2.Version, $InstalledModuleInfo2.Name, $InstalledModuleInfo2.ModuleBase)
                                    Write-Verbose $message
                                    Continue
                                }
                                else
                                {
                                    if($pkg.version -gt $InstalledModuleInfo2.Version)
                                    {
                                        $message = $LocalizedData.FoundModuleUpdate -f ($pkg.Name, $pkg.Version)
                                        Write-Verbose $message
                                    }
                                    else
                                    {
                                        $message = $LocalizedData.NoUpdateAvailable -f ($pkg.Name)
                                        Write-Verbose $message
                                        Continue
                                    }
                                }
                            }
                        }
                                    
                        if($IsSavePackage)
                        {
                            $DependencyInstallMessage = $LocalizedData.SavingDependencyModule -f ($pkg.Name, $pkg.Version, $packageName)
                        }
                        else
                        {
                            $DependencyInstallMessage = $LocalizedData.InstallingDependencyModule -f ($pkg.Name, $pkg.Version, $packageName)
                        }
                    
                        Write-Verbose  $DependencyInstallMessage
                    }

                    # check if module is in use
                    if($InstalledModuleInfo2)
                    {
                        $moduleInUse = Test-ModuleInUse -ModuleBasePath $InstalledModuleInfo2.ModuleBase `
                                                        -ModuleName $InstalledModuleInfo2.Name `
                                                        -ModuleVersion $InstalledModuleInfo2.Version `
                                                        -Verbose:$VerbosePreference `
                                                        -WarningAction $WarningPreference `
                                                        -ErrorAction $ErrorActionPreference `
                                                        -Debug:$DebugPreference
 
                        if($moduleInUse)
                        {
                            $message = $LocalizedData.ModuleIsInUse -f ($psgItemInfo.Name)
                            Write-Verbose $message
                            continue
                        }
                    }

                    Copy-Module -SourcePath $sourceModulePath -DestinationPath $destinationModulePath -PSGetItemInfo $psgItemInfo

                    # Write warning messages if externally managed module dependencies are not installed.
                    $ExternalModuleDependencies = Get-ExternalModuleDependencies -PSModuleInfo $CurrentModuleInfo
                    foreach($ExternalDependency in $ExternalModuleDependencies)
                    {
                        $depModuleInfo = Test-ModuleInstalled -Name $ExternalDependency

                        if(-not $depModuleInfo)
                        {
                            Write-Warning -Message ($LocalizedData.MissingExternallyManagedModuleDependency -f $ExternalDependency,$pkg.Name,$ExternalDependency)
                        }
                        else
                        {
                            Write-Verbose -Message ($LocalizedData.ExternallyManagedModuleDependencyIsInstalled -f $ExternalDependency)
                        }
                    }
                                    
                    # Remove the old module base folder if it is different from the required destination module path when -Force is specified
                    if($Force -and 
                        $InstalledModuleInfo2 -and
                        -not $destinationModulePath.StartsWith($InstalledModuleInfo2.ModuleBase, [System.StringComparison]::OrdinalIgnoreCase))
                    {
                        Microsoft.PowerShell.Management\Remove-Item -Path $InstalledModuleInfo2.ModuleBase `
                                                                    -Force -Recurse `
                                                                    -ErrorAction SilentlyContinue `
                                                                    -WarningAction SilentlyContinue `
                                                                    -Confirm:$false -WhatIf:$false
                    }

                    if($IsSavePackage)
                    {
                        $message = $LocalizedData.ModuleSavedSuccessfully -f ($psgItemInfo.Name)
                    }
                    else
                    {
                        $message = $LocalizedData.ModuleInstalledSuccessfully -f ($psgItemInfo.Name)
                    }                
                    Write-Verbose $message
                }


                if($packageType -eq $script:PSArtifactTypeScript)
                {
                    if ($psgItemInfo.PowerShellGetFormatVersion -and 
                        ($script:SupportedPSGetFormatVersionMajors -notcontains $psgItemInfo.PowerShellGetFormatVersion.Major))
                    {
                        $message = $LocalizedData.NotSupportedPowerShellGetFormatVersionScripts -f ($psgItemInfo.Name, $psgItemInfo.PowerShellGetFormatVersion, $psgItemInfo.Name)
                        Write-Error -Message $message -ErrorId "NotSupportedPowerShellGetFormatVersion" -Category InvalidOperation
                        continue
                    }

                    $sourceScriptPath = Microsoft.PowerShell.Management\Join-Path -Path $tempPackagePath -ChildPath "$($pkg.Name).ps1"
                    
                    # Validate the script
                    $currentScriptInfo = Test-ScriptFile -Path $sourceScriptPath -ErrorAction SilentlyContinue
                    
                    if(-not $IsSavePackage -and -not $currentScriptInfo)
                    {
                        $message = $LocalizedData.InvalidPowerShellScriptFile -f ($pkg.Name)
                        Write-Error -Message $message -ErrorId "InvalidPowerShellScriptFile" -Category InvalidOperation -TargetObject $pkg.Name
                        continue
                    }

                    # Test if script is already installed
                    $InstalledScriptInfo2 = if(-not $IsSavePackage){ Test-ScriptInstalled -Name $pkg.Name }

                    if($pkg.Name -ne $packageName)
                    {
                        if(-not $Force -and $InstalledScriptInfo2)
                        {
                            if(-not $installUpdate)
                            {
                                $message = $LocalizedData.ScriptAlreadyInstalledVerbose -f ($InstalledScriptInfo2.Version, $InstalledScriptInfo2.Name, $InstalledScriptInfo2.ScriptBase)
                                Write-Verbose $message
                                Continue
                            }
                            else
                            {
                                if($pkg.version -gt $InstalledScriptInfo2.Version)
                                {
                                    $message = $LocalizedData.FoundScriptUpdate -f ($pkg.Name, $pkg.Version)
                                    Write-Verbose $message
                                }
                                else
                                {
                                    $message = $LocalizedData.NoScriptUpdateAvailable -f ($pkg.Name)
                                    Write-Verbose $message
                                    Continue
                                }
                            }
                        }
                                    
                        if($IsSavePackage)
                        {
                            $DependencyInstallMessage = $LocalizedData.SavingDependencyScript -f ($pkg.Name, $pkg.Version, $packageName)
                        }
                        else
                        {
                            $DependencyInstallMessage = $LocalizedData.InstallingDependencyScript -f ($pkg.Name, $pkg.Version, $packageName)
                        }
                    
                        Write-Verbose  $DependencyInstallMessage
                    }

                    Write-Debug "SourceScriptPath is $sourceScriptPath and DestinationscriptPath is $destinationscriptPath"
                    Copy-ScriptFile -SourcePath $sourceScriptPath -DestinationPath $destinationscriptPath -PSGetItemInfo $psgItemInfo -Scope $Scope

                    # Write warning messages if externally managed module dependencies are not installed.
                    foreach($ExternalDependency in $currentScriptInfo.ExternalModuleDependencies)
                    {
                        $depModuleInfo = Test-ModuleInstalled -Name $ExternalDependency

                        if(-not $depModuleInfo)
                        {
                            Write-Warning -Message ($LocalizedData.ScriptMissingExternallyManagedModuleDependency -f $ExternalDependency,$pkg.Name,$ExternalDependency)
                        }
                        else
                        {
                            Write-Verbose -Message ($LocalizedData.ExternallyManagedModuleDependencyIsInstalled -f $ExternalDependency)
                        }
                    }

                    # Write warning messages if externally managed script dependencies are not installed.
                    foreach($ExternalDependency in $currentScriptInfo.ExternalScriptDependencies)
                    {
                        $depScriptInfo = Test-ScriptInstalled -Name $ExternalDependency

                        if(-not $depScriptInfo)
                        {
                            Write-Warning -Message ($LocalizedData.ScriptMissingExternallyManagedScriptDependency -f $ExternalDependency,$pkg.Name,$ExternalDependency)
                        }
                        else
                        {
                            Write-Verbose -Message ($LocalizedData.ScriptExternallyManagedScriptDependencyIsInstalled -f $ExternalDependency)
                        }
                    }
                                    
                    # Remove the old scriptfile if it's path different from the required destination script path when -Force is specified
                    if($Force -and 
                        $InstalledScriptInfo2 -and
                        -not $destinationscriptPath.StartsWith($InstalledScriptInfo2.ScriptBase, [System.StringComparison]::OrdinalIgnoreCase))
                    {
                        Microsoft.PowerShell.Management\Remove-Item -Path $InstalledScriptInfo2.Path `
                                                                    -Force `
                                                                    -ErrorAction SilentlyContinue `
                                                                    -WarningAction SilentlyContinue `
                                                                    -Confirm:$false -WhatIf:$false
                    }

                    if($IsSavePackage)
                    {
                        $message = $LocalizedData.ScriptSavedSuccessfully -f ($psgItemInfo.Name)
                    }
                    else
                    {
                        $message = $LocalizedData.ScriptInstalledSuccessfully -f ($psgItemInfo.Name)
                    }                
                    Write-Verbose $message
                }

                Write-Output -InputObject $sid
            }
        }
        finally
        {
            Microsoft.PowerShell.Management\Remove-Item $tempDestination -Force -Recurse -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
        }
    }
}

function Uninstall-Package
{ 
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $fastPackageReference
    )

    Write-Debug -Message ($LocalizedData.ProviderApiDebugMessage -f ('Uninstall-Package'))

    Write-Debug -Message ($LocalizedData.FastPackageReference -f $fastPackageReference)
    
    # take the fastPackageReference and get the package object again.
    $parts = $fastPackageReference -Split '[|]'
    $Force = $false

    $options = $request.Options
    if($options)
    {
        foreach( $o in $options.Keys )
        {
            Write-Debug -Message ("OPTION: {0} => {1}" -f ($o, $request.Options[$o]) )
        }
    }

    if($parts.Length -eq 5)
    {
        $providerName = $parts[0]
        $packageName = $parts[1]
        $version = $parts[2]
        $sourceLocation= $parts[3]
        $artfactType = $parts[4]

        if($request.IsCanceled)
        {
            return
        }
        
        if($options.ContainsKey('Force'))
        {
            $Force = $options['Force']
        }

        if($artfactType -eq $script:PSArtifactTypeModule)
        {
            $moduleName = $packageName
            $InstalledModuleInfo = $script:PSGetInstalledModules["$($moduleName)$($version)"] 

            if(-not $InstalledModuleInfo)
            {
                $message = $LocalizedData.ModuleUninstallationNotPossibleAsItIsNotInstalledUsingPowerShellGet -f $moduleName

                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage $message `
                           -ErrorId "ModuleUninstallationNotPossibleAsItIsNotInstalledUsingPowerShellGet" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument
            
                return
            }

            $moduleBase = $InstalledModuleInfo.PSGetItemInfo.InstalledLocation

            if(-not (Test-RunningAsElevated) -and $moduleBase.StartsWith($script:programFilesModulesPath, [System.StringComparison]::OrdinalIgnoreCase))
            {                            
                $message = $LocalizedData.AdminPrivilegesRequiredForUninstall -f ($moduleName, $moduleBase)

                ThrowError -ExceptionName "System.InvalidOperationException" `
                           -ExceptionMessage $message `
                           -ErrorId "AdminPrivilegesRequiredForUninstall" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidOperation

                return
            }

            $dependentModuleScript = {
                                param ([string] $moduleName)
                                Microsoft.PowerShell.Core\Get-Module -ListAvailable | 
                                Microsoft.PowerShell.Core\Where-Object {                            
                                    ($moduleName -ne $_.Name) -and (
                                    ($_.RequiredModules -and $_.RequiredModules.Name -contains $moduleName) -or
                                    ($_.NestedModules -and $_.NestedModules.Name -contains $moduleName))
                                }
                            }
            $dependentModulesJob =  Microsoft.PowerShell.Core\Start-Job -ScriptBlock $dependentModuleScript -ArgumentList $moduleName
            Microsoft.PowerShell.Core\Wait-Job -job $dependentModulesJob
            $dependentModules = Microsoft.PowerShell.Core\Receive-Job -job $dependentModulesJob

            if(-not $Force -and $dependentModules)
            {
                $message = $LocalizedData.UnableToUninstallAsOtherModulesNeedThisModule -f ($moduleName, $version, $moduleBase, $(($dependentModules.Name | Select-Object -Unique) -join ','), $moduleName)

                ThrowError -ExceptionName "System.InvalidOperationException" `
                           -ExceptionMessage $message `
                           -ErrorId "UnableToUninstallAsOtherModulesNeedThisModule" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidOperation

                return
            }

            $moduleInUse = Test-ModuleInUse -ModuleBasePath $moduleBase `
                                            -ModuleName $InstalledModuleInfo.PSGetItemInfo.Name`
                                            -ModuleVersion $InstalledModuleInfo.PSGetItemInfo.Version `
                                            -Verbose:$VerbosePreference `
                                            -WarningAction $WarningPreference `
                                            -ErrorAction $ErrorActionPreference `
                                            -Debug:$DebugPreference

            if($moduleInUse)
            {
                $message = $LocalizedData.ModuleIsInUse -f ($moduleName)

                ThrowError -ExceptionName "System.InvalidOperationException" `
                           -ExceptionMessage $message `
                           -ErrorId "ModuleIsInUse" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidOperation

                return
            }

            $ModuleBaseFolderToBeRemoved = $moduleBase

            # With SxS version support, more than one version of the module can be installed.
            # - Remove the parent directory of the module version base when only one version is installed
            # - Don't remove the modulebase when it was installed before SxS version support and 
            #   other versions are installed under the module base folder
            # 
            if(Test-ModuleSxSVersionSupport)
            {
                $ModuleBaseWithoutVersion = $moduleBase
                $IsModuleInstalledAsSxSVersion = $false

                if($moduleBase.EndsWith("$version", [System.StringComparison]::OrdinalIgnoreCase))
                {
                    $IsModuleInstalledAsSxSVersion = $true
                    $ModuleBaseWithoutVersion = Microsoft.PowerShell.Management\Split-Path -Path $moduleBase -Parent
                }

                $InstalledVersionsWithSameModuleBase = @()
                Get-Module -Name $moduleName -ListAvailable | 
                    Microsoft.PowerShell.Core\ForEach-Object {
                        if($_.ModuleBase.StartsWith($ModuleBaseWithoutVersion, [System.StringComparison]::OrdinalIgnoreCase))
                        {
                            $InstalledVersionsWithSameModuleBase += $_.ModuleBase
                        }
                    }

                # Remove ..\ModuleName directory when only one module is installed with the same ..\ModuleName path 
                # like ..\ModuleName\1.0 or ..\ModuleName
                if($InstalledVersionsWithSameModuleBase.Count -eq 1)
                {
                    $ModuleBaseFolderToBeRemoved = $ModuleBaseWithoutVersion
                }
                elseif($ModuleBaseWithoutVersion -eq $moduleBase)
                {
                    # There are version specific folders under the same module base dir
                    # Throw an error saying uninstall other versions then uninstall this current version
                    $message = $LocalizedData.UnableToUninstallModuleVersion -f ($moduleName, $version, $moduleBase)

                    ThrowError -ExceptionName "System.InvalidOperationException" `
                               -ExceptionMessage $message `
                               -ErrorId "UnableToUninstallModuleVersion" `
                               -CallerPSCmdlet $PSCmdlet `
                               -ErrorCategory InvalidOperation

                    return
                }
                # Otherwise specified version folder will be removed as current module base is assigned to $ModuleBaseFolderToBeRemoved
            }

            Microsoft.PowerShell.Management\Remove-Item -Path $ModuleBaseFolderToBeRemoved `
                                                        -Force -Recurse `
                                                        -ErrorAction SilentlyContinue `
                                                        -WarningAction SilentlyContinue `
                                                        -Confirm:$false -WhatIf:$false        
                                                    
            $message = $LocalizedData.ModuleUninstallationSucceeded -f $moduleName, $moduleBase
            Write-Verbose  $message       

            Write-Output -InputObject $InstalledModuleInfo.SoftwareIdentity
        }
        elseif($artfactType -eq $script:PSArtifactTypeScript)
        {
            $scriptName = $packageName
            $InstalledScriptInfo = $script:PSGetInstalledScripts["$($scriptName)$($version)"] 

            if(-not $InstalledScriptInfo)
            {
                $message = $LocalizedData.ScriptUninstallationNotPossibleAsItIsNotInstalledUsingPowerShellGet -f $scriptName
                ThrowError -ExceptionName "System.ArgumentException" `
                           -ExceptionMessage $message `
                           -ErrorId "ScriptUninstallationNotPossibleAsItIsNotInstalledUsingPowerShellGet" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidArgument
            
                return
            }

            $scriptBase = $InstalledScriptInfo.PSGetItemInfo.InstalledLocation
            $installedScriptInfoPath = $script:PSGetAppLocalScriptsPath

            if($scriptBase.StartsWith($script:ProgramFilesScriptsPath, [System.StringComparison]::OrdinalIgnoreCase))
            {
                if(-not (Test-RunningAsElevated))
                {                            
                    $message = $LocalizedData.AdminPrivilegesRequiredForScriptUninstall -f ($scriptName, $scriptBase)

                    ThrowError -ExceptionName "System.InvalidOperationException" `
                               -ExceptionMessage $message `
                               -ErrorId "AdminPrivilegesRequiredForUninstall" `
                               -CallerPSCmdlet $PSCmdlet `
                               -ErrorCategory InvalidOperation

                    return
                }

                $installedScriptInfoPath = $script:PSGetProgramDataScriptsPath
            }

            # Check if there are any dependent scripts
            $dependentScriptDetails = $script:PSGetInstalledScripts.Values | 
                                          Microsoft.PowerShell.Core\Where-Object {
                                              $_.PSGetItemInfo.Dependencies -contains $scriptName
                                          }

            $dependentScriptNames = $dependentScriptDetails | 
                                        Microsoft.PowerShell.Core\ForEach-Object { $_.PSGetItemInfo.Name }

            if(-not $Force -and $dependentScriptNames)
            {
                $message = $LocalizedData.UnableToUninstallAsOtherScriptsNeedThisScript -f 
                               ($scriptName, 
                                $version, 
                                $scriptBase, 
                                $(($dependentScriptNames | Select-Object -Unique) -join ','), 
                                $scriptName)

                ThrowError -ExceptionName 'System.InvalidOperationException' `
                           -ExceptionMessage $message `
                           -ErrorId 'UnableToUninstallAsOtherScriptsNeedThisScript' `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidOperation
                return
            }

            $scriptFilePath = Microsoft.PowerShell.Management\Join-Path -Path $scriptBase `
                                                                        -ChildPath "$($scriptName).ps1"

            $installledScriptInfoFilePath = Microsoft.PowerShell.Management\Join-Path -Path $installedScriptInfoPath `
                                                                                      -ChildPath "$($scriptName)_$($script:InstalledScriptInfoFileName)" 

            # Remove the script file and it's corresponding InstalledScriptInfo.xml
            if(Microsoft.PowerShell.Management\Test-Path -Path $scriptFilePath -PathType Leaf)
            {
                Microsoft.PowerShell.Management\Remove-Item -Path $scriptFilePath `
                                                            -Force `
                                                            -ErrorAction SilentlyContinue `
                                                            -WarningAction SilentlyContinue `
                                                            -Confirm:$false -WhatIf:$false
            }

            if(Microsoft.PowerShell.Management\Test-Path -Path $installledScriptInfoFilePath -PathType Leaf)
            {
                Microsoft.PowerShell.Management\Remove-Item -Path $installledScriptInfoFilePath `
                                                            -Force `
                                                            -ErrorAction SilentlyContinue `
                                                            -WarningAction SilentlyContinue `
                                                            -Confirm:$false -WhatIf:$false
            }

            $message = $LocalizedData.ScriptUninstallationSucceeded -f $scriptName, $scriptBase
            Write-Verbose $message

            Write-Output -InputObject $InstalledScriptInfo.SoftwareIdentity
        }
    }
}

function Get-InstalledPackage
{ 
    [CmdletBinding()]
    param
    (
        [Parameter()]
        [string]
        $Name,

        [Parameter()]
        [string]
        $RequiredVersion,

        [Parameter()]
        [string]
        $MinimumVersion,

        [Parameter()]
        [string]
        $MaximumVersion
    )

    Write-Debug -Message ($LocalizedData.ProviderApiDebugMessage -f ('Get-InstalledPackage'))

    $options = $request.Options

    foreach( $o in $options.Keys )
    {
        Write-Debug ( "OPTION: {0} => {1}" -f ($o, $options[$o]) )
    }

    $artifactTypes = $script:PSArtifactTypeModule
    if($options.ContainsKey($script:PSArtifactType))
    {
        $artifactTypes = $options[$script:PSArtifactType]
    }

    if($artifactTypes -eq $script:All)
    {
        $artifactTypes = @($script:PSArtifactTypeModule,$script:PSArtifactTypeScript)
    }

    if($artifactTypes -contains $script:PSArtifactTypeModule)
    {
        Get-InstalledModuleDetails -Name $Name `
                                   -RequiredVersion $RequiredVersion `
                                   -MinimumVersion $MinimumVersion `
                                   -MaximumVersion $MaximumVersion | Microsoft.PowerShell.Core\ForEach-Object {$_.SoftwareIdentity}
    }

    if($artifactTypes -contains $script:PSArtifactTypeScript)
    {
        Get-InstalledScriptDetails -Name $Name `
                                   -RequiredVersion $RequiredVersion `
                                   -MinimumVersion $MinimumVersion `
                                   -MaximumVersion $MaximumVersion | Microsoft.PowerShell.Core\ForEach-Object {$_.SoftwareIdentity}
    }
}

#endregion

#region Internal Utility functions for the PackageManagement Provider Implementation

function Set-InstalledScriptsVariable
{
    # Initialize list of scripts installed by the PSModule provider
    $script:PSGetInstalledScripts = [ordered]@{}    
    $scriptPaths = @($script:PSGetProgramDataScriptsPath, $script:PSGetAppLocalScriptsPath)

    foreach ($location in $scriptPaths)
    {
        # find all scripts installed using PowerShellGet
        $scriptInfoFiles = Get-ChildItem -Path $location `
                                         -Filter "*$script:InstalledScriptInfoFileName" `
                                         -ErrorAction SilentlyContinue `
                                         -WarningAction SilentlyContinue

        if($scriptInfoFiles)
        {
            foreach ($scriptInfoFile in $scriptInfoFiles)
            {
                $psgetItemInfo = DeSerialize-PSObject -Path $scriptInfoFile.FullName

                $scriptFilePath = Microsoft.PowerShell.Management\Join-Path -Path $psgetItemInfo.InstalledLocation `
                                                                            -ChildPath "$($psgetItemInfo.Name).ps1"

                # Remove the InstalledScriptInfo.xml file if the actual script file was manually uninstalled by the user
                if(-not (Microsoft.PowerShell.Management\Test-Path -Path $scriptFilePath -PathType Leaf))
                {
                    Microsoft.PowerShell.Management\Remove-Item -Path $scriptInfoFile.FullName -Force -ErrorAction SilentlyContinue

                    continue
                }

                $package = New-SoftwareIdentityFromPSGetItemInfo -PSGetItemInfo $psgetItemInfo

                if($package)
                {
                    $script:PSGetInstalledScripts["$($psgetItemInfo.Name)$($psgetItemInfo.Version)"] = @{
                                                                                                            SoftwareIdentity = $package
                                                                                                            PSGetItemInfo = $psgetItemInfo
                                                                                                        }
                }
            }
        }
    }
}

function Get-InstalledScriptDetails
{ 
    [CmdletBinding()]
    param
    (
        [Parameter()]
        [string]
        $Name,

        [Parameter()]
        [string]
        $RequiredVersion,

        [Parameter()]
        [string]
        $MinimumVersion,

        [Parameter()]
        [string]
        $MaximumVersion
    )

    Set-InstalledScriptsVariable

    # Keys in $script:PSGetInstalledScripts are "<ScriptName><ScriptVersion>", 
    # first filter the installed scripts using "$Name*" wildcard search
    # then apply $Name wildcard search to get the script name which meets the specified name with wildcards.
    #
    $wildcardPattern = New-Object System.Management.Automation.WildcardPattern "$Name*",$script:wildcardOptions
    $nameWildcardPattern = New-Object System.Management.Automation.WildcardPattern $Name,$script:wildcardOptions

    $script:PSGetInstalledScripts.GetEnumerator() | Microsoft.PowerShell.Core\ForEach-Object {
                                                        if($wildcardPattern.IsMatch($_.Key))
                                                        {
                                                            $InstalledScriptDetails = $_.Value

                                                            if(-not $Name -or $nameWildcardPattern.IsMatch($InstalledScriptDetails.PSGetItemInfo.Name))
                                                            {
                                                                if($RequiredVersion)
                                                                {
                                                                   if($RequiredVersion -eq $InstalledScriptDetails.PSGetItemInfo.Version)
                                                                   {
                                                                       $InstalledScriptDetails
                                                                   }
                                                                }
                                                                else
                                                                {
                                                                    if( (-not $MinimumVersion -or ($MinimumVersion -le $InstalledScriptDetails.PSGetItemInfo.Version)) -and 
                                                                        (-not $MaximumVersion -or ($MaximumVersion -ge $InstalledScriptDetails.PSGetItemInfo.Version)))
                                                                    {
                                                                        $InstalledScriptDetails
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
}

function Get-InstalledModuleDetails
{ 
    [CmdletBinding()]
    param
    (
        [Parameter()]
        [string]
        $Name,

        [Parameter()]
        [string]
        $RequiredVersion,

        [Parameter()]
        [string]
        $MinimumVersion,

        [Parameter()]
        [string]
        $MaximumVersion
    )

    Set-InstalledModulesVariable

    # Keys in $script:PSGetInstalledModules are "<ModuleName><ModuleVersion>", 
    # first filter the installed modules using "$Name*" wildcard search
    # then apply $Name wildcard search to get the module name which meets the specified name with wildcards.
    #
    $wildcardPattern = New-Object System.Management.Automation.WildcardPattern "$Name*",$script:wildcardOptions
    $nameWildcardPattern = New-Object System.Management.Automation.WildcardPattern $Name,$script:wildcardOptions

    $script:PSGetInstalledModules.GetEnumerator() | Microsoft.PowerShell.Core\ForEach-Object {
                                                        if($wildcardPattern.IsMatch($_.Key))
                                                        {
                                                            $InstalledModuleDetails = $_.Value

                                                            if(-not $Name -or $nameWildcardPattern.IsMatch($InstalledModuleDetails.PSGetItemInfo.Name))
                                                            {
                                                                if($RequiredVersion)
                                                                {
                                                                   if($RequiredVersion -eq $InstalledModuleDetails.PSGetItemInfo.Version)
                                                                   {
                                                                       $InstalledModuleDetails
                                                                   }
                                                                }
                                                                else
                                                                {
                                                                    if( (-not $MinimumVersion -or ($MinimumVersion -le $InstalledModuleDetails.PSGetItemInfo.Version)) -and 
                                                                        (-not $MaximumVersion -or ($MaximumVersion -ge $InstalledModuleDetails.PSGetItemInfo.Version)))
                                                                    {
                                                                        $InstalledModuleDetails
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
}

function New-SoftwareIdentityFromPackage
{
    param
    (
        [Parameter(Mandatory=$true)]
        $Package,

        [Parameter(Mandatory=$true)]
        [string]
        $PackageManagementProviderName,

        [Parameter(Mandatory=$true)]
        [string]
        $SourceLocation,

        [Parameter()]
        [switch]
        $IsFromTrustedSource,

        [Parameter(Mandatory=$true)]
        $request,

        [Parameter(Mandatory=$true)]
        [string]
        $Type,

        [Parameter()]
        [string]
        $InstalledLocation
    )

    $fastPackageReference = New-FastPackageReference -ProviderName $PackageManagementProviderName `
                                                     -PackageName $Package.Name `
                                                     -Version $Package.Version `
                                                     -Source $SourceLocation `
                                                     -ArtifactType $Type

    $links = New-Object -TypeName  System.Collections.ArrayList
    foreach($lnk in $Package.Links)
    {
        if( $lnk.Relationship -eq "icon" -or $lnk.Relationship -eq "license" -or $lnk.Relationship -eq "project" )
        {
            $links.Add( (New-Link -Href $lnk.HRef -RelationShip $lnk.Relationship )  )
        }
    }

    $entities = New-Object -TypeName  System.Collections.ArrayList
    foreach( $entity in $Package.Entities )
    {
        if( $entity.Role -eq "author" -or $entity.Role -eq "owner" )
        {
            $entities.Add( (New-Entity -Name $entity.Name -Role $entity.Role -RegId $entity.RegId -Thumbprint $entity.Thumbprint)  )
        }
    }

    $deps = (new-Object -TypeName  System.Collections.ArrayList)
    foreach( $dep in $pkg.Dependencies ) 
    {
        # Add each dependency and say it's from this provider.
        $newDep = New-Dependency -ProviderName $script:PSModuleProviderName `
                                 -PackageName $request.Services.ParsePackageName($dep) `
                                 -Version $request.Services.ParsePackageVersion($dep) `
                                 -Source $SourceLocation

        $deps.Add( $newDep )
    }


    $details =  New-Object -TypeName  System.Collections.Hashtable
    $details.Add( "description" , (Get-First $Package.Metadata["description"]) )
    $details.Add( "copyright" , (Get-First $Package.Metadata["copyright"]) )
    $details.Add( "published" , (Get-First $Package.Metadata["published"]) )
    $details.Add( "tags" , (Get-First $Package.Metadata["tags"]) )
    $details.Add( "releaseNotes" , (Get-First $Package.Metadata["releaseNotes"]) )
    $details.Add( "PackageManagementProvider" , $PackageManagementProviderName )
    $details.Add( "Type" , $Type )

    if($InstalledLocation)
    {
        $details.Add( $script:InstalledLocation , $InstalledLocation )
    }

    $sourceName = (Get-SourceName -Location $SourceLocation)
    
    if($sourceName)
    {
        $details.Add( "SourceName" , $sourceName )
    }

    $params = @{FastPackageReference = $fastPackageReference;
                Name = $Package.Name;
                Version = $Package.Version;
                versionScheme  = "MultiPartNumeric";
                Source = $SourceLocation;
                Summary = $Package.Summary;
                SearchKey = $Package.Name;
                FullPath = $Package.FullPath;
                FileName = $Package.Name;
                Details = $details;
                Entities = $entities;
                Links = $links;
                Dependencies = $deps;
               }

    if($IsFromTrustedSource)
    {
        $params["FromTrustedSource"] = $true
    }

    $sid = New-SoftwareIdentity @params

    return $sid
}

function New-PackageSourceFromModuleSource
{
    param
    (
        [Parameter(Mandatory=$true)]
        $ModuleSource
    )

    $ScriptSourceLocation = $null
    if(Get-Member -InputObject $ModuleSource -Name $script:ScriptSourceLocation)
    {
        $ScriptSourceLocation = $ModuleSource.ScriptSourceLocation
    }

    $ScriptPublishLocation = $ModuleSource.PublishLocation
    if(Get-Member -InputObject $ModuleSource -Name $script:ScriptPublishLocation)
    {
        $ScriptPublishLocation = $ModuleSource.ScriptPublishLocation
    }

    $packageSourceDetails = @{}
    $packageSourceDetails["InstallationPolicy"] = $ModuleSource.InstallationPolicy
    $packageSourceDetails["PackageManagementProvider"] = (Get-ProviderName -PSCustomObject $ModuleSource)
    $packageSourceDetails[$script:PublishLocation] = $ModuleSource.PublishLocation
    $packageSourceDetails[$script:ScriptSourceLocation] = $ScriptSourceLocation
    $packageSourceDetails[$script:ScriptPublishLocation] = $ScriptPublishLocation

    $ModuleSource.ProviderOptions.GetEnumerator() | Microsoft.PowerShell.Core\ForEach-Object {
                                                        $packageSourceDetails[$_.Key] = $_.Value
                                                    }

    # create a new package source
    $src =  New-PackageSource -Name $ModuleSource.Name `
                              -Location $ModuleSource.SourceLocation `
                              -Trusted $ModuleSource.Trusted `
                              -Registered $ModuleSource.Registered `
                              -Details $packageSourceDetails

    Write-Verbose ( $LocalizedData.RepositoryDetails -f ($src.Name, $src.Location, $src.IsTrusted, $src.IsRegistered) )

    # return the package source object.
    Write-Output -InputObject $src
}

function New-ModuleSourceFromPackageSource
{
    param
    (
        [Parameter(Mandatory=$true)]
        $PackageSource
    )

    $moduleSource = Microsoft.PowerShell.Utility\New-Object PSCustomObject -Property ([ordered]@{
            Name = $PackageSource.Name
            SourceLocation =  $PackageSource.Location
            Trusted=$PackageSource.IsTrusted
            Registered=$PackageSource.IsRegistered
            InstallationPolicy = $PackageSource.Details['InstallationPolicy']
            PackageManagementProvider=$PackageSource.Details['PackageManagementProvider']
            PublishLocation=$PackageSource.Details[$script:PublishLocation]
            ScriptSourceLocation=$PackageSource.Details[$script:ScriptSourceLocation]
            ScriptPublishLocation=$PackageSource.Details[$script:ScriptPublishLocation]
            ProviderOptions = @{}
        })

    $PackageSource.Details.GetEnumerator() | Microsoft.PowerShell.Core\ForEach-Object {
                                                if($_.Key -ne 'PackageManagementProvider' -and 
                                                   $_.Key -ne $script:PublishLocation -and
                                                   $_.Key -ne $script:ScriptPublishLocation -and
                                                   $_.Key -ne $script:ScriptSourceLocation -and
                                                   $_.Key -ne 'InstallationPolicy')
                                                {
                                                    $moduleSource.ProviderOptions[$_.Key] = $_.Value
                                                }
                                             }

    $moduleSource.PSTypeNames.Insert(0, "Microsoft.PowerShell.Commands.PSRepository")

    # return the module source object.
    Write-Output -InputObject $moduleSource
}

function New-FastPackageReference
{
    param
    (
        [Parameter(Mandatory=$true)]
        [string]
        $ProviderName,
		
        [Parameter(Mandatory=$true)]
        [string]
        $PackageName,

        [Parameter(Mandatory=$true)]
        [string]
        $Version,

        [Parameter(Mandatory=$true)]
        [string]
        $Source,

        [Parameter(Mandatory=$true)]
        [string]
        $ArtifactType
    )

    return "$ProviderName|$PackageName|$Version|$Source|$ArtifactType"
}

function Get-First 
{
    param
    (
        [Parameter(Mandatory=$true)]
        $IEnumerator
    ) 

    foreach($item in $IEnumerator)
    {
        return $item
    }

    return $null
}

function Set-InstalledModulesVariable
{
    # Initialize list of modules installed by the PSModule provider
    $script:PSGetInstalledModules = [ordered]@{}

    $modulePaths = @($script:ProgramFilesModulesPath, $script:MyDocumentsModulesPath)
    
    foreach ($location in $modulePaths)
    {
        # find all modules installed using PowerShellGet
        $moduleBases = Get-ChildItem $location -Recurse `
                                    -Attributes Hidden -Filter $script:PSGetItemInfoFileName `
                                    -ErrorAction SilentlyContinue `
                                    -WarningAction SilentlyContinue `
                                    | Foreach-Object { $_.Directory }        

        
        foreach ($moduleBase in $moduleBases)
        {
            $PSGetItemInfoPath = Microsoft.PowerShell.Management\Join-Path $moduleBase.FullName $script:PSGetItemInfoFileName

            # Check if this module got installed using PSGet, read its contents to create a SoftwareIdentity object
            if (Microsoft.PowerShell.Management\Test-Path $PSGetItemInfoPath)
            {
                $psgetItemInfo = DeSerialize-PSObject -Path $PSGetItemInfoPath

                # Add InstalledLocation if this module was installed with older version of PowerShellGet
                if(-not (Get-Member -InputObject $psgetItemInfo -Name $script:InstalledLocation))
                {
                    Microsoft.PowerShell.Utility\Add-Member -InputObject $psgetItemInfo `
                                                            -MemberType NoteProperty `
                                                            -Name $script:InstalledLocation `
                                                            -Value $moduleBase.FullName
                }

                $package = New-SoftwareIdentityFromPSGetItemInfo -PSGetItemInfo $psgetItemInfo

                if($package)
                {
                    $script:PSGetInstalledModules["$($psgetItemInfo.Name)$($psgetItemInfo.Version)"] = @{
                                                                                                            SoftwareIdentity = $package
                                                                                                            PSGetItemInfo = $psgetItemInfo
                                                                                                        }
                }
            }
        }
    }
}

function New-SoftwareIdentityFromPSGetItemInfo
{
    param
    (
        [Parameter(Mandatory=$true)]
        $PSGetItemInfo
    )

    $SourceLocation = $psgetItemInfo.RepositorySourceLocation

    if(Get-Member -InputObject $PSGetItemInfo -Name $script:PSArtifactType)
    {
        $artifactType = $psgetItemInfo.Type
    }
    else
    {
        $artifactType = $script:PSArtifactTypeModule
    }

    $fastPackageReference = New-FastPackageReference -ProviderName (Get-ProviderName -PSCustomObject $psgetItemInfo) `
                                                     -PackageName $psgetItemInfo.Name `
                                                     -Version $psgetItemInfo.Version `
                                                     -Source $SourceLocation `
                                                     -ArtifactType $artifactType

    $links = New-Object -TypeName  System.Collections.ArrayList
    if($psgetItemInfo.IconUri)
    {
        $links.Add( (New-Link -Href $psgetItemInfo.IconUri -RelationShip "icon") )
    }
    
    if($psgetItemInfo.LicenseUri)
    {
        $links.Add( (New-Link -Href $psgetItemInfo.LicenseUri -RelationShip "license") )
    }

    if($psgetItemInfo.ProjectUri)
    {
        $links.Add( (New-Link -Href $psgetItemInfo.ProjectUri -RelationShip "project") )
    }
    
    $entities = New-Object -TypeName  System.Collections.ArrayList
    if($psgetItemInfo.Author)
    {
        $entities.Add( (New-Entity -Name $psgetItemInfo.Author -Role 'author') )
    }

    if($psgetItemInfo.CompanyName -and $psgetItemInfo.CompanyName.ToString())
    {
        $entities.Add( (New-Entity -Name $psgetItemInfo.CompanyName -Role 'owner') )
    }

    $details =  @{
                    description    = $psgetItemInfo.Description
                    copyright      = $psgetItemInfo.Copyright
                    published      = $psgetItemInfo.PublishedDate.ToString()
                    tags           = $psgetItemInfo.Tags
                    releaseNotes   = $psgetItemInfo.ReleaseNotes
                    PackageManagementProvider = (Get-ProviderName -PSCustomObject $psgetItemInfo)
                 }

    if(Get-Member -InputObject $psgetItemInfo -Name $script:InstalledLocation)
    {
        $details[$script:InstalledLocation] = $psgetItemInfo.InstalledLocation
    }

    $details[$script:PSArtifactType] = $artifactType

    $sourceName = Get-SourceName -Location $SourceLocation
    if($sourceName)
    {
        $details["SourceName"] = $sourceName
    }

    $params = @{
                FastPackageReference = $fastPackageReference;
                Name = $psgetItemInfo.Name;
                Version = $psgetItemInfo.Version;
                versionScheme  = "MultiPartNumeric";
                Source = $SourceLocation;
                Summary = $psgetItemInfo.Description;
                Details = $details;
                Entities = $entities;
                Links = $links
               }

    if($sourceName -and $script:PSGetModuleSources[$sourceName].Trusted)
    {
        $params["FromTrustedSource"] = $true
    }

    $sid = New-SoftwareIdentity @params

    return $sid
}

#endregion

#region Common functions

function DeSerialize-PSObject
{
    [CmdletBinding(PositionalBinding=$false)]    
    Param
    (
        [Parameter(Mandatory=$true)]        
        $Path
    )
    $filecontent = Microsoft.PowerShell.Management\Get-Content -Path $Path
    [System.Management.Automation.PSSerializer]::Deserialize($filecontent)    
}

function Log-ArtifactNotFoundInPSGallery
{
    [CmdletBinding()]
    Param
    (     
        [Parameter()]
        [string[]]
        $SearchedName,
                   
        [Parameter()]
        [string[]]
        $FoundName,

        [Parameter(Mandatory=$true)]
        [string]
        $operationName
    )

    if (-not $script:TelemetryEnabled)
    {            
        return
    }

    if(-not $SearchedName)
    {
        return
    }

    $SearchedNameNoWildCards = @()

    # Ignore wild cards  
    foreach ($artifactName in $SearchedName)
    {
        if (-not (Test-WildcardPattern $artifactName))
        {
            $SearchedNameNoWildCards += $artifactName
        }
    }

    # Find artifacts searched, but not found in the specified gallery
    $notFoundArtifacts = @()
    foreach ($element in $SearchedNameNoWildCards)
    {
        if (-not ($FoundName -contains $element))
        {
            $notFoundArtifacts += $element
        }
    }

    # Perform Telemetry only if searched artifacts are not available in specified Gallery
    if ($notFoundArtifacts)
    {
        [Microsoft.PowerShell.Get.Telemetry]::TraceMessageArtifactsNotFound($notFoundArtifacts, $operationName)
    }   
}

function Get-ValidModuleLocation
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $LocationString,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $ParameterName
    )

    # Get the actual Uri from the Location
    if(-not (Microsoft.PowerShell.Management\Test-Path $LocationString))
    {
        # Append '/api/v2/' to the $LocationString, return if that URI works.
        if(($LocationString -notmatch 'LinkID') -and 
           -not ($LocationString.EndsWith('/api/v2', [System.StringComparison]::OrdinalIgnoreCase)) -and
           -not ($LocationString.EndsWith('/api/v2/', [System.StringComparison]::OrdinalIgnoreCase))
            )
        {
            $tempLocation = $null

            if($LocationString.EndsWith('/', [System.StringComparison]::OrdinalIgnoreCase))
            {
                $tempLocation = $LocationString + 'api/v2/'
            }
            else
            {
                $tempLocation = $LocationString + '/api/v2/'
            }

            if($tempLocation)
            {
                # Ping and resolve the specified location
                $tempLocation = Resolve-Location -Location $tempLocation `
                                                 -LocationParameterName $ParameterName `
                                                 -ErrorAction SilentlyContinue `
                                                 -WarningAction SilentlyContinue                
                if($tempLocation)
                {
                   return $tempLocation
                }
                # No error if we can't resolve the URL appended with '/api/v2/'
            }
        }

        # Ping and resolve the specified location
        $LocationString = Resolve-Location -Location $LocationString `
                                           -LocationParameterName $ParameterName `
                                           -CallerPSCmdlet $PSCmdlet   
    }

    return $LocationString
}

function Save-ModuleSources
{
    if($script:PSGetModuleSources)
    {
        if(-not (Microsoft.PowerShell.Management\Test-Path $script:PSGetAppLocalPath))
        {
            $null = Microsoft.PowerShell.Management\New-Item -Path $script:PSGetAppLocalPath `
                                                             -ItemType Directory -Force `
                                                             -ErrorAction SilentlyContinue `
                                                             -WarningAction SilentlyContinue `
                                                             -Confirm:$false -WhatIf:$false
        }        
        Microsoft.PowerShell.Utility\Out-File -FilePath $script:PSGetModuleSourcesFilePath -Force -InputObject ([System.Management.Automation.PSSerializer]::Serialize($script:PSGetModuleSources))
   }   
}

function Test-ModuleSxSVersionSupport
{
    # Side-by-Side module version is avialable on PowerShell 5.0 or later versions only
    # By default, PowerShell module versions will be installed/updated Side-by-Side.
    $PSVersionTable.PSVersion -ge [Version]"5.0"
}

function Test-ModuleInstalled
{
    [CmdletBinding(PositionalBinding=$false)]
    [OutputType("PSModuleInfo")]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Name,

        [Parameter()]
        [Version]
        $RequiredVersion
    )

    # Check if module is already installed
    $availableModule = Microsoft.PowerShell.Core\Get-Module -ListAvailable -Name $Name -Verbose:$false | 
                           Microsoft.PowerShell.Core\Where-Object {-not (Test-ModuleSxSVersionSupport) -or -not $RequiredVersion -or ($RequiredVersion -eq $_.Version)} | 
                               Microsoft.PowerShell.Utility\Select-Object -Unique

    return $availableModule
}

function Test-ScriptInstalled
{
    [CmdletBinding(PositionalBinding=$false)]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Name,

        [Parameter()]
        [Version]
        $RequiredVersion
    )

    $scriptInfo = $null
    $scriptFileName = "$Name.ps1"
    $scriptPaths = @($script:ProgramFilesScriptsPath, $script:MyDocumentsScriptsPath)    
    $scriptInfos = @()

    foreach ($location in $scriptPaths)
    {
        $scriptFilePath = Microsoft.PowerShell.Management\Join-Path -Path $location -ChildPath $scriptFileName

        if(Microsoft.PowerShell.Management\Test-Path -Path $scriptFilePath -PathType Leaf)
        {
            $sinfo = Test-ScriptFile -Path $scriptFilePath

            if($sinfo)
            {
                $scriptInfos += $sinfo
            }
        }
    }

    $scriptInfo = $scriptInfos | Microsoft.PowerShell.Core\Where-Object {
                                                                (-not $RequiredVersion) -or ($RequiredVersion -eq $_.Version)
                                                            } | Microsoft.PowerShell.Utility\Select-Object -First 1

    return $scriptInfo
}

function Get-AvailableScriptFilePath
{
    [CmdletBinding(PositionalBinding=$false)]
    Param
    (
        [Parameter()]
        [string]
        $Name
    )

    $scriptInfo = $null
    $scriptFileName = '*.ps1'
    $scriptBasePaths = @($script:ProgramFilesScriptsPath, $script:MyDocumentsScriptsPath)    
    $scriptFilePaths = @()
    $wildcardPattern = $null

    if($Name)
    {
        if(Test-WildcardPattern -Name $Name)
        {
            $wildcardPattern = New-Object System.Management.Automation.WildcardPattern $Name,$script:wildcardOptions
        }
        else
        {
            $scriptFileName = "$Name.ps1"
        }

    }

    foreach ($location in $scriptBasePaths)
    {
        $scriptFiles = Get-ChildItem -Path $location `
                                     -Filter $scriptFileName `
                                     -ErrorAction SilentlyContinue `
                                     -WarningAction SilentlyContinue
        
        if($wildcardPattern)
        {
            $scriptFiles | Microsoft.PowerShell.Core\ForEach-Object {
                                if($wildcardPattern.IsMatch($_.BaseName))
                                {
                                    $scriptFilePaths += $_.FullName
                                }
                           }
        }
        else
        {
            $scriptFiles | Microsoft.PowerShell.Core\ForEach-Object { $scriptFilePaths += $_.FullName }
        }
    }

    return $scriptFilePaths
}

function Get-InstalledScriptFilePath
{
    [CmdletBinding(PositionalBinding=$false)]
    Param
    (
        [Parameter()]
        [string]
        $Name
    )

    $installedScriptFilePaths = @()
    $scriptFilePaths = Get-AvailableScriptFilePath @PSBoundParameters

    foreach ($scriptFilePath in $scriptFilePaths)
    {
        $scriptInfo = Test-ScriptInstalled -Name ([System.IO.Path]::GetFileNameWithoutExtension($scriptFilePath))

        if($scriptInfo)
        {
            $installedScriptInfoFilePath = $null
            $installedScriptInfoFileName = "$($scriptInfo.Name)_$script:InstalledScriptInfoFileName"

            if($scriptInfo.Path.StartsWith($script:ProgramFilesScriptsPath, [System.StringComparison]::OrdinalIgnoreCase))
            {
                $installedScriptInfoFilePath = Microsoft.PowerShell.Management\Join-Path -Path $script:PSGetProgramDataScriptsPath `
                                                                                         -ChildPath $installedScriptInfoFileName
            }
            elseif($scriptInfo.Path.StartsWith($script:MyDocumentsScriptsPath, [System.StringComparison]::OrdinalIgnoreCase))
            {
                $installedScriptInfoFilePath = Microsoft.PowerShell.Management\Join-Path -Path $script:PSGetAppLocalScriptsPath `
                                                                                         -ChildPath $installedScriptInfoFileName
            }

            if($installedScriptInfoFilePath -and (Microsoft.PowerShell.Management\Test-Path -Path $installedScriptInfoFilePath -PathType Leaf))
            {
                $installedScriptFilePaths += $scriptInfo.Path
            }
        }
    }

    return $installedScriptFilePaths
}


function Update-ModuleManifest
{
<#
.ExternalHelp PSModule.psm1-help.xml
#>
[CmdletBinding(SupportsShouldProcess=$true,
                   PositionalBinding=$false,
                   HelpUri='http://go.microsoft.com/fwlink/?LinkId=619311')]
    Param
    (
        [Parameter(Mandatory=$true,
                   Position=0,                    
                   ValueFromPipelineByPropertyName=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Path,

        [ValidateNotNullOrEmpty()]
        [Object[]]
        $NestedModules,

        [ValidateNotNullOrEmpty()]
        [Guid]
        $Guid,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $Author,

        [Parameter()] 
        [ValidateNotNullOrEmpty()]
        [String]
        $CompanyName,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $Copyright,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $RootModule,
        
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Version]
        $ModuleVersion,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $Description,
        
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [System.Reflection.ProcessorArchitecture]
        $ProcessorArchitecture,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Version]
        $PowerShellVersion,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Version]
        $ClrVersion,
        
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Version]
        $DotNetFrameworkVersion,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [String]
        $PowerShellHostName,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Version]
        $PowerShellHostVersion,
        
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Object[]]
        $RequiredModules,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $TypesToProcess,
        
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $FormatsToProcess,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $ScriptsToProcess,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $RequiredAssemblies,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $FileList,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [object[]]
        $ModuleList,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $FunctionsToExport,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $AliasesToExport,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $VariablesToExport,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $CmdletsToExport,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $DscResourcesToExport,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [System.Collections.Hashtable]
        $PrivateData,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Tags,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $ProjectUri,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $LicenseUri,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $IconUri,

        [Parameter()]
        [string[]]
        $ReleaseNotes,
                
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $HelpInfoUri,

        [Parameter()]
        [switch]
        $PassThru,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [String]
        $DefaultCommandPrefix,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [String[]]
        $ExternalModuleDependencies,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [String[]]
        $PackageManagementProviders
    )

    Import-LocalizedData -BindingVariable ModuleManifestHashTable `
                             -FileName (Microsoft.PowerShell.Management\Split-Path $Path -Leaf) `
                             -BaseDirectory (Microsoft.PowerShell.Management\Split-Path $Path -Parent) `
                             -ErrorAction SilentlyContinue `
                             -WarningAction SilentlyContinue

    if(-not (Microsoft.PowerShell.Management\Test-Path $Path))
    {
        $message = $LocalizedData.UpdateModuleManifestPathCannotFound -f ($Path)
        ThrowError -ExceptionName "System.ArgumentException" `
                   -ExceptionMessage $message `
                   -ErrorId "InvalidModuleManifestFilePath" `
                   -ExceptionObject $Path `
                   -CallerPSCmdlet $PSCmdlet `
                   -ErrorCategory InvalidArgument
      }
         

    #Get the original module manifest and migrate all the fields to the new module manifest, including the specified parameter values
    try
    {
        $moduleInfo = Microsoft.PowerShell.Core\Test-ModuleManifest -Path $Path
    }
    catch
    {
        $message = $LocalizedData.TestModuleManifestFail -f ($_.Exception.Message)
        ThrowError -ExceptionName "System.ArgumentException" `
                   -ExceptionMessage $message `
                   -ErrorId "InvalidModuleManifestFile" `
                   -ExceptionObject $Path `
                   -CallerPSCmdlet $PSCmdlet `
                   -ErrorCategory InvalidArgument
        return
    }
    
    #Params to pass to New-ModuleManifest module                                                                    
    $params = @{} 

    #NestedModules is read-only property
    if($NestedModules)
    {
        $params.Add("NestedModules",$NestedModules)
    }
    else
    {
        #Get the original module info from ManifestHashTab
        if($ModuleManifestHashTable -and $ModuleManifestHashTable.ContainsKey("NestedModules"))
        {
            $params.Add("NestedModules",$ModuleManifestHashtable.NestedModules)
        }
    }

    #Guid is read-only property
    if($Guid)
    {
        $params.Add("Guid",$Guid)
    }
    else
    {
        $params.Add("Guid",$moduleInfo.Guid)
    }

    if($Author)
    {
        $params.Add("Author",$Author)
    }
    else
    {
        $params.Add("Author",$moduleInfo.Author)
    }
    
    if($CompanyName)
    {
        $params.Add("CompanyName",$CompanyName)
    }
    else
    {
        $params.Add("CompanyName",$moduleInfo.CompanyName)
    }

    if($Copyright)
    {
        $params.Add("CopyRight",$Copyright)
    }
    else
    {
        $params.Add("Copyright",$moduleInfo.Copyright)
    }

    if($RootModule)
    {
        $params.Add("RootModule",$RootModule)
    }
    else
    {
        $params.Add("RootModule",$moduleInfo.RootModule)
    }

    if($ModuleVersion)
    {
        $params.Add("ModuleVersion",$ModuleVersion)
    }
    else
    {
        $params.Add("ModuleVersion",$moduleInfo.Version)
    }
    
    if($Description)
    {
        $params.Add("Description",$Description)
    }
    else
    {
        $params.Add("Description",$moduleInfo.Description)
    }

    if($ProcessorArchitecture)
    {
        $params.Add("ProcessorArchitecture",$ProcessorArchitecture)
    }
    else
    {
        $params.Add("ProcessorArchitecture",$moduleInfo.ProcessorArchitecture)
    }

    if($PowerShellVersion)
    {
        $params.Add("PowerShellVersion",$PowerShellVersion)
    }
    else
    {
        $params.Add("PowerShellVersion",$moduleinfo.PowerShellVersion)
    }

    if($ClrVersion)
    {
        $params.Add("ClrVersion",$ClrVersion)
    }
    else
    {
        $params.Add("ClrVersion",$moduleInfo.ClrVersion)
    }

    if($DotNetFrameworkVersion)
    {
        $params.Add("DotNetFrameworkVersion",$DotNetFrameworkVersion)
    }
    else
    {
        $params.Add("DotNetFrameworkVersion",$moduleInfo.DotNetFrameworkVersion)
    }

    if($PowerShellHostName)
    {
        $params.Add("PowerShellHostName",$PowerShellHostName)
    }
    else
    {
        $params.Add("PowerShellHostName",$moduleInfo.PowerShellHostName)
    }

    if($PowerShellHostVersion)
    {
        $params.Add("PowerShellHostVersion",$PowerShellHostVersion)
    }
    else
    {
        $params.Add("PowerShellHostVersion",$moduleInfo.PowerShellHostVersion)
    }

    if($RequiredModules)
    {
        $params.Add("RequiredModules",$RequiredModules)
    }
    else
    {
        if($ModuleManifestHashTable -and $ModuleManifestHashTable.ContainsKey("RequiredModules"))
        {
            $params.Add("RequiredModules",$ModuleManifestHashtable.RequiredModules)
        }
    }

    if($TypesToProcess)
    {
        $params.Add("TypesToProcess",$TypesToProcess)
    }
    else
    {
        $params.Add("TypesToProcess",$moduleInfo.ExportedTypeFiles)
    }

    if($FormatsToProcess)
    {
        $params.Add("FormatsToProcess",$FormatsToProcess)
    }
    else
    {
        $params.Add("FormatsToProcess",$moduleInfo.ExportedFormatFiles)
    }

    if($ScriptsToProcess)
    {
        $params.Add("ScriptsToProcess",$ScriptstoProcess)
    }
    else
    {
        $params.Add("ScriptsToProcess",$moduleInfo.Scripts)
    }

    if($RequiredAssemblies)
    {
        $params.Add("RequiredAssemblies",$RequiredAssemblies)
    }
    else
    {
        $params.Add("RequiredAssemblies",$moduleInfo.RequiredAssemblies)
    }

    if($FileList)
    {
        $params.Add("FileList",$FileList)
    }
    else
    {
        $params.Add("FileList",$moduleInfo.FileList)
    }

    if($ModuleList)
    {
        $params.Add("ModuleList",$ModuleList)
    }
    else
    {
        if($ModuleManifestHashTable -and $ModuleManifestHashTable.ContainsKey("ModuleList"))
        {
            $params.Add("ModuleList",$ModuleManifestHashtable.ModuleList)
        }
    }

    if($FunctionsToExport)
    {
        $params.Add("FunctionsToExport",$FunctionsToExport)
    }
    else
    {
        #Since $moduleInfo.ExportedFunctions is a hashtable, we need to take the name of the 
        #functions and make them into a list
        if($moduleInfo.ExportedFunctions)
        {
            $params.Add("FunctionsToExport",$moduleInfo.ExportedFunctions.Keys)
        }
    }
    

    if($AliasesToExport)
    {
        $params.Add("AliasesToExport",$AliasesToExport)
    }
    else
    {
        if($moduleInfo.ExportedAliases)
        {
            $params.Add("AliasesToExport",$moduleInfo.ExportedAliases.Keys)
        }
    }
    if($VariablesToExport)
    {
        $params.Add("VariablesToExport",$VariablesToExport)
    }
    else
    {
        if($moduleInfo.ExportedVariables)
        { 
            $params.Add("VariablesToExport",$moduleInfo.ExportedVariables.Keys)
        }
    }
    if($CmdletsToExport)
    {
        $params.Add("CmdletsToExport", $CmdletsToExport)
    }
    else
    {
        if($moduleInfo.ExportedCmdlets)
        {
            $params.Add("CmdletsToExport",$moduleInfo.ExportedCmdlets.Keys)
        }
    }

    if($DscResourcesToExport)
    {
        #DscResourcesToExport field is not available in PowerShell version lower than 5.0
        
        if  (($PSVersionTable.PSVersion -lt [Version]"5.0") -or ($PowerShellVersion -and $PowerShellVersion -lt [Version]"5.0") `
             -or (-not $PowerShellVersion -and $moduleInfo.PowerShellVersion -and $moduleInfo.PowerShellVersion -lt [Version]"5.0") `
             -or (-not $PowerShellVersion -and -not $moduleInfo.PowerShellVersion))
        {
                ThrowError -ExceptionName "System.ArgumentException" `
                   -ExceptionMessage $LocalizedData.ExportedDscResourcesNotSupportedOnLowerPowerShellVersion `
                   -ErrorId "ExportedDscResourcesNotSupported" `
                   -ExceptionObject $DscResourcesToExport `
                   -CallerPSCmdlet $PSCmdlet `
                   -ErrorCategory InvalidArgument
                return  
        }

        $params.Add("DscResourcesToExport",$DscResourcesToExport)
    }
    else
    {
        if($moduleInfo.ExportedDscResources)
        {
            $params.Add("DscResourcesToExport",$moduleInfo.ExportedDscResources)
        }
    }

    if($HelpInfoUri)
    {
        $params.Add("HelpInfoUri",$HelpInfoUri)
    }
    else
    {
        $params.Add("HelpInfoUri",$moduleInfo.HelpInfoUri)
    }

    if($DefaultCommandPrefix)
    {
        $params.Add("DefaultCommandPrefix",$DefaultCommandPrefix)
    }

    #Create a temp file path and generate a new temporary manifest with the input
    $DestinationPath = Microsoft.PowerShell.Management\Join-Path -Path $script:TempPath -ChildPath "$(Microsoft.PowerShell.Utility\Get-Random)"
    $null = Microsoft.PowerShell.Management\New-Item -Path $DestinationPath -ItemType Directory -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
    $tempPath = Microsoft.PowerShell.Management\Join-Path -Path $DestinationPath -ChildPath "NewManifest.psd1"
    $params.Add("Path",$tempPath)
    
    #Terminates if there is error creating new module manifest
    try{
        Microsoft.PowerShell.Core\New-ModuleManifest @params 
    }
    catch
    {
        $ErrorMessage = $LocalizedData.UpdatedModuleManifestNotValid -f ($Path, $_.Exception.Message)
        ThrowError -ExceptionName "System.ArgumentException" `
                   -ExceptionMessage $ErrorMessage `
                   -ErrorId "NewModuleManifestFailure" `
                   -ExceptionObject $params `
                   -CallerPSCmdlet $PSCmdlet `
                   -ErrorCategory InvalidArgument
        return
    }

    #Manually update the section in PrivateData since New-ModuleManifest works differently on different PS version
    $PrivateDataInput = ""
    $ExistingData = $moduleInfo.PrivateData
    $Data = @{}
    if($ExistingData)
    {
        foreach($key in $ExistingData.Keys)
        {
            if($key -ne "PSData"){
                $Data.Add($key,$ExistingData[$key])
            }
            else
            {
                $PSData = $ExistingData["PSData"]
                foreach($entry in $PSData.Keys)
                {
                    $Data.Add($entry,$PSData[$Entry])
                }
            }
        }
    }

    if($PrivateData)
    {
        foreach($key in $PrivateData.Keys)
        {
            #if user provides PSData within PrivateData, we will parse through the PSData
            if($key -ne "PSData")
            {
                $Data[$key] = $PrivateData[$Key]
            }

            else
            {
                $PSData = $ExistingData["PSData"]
                foreach($entry in $PSData.Keys)
                {
                    $Data[$entry] = $PSData[$entry]
                }
            }
        }
    }

    #Tags is a read-only property
    if($Tags)
    {
       $Data["Tags"] = $Tags 
    }

    #The following Uris and ReleaseNotes cannot be empty
    if($ProjectUri)
    {
        $Data["ProjectUri"] = $ProjectUri
    }

    if($LicenseUri)
    {
        $Data["LicenseUri"] = $LicenseUri
    }
    if($IconUri)
    {
        $Data["IconUri"] = $IconUri
    }

    if($ReleaseNotes)
    {
        #If value is provided as an array, we append the string.
        $Data["ReleaseNotes"] = $($ReleaseNotes -join "`n")
    }
        
    if($ExternalModuleDependencies)
    {
        #ExternalModuleDependencies have to be specified either under $RequiredModules or $NestedModules
        #Extract all the module names specified in the moduleInfo of NestedModules and RequiredModules
        $DependentModuleNames = @()
        foreach($moduleInfo in $params["NestedModules"])
        {
            if($moduleInfo.GetType() -eq [System.Collections.Hashtable])
            {
                $DependentModuleNames += $moduleInfo.ModuleName
            }
        }

        foreach($moduleInfo in $params["RequiredModules"])
        {
            if($moduleInfo.GetType() -eq [System.Collections.Hashtable])
            {
                $DependentModuleNames += $moduleInfo.ModuleName
            }
        }

        foreach($dependency in $ExternalModuleDependencies)
        {
            if($params["NestedModules"] -notcontains $dependency -and $params["RequiredModules"] -notContains $dependency `
            -and $DependentModuleNames -notcontains $dependency)
            {
                
                $message = $LocalizedData.ExternalModuleDependenciesNotSpecifiedInRequiredOrNestedModules -f ($dependency)
                ThrowError -ExceptionName "System.ArgumentException" `
                    -ExceptionMessage $message `
                    -ErrorId "InvalidExternalModuleDependencies" `
                    -ExceptionObject $Exception `
                    -CallerPSCmdlet $PSCmdlet `
                    -ErrorCategory InvalidArgument
                    return  
                }
        }
        if($Data.ContainsKey("ExternalModuleDependencies"))
        {
            $Data["ExternalModuleDependencies"] = $ExternalModuleDependencies
        }
        else
        {
            $Data.Add("ExternalModuleDependencies", $ExternalModuleDependencies)
        }
    }
    if($PackageManagementProviders)
    {
        #Check if the provided value is within the relative path
        $ModuleBase = Microsoft.PowerShell.Management\Split-Path $Path -Parent
        $Files = Microsoft.PowerShell.Management\Get-ChildItem -Path $ModuleBase
        foreach($provider in $PackageManagementProviders)
        {
            if ($Files.Name -notcontains $provider)
            {
                $message = $LocalizedData.PackageManagementProvidersNotInModuleBaseFolder -f ($provider,$ModuleBase)
                ThrowError -ExceptionName "System.ArgumentException" `
                    -ExceptionMessage $message `
                    -ErrorId "InvalidPackageManagementProviders" `
                    -ExceptionObject $PackageManagementProviders `
                    -CallerPSCmdlet $PSCmdlet `
                    -ErrorCategory InvalidArgument
                return  
            }
        }

        $Data["PackageManagementProviders"] = $PackageManagementProviders
    }
       
    $PrivateDataInput = Get-PrivateData -PrivateData $Data
        
    #Repleace the PrivateData section by first locating the linenumbers of start line and endline.  
    $PrivateDataBegin = Select-String -Path $tempPath -Pattern "PrivateData ="
    $PrivateDataBeginLine = $PrivateDataBegin.LineNumber
    
    $newManifest = Microsoft.PowerShell.Management\Get-Content -Path $tempPath
    #Look up the endline of PrivateData section by finding the matching brackets since private data could 
    #consist of multiple pairs of brackets.
    $PrivateDataEndLine=0
    if($PrivateDataBegin -match "@{")
    {
        $leftBrace = 0
        $EndLineOfFile = $newManifest.Length-1
        
        For($i = $PrivateDataBeginLine;$i -lt $EndLineOfFile; $i++)
        {
            if($newManifest[$i] -match "{")
            {
                $leftBrace ++
            }
            elseif($newManifest[$i] -match "}")
            {
                if($leftBrace -gt 0)
                {
                    $leftBrace --
                }
                else
                {
                   $PrivateDataEndLine = $i
                   break
                }
            }
        } 
    }

    
    try
    {
        if($PrivateDataEndLine -ne 0)
        {
            #If PrivateData section has more than one line, we will remove the old content and insert the new PrivataData
            $newManifest  | where {$_.readcount -le $PrivateDataBeginLine -or $_.readcount -gt $PrivateDataEndLine+1} `
            | ForEach-Object {
                $_
                if($_ -match "PrivateData = ")
                 {
                    $PrivateDataInput
                }
              } | Set-Content -Path $tempPath
        }

        #In lower version, PrivateData is just a single line
        else
        {
            $newManifest  | where {$_.readcount -lt $PrivateDataBeginLine } `
            | ForEach-Object {
                $_
                if($_ -match "PrivateData = ")
                {
                   $PrivateDataInput
                }
            } | Set-Content -Path $tempPath
        }
 
        #Verify the new module manifest is valid
        $testModuleInfo = Microsoft.PowerShell.Core\Test-ModuleManifest -Path $tempPath `
                                                                    -Verbose:$VerbosePreference `
    }
    #Catch the exceptions from Test-ModuleManifest
    catch
    {
        $message = $LocalizedData.UpdatedModuleManifestNotValid -f ($Path, $_.Exception.Message)
       
        ThrowError -ExceptionName "System.ArgumentException" `
                   -ExceptionMessage $message `
                   -ErrorId "UpdateManifestFileFail" `
                   -ExceptionObject $_.Exception `
                   -CallerPSCmdlet $PSCmdlet `
                   -ErrorCategory InvalidArgument
        return
    }
    
    try
    {
    	$newContent = Microsoft.PowerShell.Management\Get-Content -Path $tempPath
    
    	#Ask for confirmation of the new manifest before replacing the original one
    	if($PSCmdlet.ShouldProcess($Path,$LocalizedData.UpdateManifestContentMessage+$newContent))
    	{
            Microsoft.PowerShell.Management\Set-Content -Path $Path -Value $newContent
    	}

    	#Return the new content if -PassThru is specified
    	if($PassThru)
    	{
        	return $newContent
    	}
    }
    finally
    {
        Microsoft.PowerShell.Management\Remove-Item -Path $DestinationPath -Recurse -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
    }
}

#Utility function to help form the content string for PrivateData
function Get-PrivateData
{
    param
    (
        [System.Collections.Hashtable]
        $PrivateData
    )

    if($PrivateData.Keys.Count -eq 0)
    {
        $content = "
    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        # Tags = @()

        # A URL to the license for this module.
        # LicenseUri = ''

        # A URL to the main website for this project.
        # ProjectUri = ''

        # A URL to an icon representing this module.
        # IconUri = ''

        # ReleaseNotes of this module
        # ReleaseNotes = ''

        # External dependent modules of this module
        # ExternalModuleDependencies = ''

    } # End of PSData hashtable

} # End of PrivateData hashtable"
        return $content
    }


    #Validate each of the property of PSData is of the desired data type
    $Tags= $PrivateData["Tags"] -join "','" | %{"'$_'"}
    $LicenseUri = $PrivateData["LicenseUri"] -join "','" | %{"'$_'"}
    $ProjectUri = $PrivateData["ProjectUri"] -join "','" | %{"'$_'"}
    $IconUri = $PrivateData["IconUri"] -join "','" | %{"'$_'"}
    $ReleaseNotes = $PrivateData["ReleaseNotes"] -join "','" | %{"'$_'"}
    $ExternalModuleDependencies = $PrivateData["ExternalModuleDependencies"] -join "','" | %{"'$_'"} 
    
    $DefaultProperties = @("Tags","LicenseUri","ProjectUri","IconUri","ReleaseNotes","ExternalModuleDependencies")

    $ExtraProperties = @()
    foreach($key in $PrivateData.Keys)
    {
        if($DefaultProperties -notcontains $key)
        {
            $PropertyString = "#"+"$key"+ " of this module"
            $PropertyString += "`n    "
            $PropertyString += $key +" = " + "'"+$PrivateData[$key]+"'"
            $ExtraProperties += ,$PropertyString
        }
    }

    $ExtraPropertiesString = ""
    $firstProperty = $true
    foreach($property in $ExtraProperties)
    {
        if($firstProperty)
        {
            $firstProperty = $false
        }
        else
        {
            $ExtraPropertiesString += "`n`n    "
        }
        $ExtraPropertiesString += $Property
    }

    $TagsLine ="# Tags = @()"
    if($Tags -ne "''")
    {
        $TagsLine = "Tags = "+$Tags
    }
    $LicenseUriLine = "# LicenseUri = ''"
    if($LicenseUri -ne "''")
    {
        $LicenseUriLine = "LicenseUri = "+$LicenseUri
    }
    $ProjectUriLine = "# ProjectUri = ''"
    if($ProjectUri -ne "''")
    {
        $ProjectUriLine = "ProjectUri = " +$ProjectUri
    }
    $IconUriLine = "# IconUri = ''"
    if($IconUri -ne "''")
    {
        $IconUriLine = "IconUri = " +$IconUri
    }           
    $ReleaseNotesLine = "# ReleaseNotes = ''"
    if($ReleaseNotes -ne "''")
    {
        $ReleaseNotesLine = "ReleaseNotes = "+$ReleaseNotes
    }
    $ExternalModuleDependenciesLine ="# ExternalModuleDependencies = ''"
    if($ExternalModuleDependencies -ne "''")
    {
        $ExternalModuleDependenciesLine = "ExternalModuleDependencies = "+$ExternalModuleDependencies
    }

    if(-not $ExtraPropertiesString -eq "")
    {
        $Content = "
    ExtraProperties

    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        $TagsLine

        # A URL to the license for this module.
        $LicenseUriLine

        # A URL to the main website for this project.
        $ProjectUriLine

        # A URL to an icon representing this module.
        $IconUriLine

        # ReleaseNotes of this module
        $ReleaseNotesLine

        # External dependent modules of this module
        $ExternalModuleDependenciesLine

    } # End of PSData hashtable
    
} # End of PrivateData hashtable"
        
        #Replace the Extra PrivateData in the block
        $Content -replace "ExtraProperties", $ExtraPropertiesString
    }
    else
    {
        $content = "
    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        $TagsLine

        # A URL to the license for this module.
        $LicenseUriLine

        # A URL to the main website for this project.
        $ProjectUriLine

        # A URL to an icon representing this module.
        $IconUriLine

        # ReleaseNotes of this module
        $ReleaseNotesLine

        # External dependent modules of this module
        $ExternalModuleDependenciesLine

    } # End of PSData hashtable
    
 } # End of PrivateData hashtable" 
        return $content
    }
}

function Copy-ScriptFile
{
    [CmdletBinding(PositionalBinding=$false)]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $SourcePath,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $DestinationPath,

        [Parameter(Mandatory=$true)]
        [ValidateNotNull()]
        [PSCustomObject]
        $PSGetItemInfo,

        [Parameter()]
        [string]
        $Scope
    )
    
    # Copy the script file to destination
    if(-not (Microsoft.PowerShell.Management\Test-Path -Path $DestinationPath))
    {
        $null = Microsoft.PowerShell.Management\New-Item -Path $DestinationPath `
                                                         -ItemType Directory `
                                                         -Force `
                                                         -ErrorAction SilentlyContinue `
                                                         -WarningAction SilentlyContinue `
                                                         -Confirm:$false `
                                                         -WhatIf:$false
    }

    Microsoft.PowerShell.Management\Copy-Item -Path $SourcePath -Destination $DestinationPath -Force -Confirm:$false -WhatIf:$false -Verbose

    if($Scope)
    {
        # Create <Name>_InstalledScriptInfo.xml
        $InstalledScriptInfoFileName = "$($PSGetItemInfo.Name)_$script:InstalledScriptInfoFileName"

        if($scope -eq 'AllUsers')
        {
            $scriptInfopath = Microsoft.PowerShell.Management\Join-Path -Path $script:PSGetProgramDataScriptsPath `
                                                                        -ChildPath $InstalledScriptInfoFileName
        }
        else
        {
            $scriptInfopath = Microsoft.PowerShell.Management\Join-Path -Path $script:PSGetAppLocalScriptsPath `
                                                                        -ChildPath $InstalledScriptInfoFileName
        }

        Microsoft.PowerShell.Utility\Out-File -FilePath $scriptInfopath `
                                              -Force `
                                              -InputObject ([System.Management.Automation.PSSerializer]::Serialize($PSGetItemInfo))
    }
}

function Copy-Module
{
    [CmdletBinding(PositionalBinding=$false)]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $SourcePath,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $DestinationPath,

        [Parameter(Mandatory=$true)]
        [ValidateNotNull()]
        [PSCustomObject]
        $PSGetItemInfo
    )
    
    if(Microsoft.PowerShell.Management\Test-Path $DestinationPath)
    {
        Microsoft.PowerShell.Management\Remove-Item -Path $DestinationPath -Recurse -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
    }

    # Copy the module to destination
    $null = Microsoft.PowerShell.Management\New-Item -Path $DestinationPath -ItemType Directory -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
    Microsoft.PowerShell.Management\Copy-Item -Path "$SourcePath\*" -Destination $DestinationPath -Force -Recurse -Confirm:$false -WhatIf:$false
    
    # Remove the *.nupkg file
    if(Microsoft.PowerShell.Management\Test-Path "$DestinationPath\$($PSGetItemInfo.Name).nupkg")
    {
        Microsoft.PowerShell.Management\Remove-Item -Path "$DestinationPath\$($PSGetItemInfo.Name).nupkg" -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
    }
                    
    # Create PSGetModuleInfo.xml
    $psgetItemInfopath = Microsoft.PowerShell.Management\Join-Path $DestinationPath $script:PSGetItemInfoFileName        

    Microsoft.PowerShell.Utility\Out-File -FilePath $psgetItemInfopath -Force -InputObject ([System.Management.Automation.PSSerializer]::Serialize($PSGetItemInfo))
    
    [System.IO.File]::SetAttributes($psgetItemInfopath, [System.IO.FileAttributes]::Hidden)
}

function Test-ModuleInUse
{
    [CmdletBinding()]
    [OutputType([bool])]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $ModuleBasePath,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $ModuleName,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [Version]
        $ModuleVersion
    )

    $dllsInModule = Get-ChildItem -Path $ModuleBasePath `
                                  -Filter *.dll `
                                  -Recurse `
                                  -ErrorAction SilentlyContinue `
                                  -WarningAction SilentlyContinue | Microsoft.PowerShell.Core\Foreach-Object{$_.FullName}
    if($dllsInModule)
    {
        $currentProcesses = Get-Process
        $processesDlls = $currentProcesses | Microsoft.PowerShell.Core\Foreach-Object{$_.Modules} | Sort-Object -Unique
        
        $moduleDllsInUse = $processesDlls | Where-Object {$_ -and ($dllsInModule -contains $_.FileName)}
    
        if($moduleDllsInUse)
        {
            $processes = $moduleDllsInUse | Microsoft.PowerShell.Core\Foreach-Object{$dllName = $_.ModuleName; $currentProcesses | Where-Object {$_ -and $_.Modules -and $_.Modules.ModuleName -eq $dllName} } | Select-Object -Unique
        
            if($processes)
            {
                $message = $LocalizedData.ModuleInUseWithProcessDetails -f ($ModuleVersion, $ModuleName, $( $($processes | Microsoft.PowerShell.Core\Foreach-Object{"$($_.ProcessName):$($_.Id)"} ) -join ",") )
                Write-Error -Message $message -ErrorId "ModuleIsInUse" -Category InvalidOperation

                return $true
            }
        }
    }

    return $false
}

function Test-ValidManifestModule
{
    [CmdletBinding()]
    [OutputType([bool])]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $ModuleBasePath
    )

    $moduleName = Microsoft.PowerShell.Management\Split-Path $ModuleBasePath -Leaf
    $manifestPath = Microsoft.PowerShell.Management\Join-Path $ModuleBasePath "$moduleName.psd1"
    $PSModuleInfo = $null
        
    if(Microsoft.PowerShell.Management\Test-Path $manifestPath)
    {
       $PSModuleInfo = Microsoft.PowerShell.Core\Test-ModuleManifest -Path $manifestPath -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
    }

    return $PSModuleInfo
}

function Get-ScriptSourceLocation
{
    [CmdletBinding()]
    Param
    (
        [Parameter()]
        [String]
        $Location
    )

    $scriptLocation = $null

    if($Location)
    {
        # For local dir or SMB-share locations, ScriptSourceLocation is SourceLocation.
        if(Microsoft.PowerShell.Management\Test-Path -Path $Location)
        {
            $scriptLocation = $Location
        }
        else
        {
            $tempScriptLocation = $null

            if($Location.EndsWith('/api/v2', [System.StringComparison]::OrdinalIgnoreCase))
            {
                $tempScriptLocation = $Location + '/artifacts/psscript/'
            }
            elseif($Location.EndsWith('/api/v2/', [System.StringComparison]::OrdinalIgnoreCase))
            {
                $tempScriptLocation = $Location + 'artifacts/psscript/'
            }

            if($tempScriptLocation)
            {
                # Ping and resolve the specified location
                $scriptLocation = Resolve-Location -Location $tempScriptLocation `
                                                   -LocationParameterName 'ScriptSourceLocation' `
                                                   -ErrorAction SilentlyContinue `
                                                   -WarningAction SilentlyContinue
            }
        }
    }

    return $scriptLocation
}

function Get-PublishLocation
{
    [CmdletBinding()]
    Param
    (
        [Parameter()]
        [String]
        $Location
    )

    $PublishLocation = $null

    if($Location)
    {
        # For local dir or SMB-share locations, ScriptPublishLocation is PublishLocation.
        if(Microsoft.PowerShell.Management\Test-Path -Path $Location)
        {
            $PublishLocation = $Location
        }
        else
        {
            $tempPublishLocation = $null

            if($Location.EndsWith('/api/v2', [System.StringComparison]::OrdinalIgnoreCase))
            {
                $tempPublishLocation = $Location + '/package/'
            }
            elseif($Location.EndsWith('/api/v2/', [System.StringComparison]::OrdinalIgnoreCase))
            {
                $tempPublishLocation = $Location + 'package/'
            }

            if($tempPublishLocation)
            {
                $PublishLocation = $tempPublishLocation
            }
        }
    }

    return $PublishLocation
}

function Resolve-Location
{
    [CmdletBinding()]
    [OutputType([string])]
    Param
    (
        [Parameter(Mandatory=$true)]
        [string]
        $Location,

        [Parameter(Mandatory=$true)]
        [string]
        $LocationParameterName,

        [Parameter()]
        [System.Management.Automation.PSCmdlet]
        $CallerPSCmdlet
    )

    # Ping and resolve the specified location
    if(-not (Test-WebUri -uri $Location))
    {
        if(Microsoft.PowerShell.Management\Test-Path -Path $Location)
        {
            return $Location
        }
        elseif($CallerPSCmdlet)
        {
            $message = $LocalizedData.PathNotFound -f ($Location)
            ThrowError -ExceptionName "System.ArgumentException" `
                       -ExceptionMessage $message `
                       -ErrorId "PathNotFound" `
                       -CallerPSCmdlet $CallerPSCmdlet `
                       -ErrorCategory InvalidArgument `
                       -ExceptionObject $Location
        }
    }
    else
    {
        $pingResult = Ping-Endpoint -Endpoint $Location
        $statusCode = $null
        $exception = $null
        $resolvedLocation = $null
        if($pingResult -and $pingResult.ContainsKey($Script:ResponseUri))
        {
            $resolvedLocation = $pingResult[$Script:ResponseUri]
        }

        if($pingResult -and $pingResult.ContainsKey($Script:StatusCode))
        {
            $statusCode = $pingResult[$Script:StatusCode]
        }

        Write-Debug -Message "Ping-Endpoint: location=$Location, statuscode=$statusCode, resolvedLocation=$resolvedLocation"

        if(($statusCode -eq 200) -and $resolvedLocation)
        {
            return $resolvedLocation
        }
        elseif($CallerPSCmdlet)
        {
            $message = $LocalizedData.InvalidWebUri -f ($Location, $LocationParameterName)
            ThrowError -ExceptionName "System.ArgumentException" `
                       -ExceptionMessage $message `
                       -ErrorId "InvalidWebUri" `
                       -CallerPSCmdlet $CallerPSCmdlet `
                       -ErrorCategory InvalidArgument `
                       -ExceptionObject $Location
        }
    }
}

function Test-WebUri
{
    [CmdletBinding()]
    [OutputType([bool])]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $uri
    )

    return ($uri.AbsoluteURI -ne $null) -and ($uri.Scheme -match '[http|https]')
}

function Test-WildcardPattern
{
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory=$true)]
        [ValidateNotNull()]
        $Name
    )

    return [System.Management.Automation.WildcardPattern]::ContainsWildcardCharacters($Name)    
}

# Utility to throw an errorrecord
function ThrowError
{
    param
    (        
        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.PSCmdlet]
        $CallerPSCmdlet,

        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.String]        
        $ExceptionName,

        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.String]
        $ExceptionMessage,
        
        [System.Object]
        $ExceptionObject,
        
        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.String]
        $ErrorId,

        [parameter(Mandatory = $true)]
        [ValidateNotNull()]
        [System.Management.Automation.ErrorCategory]
        $ErrorCategory
    )
        
    $exception = New-Object $ExceptionName $ExceptionMessage;
    $errorRecord = New-Object System.Management.Automation.ErrorRecord $exception, $ErrorId, $ErrorCategory, $ExceptionObject    
    $CallerPSCmdlet.ThrowTerminatingError($errorRecord)
}


#endregion

# Create install locations for scripts if they are not already created
if(-not (Microsoft.PowerShell.Management\Test-Path -Path $script:ProgramFilesScriptsPath) -and (Test-RunningAsElevated))
{
    $null = Microsoft.PowerShell.Management\New-Item -Path $script:ProgramFilesScriptsPath `
                                                     -ItemType Directory `
                                                     -Force `
                                                     -Confirm:$false `
                                                     -WhatIf:$false
}

if(-not (Microsoft.PowerShell.Management\Test-Path -Path $script:MyDocumentsScriptsPath))
{
    $null = Microsoft.PowerShell.Management\New-Item -Path $script:MyDocumentsScriptsPath `
                                                     -ItemType Directory `
                                                     -Force `
                                                     -Confirm:$false `
                                                     -WhatIf:$false
}

Set-Alias -Name fimo -Value Find-Module
Set-Alias -Name inmo -Value Install-Module
Set-Alias -Name upmo -Value Update-Module
Set-Alias -Name pumo -Value Publish-Module

Export-ModuleMember -Function Find-Module, `
                              Save-Module, `
                              Install-Module, `
                              Update-Module, `
                              Publish-Module, `
                              Uninstall-Module, `
                              Get-InstalledModule, `
                              Find-PSRepositoryItem, `
                              Install-Script, `
                              Find-Script, `
                              Save-Script, `
                              Update-Script, `
                              Publish-Script,  `
                              Get-InstalledScript, `
                              Uninstall-Script, `
                              Test-ScriptFile, `
                              New-ScriptFile, `
                              Update-ScriptFile, `
                              Get-PSRepository, `
                              Register-PSRepository, `
                              Unregister-PSRepository, `
                              Set-PSRepository, `
                              Find-Package, `
                              Get-PackageDependencies, `
                              Install-Package, `
                              Uninstall-Package, `
                              Get-InstalledPackage, `
                              Remove-PackageSource, `
                              Resolve-PackageSource, `
                              Add-PackageSource, `
                              Get-DynamicOptions, `
                              Initialize-Provider, `
                              Get-Feature, `
                              Get-PackageProviderName, `
                              Update-ModuleManifest `
                    -Alias    fimo, `
                              inmo, `
                              upmo, `
                              pumo



# SIG # Begin signature block
# MIIavwYJKoZIhvcNAQcCoIIasDCCGqwCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUTGAlDB92XQBeIc/uUM/tNmG8
# sPegghWCMIIEwzCCA6ugAwIBAgITMwAAAHPGWcJSl4OjOgAAAAAAczANBgkqhkiG
# 9w0BAQUFADB3MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4G
# A1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSEw
# HwYDVQQDExhNaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EwHhcNMTUwMzIwMTczMjA0
# WhcNMTYwNjIwMTczMjA0WjCBszELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hp
# bmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jw
# b3JhdGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNO
# OkJCRUMtMzBDQS0yREJFMSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBT
# ZXJ2aWNlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAp0QvcscV762c
# vJQkN4+yFC55LDaPb7KevwD6jHhhG5S5Uij0cT8HGE/y6Je/f3Ow4zVsoSviUbYn
# qqI1ASnzKaVQ3natkrIUuQ8Mllkya3MeSL9Q877ogSskJFB0fOph5o8RAe6yfSD1
# CkMqVGVAxRwMNFDik+TCDS7gUJlQaAZ9h3v2jQWOR+Xt0ELjY93j7iXPqVCjT4K7
# x5WFfasB4FBCFeBZg8lR4D2gKOh/gnzSuRoCHqhzdFfIf7gJs7pF4EfCdNSp2BLX
# Lxuc1K567c/CWXMh3LDjZMMd5i8EvFv9ssV+Nua6VnlcHRWrsaB9FygH8+OpkVg8
# tkWf1jVh3QIDAQABo4IBCTCCAQUwHQYDVR0OBBYEFDUsc4HZ7HD5Sj2P/0fAfApo
# obgbMB8GA1UdIwQYMBaAFCM0+NlSRnAK7UD7dvuzK7DDNbMPMFQGA1UdHwRNMEsw
# SaBHoEWGQ2h0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3Rz
# L01pY3Jvc29mdFRpbWVTdGFtcFBDQS5jcmwwWAYIKwYBBQUHAQEETDBKMEgGCCsG
# AQUFBzAChjxodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2NlcnRzL01pY3Jv
# c29mdFRpbWVTdGFtcFBDQS5jcnQwEwYDVR0lBAwwCgYIKwYBBQUHAwgwDQYJKoZI
# hvcNAQEFBQADggEBABhW2Lwu5/R0+yuB1kWyYWp9G8CaWAHqZhnXuCn1jzz09iI2
# d1FUmQud9f7Fg9U7F18kV7sSywfz8omzn+eIMTZc0N0QbbGdHG5zeUCA26QRbUwQ
# 6BCVoUNlxEgptx5suXvzd7dgvF0jpzSnWPUVzaasjBvdqMfy/L2f24Jaiu9s8vsu
# w79c0Y2DVhPd4x2T7ReueUVSCxzhK8AzUN271fiW2JRLQ0tRCF8tnA5TKJe7RuvG
# emKndxIklRnPRf1Y2R0getwBvO8Lg3pDeZDUR+AIteZ96oBsSHnsJwxb8T45Ur6a
# lIw5sEMholc7XInenHZH5DEg0aJpQ86Btpv5rzgwggTsMIID1KADAgECAhMzAAAB
# Cix5rtd5e6asAAEAAAEKMA0GCSqGSIb3DQEBBQUAMHkxCzAJBgNVBAYTAlVTMRMw
# EQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVN
# aWNyb3NvZnQgQ29ycG9yYXRpb24xIzAhBgNVBAMTGk1pY3Jvc29mdCBDb2RlIFNp
# Z25pbmcgUENBMB4XDTE1MDYwNDE3NDI0NVoXDTE2MDkwNDE3NDI0NVowgYMxCzAJ
# BgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25k
# MR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xDTALBgNVBAsTBE1PUFIx
# HjAcBgNVBAMTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjCCASIwDQYJKoZIhvcNAQEB
# BQADggEPADCCAQoCggEBAJL8bza74QO5KNZG0aJhuqVG+2MWPi75R9LH7O3HmbEm
# UXW92swPBhQRpGwZnsBfTVSJ5E1Q2I3NoWGldxOaHKftDXT3p1Z56Cj3U9KxemPg
# 9ZSXt+zZR/hsPfMliLO8CsUEp458hUh2HGFGqhnEemKLwcI1qvtYb8VjC5NJMIEb
# e99/fE+0R21feByvtveWE1LvudFNOeVz3khOPBSqlw05zItR4VzRO/COZ+owYKlN
# Wp1DvdsjusAP10sQnZxN8FGihKrknKc91qPvChhIqPqxTqWYDku/8BTzAMiwSNZb
# /jjXiREtBbpDAk8iAJYlrX01boRoqyAYOCj+HKIQsaUCAwEAAaOCAWAwggFcMBMG
# A1UdJQQMMAoGCCsGAQUFBwMDMB0GA1UdDgQWBBSJ/gox6ibN5m3HkZG5lIyiGGE3
# NDBRBgNVHREESjBIpEYwRDENMAsGA1UECxMETU9QUjEzMDEGA1UEBRMqMzE1OTUr
# MDQwNzkzNTAtMTZmYS00YzYwLWI2YmYtOWQyYjFjZDA1OTg0MB8GA1UdIwQYMBaA
# FMsR6MrStBZYAck3LjMWFrlMmgofMFYGA1UdHwRPME0wS6BJoEeGRWh0dHA6Ly9j
# cmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3RzL01pY0NvZFNpZ1BDQV8w
# OC0zMS0yMDEwLmNybDBaBggrBgEFBQcBAQROMEwwSgYIKwYBBQUHMAKGPmh0dHA6
# Ly93d3cubWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljQ29kU2lnUENBXzA4LTMx
# LTIwMTAuY3J0MA0GCSqGSIb3DQEBBQUAA4IBAQCmqFOR3zsB/mFdBlrrZvAM2PfZ
# hNMAUQ4Q0aTRFyjnjDM4K9hDxgOLdeszkvSp4mf9AtulHU5DRV0bSePgTxbwfo/w
# iBHKgq2k+6apX/WXYMh7xL98m2ntH4LB8c2OeEti9dcNHNdTEtaWUu81vRmOoECT
# oQqlLRacwkZ0COvb9NilSTZUEhFVA7N7FvtH/vto/MBFXOI/Enkzou+Cxd5AGQfu
# FcUKm1kFQanQl56BngNb/ErjGi4FrFBHL4z6edgeIPgF+ylrGBT6cgS3C6eaZOwR
# XU9FSY0pGi370LYJU180lOAWxLnqczXoV+/h6xbDGMcGszvPYYTitkSJlKOGMIIF
# vDCCA6SgAwIBAgIKYTMmGgAAAAAAMTANBgkqhkiG9w0BAQUFADBfMRMwEQYKCZIm
# iZPyLGQBGRYDY29tMRkwFwYKCZImiZPyLGQBGRYJbWljcm9zb2Z0MS0wKwYDVQQD
# EyRNaWNyb3NvZnQgUm9vdCBDZXJ0aWZpY2F0ZSBBdXRob3JpdHkwHhcNMTAwODMx
# MjIxOTMyWhcNMjAwODMxMjIyOTMyWjB5MQswCQYDVQQGEwJVUzETMBEGA1UECBMK
# V2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0
# IENvcnBvcmF0aW9uMSMwIQYDVQQDExpNaWNyb3NvZnQgQ29kZSBTaWduaW5nIFBD
# QTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALJyWVwZMGS/HZpgICBC
# mXZTbD4b1m/My/Hqa/6XFhDg3zp0gxq3L6Ay7P/ewkJOI9VyANs1VwqJyq4gSfTw
# aKxNS42lvXlLcZtHB9r9Jd+ddYjPqnNEf9eB2/O98jakyVxF3K+tPeAoaJcap6Vy
# c1bxF5Tk/TWUcqDWdl8ed0WDhTgW0HNbBbpnUo2lsmkv2hkL/pJ0KeJ2L1TdFDBZ
# +NKNYv3LyV9GMVC5JxPkQDDPcikQKCLHN049oDI9kM2hOAaFXE5WgigqBTK3S9dP
# Y+fSLWLxRT3nrAgA9kahntFbjCZT6HqqSvJGzzc8OJ60d1ylF56NyxGPVjzBrAlf
# A9MCAwEAAaOCAV4wggFaMA8GA1UdEwEB/wQFMAMBAf8wHQYDVR0OBBYEFMsR6MrS
# tBZYAck3LjMWFrlMmgofMAsGA1UdDwQEAwIBhjASBgkrBgEEAYI3FQEEBQIDAQAB
# MCMGCSsGAQQBgjcVAgQWBBT90TFO0yaKleGYYDuoMW+mPLzYLTAZBgkrBgEEAYI3
# FAIEDB4KAFMAdQBiAEMAQTAfBgNVHSMEGDAWgBQOrIJgQFYnl+UlE/wq4QpTlVnk
# pDBQBgNVHR8ESTBHMEWgQ6BBhj9odHRwOi8vY3JsLm1pY3Jvc29mdC5jb20vcGtp
# L2NybC9wcm9kdWN0cy9taWNyb3NvZnRyb290Y2VydC5jcmwwVAYIKwYBBQUHAQEE
# SDBGMEQGCCsGAQUFBzAChjhodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2Nl
# cnRzL01pY3Jvc29mdFJvb3RDZXJ0LmNydDANBgkqhkiG9w0BAQUFAAOCAgEAWTk+
# fyZGr+tvQLEytWrrDi9uqEn361917Uw7LddDrQv+y+ktMaMjzHxQmIAhXaw9L0y6
# oqhWnONwu7i0+Hm1SXL3PupBf8rhDBdpy6WcIC36C1DEVs0t40rSvHDnqA2iA6VW
# 4LiKS1fylUKc8fPv7uOGHzQ8uFaa8FMjhSqkghyT4pQHHfLiTviMocroE6WRTsgb
# 0o9ylSpxbZsa+BzwU9ZnzCL/XB3Nooy9J7J5Y1ZEolHN+emjWFbdmwJFRC9f9Nqu
# 1IIybvyklRPk62nnqaIsvsgrEA5ljpnb9aL6EiYJZTiU8XofSrvR4Vbo0HiWGFzJ
# NRZf3ZMdSY4tvq00RBzuEBUaAF3dNVshzpjHCe6FDoxPbQ4TTj18KUicctHzbMrB
# 7HCjV5JXfZSNoBtIA1r3z6NnCnSlNu0tLxfI5nI3EvRvsTxngvlSso0zFmUeDord
# EN5k9G/ORtTTF+l5xAS00/ss3x+KnqwK+xMnQK3k+eGpf0a7B2BHZWBATrBC7E7t
# s3Z52Ao0CW0cgDEf4g5U3eWh++VHEK1kmP9QFi58vwUheuKVQSdpw5OPlcmN2Jsh
# rg1cnPCiroZogwxqLbt2awAdlq3yFnv2FoMkuYjPaqhHMS+a3ONxPdcAfmJH0c6I
# ybgY+g5yjcGjPa8CQGr/aZuW4hCoELQ3UAjWwz0wggYHMIID76ADAgECAgphFmg0
# AAAAAAAcMA0GCSqGSIb3DQEBBQUAMF8xEzARBgoJkiaJk/IsZAEZFgNjb20xGTAX
# BgoJkiaJk/IsZAEZFgltaWNyb3NvZnQxLTArBgNVBAMTJE1pY3Jvc29mdCBSb290
# IENlcnRpZmljYXRlIEF1dGhvcml0eTAeFw0wNzA0MDMxMjUzMDlaFw0yMTA0MDMx
# MzAzMDlaMHcxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYD
# VQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xITAf
# BgNVBAMTGE1pY3Jvc29mdCBUaW1lLVN0YW1wIFBDQTCCASIwDQYJKoZIhvcNAQEB
# BQADggEPADCCAQoCggEBAJ+hbLHf20iSKnxrLhnhveLjxZlRI1Ctzt0YTiQP7tGn
# 0UytdDAgEesH1VSVFUmUG0KSrphcMCbaAGvoe73siQcP9w4EmPCJzB/LMySHnfL0
# Zxws/HvniB3q506jocEjU8qN+kXPCdBer9CwQgSi+aZsk2fXKNxGU7CG0OUoRi4n
# rIZPVVIM5AMs+2qQkDBuh/NZMJ36ftaXs+ghl3740hPzCLdTbVK0RZCfSABKR2YR
# JylmqJfk0waBSqL5hKcRRxQJgp+E7VV4/gGaHVAIhQAQMEbtt94jRrvELVSfrx54
# QTF3zJvfO4OToWECtR0Nsfz3m7IBziJLVP/5BcPCIAsCAwEAAaOCAaswggGnMA8G
# A1UdEwEB/wQFMAMBAf8wHQYDVR0OBBYEFCM0+NlSRnAK7UD7dvuzK7DDNbMPMAsG
# A1UdDwQEAwIBhjAQBgkrBgEEAYI3FQEEAwIBADCBmAYDVR0jBIGQMIGNgBQOrIJg
# QFYnl+UlE/wq4QpTlVnkpKFjpGEwXzETMBEGCgmSJomT8ixkARkWA2NvbTEZMBcG
# CgmSJomT8ixkARkWCW1pY3Jvc29mdDEtMCsGA1UEAxMkTWljcm9zb2Z0IFJvb3Qg
# Q2VydGlmaWNhdGUgQXV0aG9yaXR5ghB5rRahSqClrUxzWPQHEy5lMFAGA1UdHwRJ
# MEcwRaBDoEGGP2h0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1
# Y3RzL21pY3Jvc29mdHJvb3RjZXJ0LmNybDBUBggrBgEFBQcBAQRIMEYwRAYIKwYB
# BQUHMAKGOGh0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljcm9z
# b2Z0Um9vdENlcnQuY3J0MBMGA1UdJQQMMAoGCCsGAQUFBwMIMA0GCSqGSIb3DQEB
# BQUAA4ICAQAQl4rDXANENt3ptK132855UU0BsS50cVttDBOrzr57j7gu1BKijG1i
# uFcCy04gE1CZ3XpA4le7r1iaHOEdAYasu3jyi9DsOwHu4r6PCgXIjUji8FMV3U+r
# kuTnjWrVgMHmlPIGL4UD6ZEqJCJw+/b85HiZLg33B+JwvBhOnY5rCnKVuKE5nGct
# xVEO6mJcPxaYiyA/4gcaMvnMMUp2MT0rcgvI6nA9/4UKE9/CCmGO8Ne4F+tOi3/F
# NSteo7/rvH0LQnvUU3Ih7jDKu3hlXFsBFwoUDtLaFJj1PLlmWLMtL+f5hYbMUVbo
# nXCUbKw5TNT2eb+qGHpiKe+imyk0BncaYsk9Hm0fgvALxyy7z0Oz5fnsfbXjpKh0
# NbhOxXEjEiZ2CzxSjHFaRkMUvLOzsE1nyJ9C/4B5IYCeFTBm6EISXhrIniIh0EPp
# K+m79EjMLNTYMoBMJipIJF9a6lbvpt6Znco6b72BJ3QGEe52Ib+bgsEnVLaxaj2J
# oXZhtG6hE6a/qkfwEm/9ijJssv7fUciMI8lmvZ0dhxJkAj0tr1mPuOQh5bWwymO0
# eFQF1EEuUKyUsKV4q7OglnUa2ZKHE3UiLzKoCG6gW4wlv6DvhMoh1useT8ma7kng
# 9wFlb4kLfchpyOZu6qeXzjEp/w7FW1zYTRuh2Povnj8uVRZryROj/TGCBKcwggSj
# AgEBMIGQMHkxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYD
# VQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xIzAh
# BgNVBAMTGk1pY3Jvc29mdCBDb2RlIFNpZ25pbmcgUENBAhMzAAABCix5rtd5e6as
# AAEAAAEKMAkGBSsOAwIaBQCggcAwGQYJKoZIhvcNAQkDMQwGCisGAQQBgjcCAQQw
# HAYKKwYBBAGCNwIBCzEOMAwGCisGAQQBgjcCARUwIwYJKoZIhvcNAQkEMRYEFMcI
# APwhtMCyKyeZxqCk/p8+FjvuMGAGCisGAQQBgjcCAQwxUjBQoCaAJABXAGkAbgBk
# AG8AdwBzACAAUABvAHcAZQByAFMAaABlAGwAbKEmgCRodHRwOi8vd3d3Lm1pY3Jv
# c29mdC5jb20vcG93ZXJzaGVsbCAwDQYJKoZIhvcNAQEBBQAEggEAc4/mLa9Sd5Su
# MT+22YIuKB8GxxHVgNRrzNJRfT8NzR8xi11JgZ7tIrLGJtyj+27GdTosIq2JO8MU
# PjftiqOCr12+pR1UeZjSDp1sYsz3S6WnMNMW115vnPONVxtrTslXU6qFPWHd0xyh
# TuuttVdCoaI1EXmjJ48QFYC96v5recIqIAe58ZU15mgaO6+1J09p6drao/OUQP2j
# xXVT5dbBXoqoq+uCpLGriyEuzu8aHRr7GLnkYREhiS6k2/buFSNas5LwaE7AO04v
# HMhReBH2b02cMTbH6t9ymycagvLoRLPGcn7bqbjOn1gh8crjEKCcZxJltw9gJ1Al
# 0XStSHLJCqGCAigwggIkBgkqhkiG9w0BCQYxggIVMIICEQIBATCBjjB3MQswCQYD
# VQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEe
# MBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSEwHwYDVQQDExhNaWNyb3Nv
# ZnQgVGltZS1TdGFtcCBQQ0ECEzMAAABzxlnCUpeDozoAAAAAAHMwCQYFKw4DAhoF
# AKBdMBgGCSqGSIb3DQEJAzELBgkqhkiG9w0BBwEwHAYJKoZIhvcNAQkFMQ8XDTE1
# MDYzMDIwMTMwMFowIwYJKoZIhvcNAQkEMRYEFNznxQG21MsjVrRUoY1N5dL03TrZ
# MA0GCSqGSIb3DQEBBQUABIIBAIFirsJOO1uyBmp4h5pJF5tIM4fsHXgE5Q0tHcF6
# 3CHyYIEIPB/hmjfLpO436OezVIvXX7zX2FJE6xp9+lflUKzjsTlH5r+sWQAVgjQS
# /K8xVezKentKX2bpU5ZupHmeRqLz9FQn3ldjtKnq+Fn/slIo4IGK9r7qD1+hYesh
# +mBSOgbxNlXDi2bv/bkjTtltbcmcjhm3iQYS6Ea3NJbluG2m9tQoiWbhneNHINEu
# ueAPOapujGAcvhp/AYbt0XJ0lz8RPznmJIPNf1mNxKjCcfEwS3XW2eWf85kQS33p
# X2zxVUYqInG3SwclnykiBN5beSlccpdW8HgRnRSCzMTNPXE=
# SIG # End signature block
