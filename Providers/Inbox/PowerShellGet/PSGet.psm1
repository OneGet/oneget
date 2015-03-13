
#########################################################################################
#
# Copyright (c) Microsoft Corporation. All rights reserved.
#
# PowerShellGet Module
#
#########################################################################################

Microsoft.PowerShell.Core\Set-StrictMode -Version Latest

$script:ProgramFilesModulesPath = Microsoft.PowerShell.Management\Join-Path -Path $env:ProgramFiles -ChildPath "WindowsPowerShell\Modules"

$script:MyDocumentsModulesPath = if([Environment]::GetFolderPath("MyDocuments"))
                                 {
                                    Microsoft.PowerShell.Management\Join-Path -Path ([Environment]::GetFolderPath("MyDocuments")) -ChildPath "WindowsPowerShell\Modules"
                                 } 
                                 else
                                 {
                                    Microsoft.PowerShell.Management\Join-Path -Path $env:USERPROFILE -ChildPath "Documents\WindowsPowerShell\Modules"
                                 }
$script:TempPath = ([System.IO.DirectoryInfo]$env:TEMP).FullName
$script:PSGetItemInfoFileName = "PSGetModuleInfo.xml"
$script:PSGetAppLocalPath="$env:LOCALAPPDATA\Microsoft\Windows\PowerShell\PowerShellGet"
$script:PSGetModuleSourcesFilePath = Microsoft.PowerShell.Management\Join-Path -Path $script:PSGetAppLocalPath -ChildPath "PSRepositories.xml"
$script:PSGetModuleSources = $null
$script:PSGetInstalledModules = $null

# Public PSGallery module source name and location
$Script:PSGalleryModuleSource="PSGallery"
$Script:PSGallerySourceUri  = 'https://go.microsoft.com/fwlink/?LinkID=397631&clcid=0x409'
$Script:PSGalleryPublishUri = 'https://go.microsoft.com/fwlink/?LinkID=397527&clcid=0x409'

# PSGallery V3 Source
$Script:PSGalleryV3SourceUri = 'https://go.microsoft.com/fwlink/?LinkId=528403&clcid=0x409'

$Script:PSGalleryV2ApiAvailable = $true
$Script:PSGalleryV3ApiAvailable = $false

# Internal MSPSGallery module source name and location
$Script:InternalSourceName = "MSPSGallery"
$Script:InternalSourceUri = 'http://go.microsoft.com/fwlink/?LinkID=397633&clcid=0x409'
$Script:InternalPublishUri = 'http://go.microsoft.com/fwlink/?LinkID=397635&clcid=0x409'

$script:PSModuleProviderName = "PSModule"
$script:OneGetProviderParam  = "OneGetProvider"
$script:PublishLocation = "PublishLocation"
$script:NuGetProviderName = "NuGet"
$script:SupportsPSModulesFeatureName="supports-powershell-modules"
$script:FastPackRefHastable = @{}
$script:NuGetBinaryProgramDataPath="$env:ProgramFiles\OneGet\ProviderAssemblies"
$script:NuGetBinaryLocalAppDataPath="$env:LOCALAPPDATA\OneGet\ProviderAssemblies"
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
$script:IncludeValidSet = @("DscResource","Cmdlet","Function")
$script:DscResource = "PSDscResource"
$script:Command     = "PSCommand"
$script:Cmdlet      = "PSCmdlet"
$script:Function    = "PSFunction"
$script:Includes    = "PSIncludes"
$script:Tag         = "Tag"
$script:NotSpecified= '_NotSpecified_'

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

$script:OneGetMessageResolverScriptBlock =  {
                                                param($i, $Message)
                                                return (OneGetMessageResolver -MsgId $i, -Message $Message)			
                                            }		

$script:OneGetInstallModuleMessageResolverScriptBlock =  {
                                                param($i, $Message)
                                                $PackageTarget = $LocalizedData.InstallModulewhatIfMessage
                                                switch ($i)
                                                {
                                                    'ActionInstallPackage' { return "Install-Module" }              
                                                    'TargetPackage' { return $PackageTarget }
                                                     Default {
                                                        return (OneGetMessageResolver -MsgId $i, -Message $Message)
                                                     }
                                                }                                                
                                            }		

$script:OneGetUpdateModuleMessageResolverScriptBlock =  {
                                                param($i, $Message)
                                                $PackageTarget = ($LocalizedData.UpdateModulewhatIfMessage -replace "__OLDVERSION__",$($psgetItemInfo.Version))                                                
                                                switch ($i)
                                                {
                                                    'ActionInstallPackage' { return "Update-Module" }              
                                                    'TargetPackage' { return $PackageTarget }
                                                     Default {
                                                        return (OneGetMessageResolver -MsgId $i, -Message $Message)
                                                     }
                                                }                                                
                                            }		
                                            
