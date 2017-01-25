#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#  Licensed under the Apache License, Version 2.0 (the "License");
#  you may not use this file except in compliance with the License.
#  You may obtain a copy of the License at
#  http://www.apache.org/licenses/LICENSE-2.0
#
#  Unless required by applicable law or agreed to in writing, software
#  distributed under the License is distributed on an "AS IS" BASIS,
#  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#  See the License for the specific language governing permissions and
#  limitations under the License.
#
# ------------------ PackageManagement Test  ----------------------------------------------

# ------------------------------------------------------------------------------
# Actual Tests:

try {
    $WindowsPowerShell = $PSHOME.Trim('\').EndsWith('\WindowsPowerShell\v1.0', [System.StringComparison]::OrdinalIgnoreCase)    
    $Runtime = [System.Runtime.InteropServices.RuntimeInformation]
    $OSPlatform = [System.Runtime.InteropServices.OSPlatform]

    $IsCoreCLR = $true
    $IsLinux = $Runtime::IsOSPlatform($OSPlatform::Linux)
    $IsOSX = $Runtime::IsOSPlatform($OSPlatform::OSX)
    $IsWindows = $Runtime::IsOSPlatform($OSPlatform::Windows)
} catch {
    # If these are already set, then they're read-only and we're done
    try {
        $IsCoreCLR = $false
        $IsLinux = $false
        $IsOSX = $false
        $IsWindows = $true
        $WindowsPowerShell = $true
    }
    catch { }
}

$LinuxProgramFilesFolder="/usr/local/share/PackageManagement/NuGet/Packages"

$source = "http://www.nuget.org/api/v2/"
$sourceWithoutSlash = "http://www.nuget.org/api/v2"
$fwlink = "http://go.microsoft.com/fwlink/?LinkID=623861&clcid=0x409"
$longName = "THISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERS";
$workingMaximumVersions = {"2.0", "2.5", "3.0"};
$packageNames = @("AzureContrib", "AWSSDK", "TestLib");
$minimumVersions = @("1.0", "1.3", "1.5");
$maximumVersions = @("1.8", "2.1", "2.3");
$dtlgallery = "https://dtlgalleryint.cloudapp.net/api/v2/"
$providerName ="Microsoft-Windows-PowerShell"
$vstsFeed = "https://powershellgettest.pkgs.visualstudio.com/DefaultCollection/_packaging/psgettestfeed/nuget/v2"
$vstsFeedWithSlash = "https://powershellgettest.pkgs.visualstudio.com/DefaultCollection/_packaging/psgettestfeed/nuget/v2/"
#$proxyPath = "$env:tmp\ProxyConsoleProgram\Microsoft.HttpForwarder.Console.exe"
$password = ConvertTo-SecureString "4bwvgxrbzvlxc7xgv22eehlix3enmrdwblrxkirnrc3uak23naoa" -AsPlainText -Force
$vstsCredential = New-Object System.Management.Automation.PSCredential "quoct", $password
$dependenciesSource = "$PSScriptRoot\..\..\Unit\Providers\Dependencies"

$pkgSources = @("NUGETTEST101", "NUGETTEST202", "NUGETTEST303");

$nuget = "nuget"

# returns true if the test for the current nuget version should be skipped or not
# Example: if we want to skip test for any nuget version below 2.8.5.205, we will use
# SkipVersion -maxVersion 2.8.5.205
function SkipVersion([version]$minVersion,[version]$maxVersion) {
    # if min version is not null and the current nuget version is less than that, then no skip
    if ($minVersion -ne $null -and $nugetVersion -lt $minVersion) {
        return $false
    }

    if ($maxVersion -ne $null -and $nugetVersion -gt $maxVersion) {
        return $false
    }

    return $true
}

Describe "Find, Get, Save, and Install-Package with Culture" -Tags "Feature" {

    <#
    (get-packageprovider -name "OneGetTest" -list).name | should match "OneGetTest"
    $x = PowerShell '(Import-PackageProvider -name OneGetTest -RequiredVersion 9.9 -WarningAction SilentlyContinue -force ).Name'
    $x | should match "OneGetTest"
    #>
 
    it "EXPECTED: Find a package should not show Culture" -Skip:($IsCoreCLR -and (-not $IsWindows)) {
    
        $packages = Find-Package -ProviderName OneGetTest -DisplayCulture
        $packages.Culture | Should Not BeNullOrEmpty
        $packages.Name | Should Not BeNullOrEmpty
	}

    it "EXPECTED: Find a package with a DisplayCulture" -Skip:($IsCoreCLR -and (-not $IsWindows)) {
    
        $packages = Find-Package -DisplayCulture
        $packages.Culture | Should Not BeNullOrEmpty
        $packages.Name | Should Not BeNullOrEmpty
	}

    it "EXPECTED: Get a package should not show Culture" -Skip:($IsCoreCLR -and (-not $IsWindows)) {
    
        $packages = Get-Package -DisplayCulture -ProviderName OneGetTest
        $packages.Culture | Should Not BeNullOrEmpty
        $packages.Name | Should Not BeNullOrEmpty
	}

    it "EXPECTED: Install a package with a DisplayCulture" -Skip:($IsCoreCLR -and (-not $IsWindows)) {
    
        $packages = install-Package -ProviderName OneGetTest -name jquery -force -DisplayCulture
        $packages.Culture | Should Not BeNullOrEmpty
        $packages.Name | Should Not BeNullOrEmpty
	}

    it "EXPECTED: Save a package with a DisplayCulture" -Skip {
    
        $packages = save-Package -ProviderName OneGetTest -name jquery -DisplayCulture -path $destination
        $packages.Culture | Should Not BeNullOrEmpty
        $packages.Name | Should Not BeNullOrEmpty
	}
}

Describe "Event Test" -Tags "Feature" {
 
    it "EXPECTED: install a package should raise event" -Skip:(-not $WindowsPowerShell) {
     
        Install-Package EntityFramework -ProviderName nuget -requiredVersion 6.1.3  -Destination $TestDrive -source 'http://www.nuget.org/api/v2/' -force
        
        $retryCount= 5
        while($retryCount -gt 0)
        {
            $events = @(Get-WinEvent -FilterHashtable @{ ProviderName = $providerName; Id = 4101 } -ErrorAction SilentlyContinue) 

            try
            {
                if($events)
                {
                    $events[0].Message | Should Match "Package=EntityFramework"  
                    break
                }
            }
            catch
            {
            }
            $retryCount--
            Start-Sleep -Milliseconds 500
        }

        if($events)
        {         
            $event= $events[0]  
            $event.ProviderName | Should Match "Microsoft-Windows-PowerShell"
            $event.Id | Should Match 4101                     
            $event.Message | Should Match "Installed"
            $event.Message | Should Match "Package=EntityFramework"  
            $event.Message | Should Match "Version=6.1.3"  
            $event.Message | Should Match "Provider=NuGet"   
            $event.Message | Should Match "Source=http://www.nuget.org/api/v2/" 
            #$event.Message | Should Match ([regex]::Escape("DestinationPath=$env:tmp"))

        }       
        else 
        {         
            # this will fail the test
            $events | Should Not BeNullOrEmpty
        }       
               
	}

    it "EXPECTED: install a package should report destination" -Skip:(-not $WindowsPowerShell) {

        Import-PackageProvider OneGetTest -Force
        Install-Package Bla -ProviderName OneGetTest -Force
        
        $retryCount= 5
        while($retryCount -gt 0)
        {
            $events = @(Get-WinEvent -FilterHashtable @{ ProviderName = $providerName; Id = 4101 } -ErrorAction SilentlyContinue) 

            try
            {
                if($events)
                {
                    $events[0].Message | Should Match "Package=11160201-1500_amd64fre_ServerDatacenterCore_en-us.wim"  
                    break
                }
            }
            catch
            {
            }
            $retryCount--
            Start-Sleep -Milliseconds 500
        }

        if($events)
        {         
            $event= $events[0]  
            $event.ProviderName | Should Match "Microsoft-Windows-PowerShell"
            $event.Id | Should Match 4101                     
            $event.Message | Should Match "Installed"
            $event.Message | Should Match "Package=11160201-1500_amd64fre_ServerDatacenterCore_en-us.wim"  
            $event.Message | Should Match "Version=1.0.0.0"  
            $event.Message | Should Match "Provider=OneGetTest"   
            $event.Message | Should Match "Source=from a funland"
            $fullPath = [System.IO.Path]::GetFullPath($env:tmp)
            #$event.Message | Should Match ([regex]::Escape("DestinationPath=$fullPath\Test"))

        }       
        else 
        {         
            # this will fail the test
            $events | Should Not BeNullOrEmpty
        }       
               
	}

    it "EXPECTED: uninstall a package should raise event" -Skip:(-not $WindowsPowerShell) {
     
        Install-Package EntityFramework -ProviderName nuget -requiredVersion 6.1.3  -Destination $TestDrive -source 'http://www.nuget.org/api/v2/' -force 
        UnInstall-Package EntityFramework -ProviderName nuget -Destination $TestDrive

        $retryCount= 5
        while($retryCount -gt 0)
        {
            $events = @(Get-WinEvent -FilterHashtable @{ ProviderName = $providerName; Id = 4102 } -ErrorAction SilentlyContinue)  

            try
            {
                if($events)
                {
                    $events[0].Message | Should Match "Package=EntityFramework"  
                    break
                }
            }
            catch
            {
            }
            $retryCount--
            Start-Sleep -Milliseconds 500
        }

        if($events)
        {         
            $event= $events[0]
                        
            $event.ProviderName | Should Match "Microsoft-Windows-PowerShell"
            $event.Id | Should Match 4102
            $event.Message | Should Match "Uninstalled"
            $event.Message | Should Match "Package=EntityFramework"  
            $event.Message | Should Match "Version=6.1.3"  
            $event.Message | Should Match "Provider=NuGet"   
            $event.Message | Should Match "EntityFramework.6.1.3.nupkg"  

        }       
        else 
        {         
            # this will fail the test
            $events | Should Not BeNullOrEmpty
        }
               
	}

    it "EXPECTED: save a package should raise event" -Skip:(-not $WindowsPowerShell) {
     
        save-Package EntityFramework -ProviderName nuget -path $TestDrive -requiredVersion 6.1.3 -source 'http://www.nuget.org/api/v2/' -force

        $retryCount= 5
        while($retryCount -gt 0)
        {
            $events = @(Get-WinEvent -FilterHashtable @{ ProviderName = $providerName; Id = 4103} -ErrorAction SilentlyContinue)   

            try
            {
                if($events)
                {
                    $events[0].Message | Should Match "Package=EntityFramework"  
                    break
                }
            }
            catch
            {
            }
            $retryCount--
            Start-Sleep -Milliseconds 500
        }

        if($events)
        {         
            $event= $events[0]
                        
            $event.ProviderName | Should Match "Microsoft-Windows-PowerShell"
            $event.Id | Should Match 4103      
            $event.Message | Should Match "Downloaded"
            $event.Message | Should Match "Package=EntityFramework"  
            $event.Message | Should Match "Version=6.1.3"  
            $event.Message | Should Match "Provider=NuGet"   
            $event.Message | Should Match "Source=http://www.nuget.org/api/v2/"
            # $event.Message | should Match ([regex]::Escape("DestinationPath=$env:tmp"))
        }       
        else 
        {         
            # this will fail the test
            $events | Should Not BeNullOrEmpty
        }             
	}
}

Describe "Find-Package" -Tags @('Feature','SLOW'){
    it "EXPECTED: Find a package with a location created via new-psdrive" -Skip:($IsCoreCLR) {
        $Error.Clear()
        New-PSDrive -Name xx -PSProvider FileSystem -Root $TestDrive -warningaction:silentlycontinue -ea silentlycontinue > $null; find-package -name "fooobarrr" -provider nuget -source xx:\  -warningaction:silentlycontinue -ea silentlycontinue
        $ERROR[0].FullyQualifiedErrorId | should  Not Be "SourceNotFound,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackage"
        $ERROR[0].FullyQualifiedErrorId | should be "NoMatchFoundForCriteria,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackage"
	}

    It "EXPECTED: Finds 'Zlib' Package" {
        $version = "1.2.8.8"
        $expectedDependencies = @("zlib.v120.windesktop.msvcstl.dyn.rt-dyn/[1.2.8.8]", "zlib.v140.windesktop.msvcstl.dyn.rt-dyn/[1.2.8.8]")
        $zlib = find-package -name "zlib" -provider $nuget -source $source -RequiredVersion $version
        $zlib.name | should match "zlib"
        $zlib.Dependencies.Count | should be 2

        $zlib.Meta.Attributes["packageSize"] | should match "2742"

        [long]$zlib.Meta.Attributes["versionDownloadCount"] -ge 4961 | should be $true
        $zlib.Meta.Attributes["requireLicenseAcceptance"] | should match "False"
        $zlib.TagId | should match "zlib#1.2.8.8"
        
        foreach ($dep in $zlib.Dependencies) {
            $match = $false
            foreach ($expectedDependency in $expectedDependencies) {
                if ($dep.EndsWith($expectedDependency)) {
                    $match = $true
                    break
                }
            }

            $match | should be $true
        }
    }

    It "EXPECTED: Finds 100 packages should throw error" {
        $packages = Find-Package -Provider $nuget -Source $source | Select -First 100

        { Find-Package -ProviderName $nuget -Source $source -Name $packages.Name -ErrorAction Stop } | should throw
    }


    It "EXPECTED: Finds 128 packages should throw error" {
        $packages = Find-Package -Provider $nuget -Source $source | Select -First 127

        {Find-Package -ProviderName $nuget -Source $source -Name $packages.Name -ErrorAction Stop} | should throw
    }

    It "EXPECTED: Finds 'TestPackage' Package using fwlink" {
        (find-package -name "TestPackage" -provider $nuget -source $fwlink).name | should match "TestPackage"
    }

    It "EXPECTED: Finds work with dependencies loop" {
        (find-package -name "ModuleWithDependenciesLoop" -provider $nuget -source "$dependenciesSource\SimpleDependenciesLoop").name | should match "ModuleWithDependenciesLoop"
    }

    It "EXPECTED: Finds 'Zlib' Package with -IncludeDependencies" {
        $version = "1.2.8.8"
        $packages = Find-Package -Name "zlib" -ProviderName $nuget -Source $source -RequiredVersion $version -IncludeDependencies
        $packages.Count | should match 3
        $expectedPackages = @("zlib", "zlib.v120.windesktop.msvcstl.dyn.rt-dyn", "zlib.v140.windesktop.msvcstl.dyn.rt-dyn")

        foreach ($expectedPackage in $expectedPackages) {
            $match = $false
            foreach ($package in $packages) {
                # All the packages have the same version for zlib
                if ($package.Name -match $expectedPackage -and $package.Version -match $version) {
                    $match = $true
                    break
                }
            }

            $match | should be $true
        }

    }

    It "EXPECTED: Finds package with Credential" -Skip {
        $credPackage = Find-Package Contoso -Credential $vstsCredential -Source $vstsFeed -ProviderName $Nuget
        $credPackage.Count | should be 1
        $credPackage.Name | should match "Contoso"

        # find all packages should not error out
        $packages = Find-Package -Credential $vstsCredential -Source $vstsFeed -ProviderName $Nuget
        # should have at least 40 packages
        $packages.Count -ge 40 | should be $true
    }

    It "EXPECTED: Cannot find unlisted package" {
        find-package -provider $nuget -source $dtlgallery -name hellops -erroraction silentlycontinue
        $Error[0].FullyQualifiedErrorId | should be "NoMatchFoundForCriteria,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackage"
    }

    It "EXPECTED: Cannot find unlisted package with all versions parameter" {
        $packages = find-package -name gistprovider -provider $nuget -source $dtlgallery -AllVersions
        # we should still be able to find at least 2 listed package
        $packages.Count -gt 1 | should be $true
        # this version is unlisted
        $packages.Version.Contains("0.6") | should be $false
        # this version is listed
        $packages.Version.Contains("1.2") | should be $true
    }

    It "EXPECTED: Cannot find unlisted package with all versions and maximum versions" {
        $packages = find-package -name gistprovider -provider $nuget -source $dtlgallery -AllVersions -MaximumVersion 1.3
        # we should still be able to find 2 listed package (which is version 1.2 and 1.3)
        $packages.Count -eq 2 | should be $true
        # this version is unlisted
        $packages.Version.Contains("0.6") | should be $false
        # this version is listed
        $packages.Version.Contains("1.2") | should be $true
    }

    It "EXPECTED: Cannot find unlisted package with all versions and minimum versions" {
        $packages = find-package -name gistprovider -provider $nuget -source $dtlgallery -AllVersions -MinimumVersion 0.5
        # we should still be able to find at least 2 listed package (which is version 1.2 and 1.3)
        $packages.Count -gt 2 | should be $true
        # this version is unlisted
        $packages.Version.Contains("0.6") | should be $false
        # this version is listed
        $packages.Version.Contains("1.2") | should be $true
    }

    It "EXPECTED: Finds unlisted package with required version" {
        (find-package -name hellops -provider $nuget -source $dtlgallery -requiredversion 0.1.0).Name | should match "HellOps"
    }

    It "EXPECTED: Cannot find unlisted package with maximum versions" {
        # error out because all the versions below 0.6 are unlisted
        find-package -provider $nuget -source $dtlgallery -name gistprovider -maximumversion 0.6 -erroraction silentlycontinue
        $Error[0].FullyQualifiedErrorId | should be "NoMatchFoundForCriteria,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackage"
    }

    It "EXPECTED: Cannot find unlisted package with minimum versions" {
        $packages = find-package -name gistprovider -provider $nuget -source $dtlgallery -AllVersions -MinimumVersion 0.5
        # we should still be able to find at least 2 listed package (which is version 1.2 and 1.3)
        $packages.Count -gt 2 | should be $true
        # this version is unlisted
        $packages.Version.Contains("0.6") | should be $false
        # this version is listed
        $packages.Version.Contains("1.2") | should be $true
    }

    It "EXPECTED: Finds 'awssdk' package which has more than 200 versions" {
        (find-package -name "awssdk" -provider $nuget -source $source -AllVersions).Count -gt 200 | should be $true

        # Uncomment this once publish the new version of nuget
        $awssdk = Find-Package -Name "awssdk" -Provider $nuget -source $source -RequiredVersion 2.3.53
        [long]$awssdk.Meta.Attributes["downloadCount"] -ge 1023357 | should be $true
        $awssdk.Meta.Attributes["updated"] | should match "2015-12-15T17:46:22Z"
        $awssdk.TagId | should match "AWSSDK#2.3.53.0" 
    }

	It "EXPECTED: Finds A Combination Of Packages With Various Versions" {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				(find-package -name $x -source $source -provider $nuget -minimumversion $y -maximumversion $z).name | should be $x
				}
			}
		}
    }

	It "EXPECTED: Finds 'Zlib' Package After Piping The Provider" {
    	(get-packageprovider -name $nuget | find-package -name zlib -source $source ).name | should be "zlib"
    }

	It "EXPECTED: -FAILS- To Find Package Due To Too Long Of Name" {
    	{ find-package -name $longName -provider $nuget -source $source -EA Stop } | should throw
    }

	It "EXPECTED: -FAILS- To Find Package Due To Invalid Name" {
    	{find-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -provider $nuget -source $source -EA stop} | should throw
    }

	It "EXPECTED: -FAILS- To Find Package Due To Negative Maximum Version Parameter" {
    	{find-package -name "zlib" -provider $nuget -source $source -maximumversion "-1.5" -EA stop} | should throw
    }

	It "EXPECTED: -FAILS- To Find Package Due To Negative Minimum Version Parameter" {
    	{find-package -name "zlib" -provider $nuget -source $source -minimumversion "-1.5" -EA stop} | should throw
    }

	It "EXPECTED: -FAILS- To Find Package Due To Negative Required Version Parameter" {
    	{find-package -name "zlib" -provider $nuget -source $source -requiredversion "-1.5" -EA stop } | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Out Of Bounds Required Version Parameter" {
	    {find-package -name "zlib" -provider $nuget -source $source -minimumversion "1.0" -maximumversion "1.5" -requiredversion "2.0" -EA stop} | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
	    {find-package -name "zlib" -provider $nuget -source $source -minimumversion "1.5" -maximumversion "1.0" -EA stop} | should throw
    }

    It "EXPECTED: -FAILS- Find-Package with wrong source should not error out about dynamic parameter" {
        $Error.Clear()
        find-package -source WrongSource -name zlib -provider nuget -Contains PackageManagement -ErrorVariable errorVar -ErrorAction SilentlyContinue 
        $errorVar.FullyQualifiedErrorId | should be "SourceNotFound,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackage"
    }

    It "EXPECTED: -FAILS- Find-Package with wrong source and wrong dynamic parameter" {
        $Error.Clear()
        find-package -source WrongSource -name zlib -provider nuget -WrongDynamicParameter PackageManagement -ErrorVariable errorVar -ErrorAction SilentlyContinue        
        $errorVar.FullyQualifiedErrorId | should be "SourceNotFound,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackage"
    }
}

