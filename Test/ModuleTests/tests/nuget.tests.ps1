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

$source = "http://nuget.org/api/v2"
$longName = "THISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERS";
$workingMaximumVersions = {"2.0", "2.5", "3.0"};
$packageNames = @("Azurecontrib", "AWSSDK", "TestLib");
$minimumVersions = @("1.0", "1.3", "1.5");
$maximumVersions = @("2.0", "2.5", "3.0");
$destination = "$env:tmp\nugettests"
mkdir $destination -ea silentlycontinue

$pkgSources = @("NUGETTEST101", "NUGETTEST202", "NUGETTEST303");


Describe "Find-Package" {
    # make sure that packagemanagement is loaded
    import-packagemanagement

    It "EXPECTED: Finds 'Zlib' Package" {
        (find-package -name "zlib" -provider "nuget" -source $source).name | should match "zlib"
    }
	It "EXPECTED: Finds A Combination Of Packages With Various Versions" {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				(find-package -name $x -source $source -provider "nuget" -minimumversion $y -maximumversion $z).name | should be $x
				}
			}
		}
	}

	It "EXPECTED: Finds 'Zlib' Package After Piping The Provider" {
		(get-packageprovider -name "nuget" | find-package -name zlib -source $source ).name | should be "zlib"
	}

	It "EXPECTED: -FAILS- To Find Package Due To Too Long Of Name" {
		(find-package -name $longName -provider "nuget" -source $source -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Invalid Name" {
		(find-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -provider "nuget" -source $source -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Negative Maximum Version Parameter" {
		(find-package -name "zlib" -provider "nuget" -source $source -maximumversion "-1.5" -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Negative Minimum Version Parameter" {
		(find-package -name "zlib" -provider "nuget" -source $source -minimumversion "-1.5" -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Negative Required Version Parameter" {
		(find-package -name "zlib" -provider "nuget" -source $source -requiredversion "-1.5" -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Out Of Bounds Required Version Parameter" {
		(find-package -name "zlib" -provider "nuget" -source $source -minimumversion "1.0" -maximumversion "1.5" -requiredversion "2.0" -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
		(find-package -name "zlib" -provider "nuget" -source $source -minimumversion "1.5" -maximumversion "1.0" -EA silentlycontinue) | should throw
	}
}

Describe Save-Package {
	# make sure packagemanagement is loaded
	import-packagemanagement

	it "EXPECTED: Saves 'Zlib' Package To Packages Directory" {
		(save-package -name "zlib" -provider "nuget" -source $source -Path $destination)
		(test-path $destination\zlib*) | should be $true
		if (Test-Path $destination\zlib*) {
			rm $destination\zlib*
		}
	}

	it "EXPECTED: Saves Various Packages With Various Version Parameters To Packages Directory" {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				(save-package -name $x -source $source -provider "nuget" -minimumversion $y -maximumversion $z -Path $destination)
				(Test-Path -Path $destination\$x*) | should be $true
				if (Test-Path -Path $destination\$x*) {
					rm $destination\$x*
					}
				}
			}
		}
	}

	It "EXPECTED: Saves 'Zlib' Package After Having The Provider Piped" {
	(find-package -name "zlib" -provider "nuget" -source $source | save-package -Path $destination)
	(Test-Path -Path $destination\zlib*) | should be $true
	if (Test-Path -Path $destination\zlib*) {
		rm $destination\zlib*
		}
	}

	It "EXPECTED: -FAILS- To Save Package Due To Too Long Of Name" {
		(save-package -name $longName -provider "nuget" -source $source -Path $destination -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Save Package Due To Invalid Name" {
		(save-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -provider "nuget" -source $source -Path $destination -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Save Package Due To Negative Maximum Version Parameter" {
		(save-package -name "zlib" -provider "nuget" -source $source -maximumversion "-1.5" -Path $destination -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Save Package Due To Negative Minimum Version Parameter" {
		(save-package -name "zlib" -provider "nuget" -source $source -minimumversion "-1.5" -Path $destination -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Save Package Due To Negative Required Version Parameter" {
		(save-package -name "zlib" -provider "nuget" -source $source -requiredversion "-1.5" -Path $destination -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Save Package Due To Out Of Bounds Required Version Parameter" {
		(save-package -name "zlib" -provider "nuget" -source $source -minimumversion "1.0" -maximumversion "1.5" -requiredversion "2.0" -Path $destination -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Save Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
		(save-package -name "zlib" -provider "nuget" -source $source -minimumversion "1.5" -maximumversion "1.0" -Path $destination -EA silentlycontinue) | should throw
	}
}

Describe Install-Package {
	# make sure packagemanagement is loaded
	import-packagemanagement

	it "EXPECTED: Installs 'Zlib' Package To Packages Directory" {
		(install-package -name "zlib" -provider "nuget" -source $source -destination $destination -force)
		(test-path $destination\zlib*) | should be $true
		if (Test-Path $destination\zlib*) {
			(Remove-Item -Recurse -Force -Path $destination\zlib*)
		}
	}

	it "EXPECTED: Installs Various Packages With Various Version Parameters To Packages Directory" {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				(install-package -name $x -source $source -provider "nuget" -minimumversion $y -maximumversion $z -destination $destination -force)
				(Test-Path -Path $destination\$x*) | should be $true
				if (Test-Path -Path $destination\$x*) {
					(Remove-Item -Recurse -Force -Path $destination\$x*)
					}
				}
			}
		}
	}

	It "EXPECTED: Installs 'Zlib' Package After Having The Provider Piped" {
	(find-package -name "zlib" -provider "nuget" -source $source | install-package -destination $destination -force)
	(Test-Path -Path $destination\zlib*) | should be $true
	if (Test-Path -Path $destination\zlib*) {
		(Remove-Item -Recurse -Force -Path $destination\zlib*)
		}
	}

	It "EXPECTED: -FAILS- To Install Package Due To Too Long Of Name" {
		(install-package -name $longName -provider "nuget" -source $source -destination $destination -force -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Install  Package Due To Invalid Name" {
		(install-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -provider "nuget" -source $source -destination $destination -force -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Install Package Due To Negative Maximum Version Parameter" {
		(install-package -name "zlib" -provider "nuget" -source $source -maximumversion "-1.5" -destination $destination -force -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Install Package Due To Negative Maximum Version Parameter" {
		(install-package -name "zlib" -provider "nuget" -source $source -minimumversion "-1.5" -destination $destination -force -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Install Package Due To Negative Maximum Version Parameter" {
		(install-package -name "zlib" -provider "nuget" -source $source -requiredversion "-1.5" -destination $destination -force -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Install Package Due To Out Of Bounds Required Version Parameter" {
		(install-package -name "zlib" -provider "nuget" -source $source -minimumversion "1.0" -maximumversion "1.5" -requiredversion "2.0" -destination $destination -force -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Install Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
		(install-package -name "zlib" -provider "nuget" -source $source -minimumversion "1.5" -maximumversion "1.0" -destination $destination -force -EA silentlycontinue) | should throw
	}
}

Describe Get-Package {
	# make sure packagemanagement is loaded
	import-packagemanagement

	it "EXPECTED: Gets The 'Adept.NugetRunner' Package After Installing" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -source $source -destination $destination -force)
		(get-package -name "adept.nugetrunner" -provider "nuget" -destination $destination).name | should be "adept.nugetrunner"
		if (Test-Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
	}

<# This test doesn't make any sense.
	it "EXPECTED: Gets Various Packages With Various Version Parameters From Packages Directory After Installing" {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				(install-package -name $x -source $source -provider "nuget" -minimumversion $y -maximumversion $z -destination $destination -force)
				(Get-Package -name $x -provider "nuget" -minimumversion $y -maximumversion $z -destination $destination).name | should be $x
				if (Test-Path -Path $destination\$x*) {
					(Remove-Item -Recurse -Force -Path $destination\$x*)
					}
				}
			}
		}
	}
#>

	It "EXPECTED: Gets The 'Adept.NugetRunner' Package After Installing And After Piping The Provider" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -destination $destination -source $source -force)
		(get-packageprovider -name "nuget" | get-package "adept.nugetrunner" -destination $destination).name | should be "adept.nugetrunner"
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
	}

	It "EXPECTED: -FAILS- To Get Package Due To Too Long Of Name" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -source $source -destination $destination -force)
		(get-package -name $longName -provider "nuget" -destination $destination -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
	}

	It "EXPECTED: -FAILS- To Get Package Due To Invalid Name" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -source $source -destination $destination -force)
		(get-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -provider "nuget" -destination $destination -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
	}

	It "EXPECTED: -FAILS- To Get Package Due To Out Of Bounds Required Version Parameter" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -source $source -destination $destination -force)
		(get-package -name "adept.nugetrunner" -provider "nuget" -maximumversion "4.0" -minimumversion "1.0" -requiredversion "5.0" -destination $destination -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
	}

	It "EXPECTED: -FAILS- To Get Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -source $source -destination $destination -force)
		(get-package -name "adept.nugetrunner" -provider "nuget" -maximumversion "3.0" -minimumversion "4.0" -destination $destination -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
	}
}

Describe Uninstall-Package {
	# make sure packagemanagement is loaded
	import-packagemanagement

	it "EXPECTED: Uninstalls The'Adept.Nugetrunner' Package From The Packages Directory" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -source $source -destination $destination -force)
		(uninstall-package -name "adept.nugetrunner" -provider "nuget" -destination $destination -force)
		(Test-Path -Path $destination\adept.nugetrunner*) | should be $false
	}

<# does not make sense
	it "EXPECTED: Uninstalls Various Packages With Various Versions From The Packages Directory" {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				(install-package -name $x -source $source -provider "nuget" -minimumversion $y -maximumversion $z -destination $destination -force)
				(uninstall-package -name $x -provider "nuget" -minimumversion $y -maximumversion $z -destination $destination -force)
				(Test-Path -Path $destination\$x*) | should be $false
				}
			}
		}
	}
