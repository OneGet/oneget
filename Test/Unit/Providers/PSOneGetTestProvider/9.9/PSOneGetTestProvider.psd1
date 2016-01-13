@{
	ModuleVersion = '9.9'
	GUID = '12312312-4a6c-43c5-ba3f-619b18bbb123'
	Author = 'Microsoft Corporation'
	CompanyName = 'Microsoft Corporation'
	Copyright = '© Microsoft Corporation. All rights reserved.'
	PowerShellVersion = '3.0'
	VariablesToExport = "*"
	PrivateData = @{ 
		"PackageManagementProviders" = @(
			'OneGetTestProvider.psm1'
		)

        PSData = @{
            # Tags applied to this module. These help with module discovery in online galleries.
            Tags = 'Packagemanagement','Provider'

            # A URL to the license for this module.
            LicenseUri = 'http://oneget.org/license'

            # A URL to the main website for this project.
            ProjectUri = 'http://oneget.org/project'

            # A URL to an icon representing this module.
            IconUri = 'http://oneget.org/icon'

            # ReleaseNotes of this module
            # ReleaseNotes = ''

            # External dependent modules of this module
            # ExternalModuleDependencies = ''
        } # End of PSData hashtable        
	}
}