Describe "Save-Package" -Tags "Feature" {
	# make sure packagemanagement is loaded
    BeforeEach{
        if(-not (test-path $TestDrive) ) {
               if($IsWindows) { mkdir $TestDrive -ea silentlycontinue}
               else{mkdir $TestDrive -p}
        }

        $destination = $TestDrive
    }
   

    It "EXPECTED success: save-package path should be created with -force " {
        $dest = "$destination\NeverEverExists"
        $package = save-package -name TSDProvider -path $dest -source $dtlgallery -provider $nuget -force
       
        $package.Name | should be "TSDProvider"
        (test-path "$dest\TSDProvider*") | should be $true

        # now let's run save-package from the local repository
        $dest2 = "$destination\NeverEverExists2"
        $package2 = save-package -name TSDProvider -path $dest2 -source $dest -provider $nuget -force
        $package2.Name | should be "TSDProvider"
        (test-path "$dest2\TSDProvider*") | should be $true

        if (test-path "$dest2") {
            Remove-Item $dest2 -force -Recurse
        }
        if (test-path "$dest\TSDProvider*") {
            Remove-Item $dest\TSDProvider* -force -Recurse
        }
        if (test-path "$dest") {
            Remove-Item $dest -force -Recurse
        }
    }

    It "EXPECTED success: save-package -LiteralPath" {
        
        $package = save-package -LiteralPath $destination -ProviderName nuget -Source $dtlgallery -name TSDProvider
       
        $package.Name | should be "TSDProvider"
        (test-path "$destination\TSDProvider*") | should be $true
        if (test-path "$destination\TSDProvider*") {
            Remove-Item $destination\TSDProvider* -force -Recurse
        }
    }

    It "EXPECTED success: save-package -LiteralPath2" {
        if($IsWindows)
        {
            $dest = "$destination\NeverEverExists"
        }
        else
        {
            $dest = "$destination/NeverEverExists"
        }
        $package = save-package -LiteralPath $dest -ProviderName nuget -Source $dtlgallery -name TSDProvider -force
       
        $package.Name | should be "TSDProvider"
        (test-path "$dest\TSDProvider*") | should be $true
        if (test-path "$dest\TSDProvider*") {
            Remove-Item $dest\TSDProvider* -force -Recurse
        }
        if (test-path "$dest") {
            Remove-Item $dest -force -Recurse
        }
    }

    It "EXPECTED success: find-package and save-package" {
        $package = find-package -name TSDProvider -provider $nuget -source $dtlgallery | save-package -path $destination
       
        $package.Name | should be "TSDProvider"
        (test-path "$destination\TSDProvider*") | should be $true
        if (test-path "$destination\TSDProvider*") {
            Remove-Item $destination\TSDProvider* -force -Recurse
        }
    }

    It "save-package -name with wildcards, Expect error" {
        $Error.Clear()
        $package =  save-package -path $destination -name DOESNOTEXIST* -warningaction:silentlycontinue -ErrorVariable wildcardError -ErrorAction SilentlyContinue        
        $wildcardError.FullyQualifiedErrorId | should be "WildCardCharsAreNotSupported,Microsoft.PowerShell.PackageManagement.Cmdlets.SavePackage"
    }

	it "EXPECTED: Saves 'Zlib' Package To Packages Directory" -Skip:($IsCoreCLR) {
        $version = "1.2.8.8"
        $expectedPackages = @("zlib", "zlib.v120.windesktop.msvcstl.dyn.rt-dyn", "zlib.v140.windesktop.msvcstl.dyn.rt-dyn")
        $newDestination = Join-Path $TestDrive "nugetinstallation"
		
        try {
            $packages = Save-Package -Name "zlib" -ProviderName $nuget -Source $source -RequiredVersion $version -Path $destination
            $packages.Count | should match 3
            
            foreach ($expectedPackage in $expectedPackages) {
                #each of the expected package should be there
                Test-Path "$destination\$expectedPackage*" | should be $true

                $match = $false
                foreach ($package in $packages) {
                    # All the packages have the same version for zlib
                    if ($package.Name -match $expectedPackage -and $package.Version -match $version) {
                        $match = $true
                        break
                    }
                }

                $match | should be $true
            }        
           
            if(-not (test-path $newDestination) ) {
               if($IsWindows) { mkdir $newDestination -ea silentlycontinue}
               else{mkdir $newDestination -p}
            }

            # make sure we can install the package. To do this, we need to save the dependencies first
            (save-package -name "zlib.v120.windesktop.msvcstl.dyn.rt-dyn" -RequiredVersion $version -provider $nuget -source $source -path $destination)
            (save-package -name "zlib.v140.windesktop.msvcstl.dyn.rt-dyn" -RequiredVersion $version -provider $nuget -source $source -path $destination)

		    (install-package -name "zlib" -provider $nuget -source $destination -destination $newDestination -force -RequiredVersion $version)
		    (test-path "$newDestination\zlib.1.2*") | should be $true

            # Test that we have the nupkg file
            (test-path "$newDestination\zlib.1.2*\zlib*.nupkg") | should be $true
            # Test that dependencies are installed
            (test-path "$newDestination\zlib.v120*") | should be $true
            (test-path "$newDestination\zlib.v120*\zlib.v120*.nupkg") | should be $true
            (test-path "$newDestination\zlib.v140*") | should be $true
            (test-path "$newDestination\zlib.v140*\zlib.v140*.nupkg") | should be $true
        }
        finally {
            if (Test-Path $newDestination) {
                Remove-Item -Recurse -Force -Path $newDestination -ErrorAction SilentlyContinue -WarningAction SilentlyContinue 
            }

		    if (Test-Path $destination\zlib*) {
			    Remove-Item $destination\zlib* -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Force -Recurse
		    }

        }
    }

    it "EXPECTED: Saves 'Zlib' Package to Packages Directory and install it without dependencies" {
        $version = "1.2.8.8"
        $newDestination = "$TestDrive\newdestination\nugetinstallation"

        try {
		    (save-package -name "zlib" -provider $nuget -source $source -Path $destination -RequiredVersion $version)
		    (test-path $destination\zlib*) | should be $true
            remove-item $destination\zlib.v1* -force -Recurse -ErrorAction SilentlyContinue 

            $Error.Clear()
            install-package -name zlib -provider $nuget -source $destination -destination $newDestination -force -RequiredVersion $version -ErrorAction SilentlyContinue
            $Error[0].FullyQualifiedErrorId | should match "UnableToFindDependencyPackage,Microsoft.PowerShell.PackageManagement.Cmdlets.InstallPackage"
            (Test-Path "$newDestination\zlib*") | should be $false
        }
        finally {
            if (Test-Path $newDestination) {
                Remove-Item -Recurse -Force -Path $newDestination
            }

		    if (Test-Path $destination\zlib*) {
			    Remove-Item $destination\zlib* -Force -Recurse
		    }

        }
    }

    It "EXPECTED: Saves work with dependencies loop" -Skip:($IsCoreCLR) {
        try {
            $msg = powershell "save-package -name ModuleWithDependenciesLoop -provider $nuget -source `"$dependenciesSource\SimpleDependenciesLoop`" -path $destination -ErrorAction SilentlyContinue -WarningAction SilentlyContinue; `$Error[0].FullyQualifiedErrorId"
            $msg | should match "ProviderFailToDownloadFile,Microsoft.PowerShell.PackageManagement.Cmdlets.SavePackage"
            (Test-Path $destination\ModuleWithDependenciesLoop*) | should be $false
        }
        finally {
            if (Test-Path $destination\ModuleWithDependenciesLoop*) {
                Remove-Item $destination\ModuleWithDependenciesLoop* -Force -Recurse
            }
        }
    }

    It "EXPECTED: Saves 'TestPackage' Package using fwlink" {
        try {
            (save-package -name "TestPackage" -provider $nuget -source $fwlink -Path $Destination)
            (Test-Path $destination\TestPackage*) | should be $true 
        }
        finally {
            if (Test-Path $destination\TestPackage*) {
                Remove-Item $destination\TestPackage* -Force -Recurse
            }
        }
    }

    It "EXPECTED: Saves 'awssdk' package which has more than 200 versions" -Skip {
		(save-package -name "awssdk" -provider $nuget -source $source -Path $destination -verbose)
		(test-path $destination\AWSSDK*) | should be $true
		if (Test-Path $destination\AWSSDK*) {
			Remove-Item $destination\AWSSDK* -Force -Recurse
		}    
    }

    It "EXPECTED: Saves package with Credential" -Skip {
        #TODO: Need to fix this. Already opened an issue on GitHub
        Save-Package Contoso -Credential $vstsCredential -Source $vstsFeed -ProviderName $Nuget -Path $destination
        (Test-Path $destination\Contoso*) | should be $true

        if (Test-Path $destination\Contoso*) {
            Remove-Item $destination\Contoso* -Force -Recurse
        }
    }

	it "EXPECTED: Saves Various Packages With Various Version Parameters To Packages Directory" -Skip {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				save-package -name $x -source $source -provider $nuget -minimumversion $y -maximumversion $z -Path $destination -force -verbose
				(Test-Path -Path $destination\$x*) | should be $true
				if (Test-Path -Path $destination\$x*) {
					Remove-Item $destination\$x* -Force -Recurse -verbose
					}
				}
			}
		}
	}

	It "EXPECTED: Saves 'Zlib' Package After Having The Provider Piped" {
	    (find-package -name "zlib" -provider $nuget -source $source | save-package -Path $destination)
	    (Test-Path -Path $destination\zlib*) | should be $true
	    if (Test-Path -Path $destination\zlib*) {
		    Remove-Item $destination\zlib* -Force -Recurse
        }
    }

	It "EXPECTED: -FAILS- To Save Package Due To Too Long Of Name" {
    	{save-package -name $longName -provider $nuget -source $source -Path $destination -EA stop} | should throw
	}

	It "EXPECTED: -FAILS- To Save Package Due To Invalid Name" {
    	{save-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -provider $nuget -source $source -Path $destination -EA stop} | should throw
    }

    It "EXPECTED: -FAILS- To Save Package without folder pre-created" {
    	{save-package -name Jquery -provider $nuget -source $source -Path "$destination\SavePackageTest\FolderDoesNotExist" -EA stop} | should throw
    }

    It "EXPECTED: -FAILS- To Save Package -LiteralPath without folder pre-created" {
    	{save-package -name Jquery -provider $nuget -source $source -LiteralPath "$destination\SavePackageTest\FolderDoesNotExist" -EA stop} | should throw
    }

	It "EXPECTED: -FAILS- To Save Package Due To Negative Maximum Version Parameter" {
    	{save-package -name "zlib" -provider $nuget -source $source -maximumversion "-1.5" -Path $destination -EA stop} | should throw
    }

	It "EXPECTED: -FAILS- To Save Package Due To Negative Minimum Version Parameter" {
    	{save-package -name "zlib" -provider $nuget -source $source -minimumversion "-1.5" -Path $destination -EA stop} | should throw
    }

	It "EXPECTED: -FAILS- To Save Package Due To Negative Required Version Parameter" {
    	{save-package -name "zlib" -provider $nuget -source $source -requiredversion "-1.5" -Path $destination -EA stop} | should throw
    }

	It "EXPECTED: -FAILS- To Save Package Due To Out Of Bounds Required Version Parameter" {
    	{save-package -name "zlib" -provider $nuget -source $source -minimumversion "1.0" -maximumversion "1.5" -requiredversion "2.0" -Path $destination -EA stop} | should throw
    }

	It "EXPECTED: -FAILS- To Save Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
    	{save-package -name "zlib" -provider $nuget -source $source -minimumversion "1.5" -maximumversion "1.0" -Path $destination -EA stop} | should throw
    }
}