#>
	It "EXPECTED: Uninstalls The'Adept.Nugetrunner' Package From The Packages Directory After Having The Package Piped" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -destination $destination -source $source -force)
		(get-package -name "adept.nugetrunner" -provider "nuget" -destination $destination | uninstall-package -force)
		(Test-Path -Path $destination\adept.nugetrunner*) | should be $false	
	}

	It "EXPECTED: -FAILS- To Uninstall Package Due To Too Long Of Name" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -source $source -destination $destination -force)
		(uninstall-package -name $longName -provider "nuget" -destination $destination -force -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
	}

	It "EXPECTED: -FAILS- To Uninstall Package Due To Invalid Name" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -source $source -destination $destination -force)
		(uninstall-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -provider "nuget" -destination $destination -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
	}

	It "EXPECTED: -FAILS- To Uninstall Package Due To Out Of Bounds Required Version Parameter" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -source $source -destination $destination -force)
		(uninstall-package -name "adept.nugetrunner" -provider "nuget" -maximumversion "4.0" -minimumversion "1.0" -requiredversion "5.0" -destination $destination -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
	}

	It "EXPECTED: -FAILS- To Uninstall Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -source $source -destination $destination -force)
		(uninstall-package -name "adept.nugetrunner" -provider "nuget" -maximumversion "3.0" -minimumversion "4.0" -destination $destination -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
	}

	It "EXPECTED: -FAILS- To Uninstall Package Due To Negative Maximum Version Parameter" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -source $source -destination $destination -force)
		(uninstall-package -name "zlib" -provider "nuget" -maximumversion "-1.5" -destination $destination -force -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
	}

	It "EXPECTED: -FAILS- To Uninstall Package Due To Negative Minimum Version Parameter" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -source $source -destination $destination -force)
		(uninstall-package -name "zlib" -provider "nuget" -minimumversion "-1.5" -destination $destination -force -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
	}

	It "EXPECTED: -FAILS- To Uninstall Package Due To Negative Required Version Parameter" {
		(install-package -name "adept.nugetrunner" -provider "nuget" -source $source -destination $destination -force)
		(uninstall-package -name "zlib" -provider "nuget" -requiredversion "-1.5" -destination $destination -force -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
	}
}

