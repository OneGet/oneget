function Update-Package
{
    [CmdletBinding()]
    Param
    (
        # Name of the package
        [Parameter(Mandatory=$true,
                   Position=0)]
        [string[]]
        $Name,

        # Provider associated with the package
        [Alias("Provider")]
        [string]
        $ProviderName
    )

    DynamicParam {
        $paramDictionary = New-Object System.Management.Automation.RuntimeDefinedParameterDictionary

        # Get the required dynamic parameter for install
        if (-not ([string]::IsNullOrWhiteSpace($ProviderName)))
        {
            $providerObject = Get-PackageProvider -Name $ProviderName | Select -First 1
            
            if ($null -ne $providerObject -and ($providerObject.DynamicOptions -ne $null -and $providerObject.DynamicOptions.Count -gt 0))
            {
                foreach ($option in $providerObject.DynamicOptions)
                {
                    $optionalAttribute = New-Object System.Management.Automation.ParameterAttribute
                    $optionalAttribute.Mandatory = $option.IsRequired

                    $attributes = New-Object System.Collections.ObjectModel.Collection[System.Attribute]
                    $attributes.Add($optionalAttribute)

                    $param = New-Object System.Management.Automation.RuntimeDefinedParameter($option.Name, [System.Object], $attributes)

                    $paramDictionary.Add($option.Name, $param)
                }
            }    
        }

        return $paramDictionary
    }

    Process {
        $packagesToBeUpdated = Get-Package @PSBoundParameters

        foreach ($package in $packagesToBeUpdated)
        {
            $possibleNewPackage = Find-Package -Name $package.Name -ProviderName $package.ProviderName
            $possibleNewVersion = [version]$possibleNewPackage.Version
            $version = [version]$package.Version

            if ($possibleNewVersion -gt $version)
            {
                Write-Verbose "Need to update since $possibleNewVersion is found for $($package.Name) which has version $($package.Version)"    
                $PSBoundParameters["RequiredVersion"] = $possibleNewVersion
                Install-Package @PSBoundParameters
            }
            else
            {
                Write-Verbose "$($package.Name) has the latest version $possibleNewVersion"
            } 
        }
    }
}