Describe "save-package with Whatif" -Tags "Feature" {
    # make sure that packagemanagement is loaded
    #import-packagemanagement
    $tempDir = Join-Path $TestDrive "nugettesttempfolder"    

    BeforeEach{
        $tempFile = [System.IO.Path]::GetTempFileName() 
        $whatif = "What if: Performing the operation";

        if (-not (Test-Path $tempDir))
        {
            new-item -type directory $tempDir | Out-Null
        }
    }

    AfterEach {
        if(Test-Path $tempFile)
        {
            Remove-Item $tempFile -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Force
        }
    }

     It "install-package -name nuget with whatif, Expect succeed" {
       if($PSCulture -eq 'en-US'){
        # Start Transcript
        Start-Transcript -Path $tempFile
		
        Save-Package -name jquery -force -source $source -ProviderName NuGet -Path $tempDir -warningaction:silentlycontinue -ErrorAction SilentlyContinue -whatif  

        # Stop Transcript and get content of transcript file
        Stop-Transcript
        $transcriptContent = Get-Content $tempFile

        $transcriptContent | where { $_.Contains( $whatif ) } | should be $true
        Test-Path C:\foof | should be $false


        Remove-Item $whatif -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Force
        }
    }

     It "install-package -name nuget with whatif where package has a dependencies, Expect succeed" {
        {Save-Package -name zlib -source https://www.nuget.org/api/v2/ `
            -ProviderName NuGet -Path $tempDir -whatif} | should not throw
    }
}