Describe Get-PackageProvider {
	# make sure that packagemanagement is loaded
    import-packagemanagement

	it "EXPECTED: Gets The 'Nuget' Package Provider" {
		(get-packageprovider -name "nuget" -forcebootstrap -force).name | should be "nuget"
	}
}

Describe Register-PackageSource {
    # make sure that packagemanagement is loaded
    import-packagemanagement

	it "EXPECTED: Registers A New Package Source 'NugetTest.org'" {
		(register-packagesource -name "nugettest.org" -provider "nuget" -location $source).name | should be "nugettest.org"
		(unregister-packagesource -name "nugettest.org")
	}

	it "EXPECTED: Registers Multiple Package Sources" {
		foreach ($x in $pkgSources) {
			(register-packagesource -name $x -provider "nuget" -location $source).name | should be $x
			(unregister-packagesource -name $x)
		}
	}
	it "EXPECTED: Registers A 'NugetTest' Package Source After Having The Provider Piped" {
		(get-packageprovider -name "nuget" | register-packagesource -name "nugettest" -location $source).ProviderName | should be "nuget"
		(unregister-packagesource -name "nugettest")
	}
}

Describe Unregister-PackageSource {
    # make sure that packagemanagement is loaded
    import-packagemanagement

	it "EXPECTED: Unregisters The 'NugetTest.org' Package Source" {
		(register-packagesource -name "nugettest.org" -provider "nuget" -location $source)
		(unregister-packagesource -name "nugettest.org").name | should BeNullOrEmpty
	}

	it "EXPECTED: Unregisters Multiple Package Sources" {
		foreach ($x in $pkgSources) {
			(register-packagesource -name $x -provider "nuget" -location $source)
			(unregister-packagesource -name $x).name | should BeNullOrEmpty
		}
	}
	it "EXPECTED: Unregisters A 'NugetTest' Package Source After Having The Provider Piped" {
		(get-packageprovider -name "nuget" | register-packagesource -name "nugettest" -location $source)
		(unregister-packagesource -name "nugettest").name | should BeNullOrEmpty
	}
}