function OneGetMessageResolver($MsgID, $Message) {    
              	$SourceNotFound = $LocalizedData.SourceNotFound
                $ModuleIsNotTrusted = $LocalizedData.ModuleIsNotTrusted
                $RepositoryIsNotTrusted = $LocalizedData.RepositoryIsNotTrusted

                switch ($MsgID)
                {
                   'SourceNotFound' { return $SourceNotFound }                   
                   'CaptionPackageNotTrusted' { return $ModuleIsNotTrusted }
                   'CaptionSourceNotTrusted' { return $RepositoryIsNotTrusted }
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

Microsoft.PowerShell.Utility\Import-LocalizedData  LocalizedData -filename PSGet.Resource.psd1


function Publish-Module
{
    <#
    .ExternalHelp PSGet.psm1-help.xml
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
        [ValidateNotNullOrEmpty()]
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

        $moduleSource = Get-PSRepository -Name $Repository

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

        if($moduleSource.OneGetProvider -ne $script:NuGetProviderName)
        {
            $message = $LocalizedData.PublishModuleSupportsOnlyNuGetBasedPublishLocations -f ($moduleSource.PublishLocation, $Repository, $Repository)
            ThrowError -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $message `
                        -ErrorId "PublishModuleSupportsOnlyNuGetBasedPublishLocations" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ErrorCategory InvalidArgument `
                        -ExceptionObject $Repository
        }
        
        Install-NuGetClientBinaries
    }

    Process
    {
        if($Name)
        {
            if($RequiredVersion)
            {
                $module = Microsoft.PowerShell.Core\Get-Module -FullyQualifiedName @{ModuleName=$Name;RequiredVersion=$RequiredVersion} `
                                                               -ListAvailable `
                                                               -Verbose:$false
            }
            else
            {
                $module = Microsoft.PowerShell.Core\Get-Module -Name $Name -ListAvailable -Verbose:$false
            }

            if(-not $module)
            {
                $message = $LocalizedData.ModuleNotAvailableLocally -f ($Name)
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
        $tempModulePath = "$script:TempPath\$(Microsoft.PowerShell.Utility\Get-Random)\$moduleName"
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
                Publish-PSGetExtModule -PSModuleInfo $moduleInfo `
                                       -NugetApiKey $NuGetApiKey `
                                       -Destination $DestinationLocation `
                                       -Repository $Repository `
                                       -NugetPackageRoot $tempModulePath `
                                       -FormatVersion $FormatVersion `
                                       -ReleaseNotes "$ReleaseNotes" `
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
    .ExternalHelp PSGet.psm1-help.xml
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
        
        if($PSBoundParameters.ContainsKey("Repository"))
        {
            $PSBoundParameters["Source"] = $Repository
            $null = $PSBoundParameters.Remove("Repository")
        }
        
        $PSBoundParameters["MessageResolver"] = $script:OneGetMessageResolverScriptBlock

        OneGet\Find-Package @PSBoundParameters | Microsoft.PowerShell.Core\ForEach-Object {New-PSGetItemInfo -SoftwareIdenties $_}        
    }
}

function Install-Module
{
    <#
    .ExternalHelp PSGet.psm1-help.xml
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
    }

    Process
    {
        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters["MessageResolver"] = $script:OneGetInstallModuleMessageResolverScriptBlock

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
            }

            if($PSBoundParameters.ContainsKey("Version"))
            {
                $null = $PSBoundParameters.Remove("Version")
                $PSBoundParameters["MinimumVersion"] = $MinimumVersion
            }

            $null = OneGet\Install-Package @PSBoundParameters
        }
        elseif($PSCmdlet.ParameterSetName -eq "InputObject")
        {
            $null = $PSBoundParameters.Remove("InputObject")

            foreach($inputValue in $InputObject)
            {
                if (($inputValue.PSTypeNames -notcontains "Microsoft.PowerShell.Commands.PSGetModuleInfo") -and
                    ($inputValue.PSTypeNames -notcontains "Deserialized.Microsoft.PowerShell.Commands.PSGetModuleInfo") -and
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
                $PSBoundParameters["OneGetProvider"] = $psgetModuleInfo.OneGetProvider

                $null = OneGet\Install-Package @PSBoundParameters
            }
        }
    }
}

function Update-Module
{
    <#
    .ExternalHelp PSGet.psm1-help.xml
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
            foreach($moduleName in $Name)
            {
                $availableModules = Get-Module -ListAvailable $moduleName -Verbose:$false
        
                if(-not $availableModules -and -not (Test-WildcardPattern -Name $moduleName))
                {                    
                    $message = $LocalizedData.ModuleNotInstalledOnThiseMachine -f ($moduleName)
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

        foreach($moduleBase in $moduleBasesToUpdate)
        {
            $PSGetItemInfoPath = Microsoft.PowerShell.Management\Join-Path $moduleBase $script:PSGetItemInfoFileName

            $psgetItemInfo = Microsoft.PowerShell.Utility\Import-Clixml -Path $PSGetItemInfoPath
            
            # Skip the module name if it is already tried in the current pipeline
            if($moduleNamesInPipeline -contains $psgetItemInfo.Name)
            {
                continue
            }

            $moduleNamesInPipeline += $psgetItemInfo.Name

            $message = $LocalizedData.CheckingForModuleUpdate -f ($psgetItemInfo.Name)
            Write-Verbose -Message $message

            $providerName = $script:NuGetProviderName
            if((Get-Member -InputObject $psgetItemInfo -Name OneGetProvider))
            {
                $providerName = $psgetItemInfo.OneGetProvider
            }

            $PSBoundParameters["Name"] = $psgetItemInfo.Name
            $PSBoundParameters["Location"] = $psgetItemInfo.RepositorySourceLocation

            Get-PSGalleryApiAvailability -Repository (Get-SourceName -Location $psgetItemInfo.RepositorySourceLocation)

            $PSBoundParameters["OneGetProvider"] = $providerName 
            $PSBoundParameters["InstallUpdate"] = $true

            if($moduleBase.ToString().StartsWith($script:MyDocumentsModulesPath, [System.StringComparison]::OrdinalIgnoreCase))
            {
                $PSBoundParameters["Scope"] = "CurrentUser"
            }

            $PSBoundParameters["MessageResolver"] = $script:OneGetUpdateModuleMessageResolverScriptBlock
            $sid = OneGet\Install-Package @PSBoundParameters
        }
    }
}

function Uninstall-Module
{
    <#
    .ExternalHelp PSGet.psm1-help.xml
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
        $PSBoundParameters["MessageResolver"] = $script:OneGetMessageResolverScriptBlock

        if($PSCmdlet.ParameterSetName -eq "InputObject")
        {
            $null = $PSBoundParameters.Remove("InputObject")
        
            foreach($inputValue in $InputObject)
            {
                if (($inputValue.PSTypeNames -notcontains "Microsoft.PowerShell.Commands.PSGetModuleInfo") -and
                    ($inputValue.PSTypeNames -notcontains "Deserialized.Microsoft.PowerShell.Commands.PSGetModuleInfo"))
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

                $null = OneGet\Uninstall-Package @PSBoundParameters
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

            $null = OneGet\Uninstall-Package @PSBoundParameters
        }
    }
}

function Get-InstalledModule
{
    <#
    .ExternalHelp PSGet.psm1-help.xml
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
        $PSBoundParameters["MessageResolver"] = $script:OneGetMessageResolverScriptBlock

        OneGet\Get-Package @PSBoundParameters | Microsoft.PowerShell.Core\ForEach-Object {New-PSGetItemInfo -SoftwareIdenties $_}  
    }
}

function Register-PSRepository
{
    <#
    .ExternalHelp PSGet.psm1-help.xml
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
        [ValidateSet('Trusted','Untrusted')]
        [string]
        $InstallationPolicy = 'Untrusted',

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $OneGetProvider        
    )

    DynamicParam
    {
        if (Get-Variable -Name SourceLocation -ErrorAction SilentlyContinue)
        {
            Set-Variable -Name selctedProviderName -value $null -Scope 1

            if(Get-Variable -Name OneGetProvider -ErrorAction SilentlyContinue)
            {
                $selctedProviderName = $OneGetProvider
                $null = Get-DynamicParameters -Location $SourceLocation -OneGetProvider ([REF]$selctedProviderName)
            }
            else
            {
                $dynamicParameters = Get-DynamicParameters -Location $SourceLocation -OneGetProvider ([REF]$selctedProviderName)
                Set-Variable -Name OneGetProvider -Value $selctedProviderName -Scope 1
                $null = $dynamicParameters
            }
        }
    }

    Begin
    {
        Get-PSGalleryApiAvailability -Repository $Name
        
        Install-NuGetClientBinaries
    }

    Process
    {
        if($InstallationPolicy -eq "Trusted")
        {
            $PSBoundParameters.Add("Trusted", $true)
        }

        $providerName = $null

        if($OneGetProvider)
        {            
            $providerName = $OneGetProvider
        }
        elseif($selctedProviderName)
        {
            $providerName = $selctedProviderName
        }
        else
        {
            $providerName = Get-OneGetProviderName -Location $SourceLocation
        }

        if($providerName)
        {
            $PSBoundParameters[$script:OneGetProviderParam] = $providerName
        }

        if($PublishLocation)
        {
            $PSBoundParameters[$script:PublishLocation] = Get-LocationString -LocationUri $PublishLocation
        }

        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        
        $PSBoundParameters["Location"] = Get-LocationString -LocationUri $SourceLocation
        $null = $PSBoundParameters.Remove("SourceLocation")
        $null = $PSBoundParameters.Remove("InstallationPolicy")

        $PSBoundParameters["MessageResolver"] = $script:OneGetMessageResolverScriptBlock

        $null = OneGet\Register-PackageSource @PSBoundParameters
    }
}

function Set-PSRepository
{
    <#
    .ExternalHelp PSGet.psm1-help.xml
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
        [ValidateSet('Trusted','Untrusted')]
        [string]
        $InstallationPolicy,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $OneGetProvider
    )

    DynamicParam
    {
        if (Get-Variable -Name Name -ErrorAction SilentlyContinue)
        {
            $moduleSource = Get-PSRepository -Name $Name -ErrorAction SilentlyContinue -WarningAction SilentlyContinue

            if($moduleSource)
            {
                $providerName = $moduleSource.OneGetProvider
            
                $loc = $moduleSource.SourceLocation
            
                if(Get-Variable -Name SourceLocation -ErrorAction SilentlyContinue)
                {
                    $loc = $SourceLocation
                }

                if(Get-Variable -Name OneGetProvider -ErrorAction SilentlyContinue)
                {
                    $providerName = $OneGetProvider
                }

                $null = Get-DynamicParameters -Location $loc -OneGetProvider ([REF]$providerName)
            }
        }
    }

    Begin
    {
        Get-PSGalleryApiAvailability -Repository $Name
        
        Install-NuGetClientBinaries
    }

    Process
    {
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

        if (-not $OneGetProvider)
        {
            $OneGetProvider = $ModuleSource.OneGetProvider
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

        if($SourceLocation)
        {
            $PSBoundParameters["NewLocation"] = Get-LocationString -LocationUri $SourceLocation

            $null = $PSBoundParameters.Remove("SourceLocation")
        }

        $PSBoundParameters[$script:OneGetProviderParam] = $OneGetProvider
        $PSBoundParameters.Add("Trusted", $Trusted)        
        $PSBoundParameters["Provider"] = $script:PSModuleProviderName
        $PSBoundParameters["MessageResolver"] = $script:OneGetMessageResolverScriptBlock

        $null = OneGet\Set-PackageSource @PSBoundParameters
    }
}

function Unregister-PSRepository
{
    <#
    .ExternalHelp PSGet.psm1-help.xml
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
        $PSBoundParameters["MessageResolver"] = $script:OneGetMessageResolverScriptBlock

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

            $null = OneGet\Unregister-PackageSource @PSBoundParameters
        }
    }
}

function Get-PSRepository
{
    <#
    .ExternalHelp PSGet.psm1-help.xml
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
        $PSBoundParameters["MessageResolver"] = $script:OneGetMessageResolverScriptBlock

        if($Name)
        {
            foreach($sourceName in $Name)
            {
                $PSBoundParameters["Name"] = $sourceName
                
                $packageSources = OneGet\Get-PackageSource @PSBoundParameters

                $packageSources | Microsoft.PowerShell.Core\ForEach-Object { New-ModuleSourceFromPackageSource -PackageSource $_ }
            }
        }
        else
        {
            $packageSources = OneGet\Get-PackageSource @PSBoundParameters

            $packageSources | Microsoft.PowerShell.Core\ForEach-Object { New-ModuleSourceFromPackageSource -PackageSource $_ }
        }
    }
}


#region Utility functions

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


    if ($Script:PSGalleryV3ApiAvailable)
    {
        if ($Script:PSGalleryV2ApiAvailable)
        {
            Write-Warning -Message $LocalizedData.PSGalleryApiV2Deprecated
            return
        }
        else 
        {
            ThrowError  -ExceptionName "System.InvalidOperationException" `
                        -ExceptionMessage $LocalizedData.PSGalleryApiV2Discontinued `
                        -ErrorId "PSGalleryApiV2Discontinued" `
                        -CallerPSCmdlet $PSCmdlet `
                        -ErrorCategory InvalidOperation
        }
    }

    # if V3 is not available, v2 must be 
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
                       -ExceptionMessage $LocalizedData.VersionParametersAreAllowedOnlyWithSingleModule `
                       -ErrorId "VersionParametersAreAllowedOnlyWithSingleModule" `
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
    param([switch]$Force)

    if(-not $script:PSGetModuleSources -or $Force)
    {
        if(Microsoft.PowerShell.Management\Test-Path $script:PSGetModuleSourcesFilePath)
        {
            $script:PSGetModuleSources = Microsoft.PowerShell.Utility\Import-Clixml $script:PSGetModuleSourcesFilePath
        }
        else
        {
            $script:PSGetModuleSources = [ordered]@{}
        }

        if(-not $script:PSGetModuleSources.Contains($Script:PSGalleryModuleSource))
        {
            $psgalleryLocation = $null
            try
            {
                $psgalleryLocation = Get-ValidModuleLocation -LocationString $Script:PSGallerySourceUri -ParameterName "Source" -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
            }
            catch
            {
            }

            if($psgalleryLocation)
            {
                $moduleSource = Microsoft.PowerShell.Utility\New-Object PSCustomObject -Property ([ordered]@{
                        Name = $Script:PSGalleryModuleSource
                        SourceLocation =  $psgalleryLocation
                        PublishLocation = $Script:PSGalleryPublishUri
                        Trusted=$false
                        Registered=$true
                        InstallationPolicy = 'Untrusted'
                        OneGetProvider=$script:NuGetProviderName
                        ProviderOptions = @{}
                    })

                $moduleSource.PSTypeNames.Insert(0, "Microsoft.PowerShell.Commands.PSRepository")
                $script:PSGetModuleSources.Add($Script:PSGalleryModuleSource, $moduleSource)
            }

            # Add the Internal MSPSGallery module source if it is reachable.
            if(-not $script:PSGetModuleSources.Contains($Script:InternalSourceName))
            {
                $location = $null
                $InternalPublishLocation = $null
                try
                {
                    $location = Get-ValidModuleLocation -LocationString $Script:InternalSourceUri -ParameterName "Source" -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
                    $InternalPublishLocation =  $Script:InternalPublishUri
                }
                catch
                {
                }

                if($location -and -not $location.StartsWith("http://www.microsoft.com", [System.StringComparison]::OrdinalIgnoreCase))
                {
                    $internalModuleSource = Microsoft.PowerShell.Utility\New-Object PSCustomObject -Property ([ordered]@{
                            Name = $Script:InternalSourceName
                            SourceLocation =  $location
                            PublishLocation = $InternalPublishLocation
                            Trusted=$true
                            Registered=$true
                            InstallationPolicy = 'Trusted'
                            OneGetProvider=$script:NuGetProviderName
                            ProviderOptions = @{}
                        })

                    $internalModuleSource.PSTypeNames.Insert(0, "Microsoft.PowerShell.Commands.PSRepository")
                    $script:PSGetModuleSources.Add($Script:InternalSourceName, $internalModuleSource)
                }
            }
        }
    }   
}

function Get-OneGetProviderName
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [Uri]
        $Location
    )

    $OneGetProviderName = $null
    $loc = Get-LocationString -LocationUri $Location

    $providers = OneGet\Get-PackageProvider | Where-Object { $_.Features.ContainsKey($script:SupportsPSModulesFeatureName) }

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
            $OneGetProviderName = $provider.ProviderName
            break
        }
    }

    return $OneGetProviderName
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
        $OneGetProvider
    )

    $paramDictionary = New-Object System.Management.Automation.RuntimeDefinedParameterDictionary
    $dynamicOptions = $null

    $loc = Get-LocationString -LocationUri $Location

    $providers = OneGet\Get-PackageProvider | Where-Object { $_.Features.ContainsKey($script:SupportsPSModulesFeatureName) }
            
    if ($OneGetProvider.Value)
    {
        # Skip the PSModule provider
        if($OneGetProvider.Value -ne $script:PSModuleProviderName)
        {
            $SelectedProvider = $providers | Where-Object {$_.ProviderName -eq $OneGetProvider.Value}

            if($SelectedProvider)
            {
                $res = Get-PackageSource -Location $loc -Provider $OneGetProvider.Value -ErrorAction SilentlyContinue 
            
                if($res)
                {
                    $dynamicOptions = $SelectedProvider.DynamicOptions
                }
            }
        }
    }
    else
    {
        $OneGetProvider.Value = Get-OneGetProviderName -Location $Location
        if($OneGetProvider.Value)
        {
            $provider = $providers | Where-Object {$_.ProviderName -eq $OneGetProvider.Value}
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

        $message = $LocalizedData.DynamicParameterHelpMessage -f ($option.Name, $OneGetProvider.Value, $loc, $option.Name)
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
        $OneGetProviderName
    )

    foreach($swid in $SoftwareIdenties)
    {
        $sourceName = (Get-First $swid["SourceName"])

        if(-not $sourceName)
        {
            $sourceName = (Get-SourceName -Location $swid.Source)
        }

        $published = (Get-First $swid["published"])
        $PublishedDate = New-Object System.DateTime

        $tags = (Get-First $swid["tags"]) -split " "
        $userTags = @()
        $exportedDscResources = @()
        $exportedCommands = @()
        $exportedCmdlets = @()
        $exportedFunctions = @()
        $PSGetFormatVersion = $null

        ForEach($tag in $tags)
        {
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
                $script:PSGetFormatVersion { $PSGetFormatVersion = $parts[1]; break }
                Default                    { $userTags += $tag; break }
            }
        }

        $PSGetItemInfo = Microsoft.PowerShell.Utility\New-Object PSCustomObject -Property ([ordered]@{
                Name = $swid.Name
                Version = [Version]$swid.Version
                    
                Description = (Get-First $swid["description"])
                Author = (Get-EntityName -SoftwareIdentity $swid -Role "author")
                CompanyName = (Get-EntityName -SoftwareIdentity $swid -Role "owner")
                Copyright = (Get-First $swid["copyright"])
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
                            }

                PowerShellGetFormatVersion=[Version]$PSGetFormatVersion

                ReleaseNotes = (Get-First $swid["releaseNotes"])

                RequiredModules = (Get-First $swid["requiredModules"])

                RepositorySourceLocation = $swid.Source
                Repository = if($sourceName) { $sourceName } else { $swid.Source }

                OneGetProvider = if($OneGetProviderName) { $OneGetProviderName } else { (Get-First $swid["OneGetProvider"]) }
            })

        $PSGetItemInfo.PSTypeNames.Insert(0, "Microsoft.PowerShell.Commands.PSGetModuleInfo")
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
        if($psModuleSource.SourceLocation -eq $Location)
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
    $nugetProvider = OneGet\Get-PackageProvider -ErrorAction SilentlyContinue -WarningAction SilentlyContinue | Microsoft.PowerShell.Core\Where-Object {$_.Name -eq "NuGet"}

    if($nugetProvider -and 
       $nugetProvider.Features.Exe -and 
       (Microsoft.PowerShell.Management\Test-Path $nugetProvider.Features.Exe))
    {
        $script:NuGetClient = $nugetProvider.Features.Exe
    }
    else
    {
        $ShouldContinueQueryMessage = $LocalizedData.InstallNuGetBinariesShouldContinueQuery -f @($script:NuGetBinaryProgramDataPath,$script:NuGetBinaryLocalAppDataPath)

        if($PSCmdlet.ShouldContinue($ShouldContinueQueryMessage, $LocalizedData.InstallNuGetBinariesShouldContinueCaption))
        {
            Write-Verbose -Message $LocalizedData.DownloadingNugetBinaries

            # Bootstrap the NuGet provider
            $nugetProvider = OneGet\Get-PackageProvider -Name NuGet -Force

            if($nugetProvider -and 
               $nugetProvider.Features.Exe -and 
               (Microsoft.PowerShell.Management\Test-Path $nugetProvider.Features.Exe))
            {
                $script:NuGetClient = $nugetProvider.Features.Exe
            }
        }
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
                $message = $LocalizedData.UnableResolveModuleDependency -f ($ModuleName, $Repository, $DependentModuleInfo.Name, $ModuleName, $Repository, $ModuleName)
                ThrowError -ExceptionName "System.InvalidOperationException" `
                            -ExceptionMessage $message `
                            -ErrorId "UnableResolveModuleDependency" `
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
            $FindModuleArguments['Name'] = $ModuleName
            $FindModuleArguments['MinimumVersion'] = $requiredModuleInfo.Version

            $psgetItemInfo = Find-Module @FindModuleArguments  | 
                                        Microsoft.PowerShell.Core\Where-Object {$_.Name -eq $ModuleName} | 
                                            Microsoft.PowerShell.Utility\Select-Object -Last 1

            if(-not $psgetItemInfo)
            {
                $message = $LocalizedData.UnableResolveModuleDependency -f ($ModuleName, $Repository, $DependentModuleInfo.Name, $ModuleName, $Repository, $ModuleName)
                ThrowError -ExceptionName "System.InvalidOperationException" `
                            -ExceptionMessage $message `
                            -ErrorId "UnableResolveModuleDependency" `
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

function Publish-PSGetExtModule
{
    [CmdletBinding(PositionalBinding=$false)]
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [PSModuleInfo]
        $PSModuleInfo,

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

        [Parameter()] 
        [Version]
        $FormatVersion,

        [Parameter()]
        [string]
        $ReleaseNotes,

        [Parameter()]
        [string[]]
        $Tags,
        
        [Parameter()]
        [Uri]
        $LicenseUri,

        [Parameter()]
        [Uri]
        $IconUri,
        
        [Parameter()]
        [Uri]
        $ProjectUri
    )

    if(-not (Microsoft.PowerShell.Management\Test-Path $script:NuGetClient))
    {
        Install-NuGetClientBinaries
    }

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

    # Add PSModule and PSGet format version tags
    if(-not $Tags)
    {
        $Tags = @()
    }
    
    if($FormatVersion)
    {
        $Tags += "$($script:PSGetFormatVersion)_$FormatVersion"
    }

    $Tags += "PSModule"
    if($PSModuleInfo.ExportedCommands.Count)
    {
        if($PSModuleInfo.ExportedCmdlets.Count)
        {
            $Tags += "$($script:Includes)_Cmdlet"
            $Tags += $PSModuleInfo.ExportedCmdlets.Keys | Microsoft.PowerShell.Core\ForEach-Object { "$($script:Cmdlet)_$_" }
        }

        if($PSModuleInfo.ExportedFunctions.Count)
        {
            $Tags += "$($script:Includes)_Function"
            $Tags += $PSModuleInfo.ExportedFunctions.Keys | Microsoft.PowerShell.Core\ForEach-Object { "$($script:Function)_$_" }
        }

        $Tags += $PSModuleInfo.ExportedCommands.Keys | Microsoft.PowerShell.Core\ForEach-Object { "$($script:Command)_$_" }
    }
    
    $dscResourceNames = Get-ExportedDscResources -PSModuleInfo $PSModuleInfo 
    if($dscResourceNames)
    {
        $Tags += "$($script:Includes)_DscResource"

        $Tags += $dscResourceNames | Microsoft.PowerShell.Core\ForEach-Object { "$($script:DscResource)_$_" }
    }

    # Populate the module dependencies elements from RequiredModules and 
    # NestedModules properties of the current PSModuleInfo
    $DependentModuleDetails = Get-ModuleDependencies -PSModuleInfo $PSModuleInfo `
                                                     -Repository $Repository `
                                                     -CallerPSCmdlet $PSCmdlet `
                                                     -Verbose:$VerbosePreference `
                                                     -Debug:$DebugPreference 

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
        <id>$(Get-EscapedString -ElementValue $PSModuleInfo.Name)</id>
        <version>$($PSModuleInfo.Version)</version>
        <authors>$(Get-EscapedString -ElementValue $PSModuleInfo.Author)</authors>
        <owners>$(Get-EscapedString -ElementValue $PSModuleInfo.CompanyName)</owners>
        <description>$(Get-EscapedString -ElementValue $PSModuleInfo.Description)</description>
        <releaseNotes>$(Get-EscapedString -ElementValue $ReleaseNotes)</releaseNotes>
        <copyright>$(Get-EscapedString -ElementValue $PSModuleInfo.Copyright)</copyright>
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
        
        $NupkgPath = "$NugetPackageRoot\$($PSModuleInfo.Name).$($PSModuleInfo.Version.ToString()).nupkg"
        $NuspecPath = "$NugetPackageRoot\$($PSModuleInfo.Name).nuspec"

        # Remove existing nuspec and nupkg files
        Microsoft.PowerShell.Management\Remove-Item $NupkgPath  -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
        Microsoft.PowerShell.Management\Remove-Item $NuspecPath -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
            
        Microsoft.PowerShell.Management\Set-Content -Value $nuspec -Path $NuspecPath

        # Create .nupkg file
        $output = & $script:NuGetClient pack $NuspecPath -OutputDirectory $NugetPackageRoot
        if($LASTEXITCODE)
        {
            $message = $LocalizedData.FailedToCreateCompressedModule -f ($output) 
            Write-Error -Message $message -ErrorId "FailedToCreateCompressedModule" -Category InvalidOperation
            return
        }

        # Publish the .nupkg to gallery
        $output = & $script:NuGetClient push $NupkgPath  -source $Destination -NonInteractive -ApiKey $NugetApiKey 
        if($LASTEXITCODE)
        {
            $message = $LocalizedData.FailedToPublish -f ($output) 
            Write-Error -Message $message -ErrorId "FailedToPublishTheModule" -Category InvalidOperation
        }
        else
        {
            $message = $LocalizedData.PublishedSuccessfully -f ($PSModuleInfo.Name, $Destination, $PSModuleInfo.Name)
            Write-Verbose -Message $message
        }
    }
    finally
    {
        Microsoft.PowerShell.Management\Remove-Item $NupkgPath  -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
        Microsoft.PowerShell.Management\Remove-Item $NuspecPath -Force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Confirm:$false -WhatIf:$false
    }
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
            $env:PSModulePath = Split-Path -Path $PSModuleInfo.ModuleBase -Parent

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
        [Microsoft.OneGet.MetaProvider.PowerShell.OptionCategory] 
        $category
    )

    Write-Debug ($LocalizedData.ProviderApiDebugMessage -f ('Get-DynamicOptions'))

    Write-Output -InputObject (New-DynamicOption -Category $category -Name $script:OneGetProviderParam -ExpectedType String -IsRequired $false)

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
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name "Scope" -ExpectedType String -IsRequired $false -PermittedValues @("CurrentUser","AllUsers"))
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name $script:PublishLocation -ExpectedType String -IsRequired $false)
                }

        Install 
                {
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name "Location" -ExpectedType String -IsRequired $false)
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name "InstallUpdate" -ExpectedType Switch -IsRequired $false)
                    Write-Output -InputObject (New-DynamicOption -Category $category -Name "InstallationPolicy" -ExpectedType String -IsRequired $false)
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

    if(($Name -ne $Script:PSGalleryModuleSource) -and 
       -not (Microsoft.PowerShell.Management\Test-Path $Location) -and
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

    $IsProviderSpecified = $false;
    if ($Options.ContainsKey($script:OneGetProviderParam))
    {
        $SpecifiedProviderName = $Options[$script:OneGetProviderParam] 
        
        $IsProviderSpecified = $true

        Write-Verbose ($LocalizedData.SpecifiedProviderName -f $SpecifiedProviderName)
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
        Write-Verbose ($LocalizedData.PollingOneGetProvidersForLocation -f $LocationString)

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

    # Add new module source
    $moduleSource = Microsoft.PowerShell.Utility\New-Object PSCustomObject -Property ([ordered]@{
            Name = $Name
            SourceLocation = $LocationString
            PublishLocation = $PublishLocation
            Trusted=$Trusted
            Registered= (-not $IsNewModuleSource)
            InstallationPolicy = if($Trusted) {'Trusted'} else {'Untrusted'}
            OneGetProvider = $SelectedProvider.ProviderName
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
                   -ExceptionMessage $LocalizedData.MinimumVersionAndRequiredVersionCannotBeSpecifiedTogether `
                   -ErrorId "MinimumVersionAndRequiredVersionCannotBeSpecifiedTogether" `
                   -CallerPSCmdlet $PSCmdlet `
                   -ErrorCategory InvalidArgument
    }

    if($RequiredVersion -or $MinimumVersion)
    {
        if(-not $names -or $names.Count -ne 1 -or (Test-WildcardPattern -Name $names[0]))
        {
            ThrowError -ExceptionName "System.ArgumentException" `
                       -ExceptionMessage $LocalizedData.VersionParametersAreAllowedOnlyWithSingleModule `
                       -ErrorId "VersionParametersAreAllowedOnlyWithSingleModule" `
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
                $LocationOGPHashtable[$ModuleSource.SourceLocation] = $ModuleSource.OneGetProvider
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
    elseif($options -and 
           $options.ContainsKey($script:OneGetProviderParam) -and 
           $options.ContainsKey("Location"))
    {
        $Location = $options['Location']
        $OneGetProvider = $options['OneGetProvider']

        Write-Verbose ($LocalizedData.SpecifiedLocationAndOGP -f ($Location, $OneGetProvider))

        $LocationOGPHashtable[$Location] = $OneGetProvider
    }
    else
    {
        Write-Verbose $LocalizedData.NoSourceNameIsSpecified

        $script:PSGetModuleSources.Values | Microsoft.PowerShell.Core\ForEach-Object { $LocationOGPHashtable[$_.SourceLocation] = $_.OneGetProvider }
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

    foreach($kvPair in $LocationOGPHashtable.GetEnumerator())
    {
        if($request.IsCanceled)
        {
            return
        }

        $Location = $kvPair.Key
        $ProviderName = $kvPair.Value

        Write-Verbose ($LocalizedData.GettingOneGetProviderObject -f ($ProviderName))

	    $provider = $request.SelectProvider($ProviderName)

        if(-not $provider)
        {
            Write-Error -Message ($LocalizedData.OneGetProviderIsNotAvailable -f $ProviderName)

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
        
        foreach($providerTag in $tagList)
        {
            if($request.IsCanceled)
            {
                return
            }

            if($providerTag -ne $script:NotSpecified)
            {
                $providerOptions["FilterOnTag"] = $providerTag
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

                $fastPackageReference = New-FastPackageReference -ProviderName $provider.ProviderName `
                                                                 -PackageName $pkg.Name `
                                                                 -Version $pkg.Version `
                                                                 -Source $Location

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
                                                           -OneGetProviderName $provider.ProviderName `
                                                           -SourceLocation $Location `
                                                           -IsFromTrustedSource:$FromTrustedSource
            
                    $script:FastPackRefHastable[$fastPackageReference] = $pkg

                    Write-Output -InputObject $sid
                }
            }
        }
    }
}

function Get-PackageDependencies
{ 
    param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $fastPackageReference
    )

    Set-ModuleSourcesVariable

    Write-Debug -Message ($LocalizedData.ProviderApiDebugMessage -f ('Get-PackageDependencies'))

    Write-Debug -Message ($LocalizedData.FastPackageReference -f $fastPackageReference)
    
    # take the fastPackageReference and get the package details.
    $parts = $fastPackageReference -Split '[|]'

    if( $parts.Length -eq 4 )
    {
        $providerName = $parts[0]
        $packageName = $parts[1]
        $version = $parts[2]
        $sourceLocation= $parts[3]

        $provider = $request.SelectProvider($providerName)
        if(-not $provider)
        {
            Write-Error -Message ($LocalizedData.OneGetProviderIsNotAvailable -f $providerName)

            return
        }

        if($request.IsCanceled)
        {
            return
        }

        Write-Verbose ($LocalizedData.SpecifiedLocationAndOGP -f ($provider.ProviderName, $providerName))
		
        $message = $LocalizedData.FindingModuleDependencies -f ($packageName, $version, $sourceLocation)
        Write-Verbose $message

        $pkgs = $provider.GetPackageDependencies($script:FastPackRefHastable[$fastPackageReference], (New-Request -Sources @($sourceLocation)))
        
        foreach($pkg in  $pkgs)
        {
            if($request.IsCanceled)
            {
                return
            }

            $dependentFastPackageReference = New-FastPackageReference -ProviderName $provider.ProviderName `
                                                                      -PackageName $pkg.Name `
                                                                      -Version $pkg.Version `
                                                                      -Source $sourceLocation

            $FromTrustedSource = $false

            $ModuleSourceName = Get-SourceName -Location $sourceLocation

            if($ModuleSourceName)
            {
                $FromTrustedSource = $script:PSGetModuleSources[$ModuleSourceName].Trusted
            }
            elseif($InstallationPolicy -eq "Trusted")
            {
                $FromTrustedSource = $true
            }

            $sid = New-SoftwareIdentityFromPackage -Package $pkg `
                                                    -OneGetProviderName $provider.ProviderName `
                                                    -SourceLocation $sourceLocation `
                                                    -IsFromTrustedSource:$FromTrustedSource
            
            $script:FastPackRefHastable[$dependentFastPackageReference] = $pkg

            Write-Output -InputObject $sid
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

    # take the fastPackageReference and get the package object again.
    $parts = $fastPackageReference -Split '[|]'

    if( $parts.Length -eq 4 )
    {
        $providerName = $parts[0]
        $packageName = $parts[1]
        $version = $parts[2]
        $sourceLocation= $parts[3]
        $destination = $script:programFilesModulesPath
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
                    $destination = $script:MyDocumentsModulesPath
                }
                elseif(-not (Test-RunningAsElevated) -and ($Scope -ne "CurrentUser"))
                {
                    # Throw an error when Install-Module is used as a non-admin user and '-Scope CurrentUser' is not specified
                    $message = $LocalizedData.InstallModuleNeedsCurrentUserScopeParameterForNonAdminUser -f @($script:programFilesModulesPath, $script:MyDocumentsModulesPath)

                    ThrowError -ExceptionName "System.ArgumentException" `
                                -ExceptionMessage $message `
                                -ErrorId "InstallModuleNeedsCurrentUserScopeParameterForNonAdminUser" `
                                -CallerPSCmdlet $PSCmdlet `
                                -ErrorCategory InvalidArgument
                }

                $message = $LocalizedData.ModuleDestination -f @($destination)        
                Write-Verbose $message
            }

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

        # Test if module is already installed
        $InstalledModuleInfo = Test-ModuleInstalled -Name $packageName -RequiredVersion $RequiredVersion

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
                        $message = $LocalizedData.ModuleAlreadyInstalled -f ($InstalledModuleInfo.Version, $InstalledModuleInfo.Name, $InstalledModuleInfo.ModuleBase, $InstalledModuleInfo.Version, $version)
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

        # create a temp folder and download the module
        $tempDestination = "$script:TempPath\$(Microsoft.PowerShell.Utility\Get-Random)"
        $null = Microsoft.PowerShell.Management\New-Item -Path $tempDestination -ItemType Directory -Force -Confirm:$false -WhatIf:$false

        try
        {
            $provider = $request.SelectProvider($providerName)
            if(-not $provider)
            {
                Write-Error -Message ($LocalizedData.OneGetProviderIsNotAvailable -f $providerName)

                return
            }

            if($request.IsCanceled)
            {
                return
            }

            Write-Verbose ($LocalizedData.SpecifiedLocationAndOGP -f ($provider.ProviderName, $providerName))
		
            $newRequest = New-Request -Options @{PackageSaveMode='nupkg';
                                                 Destination=$tempDestination;
                                                 SkipDependencies=$true;
                                                 ExcludeVersion=$true} `
                                      -Sources @($SourceLocation)

            $message = $LocalizedData.DownloadingModuleFromGallery -f ($packageName, $version, $sourceLocation)
            Write-Verbose $message

            $installedPkgs = $provider.InstallPackage($script:FastPackRefHastable[$fastPackageReference], $newRequest)

            foreach($pkg in $installedPkgs)
            {
                if($request.IsCanceled)
                {
                    return
                }

                $sid = New-SoftwareIdentityFromPackage -Package $pkg -SourceLocation $sourceLocation -OneGetProviderName $provider.ProviderName

                # construct the PSGetItemInfo from SoftwareIdentity and persist it
                $psgItemInfo = New-PSGetItemInfo -SoftwareIdenties $pkg -OneGetProviderName $provider.ProviderName

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

                $destinationModulePath = Microsoft.PowerShell.Management\Join-Path $destination $pkg.Name
                
                # Side-by-Side module version is avialable on PowerShell 5.0 or later versions only
                # By default, PowerShell module versions will be installed/updated Side-by-Side.
                if(Test-ModuleSxSVersionSupport)
                {
                    $destinationModulePath = Microsoft.PowerShell.Management\Join-Path $destinationModulePath $psgItemInfo.Version
                }

                # Validate the module
                if(-not (Test-ValidManifestModule -ModuleBasePath $sourceModulePath))
                {
                    $message = $LocalizedData.InvalidPSModule -f ($pkg.Name)
                    Write-Error -Message $message -ErrorId "InvalidManifestModule" -Category InvalidOperation
                    continue
                }

                # Test if module is already installed
                $InstalledModuleInfo2 = Test-ModuleInstalled -Name $pkg.Name -RequiredVersion $pkg.Version

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
                                    
                    $DependencyInstallMessage = $LocalizedData.InstallingDependencyModule -f ($pkg.Name, $pkg.Version, $packageName)
                    Write-Verbose  $DependencyInstallMessage
                }

                # check if module is in use
                if($InstalledModuleInfo2)
                {
                    $moduleInUse = Test-ModuleInUse -ModuleBasePath $InstalledModuleInfo2.ModuleBase `
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

                $message = $LocalizedData.ModuleInstalledSuccessfully -f ($psgItemInfo.Name)
                Write-Verbose $message

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

    if($parts.Length -eq 4)
    {
        $providerName = $parts[0]
        $moduleName = $parts[1]
        $version = $parts[2]
        $sourceLocation= $parts[3]

        if($request.IsCanceled)
        {
            return
        }
        
        if($options.ContainsKey('Force'))
        {
            $Force = $options['Force']
        }

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

        $moduleBase = $InstalledModuleInfo["InstalledModuleBase"]

        if(-not (Test-RunningAsElevated) -and $moduleBase.ModuleBase.StartsWith($script:programFilesModulesPath, [System.StringComparison]::OrdinalIgnoreCase))
        {                            
            $message = $LocalizedData.AdminPrivilegesRequiredForUninstall -f ($moduleName, $moduleBase)

            ThrowError -ExceptionName "System.InvalidOperationException" `
                       -ExceptionMessage $message `
                       -ErrorId "AdminPrivilegesRequiredForUninstall" `
                       -CallerPSCmdlet $PSCmdlet `
                       -ErrorCategory InvalidOperation
            
            return
        }

        $dependentModules = Microsoft.PowerShell.Core\Get-Module -ListAvailable | 
                                Microsoft.PowerShell.Core\Where-Object {
                                    $_.RequiredModules -and $_.RequiredModules.Name -contains $moduleName
                                }
        if(-not $Force -and $dependentModules)
        {
            $message = $LocalizedData.UnableToUninstallAsOtherModulesNeedThisModule -f ($moduleName, $version, $moduleBase, $($dependentModules.Name -join ','), $moduleName)

            ThrowError -ExceptionName "System.InvalidOperationException" `
                       -ExceptionMessage $message `
                       -ErrorId "UnableToUninstallAsOtherModulesNeedThisModule" `
                       -CallerPSCmdlet $PSCmdlet `
                       -ErrorCategory InvalidOperation

            return
        }

        $moduleInUse = Test-ModuleInUse -ModuleBasePath $moduleBase `
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
            Get-InstalledModuleDetails -Name $moduleName | 
                Microsoft.PowerShell.Core\ForEach-Object {
                    if($_.InstalledModuleBase.StartsWith($ModuleBaseWithoutVersion, [System.StringComparison]::OrdinalIgnoreCase))
                    {
                        $InstalledVersionsWithSameModuleBase += $_.InstalledModuleBase
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
                           -ErrorId "ModuleIsInUse" `
                           -CallerPSCmdlet $PSCmdlet `
                           -ErrorCategory InvalidOperationException

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
}

function Get-InstalledPackage
{ 
    [CmdletBinding()]
    param
    (
        [Parameter()]
        [string]
        $Name
    )

    Write-Verbose -Message ($LocalizedData.ProviderApiDebugMessage -f ('Get-InstalledPackage'))

    Get-InstalledModuleDetails -Name $Name |  Microsoft.PowerShell.Core\ForEach-Object {$_.SoftwareIdentity}
}
#endregion

#region Internal Utility functions for the OneGet Provider Implementation
function Get-InstalledModuleDetails
{ 
    [CmdletBinding()]
    param
    (
        [Parameter()]
        [string]
        $Name
    )

    Set-InstalledModulesVariable
    
    $Name = "$Name*"

    $wildcardPattern = New-Object System.Management.Automation.WildcardPattern $Name,$script:wildcardOptions

    $script:PSGetInstalledModules.GetEnumerator() | Microsoft.PowerShell.Core\ForEach-Object {
                                                        if($wildcardPattern.IsMatch($_.Key))
                                                        {
                                                            $_.Value
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
        $OneGetProviderName,

        [Parameter(Mandatory=$true)]
        [string]
        $SourceLocation,

        [Parameter()]
        [switch]
        $IsFromTrustedSource
    )

    $fastPackageReference = New-FastPackageReference -ProviderName $OneGetProviderName `
                                                     -PackageName $Package.Name `
                                                     -Version $Package.Version `
                                                     -Source $SourceLocation

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

    $details =  New-Object -TypeName  System.Collections.Hashtable
    $details.Add( "description" , (Get-First $Package["description"]) )
    $details.Add( "copyright" , (Get-First $Package["copyright"]) )
    $details.Add( "published" , (Get-First $Package["published"]) )
    $details.Add( "tags" , (Get-First $Package["tags"]) )
    $details.Add( "releaseNotes" , (Get-First $Package["releaseNotes"]) )
    $details.Add( "OneGetProvider" , $OneGetProviderName )

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
                Links = $links}

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

    $packageSourceDetails = @{}
    $packageSourceDetails["InstallationPolicy"] = $ModuleSource.InstallationPolicy
    $packageSourceDetails["OneGetProvider"] = $ModuleSource.OneGetProvider    
    $packageSourceDetails[$script:PublishLocation] = $ModuleSource.PublishLocation

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
            OneGetProvider=$PackageSource.Details['OneGetProvider']
            PublishLocation=$PackageSource.Details[$script:PublishLocation]
            ProviderOptions = @{}
        })

    $PackageSource.Details.GetEnumerator() | Microsoft.PowerShell.Core\ForEach-Object {
                                                if($_.Key -ne 'OneGetProvider' -and 
                                                   $_.Key -ne $script:PublishLocation -and
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
        $Source
    )

    return "$ProviderName|$PackageName|$Version|$Source"
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
                $psgetItemInfo = Microsoft.PowerShell.Utility\Import-Clixml -Path $PSGetItemInfoPath

                $package = New-SoftwareIdentityFromPSGetItemInfo -PSGetItemInfo $psgetItemInfo

                if($package)
                {
                    $script:PSGetInstalledModules["$($psgetItemInfo.Name)$($psgetItemInfo.Version)"] = @{
                                                                                                            SoftwareIdentity = $package
                                                                                                            PSGetItemInfo = $psgetItemInfo
                                                                                                            InstalledModuleBase = $moduleBase.FullName
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

    $fastPackageReference = New-FastPackageReference -ProviderName $psgetItemInfo.OneGetProvider `
                                                     -PackageName $psgetItemInfo.Name `
                                                     -Version $psgetItemInfo.Version `
                                                     -Source $SourceLocation

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
                    OneGetProvider = $psgetItemInfo.OneGetProvider
                 }

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

    $Exception = $null

    # Get the actual Uri from the Location
    if(-not (Microsoft.PowerShell.Management\Test-Path $LocationString))
    {
        try
        {
            $request = [System.Net.WebRequest]::Create($LocationString)
            $request.Method = 'GET'
            $response = $request.GetResponse()               
            $LocationString = $response.ResponseUri.ToString()
            $response.Close()
        }
        catch
        {
            $Exception = $_             
        }

        if($Exception)
        {
            $message = $LocalizedData.InvalidWebUri -f ($LocationString, $ParameterName)
            ThrowError -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage $message `
                        -ErrorId "InvalidWebUri" `
                        -ExceptionObject $Exception `
                        -CallerPSCmdlet $PSCmdlet `
                        -ErrorCategory InvalidArgument
        }
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

        Microsoft.PowerShell.Utility\Export-Clixml -InputObject $script:PSGetModuleSources `
                                                    -Path $script:PSGetModuleSourcesFilePath `
                                                    -Force -Confirm:$false -WhatIf:$false
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
    $availableModule = Microsoft.PowerShell.Core\Get-Module -ListAvailable -Name $Name -Verbose:$false | Microsoft.PowerShell.Utility\Select-Object -First 1
 
    if($RequiredVersion -and $availableModule -and ($availableModule.Version -ne $RequiredVersion))
    {
        $availableModule = Microsoft.PowerShell.Core\Get-Module -FullyQualifiedName @{ModuleName=$Name;RequiredVersion=$RequiredVersion} `
                                                                -ListAvailable `
                                                                -Verbose:$false `
                                                                -ErrorAction SilentlyContinue `
                                                                -WarningAction SilentlyContinue | Microsoft.PowerShell.Utility\Select-Object -First 1
    }

    return $availableModule
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
    Microsoft.PowerShell.Utility\Export-Clixml -InputObject $PSGetItemInfo -Path $psgetItemInfopath -Force -Confirm:$false -WhatIf:$false
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
        $ModuleBasePath
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
            $processes = $moduleDllsInUse | Microsoft.PowerShell.Core\Foreach-Object{$dllName = $_.ModuleName; $currentProcesses | Where-Object {$_ -and $_.Modules -and $_.Modules.ModuleName -eq $dllName} }
        
            if($processes)
            {
                $moduleName = Microsoft.PowerShell.Management\Split-Path $ModuleBasePath -Leaf

                $message = $LocalizedData.ModuleInUseWithProcessDetails -f ($moduleName, $($processes | Microsoft.PowerShell.Core\Foreach-Object{"$($_.ProcessName):$($_.Id) "}))
                Write-Error -Message $message -ErrorId "ModuleToBeUpdatedIsInUse" -Category InvalidOperation

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
        
    if((Microsoft.PowerShell.Management\Test-Path $manifestPath) -and 
       (Microsoft.PowerShell.Core\Test-ModuleManifest -Path $manifestPath -ErrorAction SilentlyContinue -WarningAction SilentlyContinue))
    {
        return $true
    }

    return $false
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

if((Test-RunningAsElevated) -and ($PSVersionTable.PSVersion -lt [Version]"4.0"))
{ 
    # Check and add the $script:ProgramFilesModulesPath to PSModulePath environment variable needed for downlevel OS versions.
    $PSModulePathCurrentEnvValue = [Environment]::GetEnvironmentVariable("PSModulePath", "Machine")
    if(($PSModulePathCurrentEnvValue -split ";") -notcontains $script:ProgramFilesModulesPath)
    {
        [Environment]::SetEnvironmentVariable("PSModulePath", "$PSModulePathCurrentEnvValue;$script:ProgramFilesModulesPath", "Machine")
    }

    # Check and add the $script:ProgramFilesModulesPath to $env:PSModulePath value for the current session
    if(($env:PSModulePath -split ";") -notcontains $script:ProgramFilesModulesPath)
    {
        $env:PSModulePath = "$env:PSModulePath;$script:ProgramFilesModulesPath"
    }
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
    $connected = Microsoft.PowerShell.Management\Test-Connection -ComputerName "www.microsoft.com" -Count 1 -Quiet
    if ( -not $connected )
    {
        return
    }


    # ping V2
    try 
    {
        $request_v2 = Microsoft.PowerShell.Utility\Invoke-WebRequest -Uri ([System.Uri]"$PSGalleryV2ApiUri") -TimeoutSec 30 -UseBasicParsing -ErrorAction SilentlyContinue
        $resolvedUri_v2 = $request_v2.BaseResponse.ResponseUri
        $statusCode_v2 = [int] $request_v2.StatusCode
    }
    catch
    {
        $resolvedUri_v2 = $_.Exception.Response.ResponseUri
        $statusCode_v2 = [int] $_.Exception.Response.StatusCode.Value__
    }


    # ping V3
    try 
    {
        $request_v3 = Microsoft.PowerShell.Utility\Invoke-WebRequest -Uri ([System.Uri]"$PSGalleryV3ApiUri") -TimeoutSec 30 -UseBasicParsing -ErrorAction SilentlyContinue
        $resolvedUri_v3 = $request_v3.BaseResponse.ResponseUri
        $statusCode_v3 = [int] $request_v3.StatusCode
    }
    catch
    {
        $resolvedUri_v3 = $_.Exception.Response.ResponseUri
        $statusCode_v3 = [int] $_.Exception.Response.StatusCode.Value__
    }

    $Script:PSGalleryV2ApiAvailable = (($statusCode_v2 -eq 200) -and ($resolvedUri_v2))
    $Script:PSGalleryV3ApiAvailable = (($statusCode_v3 -eq 200) -and ($resolvedUri_v3))
}

$null = Check-PSGalleryApiAvailability -PSGalleryV2ApiUri $Script:PSGallerySourceUri -PSGalleryV3ApiUri $Script:PSGalleryV3SourceUri


Set-Alias -Name fimo -Value Find-Module
Set-Alias -Name inmo -Value Install-Module
Set-Alias -Name upmo -Value Update-Module
Set-Alias -Name pumo -Value Publish-Module

Export-ModuleMember -Function Find-Module, `
                              Install-Module, `
                              Update-Module, `
                              Publish-Module, `
                              Uninstall-Module, `
                              Get-InstalledModule, `
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
                              Get-PackageProviderName `
                    -Alias    fimo, `
                              inmo, `
                              upmo, `
                              pumo