Describe "install-package with Whatif" -Tags "Feature" {

    BeforeEach{
        if(-not (test-path $TestDrive) ) {
               if($IsWindows) { mkdir $TestDrive -ea silentlycontinue}
               else{mkdir $TestDrive -p}
        }
        $installationPath = Join-Path $TestDrive "InstallationPath"

        $tempFile = [System.IO.Path]::GetTempFileName() 
        $whatif = "What if: Performing the operation";
        Remove-Item c:\foof -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Force
    }

    AfterEach {
        if(Test-Path $tempFile)
        {
            Remove-Item $tempFile -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Force
        }
    }

     It "install-package -name nuget with whatif, Expect succeed" {

        if($PSCulture -eq 'en-US'){
            # Start Transcript
            Start-Transcript -Path $tempFile
		
            install-Package -name jquery -force -source $source -ProviderName NuGet -destination $installationPath -warningaction:silentlycontinue -ErrorAction SilentlyContinue -whatif  

            # Stop Transcript and get content of transcript file
            Stop-Transcript
            $transcriptContent = Get-Content $tempFile

            $transcriptContent | where { $_.Contains( $whatif ) } | should be $true
            Test-Path $installationPath | should be $false

            Remove-Item $whatif -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Force
        }
    }

     It "install-package -name nuget with whatif where package has a dependencies, Expect succeed" {
        {install-Package -name zlib -source https://www.nuget.org/api/v2/ `
            -ProviderName NuGet -destination $installationPath -whatif} | should not throw
    }
}

Describe "Install-Package dependencies" -Tags "Feature" {
    $tempDir = Join-Path $TestDrive "nugettesttempfolder"    

    AfterEach {
        Remove-Item "$tempDir\*" -Force -Recurse -ErrorAction SilentlyContinue
    }

    It "Skip dependencies when using -SkipDependencies" {
	$pkg = Get-PackageProvider -Name NuGet
	if($pkg.Version -le "2.8.5.205") { return }

        $version = "1.2.8.8"
        $zlib = Install-Package -Provider $nuget -Source $source -Destination $tempDir -SkipDependencies -Force zlib

        $zlib.Count | should be 1
        (test-path "$tempDir\zlib*") | should be $true
        (test-path "$tempDir\zlib.v120*") | should be $false
        (test-path "$tempDir\zlib.v140*") | should be $false
    }

    It "Install latest stable version for dependencies instead of prerelease one" -Skip {
        $version = "1.4.1"
        $nancy = Install-Package -Provider $nuget -Source $source -Destination $tempDir -Force Nancy.Hosting.Self -RequiredVersion $version

        $nancy.Count | should be 2
        (Test-Path "$tempDir\Nancy*") | should be $true
        (Test-Path "$tempDir\Nancy.Hosting.Self*") | should be $true
        (Test-Path "$tempDir\Nancy.Hosting.Self*barney*") | should be $false
    }
}