Describe Set-PackageSource {
    # make sure that packagemanagement is loaded
    import-packagemanagement

	it "EXPECTED: Sets The 'NugetTest' Package Source to 'NugetTest2'" {
		(register-packagesource -name "nugettest" -provider "nuget" -location "https://www.nuget.org/api/v2")
		(set-packagesource -name "nugettest" -provider "nuget" -location "https://www.nuget.org/api/v2" -newname "nugettest2" -newlocation "https://www.nuget.org/api/v2").name | should be "nugettest2"
		(unregister-packagesource -name "nugettest2")
	}
	
	it "EXPECTED: Sets The 'NuGetTest' Package Source to 'NugetTest2' After Piping The Provider Then Piping The Entire Source" {
		(get-packageprovider -name "nuget" | register-packagesource -name "nugettest" -location "https://www.nuget.org/api/v2" | set-packagesource -newname "nugettest2" -newlocation "https://www.nuget.org/api/v2").name | should be "nugettest2"
		(unregister-packagesource -name "nugettest2")
	}

	it "EXPECTED: Sets Multiple Package Sources To New Names" {
		(register-packagesource -name "nugettest" -provider "nuget" -location "https://www.nuget.org/api/v2")
		(set-packagesource -name "nugettest" -provider "nuget" -location "https://www.nuget.org/api/v2" -newname "nugettest2" -newlocation "https://www.nuget.org/api/v2").name | should be "nugettest2"
		(unregister-packagesource -name "nugettest2")

		(register-packagesource -name "nugettest3" -provider "nuget" -location "https://www.nuget.org/api/v2")
		(set-packagesource -name "nugettest3" -provider "nuget" -location "https://www.nuget.org/api/v2" -newname "nugettest4" -newlocation "https://www.nuget.org/api/v2").name | should be "nugettest4"
		(unregister-packagesource -name "nugettest4")
	}
}
