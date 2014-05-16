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
    HelpInfoUri = "http://go.microsoft.com/fwlink/?linkid=392040"
    ModuleVersion = "1.0.0.0"
    PowerShellVersion = "2.0"
    ClrVersion = "4.0"
    RootModule = "Microsoft.PowerShell.OneGet.dll"
	
	##
	## Hard-coded dependency on Chocolatey Module
	## This is required only so that the Chocolatey Module can expose it's own cmdlets.
	## If it wasn't shipped with OneGet, it would have been it's own module, and this
	## wouldn't be neccessary.
	##
    NestedModules = @('chocolatey.psd1')

    # TypesToProcess = ""
    # FormatsToProcess = ""
    CmdletsToExport = @(
        'Add-PackageSource',
        'Find-Package',
        'Get-Package',
        #'Get-PackageProvider', #Not Implemented Yet.
        'Get-PackageSource',
        'Install-Package',
        'Remove-PackageSource',
        'Uninstall-Package'
	)

	FormatsToProcess  = @('OneGet.format.ps1xml')

	##
	## For now, the package provider assemblies are listed here.
	## After the CTP, there should be a less-hardcoded means
	## to installing, detecting and loading providers.
	##
   	PrivateData = @{
		"OneGetModule" = "mymodule.psm1"
		"Providers" = @{
			'Assembly' = @( 
				'OneGet.PackageProvider.Chocolatey'
				#, 'Microsoft.OneGet.MetaProvider.PowerShell' 
			)
			'Module' = @( '.\ZipProvider.psm1' )
			# 'Native'=  @( 'SomeNative.DLL' )
		 }
    }
}