Describe "install-package with Scope" -tags "Feature" {


    BeforeAll {
        if ($IsWindows)
        {
            $userName = "smartguy"
            $password = "password%1"
            #net user $userName /delete | Out-Null
            net user $userName $password /add
            $secesurestring = ConvertTo-SecureString $password -AsPlainText -Force
            $credential = new-object -typename System.Management.Automation.PSCredential -argumentlist $userName, $secesurestring
        }
    }


    AfterAll {
        if ($IsWindows)
        {
             # Delete the user profile
             net user $userName /delete | Out-Null        
        }
    }  

     it "EXPECTED Success: Get and Install-Package without Scope without destination" {
            
        if ($IsWindows)
        {
            $ProgramFiles = [System.Environment]::GetEnvironmentVariable("ProgramFiles")
            $UserInstalledLocation = "$($ProgramFiles)\PackageManagement\Nuget\Packages"
        }
        else
        {
            $UserInstalledLocation = $LinuxProgramFilesFolder
        }
        
        if (Test-Path $UserInstalledLocation) {
                Remove-Item -Recurse -Force -Path $UserInstalledLocation -ErrorAction SilentlyContinue
        }

        $package = install-package -ProviderName nuget  -source  $dtlgallery -name  gistprovider -RequiredVersion 0.6 -force -verbose
    
        $package.Name | Should Match "GistProvider"

        $packages = Get-package -ProviderName nuget 

        $packages | ?{ $_.Name -eq "GistProvider" } | should not BeNullOrEmpty
   
        (Test-Path "$UserInstalledLocation\GistProvider*" -Verbose) | should be $true
	}

    it "EXPECTED Success: Get and Install-Package AllUsers Scope Without destination" {
            
        if ($IsWindows)
        {
            $ProgramFiles = [System.Environment]::GetEnvironmentVariable("ProgramFiles")
            $UserInstalledLocation = "$($ProgramFiles)\PackageManagement\Nuget\Packages"
        }
        else
        {
            $UserInstalledLocation = $LinuxProgramFilesFolder
        }
        
        if (Test-Path $UserInstalledLocation) {
                Remove-Item -Recurse -Force -Path $UserInstalledLocation -ErrorAction SilentlyContinue
        }

        $package = install-package -ProviderName nuget  -source  $dtlgallery -name  gistprovider -RequiredVersion 0.6 -scope AllUsers -force
    
        $package.Name | Should Match "GistProvider"

        $packages = Get-package -ProviderName nuget 

        $packages | ?{ $_.Name -eq "GistProvider" } | should not BeNullOrEmpty
   
        (Test-Path "$UserInstalledLocation\GistProvider*") | should be $true
	}

    it "EXPECTED Success: Get and Install-Package -Scope CurrentUser with destination" {
            
        if ($IsWindows)
        {
            $userProfile = [System.Environment]::GetEnvironmentVariable("LocalApplicationData")
            $UserInstalledLocation = "$($userProfile)\PackageManagement\Nuget\Packages"
        }
        else
        {
            $UserInstalledLocation = "$HOME\.local\share\PackageManagement\NuGet\Packages"
        }
        
        if (Test-Path $UserInstalledLocation) {
                Remove-Item -Recurse -Force -Path $UserInstalledLocation -ErrorAction SilentlyContinue
        }

        $package = install-package -ProviderName nuget  -source  $dtlgallery -name  gistprovider -RequiredVersion 0.6 -scope CurrentUser -destination $UserInstalledLocation -force
    
        $package.Name | Should Match "GistProvider"

        $packages = Get-package -ProviderName nuget 

        $packages | ?{ $_.Name -eq "GistProvider" } | should not BeNullOrEmpty
   
        (Test-Path "$UserInstalledLocation\GistProvider*") | should be $true
	}
        
    # Start job not working yet

    It "install-package CurrentUser scope in a non-admin console, expect succeed" -Skip:($IsCoreCLR){
        $Error.Clear()                             
        $job=Start-Job -ScriptBlock {Param ([Parameter(Mandatory = $True)] [string]$dtlgallery) install-package -ProviderName nuget  -source $dtlgallery -name  gistprovider -RequiredVersion 0.6 -force -scope CurrentUser} -Credential $credential -ArgumentList $dtlgallery

        $a= Receive-Job -Wait -Job $job
        $a.Name | should match 'gistprovider'
    } 

    It "install-package without scope in a non-admin console, expect fail" -Skip:($IsCoreCLR) {
       
        $Error.Clear()
                      
        $job=Start-Job -ScriptBlock {
             install-package -ProviderName nuget  -source  http://nuget.org/api/v2 -name  jquery -force
            } -Credential $credential 

        Receive-Job -Wait -Job $job -ErrorVariable theError 2>&1
        $theError.FullyQualifiedErrorId | should be "InstallRequiresCurrentUserScopeParameterForNonAdminUser,Microsoft.PowerShell.PackageManagement.Cmdlets.InstallPackage"
    } 

    It "install-package with AllUsers scope in a non-admin console, expect fail" -Skip:($IsCoreCLR){
        $Error.Clear()
                      
        $job=Start-Job -ScriptBlock {install-package -ProviderName nuget  -source  http://nuget.org/api/v2 -name  jquery -force -scope AllUsers} -Credential $credential

        Receive-Job -Wait -Job $job -ErrorVariable theError2 2>&1
        $theError2.FullyQualifiedErrorId | should be "InstallRequiresCurrentUserScopeParameterForNonAdminUser,Microsoft.PowerShell.PackageManagement.Cmdlets.InstallPackage"
    }
}

Describe Install-Package -Tags "Feature" {
    $destination = Join-Path $TestDrive "NugetPackages"
    $relativetestpath = Join-Path $TestDrive "RelativeTestPath"
 
	it "EXPECTED: Installs 'Zlib' Package To Packages Directory" {
        $version = "1.2.8.8"
		(install-package -name "zlib" -provider $nuget -source $source -destination $destination -force -RequiredVersion $version)
		(test-path $destination\zlib.1.2*) | should be $true
        # Test that dependencies are installed
        (test-path $destination\zlib.v120*) | should be $true
        (test-path $destination\zlib.v140*) | should be $true
		if (Test-Path $destination\zlib*) {
			(Remove-Item -Recurse -Force -Path $destination\zlib*)
		}
    }

    It "EXPECTED: Install package with credential" -Skip {
        try {
            Install-Package -Name Contoso -Provider $nuget -Source $vstsFeed -Credential $vstsCredential -Destination $destination -Force
            Test-Path $destination\Contoso* | should be $true
        }
        finally {
            if (Test-Path $destination\Contoso*) {
                Remove-Item -Recurse -Force -Path $destination\Contoso*
            }
        }
    }

    It "install-package -name with wildcards, Expect error" {
        $Error.Clear()
        install-Package -name gist* -force -source $source -warningaction:silentlycontinue -ErrorVariable wildcardError -ErrorAction SilentlyContinue        
        $wildcardError.FullyQualifiedErrorId| should be "WildCardCharsAreNotSupported,Microsoft.PowerShell.PackageManagement.Cmdlets.InstallPackage"
    }

    It "EXPECTED: Install as few dependencies as possible 1" {
	$pkg = Get-PackageProvider -Name NuGet
	if($pkg.Version -le "2.8.5.205") { return }
        try {
            Install-Package -Name TestModuleWithDependencyA -Provider $Nuget -source $dtlgallery -Destination $destination -Force -RequiredVersion 1.0
            (Test-Path $destination\TestModuleWithDependencyA.1.0) | should be $true
            (Test-Path $destination\TestModuleWithDependencyB.1.0) | should be $true
            # should not install B
            (Test-Path $destination\TestModuleWithDependencyB.2.0) | should be $false
            (Test-Path $destination\TestModuleWithDependencyC.1.0) | should be $true
            (Test-Path $destination\TestModuleWithDependencyD.1.0) | should be $true
        }
        finally {
            if (Test-Path $destination\TestModuleWithDependency*) {
                Remove-Item -Recurse -Force -Path $destination\TestModuleWithDependency*
            }
        }
    }

    It "EXPECTED: Install as few dependencies as possible 2" {
	$pkg = Get-PackageProvider -Name NuGet
	if($pkg.Version -le "2.8.5.205") { return }
        try {
            Install-Package -Name TestModuleWithDependencyA -Provider $Nuget -source $dtlgallery -Destination $destination -Force -RequiredVersion 3.0
            (Test-Path $destination\TestModuleWithDependencyA.3.0) | should be $true
            (Test-Path $destination\TestModuleWithDependencyB.2.0) | should be $true
            (Test-Path $destination\TestModuleWithDependencyE.1.0) | should be $true
            (Test-Path $destination\TestModuleWithDependencyC.1.0) | should be $true
            (Test-Path $destination\TestModuleWithDependencyD.1.0) | should be $true
            # should not install f
            (Test-Path $destination\TestModuleWithDependencyF.1.0) | should be $false
            # should not install version 3.0 of c
            (Test-Path $destination\TestModuleWithDependencyF.3.0) | should be $false
        }
        finally {
            if (Test-Path $destination\TestModuleWithDependency*) {
                Remove-Item -Recurse -Force -Path $destination\TestModuleWithDependency*
            }
        }
    }

    it "EXPECTED: Installs package should decode percent-encoding string" -Skip:($IsCoreCLR){
        # Tab has a ++ folder
        try {
            Install-Package -Name Tab -RequiredVersion 1.0 -Source $dtlgallery -ProviderName NuGet -Destination $destination -Force
            Test-Path "$destination\Tab.1.0.0.0\New folder ++" | should be $true
        }
        finally {
            if (Test-Path $destination\Tab*) {
                Remove-Item -Recurse -Force -Path $destination\Tab*
            }
        }
    }

    it "EXPECTED: Fails to install a module with simple dependencies loop" -Skip:($IsCoreCLR){
         $msg = powershell "Install-Package ModuleWithDependenciesLoop -ProviderName nuget -Source `"$dependenciesSource\SimpleDependenciesLoop`" -Destination $destination -ErrorAction SilentlyContinue; `$Error[0].FullyQualifiedErrorId"
         $msg | should be "DependencyLoopDetected,Microsoft.PowerShell.PackageManagement.Cmdlets.InstallPackage"

    }

    it "EXPECTED: Fails to install a module with a big dependencies loop" -Skip:($IsCoreCLR){
        $msg = powershell "Install-Package ModuleA -ProviderName nuget -source `"$dependenciesSource\BigDependenciesLoop`" -Destination $destination -ErrorAction SilentlyContinue;`$Error[0].FullyQualifiedErrorId"
        $msg | should be "DependencyLoopDetected,Microsoft.PowerShell.PackageManagement.Cmdlets.InstallPackage"
    }

    It "EXPECTED: Installs to existing relative path" {
        $oldPath = $PSScriptRoot

        try {
            if (-not (Test-Path $relativetestpath)) {
                md $relativetestpath
            }

            cd $relativetestpath

            $subdir = ".\subdir"

            if (-not (Test-Path $subdir)) {
                md $subdir
            }

            (install-package -Name "TestPackage" -ProviderName $nuget -Source $fwlink -Destination $subdir -Force)
            (Test-Path "$subdir\TestPackage*") | should be $true
        }
        finally {
            cd $oldPath
            if (Test-Path $relativetestpath) {
                Remove-Item -Recurse -Force -Path $relativetestpath -ErrorAction SilentlyContinue
            }
        }
    }

    It "EXPECTED: Installs to non existing relative path" {
        $oldPath = $PSScriptRoot

        try {
            if (-not (Test-Path $relativetestpath)) {
                md $relativetestpath
            }

            cd $relativetestpath

            $subdir = ".\subdir\subdir"

            if (Test-Path $subdir) {
                Remove-Item -Recurse -Force -Path $subdir
            }

            (install-package -Name "TestPackage" -ProviderName $nuget -Source $fwlink -Destination $subdir -Force)
            (Test-Path "$subdir\TestPackage*") | should be $true
        }
        finally {
            cd $oldPath
            if (Test-Path $relativetestpath) {
                Remove-Item -Recurse -Force -Path $relativetestpath -ErrorAction SilentlyContinue
            }
        }
    }

    It "EXPECTED: Installs 'TestPackage' Package using fwlink" {
        try {
            (install-package -name "TestPackage" -provider $nuget -source $fwlink -Destination $Destination -force)
            (Test-Path $destination\TestPackage*) | should be $true 
        }
        finally {
            if (Test-Path $destination\TestPackage*) {
                Remove-Item -Recurse -Force $destination\TestPackage*
            }
        }
    }

	it "EXPECTED: Installs 'awssdk' Package which has more than 200 versions To Packages Directory" -Skip {
		(install-package -name "awssdk" -provider $nuget -source $source -destination $destination -maximumversion 2.3 -force -Verbose)
		(test-path $destination\awssdk*) | should be $true
		if (Test-Path $destination\awssdk*) {
			(Remove-Item -Recurse -Force -Path $destination\awssdk* -Verbose)
		}
    }

	it "EXPECTED: Installs Various Packages With Various Version Parameters To Packages Directory" -Skip {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				(install-package -name $x -source $source -provider $nuget -minimumversion $y -maximumversion $z -destination $destination -force -Verbose)
				(Test-Path -Path $destination\$x*) | should be $true
				if (Test-Path -Path $destination\$x*) {
					(Remove-Item -Recurse -Force -Path $destination\$x* -ErrorAction SilentlyContinue -Verbose)
					}
				}
			}
		}
    }

	It "EXPECTED: Installs 'Zlib' Package After Having The Provider Piped" {
	    (find-package -name "zlib" -provider $nuget -source $source | install-package -destination $destination -force -Verbose)
	    (Test-Path -Path $destination\zlib*) | should be $true
	    if (Test-Path -Path $destination\zlib*) {
		    (Remove-Item -Recurse -Force -Path $destination\zlib* -ErrorAction SilentlyContinue)
		    }
	    }

	It "EXPECTED: -FAILS- To Install Package Due To Too Long Of Name" {
    	{install-package -name $longName -provider $nuget -source $source -destination $destination -force -EA stop} | should throw
	}

	It "EXPECTED: -FAILS- To Install  Package Due To Invalid Name" {
    	{install-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -provider $nuget -source $source -destination $destination -force -EA stop} | should throw
    }

	It "EXPECTED: -FAILS- To Install Package Due To Negative Maximum Version Parameter" {
    	{install-package -name "zlib" -provider $nuget -source $source -maximumversion "-1.5" -destination $destination -force -EA stop} | should throw
    }

	It "EXPECTED: -FAILS- To Install Package Due To Negative Maximum Version Parameter" {
    	{install-package -name "zlib" -provider $nuget -source $source -minimumversion "-1.5" -destination $destination -force -EA stop} | should throw
    }

	It "EXPECTED: -FAILS- To Install Package Due To Negative Maximum Version Parameter" {
    	{install-package -name "zlib" -provider $nuget -source $source -requiredversion "-1.5" -destination $destination -force -EA stop} | should throw
    }

	It "EXPECTED: -FAILS- To Install Package Due To Out Of Bounds Required Version Parameter" {
    	{install-package -name "zlib" -provider $nuget -source $source -minimumversion "1.0" -maximumversion "1.5" -requiredversion "2.0" -destination $destination -force -EA stop} | should throw
	}

	It "EXPECTED: -FAILS- To Install Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
    	{install-package -name "zlib" -provider $nuget -source $source -minimumversion "1.5" -maximumversion "1.0" -destination $destination -force -EA stop} | should throw
	}
}

