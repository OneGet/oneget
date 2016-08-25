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
ipmo "$PSScriptRoot\utility.psm1"


# ------------------------------------------------------------------------------
# Actual Tests:

$source = "http://www.nuget.org/api/v2/"
$sourceWithoutSlash = "http://www.nuget.org/api/v2"
$fwlink = "http://go.microsoft.com/fwlink/?LinkID=623861&clcid=0x409"
$longName = "THISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERS";
$workingMaximumVersions = {"2.0", "2.5", "3.0"};
$packageNames = @("Azurecontrib", "TestLib");
$minimumVersions = @("1.0", "1.3", "1.5");
$maximumVersions = @("1.8", "2.1", "2.3");
$destination = "$env:tmp\nugettests"
$relativetestpath = "$env:tmp\relativepathtestnuget"
$dependenciesSource = "$env:temp\PackageManagementDependencies"
$dtlgallery = "https://MyTestGallery.cloudapp.net/api/v2/"
Get-ChildItem -Path $dependenciesSource -Recurse -Include *.nupkg | % { $_.IsReadOnly = $false }
if( test-path $destination ) {
    rmdir -recurse -force $destination -ea silentlycontinue
}
mkdir $destination -ea silentlycontinue

$pkgSources = @("NUGETTEST101", "NUGETTEST202", "NUGETTEST303");

$nuget = "nuget"

Describe "Correct NuGet version loaded" -Tags @('BVT', 'DRT') {
    $nugetProvider = Get-PackageProvider $nuget
    $nugetProvider.Name | should match "NuGet"
    $nugetProvider.Version -eq "2.8.5.206" | should be $true
}

