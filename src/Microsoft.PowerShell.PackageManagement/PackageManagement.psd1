###
# ==++==
#
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#
###
@{
    GUID = "4ae9fd46-338a-459c-8186-07f910774cb8"
    Author = "Microsoft Corporation"
    CompanyName = "Microsoft Corporation"
    Copyright = "(C) Microsoft Corporation. All rights reserved."
    HelpInfoUri = "https://go.microsoft.com/fwlink/?linkid=855962"
    ModuleVersion = "1.4.5"
    PowerShellVersion = "3.0"
    ClrVersion = "4.0"
    RootModule = "PackageManagement.psm1"
	Description = 'PackageManagement (a.k.a. OneGet) is a new way to discover and install software packages from around the web.
 It is a manager or multiplexor of existing package managers (also called package providers) that unifies Windows package management with a single Windows PowerShell interface. With PackageManagement, you can do the following.
  - Manage a list of software repositories in which packages can be searched, acquired and installed
  - Discover software packages
  - Seamlessly install, uninstall, and inventory packages from one or more software repositories'

    CmdletsToExport = @(
        'Find-Package',
        'Get-Package',
        'Get-PackageProvider',
        'Get-PackageSource',
        'Install-Package',
        'Import-PackageProvider'
        'Find-PackageProvider'
        'Install-PackageProvider'
        'Register-PackageSource',
        'Set-PackageSource',
        'Unregister-PackageSource',
        'Uninstall-Package'
        'Save-Package'
	)

	FormatsToProcess  = @('PackageManagement.format.ps1xml')

	PrivateData = @{
        PSData = @{
            Tags = @('PackageManagement', 'PSEdition_Core', 'PSEdition_Desktop', 'Linux', 'Mac')
            ProjectUri = 'https://oneget.org'
            ReleaseNotes = @'
## 1.4.5
- Bug fix for deadlock when getting parameters in an event

## 1.4.4
- Bug fix when installing modules from private feeds

 ## 1.4.3
- Another bug fix when registering repositories with PowerShellGet

## 1.4.2
- Bug fix for passing credentials from PowerShellGet when registering repositories

## 1.4.1
- Bug fix for using credential provider installed in Visual Studio

## 1.4
- Allow credential persistance for registering private repositories and finding or installing packages from those repositories

## 1.3.2
- Enable bootstrap on PSCore
- Bug fix to run on .NET Core 3.0

## 1.3.1
- Targets net452 and netstandard2.0 instead of net451, netcoreapp2.0, and netstandard1.6
            
## Previous releases are not included in this Changelog
'@
        }
    }
}