Describe Get-Package -Tags "Feature" {
    $destination = Join-Path $TestDrive "NuGetPackages"

	it "EXPECTED: Gets The 'Adept.NugetRunner' Package After Installing" {
		(install-package -name "adept.nugetrunner" -provider $nuget -source $source -destination $destination -force)
		(get-package -name "adept.nugetrunner" -provider $nuget -destination $destination).name | should be "adept.nugetrunner"
		if (Test-Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }

	It "EXPECTED: Gets The 'Adept.NugetRunner' Package After Installing And After Piping The Provider" {
		(install-package -name "adept.nugetrunner" -provider $nuget -destination $destination -source $source -force)
		(get-packageprovider -name $nuget | get-package "adept.nugetrunner" -destination $destination).name | should be "adept.nugetrunner"
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }

	It "EXPECTED: -FAILS- To Get Package Due To Too Long Of Name" {
		(install-package -name "adept.nugetrunner" -provider $nuget -source $source -destination $destination -force)
		{get-package -name $longName -provider $nuget -destination $destination -EA stop} | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }

	It "EXPECTED: -FAILS- To Get Package Due To Invalid Name" {
		(install-package -name "adept.nugetrunner" -provider $nuget -source $source -destination $destination -force)
		{get-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -provider $nuget -destination $destination -EA stop} | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }

	It "EXPECTED: -FAILS- To Get Package Due To Out Of Bounds Required Version Parameter" {
		(install-package -name "adept.nugetrunner" -provider $nuget -source $source -destination $destination -force)
		{get-package -name "adept.nugetrunner" -provider $nuget -maximumversion "4.0" -minimumversion "1.0" -requiredversion "5.0" -destination $destination -EA stop} | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }

	It "EXPECTED: -FAILS- To Get Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
		(install-package -name "adept.nugetrunner" -provider $nuget -source $source -destination $destination -force)
		{get-package -name "adept.nugetrunner" -provider $nuget -maximumversion "3.0" -minimumversion "4.0" -destination $destination -EA stop} | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }
}

Describe Uninstall-Package -Tags "Feature" {
    $destination = Join-Path $TestDrive "NuGetPackages"

	it "EXPECTED: Uninstalls The Right version of 'Jquery'" {

		(install-package -name "Jquery" -provider $nuget -source $source -destination $destination -RequiredVersion 2.1.3 -force).Version | Should match "2.1.3" 
        (install-package -name "Jquery" -provider $nuget -source $source -destination $destination -RequiredVersion 2.1.4 -force).Version | Should match "2.1.4" 

        #uninstall the old version
		uninstall-package -name "Jquery" -provider $nuget -destination $destination -RequiredVersion 2.1.3
      
        #the old version should be gone but the later should exist
        {Get-Package -ProviderName nuget -RequiredVersion 2.1.3 -Name jquery -Destination $destination -EA stop} | should throw
        (Get-Package -ProviderName nuget -RequiredVersion 2.1.4 -Name jquery -Destination $destination).Version | should be "2.1.4"

    }

	it "EXPECTED: Uninstalls The'Adept.Nugetrunner' Package From The Packages Directory" {
		(install-package -name "adept.nugetrunner" -provider $nuget -source $source -destination $destination -force)
		(uninstall-package -name "adept.nugetrunner" -provider $nuget -destination $destination -force)
		(Test-Path -Path $destination\adept.nugetrunner*) | should be $false
    }

	It "EXPECTED: Uninstalls The'Adept.Nugetrunner' Package From The Packages Directory After Having The Package Piped" {
		(install-package -name "adept.nugetrunner" -provider $nuget -destination $destination -source $source -force)
		(get-package -name "adept.nugetrunner" -provider $nuget -destination $destination | uninstall-package -force)
		(Test-Path -Path $destination\adept.nugetrunner*) | should be $false	
    }

	It "EXPECTED: -FAILS- To Uninstall Package Due To Too Long Of Name" {
		(install-package -name "adept.nugetrunner" -provider $nuget -source $source -destination $destination -force)
		{uninstall-package -name $longName -provider $nuget -destination $destination -force -EA stop} | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }

	It "EXPECTED: -FAILS- To Uninstall Package Due To Invalid Name" {
		(install-package -name "adept.nugetrunner" -provider $nuget -source $source -destination $destination -force)
		{uninstall-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -provider $nuget -destination $destination -EA stop} | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }

	It "EXPECTED: -FAILS- To Uninstall Package Due To Out Of Bounds Required Version Parameter" {
		(install-package -name "adept.nugetrunner" -provider $nuget -source $source -destination $destination -force)
		{uninstall-package -name "adept.nugetrunner" -provider $nuget -maximumversion "4.0" -minimumversion "1.0" -requiredversion "5.0" -destination $destination -EA stop} | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }

	It "EXPECTED: -FAILS- To Uninstall Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
		(install-package -name "adept.nugetrunner" -provider $nuget -source $source -destination $destination -force)
		{uninstall-package -name "adept.nugetrunner" -provider $nuget -maximumversion "3.0" -minimumversion "4.0" -destination $destination -EA stop} | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }

	It "EXPECTED: -FAILS- To Uninstall Package Due To Negative Maximum Version Parameter" {
		(install-package -name "adept.nugetrunner" -provider $nuget -source $source -destination $destination -force)
		{uninstall-package -name "zlib" -provider $nuget -maximumversion "-1.5" -destination $destination -force -EA stop} | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }

	It "EXPECTED: -FAILS- To Uninstall Package Due To Negative Minimum Version Parameter" {
		(install-package -name "adept.nugetrunner" -provider $nuget -source $source -destination $destination -force)
		{uninstall-package -name "zlib" -provider $nuget -minimumversion "-1.5" -destination $destination -force -EA stop} | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }

	It "EXPECTED: -FAILS- To Uninstall Package Due To Negative Required Version Parameter" {
		(install-package -name "adept.nugetrunner" -provider $nuget -source $source -destination $destination -force)
		{uninstall-package -name "zlib" -provider $nuget -requiredversion "-1.5" -destination $destination -force -EA stop} | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }

	 It "E2E: Uninstall all versions of a specific package - Nuget Provider" {
        $packageName = "adept.nugetrunner"
        ($foundPackages = Find-Package -Name $packageName -Provider $nuget -Source $source -AllVersions)        

        # Install all versions of the package
        foreach ($package in $foundPackages) 
        {
            ($package | Install-Package -Destination $destination -Force)
        }

        # Uninstall all versions of the package
        Uninstall-Package -Name $packageName -Provider $nuget -AllVersions -Destination $destination
        
        # Get-Package must not return any packages - since we just uninstalled allversions of the package
        Get-Package -Name "adept.nugetrunner" -Provider $nuget -Destination $destination -AllVersions -warningaction:silentlycontinue -ea silentlycontinue
        $ERROR[0].FullyQualifiedErrorId | should be "NoMatchFound,Microsoft.PowerShell.PackageManagement.Cmdlets.GetPackage"
        
		if (Test-Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }
    
    It "uninstall-package -name with wildcards, Expect error" {
        $Error.Clear()
        $package =  uninstall-package -name packagemanagement* -warningaction:silentlycontinue -ErrorVariable wildcardError -ErrorAction SilentlyContinue        
        $wildcardError.FullyQualifiedErrorId | should be "WildCardCharsAreNotSupported,Microsoft.PowerShell.PackageManagement.Cmdlets.UninstallPackage"
    }

    It "uninstall-package -name with whitespaces only, Expect error" {
        $Error.Clear()
        $package =  uninstall-package -name " " -warningaction:silentlycontinue -ErrorVariable wildcardError -ErrorAction SilentlyContinue        
        $wildcardError.FullyQualifiedErrorId | should be "WhitespacesAreNotSupported,Microsoft.PowerShell.PackageManagement.Cmdlets.UninstallPackage"
    }
}