Describe "Find-Package" -Tags @('BVT', 'DRT'){
    # make sure that packagemanagement is loaded
    import-packagemanagement

    it "EXPECTED: Find a package with a location created via new-psdrive" {

        $Error.Clear()
        $msg =  powershell 'New-PSDrive -Name xx -PSProvider FileSystem -Root $env:tmp -warningaction:silentlycontinue -ea silentlycontinue > $null; find-package -name "fooobarrr" -provider nuget -source xx:\  -warningaction:silentlycontinue -ea silentlycontinue;$ERROR[0].FullyQualifiedErrorId'
        $msg | should  Not Be "SourceNotFound,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackage"
        $msg | should be "NoMatchFoundForCriteria,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackage"
	}

    It "EXPECTED: Finds 'Zlib' Package" {
        $version = "1.2.8.8"
        $expectedDependencies = @("zlib.v120.windesktop.msvcstl.dyn.rt-dyn/[1.2.8.8]", "zlib.v140.windesktop.msvcstl.dyn.rt-dyn/[1.2.8.8]")
        $zlib = find-package -name "zlib" -provider $nuget -source $source -RequiredVersion $version
        $zlib.name | should match "zlib"
        $zlib.Dependencies.Count | should be 2

      	$dateTime = [datetime]$zlib.Meta.Attributes["published"]
        $zlib.Meta.Attributes["packageSize"] | should match "2742"
        $dateTime.Day | should be 17
        $dateTime.Month | should be 5
        $dateTime.Year | should be 2015
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

    It "EXPECTED: Finds 64 packages" -skip {
        $packages = Find-Package -Provider $nuget -Source $source | Select -First 63

        $packages = (Find-Package -ProviderName $nuget -Source $source -Name $packages.Name)

        $packages.Count | Should be 63
    }


    It "EXPECTED: Finds 100 packages should throw error" {
        $packages = Find-Package -Provider $nuget -Source $source | Select -First 100

        { Find-Package -ProviderName $nuget -Source $source -Name $packages.Name -ErrorAction Stop } | should throw
    }


    It "EXPECTED: Finds 128 packages should throw error" {
        $packages = Find-Package -Provider $nuget -Source $source | Select -First 127

        { Find-Package -ProviderName $nuget -Source $source -Name $packages.Name -ErrorAction Stop } | should throw
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

    It "EXPECTED: Cannot find unlisted package" -Skip {
        $msg = powershell "find-package -provider $nuget -source $dtlgallery -name hellops -erroraction silentlycontinue; `$Error[0].FullyQualifiedErrorId"
        $msg | should be "NoMatchFoundForCriteria,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackage"
    }

    It "EXPECTED: Cannot find unlisted package with all versions parameter" -Skip {
        $packages = find-package -name gistprovider -provider $nuget -source $dtlgallery -AllVersions
        # we should still be able to find at least 2 listed package
        $packages.Count -gt 1 | should be $true
        # this version is unlisted
        $packages.Version.Contains("0.6") | should be $false
        # this version is listed
        $packages.Version.Contains("1.2") | should be $true
    }

    It "EXPECTED: Cannot find unlisted package with all versions and maximum versions" -Skip {
        $packages = find-package -name gistprovider -provider $nuget -source $dtlgallery -AllVersions -MaximumVersion 1.3
        # we should still be able to find 2 listed package (which is version 1.2 and 1.3)
        $packages.Count -eq 2 | should be $true
        # this version is unlisted
        $packages.Version.Contains("0.6") | should be $false
        # this version is listed
        $packages.Version.Contains("1.2") | should be $true
    }

    It "EXPECTED: Cannot find unlisted package with all versions and minimum versions" -Skip {
        $packages = find-package -name gistprovider -provider $nuget -source $dtlgallery -AllVersions -MinimumVersion 0.5
        # we should still be able to find at least 2 listed package (which is version 1.2 and 1.3)
        $packages.Count -gt 2 | should be $true
        # this version is unlisted
        $packages.Version.Contains("0.6") | should be $false
        # this version is listed
        $packages.Version.Contains("1.2") | should be $true
    }

    It "EXPECTED: Finds unlisted package with required version" -Skip{
        (find-package -name hellops -provider $nuget -source $dtlgallery -requiredversion 0.1.0).Name | should match "HellOps"
    }

    It "EXPECTED: Cannot find unlisted package with maximum versions" -Skip {
        # error out because all the versions below 0.6 are unlisted
        $msg = powershell "find-package -provider $nuget -source $dtlgallery -name gistprovider -maximumversion 0.6 -erroraction silentlycontinue; `$Error[0].FullyQualifiedErrorId"
        $msg | should be "NoMatchFoundForCriteria,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackage"
    }

    It "EXPECTED: Cannot find unlisted package with minimum versions" -Skip{
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
        $awssdk = Find-Package -Name "awssdk" -Provider $nuget -source $source -RequiredVersion 2.3.55
        [long]$awssdk.Meta.Attributes["downloadCount"] -ge 1023357 | should be $true
        $awssdk.Meta.Attributes["updated"] | should match "2016-03-09T01:06:54Z"
        $awssdk.TagId | should match "AWSSDK#2.3.55.0" 
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
    	{ find-package -name $longName -provider $nuget -source $source -ErrorAction Stop } | should throw
    }

	It "EXPECTED: -FAILS- To Find Package Due To Invalid Name" {
    	{ find-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -provider $nuget -source $source EA stop } | should throw
    }

	It "EXPECTED: -FAILS- To Find Package Due To Negative Maximum Version Parameter" {
    	{find-package -name "zlib" -provider $nuget -source $source -maximumversion "-1.5" -EA stop} | should throw
    }

	It "EXPECTED: -FAILS- To Find Package Due To Negative Minimum Version Parameter" {
    	{find-package -name "zlib" -provider $nuget -source $source -minimumversion "-1.5" -EA stop} | should throw
    }

	It "EXPECTED: -FAILS- To Find Package Due To Negative Required Version Parameter" {
    	{find-package -name "zlib" -provider $nuget -source $source -requiredversion "-1.5" -EA stop} | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Out Of Bounds Required Version Parameter" {
	    {find-package -name "zlib" -provider $nuget -source $source -minimumversion "1.0" -maximumversion "1.5" -requiredversion "2.0" -EA Stop} | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
	    {find-package -name "zlib" -provider $nuget -source $source -minimumversion "1.5" -maximumversion "1.0" -EA stop} | should throw
    }
}

Describe Save-Package -Tags @('BVT', 'DRT'){
	# make sure packagemanagement is loaded
	import-packagemanagement

	it "EXPECTED: Saves 'Zlib' Package To Packages Directory" {
        $version = "1.2.8.8"
        $expectedPackages = @("zlib", "zlib.v120.windesktop.msvcstl.dyn.rt-dyn", "zlib.v140.windesktop.msvcstl.dyn.rt-dyn")
        $newDestination = "$env:tmp\nugetinstallation"
		
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

            mkdir $newDestination
            # make sure we can install the package. To do this, we need to save the dependencies first
            (save-package -name "zlib.v120.windesktop.msvcstl.dyn.rt-dyn" -RequiredVersion $version -provider $nuget -source $source -path $destination)
            (save-package -name "zlib.v140.windesktop.msvcstl.dyn.rt-dyn" -RequiredVersion $version -provider $nuget -source $source -path $destination)

		    (install-package -name "zlib" -provider $nuget -source $destination -destination $newDestination -force -RequiredVersion $version)
		    (test-path "$newDestination\zlib.1.2*") | should be $true
            # Test that dependencies are installed
            (test-path "$newDestination\zlib.v120*") | should be $true
            (test-path "$newDestination\zlib.v140*") | should be $true
        }
        finally {
            if (Test-Path $newDestination) {
                Remove-Item -Recurse -Force -Path $newDestination
            }

		    if (Test-Path $destination\zlib*) {
			    rm $destination\zlib*
		    }

        }
    }

    it "EXPECTED: Saves 'Zlib' Package to Packages Directory and install it without dependencies" {
        $version = "1.2.8.8"
        $newDestination = "$env:tmp\nugetinstallation"

        try {
		    (save-package -name "zlib" -provider $nuget -source $source -Path $destination -RequiredVersion $version)
		    (test-path $destination\zlib*) | should be $true
            remove-item $destination\zlib.v1* -force -Recurse -ErrorAction SilentlyContinue 

            $msg = powershell "install-package -name zlib -provider $nuget -source $destination -destination $newDestination -force -RequiredVersion $version -ErrorAction SilentlyContinue; `$Error[0].FullyQualifiedErrorId"
            $msg | should match "UnableToFindDependencyPackage,Microsoft.PowerShell.PackageManagement.Cmdlets.InstallPackage"
            (Test-Path "$newDestination\zlib*") | should be $false
        }
        finally {
            if (Test-Path $newDestination) {
                Remove-Item -Recurse -Force -Path $newDestination
            }

		    if (Test-Path $destination\zlib*) {
			    rm $destination\zlib*
		    }

        }
    }

    It "EXPECTED: Saves work with dependencies loop" {
        try {
            $msg = powershell "save-package -name ModuleWithDependenciesLoop -provider $nuget -source `"$dependenciesSource\SimpleDependenciesLoop`" -path $destination -ErrorAction SilentlyContinue -WarningAction SilentlyContinue; `$Error[0].FullyQualifiedErrorId"
            $msg | should match "ProviderFailToDownloadFile,Microsoft.PowerShell.PackageManagement.Cmdlets.SavePackage"
            (Test-Path $destination\ModuleWithDependenciesLoop*) | should be $false
        }
        finally {
            if (Test-Path $destination\ModuleWithDependenciesLoop*) {
                rm $destination\ModuleWithDependenciesLoop*
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
                rm $destination\TestPackage*
            }
        }
    }

    It "EXPECTED: Saves 'awssdk' package which has more than 200 versions" {
		(save-package -name "awssdk" -provider $nuget -source $source -Path $destination)
		(test-path $destination\awssdk*) | should be $true
		if (Test-Path $destination\awssdk*) {
			rm $destination\awssdk*
		}    
    }

	it "EXPECTED: Saves Various Packages With Various Version Parameters To Packages Directory" {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				save-package -name $x -source $source -provider $nuget -minimumversion $y -maximumversion $z -Path $destination
				(Test-Path -Path $destination\$x*) | should be $true
				if (Test-Path -Path $destination\$x*) {
					rm $destination\$x*
					}
				}
			}
		}
	}

	It "EXPECTED: Saves 'Zlib' Package After Having The Provider Piped" {
	    (find-package -name "zlib" -provider $nuget -source $source | save-package -Path $destination)
	    (Test-Path -Path $destination\zlib*) | should be $true
	    if (Test-Path -Path $destination\zlib*) {
		    rm $destination\zlib*
        }
    }

	It "EXPECTED: -FAILS- To Save Package Due To Too Long Of Name" {
    	{save-package -name $longName -provider $nuget -source $source -Path $destination -EA stop} | should throw
	}

	It "EXPECTED: -FAILS- To Save Package Due To Invalid Name" {
    	{save-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -provider $nuget -source $source -Path $destination -EA stop} | should throw
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

Describe "install-package with Whatif" -Tags @('BVT', 'DRT'){
    # make sure that packagemanagement is loaded
    #import-packagemanagement

    BeforeEach{
        $tempFile = [System.IO.Path]::GetTempFileName() 
        $whatif = "What if: Performing the operation";
        Remove-Item c:\foof -ErrorAction SilentlyContinue -WarningAction SilentlyContinue -Force
    }

    AfterEach {
        if(Test-Path $tempFile)
        {
            Remove-Item $tempFile -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
        }
    }

     It "install-package -name nuget with whatif, Expect succeed" {
       if($PSCulture -eq 'en-US'){
        # Start Transcript
        Start-Transcript -Path $tempFile
		
        install-Package -name jquery -force -source $source -ProviderName NuGet -destination c:\foof -warningaction:silentlycontinue -ErrorAction SilentlyContinue -whatif  

        # Stop Transcript and get content of transcript file
        Stop-Transcript
        $transcriptContent = Get-Content $tempFile

        $transcriptContent | where { $_.Contains( $whatif ) } | should be $true
        Test-Path C:\foof | should be $false


        Remove-Item $whatif -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
        }
    }
}

Describe Install-Package -Tags @('BVT', 'DRT'){
	# make sure packagemanagement is loaded
	import-packagemanagement

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

    It "install-packageprovider -name with wildcards, Expect error" {
        $Error.Clear()
        install-Package -name gist* -force -source $source -warningaction:silentlycontinue -ErrorVariable wildcardError -ErrorAction SilentlyContinue        
        $wildcardError.FullyQualifiedErrorId| should be "WildCardCharsAreNotSupported,Microsoft.PowerShell.PackageManagement.Cmdlets.InstallPackage"
    }

    it "EXPECTED: Fails to install a module with simple dependencies loop" {
         $msg = powershell "Install-Package ModuleWithDependenciesLoop -ProviderName nuget -Source `"$dependenciesSource\SimpleDependenciesLoop`" -Destination $destination -ErrorAction SilentlyContinue; `$Error[0].FullyQualifiedErrorId"
         $msg | should be "DependencyLoopDetected,Microsoft.PowerShell.PackageManagement.Cmdlets.InstallPackage"

    }

    it "EXPECTED: Fails to install a module with a big dependencies loop" {
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

	it "EXPECTED: Installs 'awssdk' Package which has more than 200 versions To Packages Directory" {
		(install-package -name "awssdk" -provider $nuget -source $source -destination $destination -minimumversion 2.3 -force)
		(test-path $destination\awssdk*) | should be $true
		if (Test-Path $destination\awssdk*) {
			(Remove-Item -Recurse -Force -Path $destination\awssdk*)
		}
    }

	it "EXPECTED: Installs Various Packages With Various Version Parameters To Packages Directory" {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				(install-package -name $x -source $source -provider $nuget -minimumversion $y -maximumversion $z -destination $destination -force)
				(Test-Path -Path $destination\$x*) | should be $true
				if (Test-Path -Path $destination\$x*) {
					(Remove-Item -Recurse -Force -Path $destination\$x*)
					}
				}
			}
		}
    }

	It "EXPECTED: Installs 'Zlib' Package After Having The Provider Piped" {
	    (find-package -name "zlib" -provider $nuget -source $source | install-package -destination $destination -force)
	    (Test-Path -Path $destination\zlib*) | should be $true
	    if (Test-Path -Path $destination\zlib*) {
		    (Remove-Item -Recurse -Force -Path $destination\zlib*)
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

Describe Get-Package -Tags @('BVT', 'DRT'){
	# make sure packagemanagement is loaded
	import-packagemanagement

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

Describe Uninstall-Package -Tags @('BVT', 'DRT'){
	# make sure packagemanagement is loaded
	import-packagemanagement

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
        $msg = powershell 'Get-Package -Name "adept.nugetrunner" -Provider $nuget -Destination $destination -AllVersions -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "NoMatchFound,Microsoft.PowerShell.PackageManagement.Cmdlets.GetPackage"
        
		if (Test-Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }
}

Describe Get-PackageProvider -Tags @('BVT', 'DRT'){
	# make sure that packagemanagement is loaded
    import-packagemanagement

	it "EXPECTED: Gets The 'Nuget' Package Provider" {
    	(get-packageprovider -name $nuget -force).name | should be $nuget
    }
}

Describe Get-PackageSource -Tags @('BVT', 'DRT'){
	# make sure that packagemanagement is loaded
    import-packagemanagement

    BeforeAll{
         #make sure the package repository exists
        Register-PackageSource -Name 'NugetTemp1' -Location "https://www.PowerShellGallery.com/Api/V2/" -ProviderName 'nuget' -Trusted -force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
        Register-PackageSource -Name 'NugetTemp2' -Location "https://www.nuget.org/api/v2" -ProviderName 'nuget' -Trusted -force -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
    }
    AfterAll    {
        UnRegister-PackageSource -Name 'NugetTemp1' -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
        UnRegister-PackageSource -Name 'NugetTemp2' -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
    }

    It "find-install-get-package Expect succeed" {
      
        find-package jquery | install-package -destination $destination -force
        (Test-Path $destination\jquery*) | should be $true
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

Describe Register-PackageSource -Tags @('BVT', 'DRT'){
    # make sure that packagemanagement is loaded
    import-packagemanagement


	it "EXPECTED: Register a package source with a location created via new-psdrive" {
	    New-PSDrive -Name xx -PSProvider FileSystem -Root $destination	
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

    it "EXPECTED: PackageSource persists" {
        $persist = "persistsource"
        $pssource = "http://www.powershellgallery.com/api/v2/"
        $redirectedOutput = "$env:tmp\nugettests\redirectedOutput.txt"
        $redirectedError = "$env:tmp\nugettests\redirectedError.txt"
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
		$msg = powershell "register-packagesource -name `"BingProvider`" -provider $nuget -location `"http://www.example.com/`" -erroraction silentlycontinue; `$Error[0].FullyQualifiedErrorId"
        $msg | should be 'SourceLocationNotValid,Microsoft.PowerShell.PackageManagement.Cmdlets.RegisterPackageSource'
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

Describe Unregister-PackageSource -Tags @('BVT', 'DRT'){
    # make sure that packagemanagement is loaded
    import-packagemanagement

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

Describe Set-PackageSource -Tags @('BVT', 'DRT'){
    # make sure that packagemanagement is loaded
    import-packagemanagement

	it "EXPECTED: Sets The 'NugetTest' Package Source to 'NugetTest2'" {
		(register-packagesource -name "nugettest" -provider $nuget -location "https://www.nuget.org/api/v2")
		(set-packagesource -name "nugettest" -provider $nuget -location "https://www.nuget.org/api/v2" -newname "nugettest2" -newlocation "https://www.nuget.org/api/v2").name | should be "nugettest2"
		(unregister-packagesource -name "nugettest2" -provider $nuget)
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

Describe Check-ForCorrectError -Tags @('BVT', 'DRT'){  
    # make sure that packagemanagement is loaded
    import-packagemanagement

    it "EXPECTED: returns a correct error for find-package with dynamic parameter when package source is wrong" {
        $Error.Clear()
        $msg = powershell "find-package -provider $nuget -source http://wrongsource/api/v2 -FilterOnTag tag -ea silentlycontinue; `$Error[0].FullyQualifiedErrorId"
        $msg | should be "SourceNotFound,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackage"
    }

    it "EXPECTED: returns a correct error for install-package with dynamic parameter when package source is wrong" {
        $Error.Clear()
        $msg = powershell "install-package -provider $nuget -source http://wrongsource/api/v2 zlib -Destination C:\destination -ea silentlycontinue; `$Error[0].FullyQualifiedErrorId"
        $msg | should be "SourceNotFound,Microsoft.PowerShell.PackageManagement.Cmdlets.InstallPackage"
    }

}