Describe Get-PackageProvider -Tags "Feature" {

	it "EXPECTED: Gets The 'Nuget' Package Provider" {
    	(get-packageprovider -name $nuget).name | should be $nuget
    }

    it "EXPECTED: Should not raise pending reboot operations" -Skip:($IsCoreCLR){
        $count = (get-itemproperty "hklm:\system\currentcontrolset\control\session manager").PendingFileRenameOperations.Count
        $providers = powershell "get-packageprovider"
        $countAfter = (get-itemproperty "hklm:\system\currentcontrolset\control\session manager").PendingFileRenameOperations.Count
        ($count -eq $countAfter) | should be $true
    }
}

Describe Get-PackageSource -Tags "Feature" {
    $destination = Join-Path $TestDrive "NuGetPackages"

    BeforeAll{
        # Make sure there are no other package sources for nuget. The default 'nuget.org' location conflicts with the only location that works for this test.
        Get-PackageSource | Where-Object { $_.ProviderName -eq 'NuGet' } | Unregister-PackageSource

         #make sure the package repository exists
        Register-PackageSource -Name 'NugetTemp1' -Location "https://www.PowerShellGallery.com/Api/V2/" -ProviderName 'nuget' -Trusted -force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
        Register-PackageSource -Name 'NugetTemp2' -Location "https://www.nuget.org/api/v2" -ProviderName 'nuget' -Trusted -force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
    }
    AfterAll    {
        UnRegister-PackageSource -Name 'NugetTemp1' -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
        UnRegister-PackageSource -Name 'NugetTemp2' -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
    }

    It "find-install-get-package Expect succeed" {      
        find-package jquery -source NugetTemp2 -provider nuget | install-package -destination $destination -force
        (Test-Path $destination\jQuery*) | should be $true
        $a=get-package -Destination $destination -Name jquery
        $a | where { $_.Name -eq 'jQuery'  } | should be $true
    }
   
    It "get-packageprovider--find-package, Expect succeed" {
        
        $a=(get-packageprovider -name nuget| find-package  -Name jquery )   
        $a | where { $_.Name -eq 'jQuery'  } | should be $true
    }     
      
    It "get-packagesource--find-package, Expect succeed" {

        $a=(get-packagesource | find-package jquery)   
        $a | where { $_.Name -eq 'jQuery'  } | should be $true
    }
    
	it "EXPECTED: Gets The 'Nuget' Package Provider" {
    	$a = Get-PackageSource -Name  *Temp* 
        $a | ?{ $_.name -eq "NugetTemp1" } 
        $a | ?{ $_.name -eq "NugetTemp2" } 
    }

    it "EXPECTED: Gets The 'Nuget' Package Provider" {
    	$a = Get-PackageSource -Name  *Temp* -Location "https://www.nuget.org/api/v2"
        $a | ?{ $_.name -eq "NugetTemp2" } 
        $a | ?{ $_.name -ne "NugetTemp1" } 
    }
}

Describe Register-PackageSource -Tags "Feature" {
    BeforeEach{
        if(-not (test-path $TestDrive) ) {
               if($IsWindows) { mkdir $TestDrive -ea silentlycontinue}
               else{mkdir $TestDrive -p}
        }

        $destination = $TestDrive
    } 

	it "EXPECTED: Register a package source with a location created via new-psdrive" {
        if($IsWindows)
        {
            $root = $env:tmp
        }
        else
        {
            $root="/tmp"
        }

	    New-PSDrive -Name xx -PSProvider FileSystem -Root $root	
        (register-packagesource -name "psdriveTest" -provider $nuget -location xx:\).name | should be "psdriveTest"
		(unregister-packagesource -name "psdriveTest" -provider $nuget)
	}

	it "EXPECTED: Registers A New Package Source 'NugetTest.org'" {
		(register-packagesource -name "nugettest.org" -provider $nuget -location $source).name | should be "nugettest.org"
        try 
        {
            # check that even without slash, source returned is still nugettest.org
            (find-package -source $sourceWithoutSlash | Select -First 1).Source | should be "nugettest.org"
        }
        finally
        {
    		(unregister-packagesource -name "nugettest.org" -provider $nuget)
        }
	}

    it "EXPECTED: Registers a package source that requires a credential with skipvalidate" -Skip {
        (register-packagesource -name "psgettestfeed" -provider $nuget -location $vstsFeed -SKipValidate)
        try {
            (Find-Package -Source "psgettestfeed" -Name ContosoClient -Credential $vstsCredential).Name | should be "ContosoClient"
            (Find-Package -Source $vstsFeed -Name ContosoClient -Credential $vstsCredential).Name | should be "ContosoClient"
            (Find-Package -Source $vstsFeedWithSlash -Name ContosoClient -Credential $vstsCredential).Name | should be "ContosoClient"
        }
        finally
        {
            (Unregister-PackageSource -Name "psgettestfeed")
        }
    }


    it "EXPECTED: Registers a package source that requires a credential" -Skip {
        (register-packagesource -name "psgettestfeed" -provider $nuget -location $vstsFeed -Credential $vstsCredential)
        try {
            (Find-Package -Source "psgettestfeed" -Name ContosoClient -Credential $vstsCredential).Name | should be "ContosoClient"
            (Find-Package -Source $vstsFeed -Name ContosoClient -Credential $vstsCredential).Name | should be "ContosoClient"
            (Find-Package -Source $vstsFeedWithSlash -Name ContosoClient -Credential $vstsCredential).Name | should be "ContosoClient"
        }
        finally
        {
            (Unregister-PackageSource -Name "psgettestfeed")
        }
    }

    it "EXPECTED: PackageSource persists" -Skip:($IsCoreCLR){
        $persist = "persistsource"
        $pssource = "http://www.powershellgallery.com/api/v2/"
        $redirectedOutput = "$Destination\redirectedOutput.txt"
        $redirectedError = "$Destination\redirectedError.txt"

        try {
            Start-Process powershell -ArgumentList "register-packagesource -name $persist -location $pssource -provider $nuget" -wait
            Start-Process powershell -ArgumentList "get-packagesource -name $persist -provider $nuget" -wait -RedirectStandardOutput $redirectedOutput -RedirectStandardError $redirectedError
            (Test-Path $redirectedOutput) | should be $true
            (Test-Path $redirectedError) | should be $true
            $redirectedOutput | should contain $persist
            [string]::IsNullOrWhiteSpace((Get-Content $redirectedError)) | should be $true
        }
        finally {
            if (Test-Path $redirectedOutput) {
                Remove-Item -Force $redirectedOutput -ErrorAction SilentlyContinue
            }

            if (Test-Path $redirectedError) {
                Remove-Item -Force $redirectedError -ErrorAction SilentlyContinue
            }
            Start-Process powershell -ArgumentList "unregister-packagesource -name $persist -provider $nuget" -Wait
        }
    }

    it "EXPECTED: Registers a fwlink package source and use it to find-package and install-package" {
        try {
            (register-packagesource -name "fwlink" -provider $nuget -location $fwlink).name | should be "fwlink"
            (find-package -name "TestPackage" -source "fwlink" -provider $nuget).Name | should be "TestPackage"
            (install-package -name "TestPackage" -provider $nuget -source $fwlink -Destination $Destination -force)
            (Test-Path $destination\TestPackage*) | should be $true
        }
        finally {
            if (Test-Path $destination\TestPackage*) {
                Remove-Item -Recurse -Force $destination\TestPackage*
            }
            Unregister-PackageSource -Name "fwlink" -ProviderName $nuget -Force -ErrorAction SilentlyContinue
        }
    }

    it "EXPECTED: Registers an invalid package source" {
		register-packagesource -name `"BingProvider`" -provider $nuget -location `"http://www.example.com/`" -erroraction silentlycontinue
        $Error[0].FullyQualifiedErrorId | should be 'SourceLocationNotValid,Microsoft.PowerShell.PackageManagement.Cmdlets.RegisterPackageSource'
    }

	it "EXPECTED: Registers Multiple Package Sources" {
		foreach ($x in $pkgSources) {
			(register-packagesource -name $x -provider $nuget -location $source).name | should be $x
			(unregister-packagesource -name $x -provider $nuget)
		}
	}

	it "EXPECTED: Registers A 'NugetTest' Package Source After Having The Provider Piped" {
		(get-packageprovider -name $nuget | register-packagesource -name "nugettest" -location $source).ProviderName | should be $nuget
		(unregister-packagesource -name "nugettest" -provider $nuget)
    }
}

Describe Unregister-PackageSource -Tags "Feature" {

	it "EXPECTED: Unregisters The 'NugetTest.org' Package Source" {
		(register-packagesource -name "nugettest.org" -provider $nuget -location $source)
        (Find-Package -name jquery -Source "nugettest.org").Name | should not BeNullOrEmpty
		(unregister-packagesource -name "nugettest.org" -provider $nuget).name | should BeNullOrEmpty
        (Find-Package -name jquery -Source "nugettest.org" -ErrorAction SilentlyContinue -WarningAction SilentlyContinue).Name | should BeNullOrEmpty
    }

	it "EXPECTED: Unregisters Multiple Package Sources" {
		foreach ($x in $pkgSources) {
			(register-packagesource -name $x -provider $nuget -location $source)
			(unregister-packagesource -name $x -provider $nuget).name | should BeNullOrEmpty
		}
    }

	it "EXPECTED: Unregisters A 'NugetTest' Package Source After Having The Provider Piped" {
	    (get-packageprovider -name $nuget | register-packagesource -name "nugettest" -location $source)
	    (unregister-packagesource -name "nugettest" -provider $nuget).name | should BeNullOrEmpty
    }
}

Describe Set-PackageSource -Tags "Feature" {

	it "EXPECTED: Sets The 'NugetTest' Package Source to 'NugetTest2'" {
		(register-packagesource -name "nugettest" -provider $nuget -location "https://www.nuget.org/api/v2")
		(set-packagesource -name "nugettest" -provider $nuget -location "https://www.nuget.org/api/v2" -newname "nugettest2" -newlocation "https://www.nuget.org/api/v2").name | should be "nugettest2"
		(unregister-packagesource -name "nugettest2" -provider $nuget)
    }

    it "EXPECTED: Set a package source that requires a credential" -Skip {
        (register-packagesource -name "psgettestfeed" -provider $nuget -location $vstsFeed -Credential $vstsCredential)
        try {
            (Set-PackageSource -Name "psgettestfeed" -provider $nuget -NewName "psgettestfeed2" -Credential $vstsCredential)
            (Get-PackageSource -Name "psgettestfeed2").Name | should match "psgettestfeed2"
        }
        finally
        {
            (Unregister-PackageSource -Name "psgettestfeed2")
        }
    }
	
	it "EXPECTED: Sets The 'NuGetTest' Package Source to 'NugetTest2' After Piping The Provider Then Piping The Entire Source" {
		(get-packageprovider -name $nuget | register-packagesource -name "nugettest" -location "https://www.nuget.org/api/v2" | set-packagesource -newname "nugettest2" -newlocation "https://www.nuget.org/api/v2").name | should be "nugettest2"
		(unregister-packagesource -name "nugettest2" -provider $nuget)
    }

	it "EXPECTED: Sets Multiple Package Sources To New Names" {
		(register-packagesource -name "nugettest" -provider $nuget -location "https://www.nuget.org/api/v2")
		(set-packagesource -name "nugettest" -provider $nuget -location "https://www.nuget.org/api/v2" -newname "nugettest2" -newlocation "https://www.nuget.org/api/v2").name | should be "nugettest2"
		(unregister-packagesource -name "nugettest2" -provider $nuget)

		(register-packagesource -name "nugettest3" -provider $nuget -location "https://www.nuget.org/api/v2")
		(set-packagesource -name "nugettest3" -provider $nuget -location "https://www.nuget.org/api/v2" -newname "nugettest4" -newlocation "https://www.nuget.org/api/v2").name | should be "nugettest4"
		(unregister-packagesource -name "nugettest4" -provider $nuget)
    }

    it "EXPECTED: Sets the location of 'NugetTest' PackageSource from nuget.org to powershellgallery.com" {
		(register-packagesource -name "nugettest5" -provider $nuget -location "https://www.nuget.org/api/v2")
		(set-packagesource -name "nugettest5" -provider $nuget -newlocation "https://www.powershellgallery.com/api/v2/" -NewName "nugettest6").location | should be "https://www.powershellgallery.com/api/v2/"
		(unregister-packagesource -name "nugettest6" -provider $nuget)
    }
}

Describe Check-ForCorrectError -Tags "Feature" {

    it "EXPECTED: returns a correct error for find-package with dynamic parameter when package source is wrong" {
        $Error.Clear()
        find-package -provider $nuget -source http://wrongsource/api/v2 -FilterOnTag tag -ea silentlycontinue
        $Error[0].FullyQualifiedErrorId | should be "SourceNotFound,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackage"
    }

    it "EXPECTED: returns a correct error for install-package with dynamic parameter when package source is wrong" {
        $Error.Clear()
        install-package -provider $nuget -source http://wrongsource/api/v2 zlib -Destination C:\destination -ea silentlycontinue
        $Error[0].FullyQualifiedErrorId | should be "SourceNotFound,Microsoft.PowerShell.PackageManagement.Cmdlets.InstallPackage"
    }

}

Describe Test-Proxy -Tags "Feature" {

    It "EXPECTED: Register package source using proxy" -Skip {
        try {
            $processId = (Start-Process $proxyPath -PassThru).Id
		    (register-packagesource -name "nugettest7" -provider $nuget -location "https://www.nuget.org/api/v2" -Proxy http://localhost:8080).Name | should be "nugettest7"
            (find-package -Name jquery -Source "nugettest7" -provider $nuget -Proxy http://localhost:8080).Name | should be "jQuery"
            (install-package -Name jquery -Source "nugettest7" -provider $nuget -Proxy http://localhost:8080 -Force).Name | should be "jQuery"
        }
        finally {
            Stop-Process $processId
            unregister-packagesource -name "nugettest7" -provider $nuget -ErrorAction SilentlyContinue
        }
    }

    It "EXPECTED: Cannot register using wrong proxy" -Skip {
        try {
            $processId = (Start-Process $proxyPath -PassThru).Id
		    $packageSource = register-packagesource -name "nugettest7" -provider $nuget -location "https://www.nuget.org/api/v2" -Proxy http://localhost:8060 -ErrorAction SilentlyContinue
            $packageSource | should be $null
        }
        finally {
            Stop-Process $processId
            unregister-packagesource -name "nugettest7" -provider $nuget -ErrorAction SilentlyContinue
        }
    }

    It "EXPECTED: Set package source using proxy" -Skip {
        try {
            $processId = (Start-Process $proxyPath -PassThru).Id
		    (register-packagesource -name "nugettest7" -provider $nuget -location "https://www.nuget.org/api/v2" -Proxy http://localhost:8080).Name | should be "nugettest7"
		    (set-packagesource -name "nugettest7" -provider $nuget -newlocation "https://www.powershellgallery.com/api/v2/" -Proxy http://localhost:8080).Location | should be "https://www.powershellgallery.com/api/v2/"
        }
        finally {
            Stop-Process $processId
            unregister-packagesource -name "nugettest7" -provider $nuget -ErrorAction SilentlyContinue
        }
    }

    It "EXPECTED: Cannot set package source using wrong proxy" -Skip {
        try {
            $processId = (Start-Process $proxyPath -PassThru).Id
		    (register-packagesource -name "nugettest7" -provider $nuget -location "https://www.nuget.org/api/v2" -Proxy http://localhost:8080).Name | should be "nugettest7"
		    $packageSource = set-packagesource -name "nugettest7" -provider $nuget -newlocation "https://www.powershellgallery.com/api/v2/" -Proxy http://localhost:8060 -ErrorAction SilentlyContinue
            $packageSource | should be $null
        }
        finally {
            Stop-Process $processId
            unregister-packagesource -name "nugettest7" -provider $nuget -ErrorAction SilentlyContinue
        }
    }
    
    It "EXPECTED: cannot connect using the wrong proxy" -Skip {
        try {
            $processId = (Start-Process $proxyPath -PassThru).Id
            $packages = Find-Package -Provider NuGet -Proxy http://localhost:8060 -ErrorAction SilentlyContinue
            $packages | should be $null
        }
        finally {
            Stop-Process $processId
        }
    }

    It "EXPECTED: cannot connect if the server is not on the list allowed by proxy" -Skip {
        try {
            $processId = (Start-Process $proxyPath -PassThru).Id
            $packages = Find-Package -Provider NuGet -Proxy http://localhost:8080 -Source $dtlgallery -ErrorAction SilentlyContinue
            $packages | should be $null
        }
        finally {
            Stop-Process $processId
        }
    }

    It "EXPECTED: find packages using the correct proxy" -Skip {
        try {
            $processId = (Start-Process $proxyPath -PassThru).Id
            $jquery = Find-Package -Provider NuGet -Proxy http://localhost:8080 -Source $source -Name jquery

            $jquery.Name | should match jquery
        }
        finally {
            Stop-Process $processId
        }
    }

}
