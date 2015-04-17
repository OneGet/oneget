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


$longName = "THISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERS";
$packageNames = @("ResolveAlias", "Python");
$minimumVersions = @("1.0", "1.3", "1.5");
$maximumVersions = @("3.5", "4.0");
$destination = "C:\chocolatey\lib";
$pkgSources = @("CHOCOLATEYTEST101", "CHOCOLATEYTEST202", "CHOCOLATEYTEST303");


Describe "Chocolatey: Find-Package" {
    # make sure that packagemanagement is loaded
    import-packagemanagement

    It "EXPECTED: Finds 'Python' Package" {
        (find-package -name "Python" -Provider "chocolatey" -forcebootstrap -source $chocolateySource).name | should match "Python"
    }
	It "EXPECTED: Finds A Combination Of Packages With Various Versions" {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				(find-package -name $x -source $chocolateySource -Provider "chocolatey" -minimumversion $y -maximumversion $z).name | should be $x
				}
			}
		}
	}

	It "EXPECTED: Finds 'Python' Package After Piping The Provider" {
		(get-packageprovider -name "chocolatey" | find-package -name "Python").name | should be "Python"
	}

	It "EXPECTED: -FAILS- To Find Package Due To Too Long Of Name" {
		(find-package -name $longName -Provider "chocolatey" -source $chocolateySource -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Invalid Name" {
		(find-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -Provider "chocolatey" -source $chocolateySource -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Negative Maximum Version Parameter" {
		(find-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -maximumversion "-1.5" -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Negative Minimum Version Parameter" {
		(find-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -minimumversion "-1.5" -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Negative Required Version Parameter" {
		(find-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -requiredversion "-1.5" -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Out Of Bounds Required Version Parameter" {
		(find-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -minimumversion "1.0" -maximumversion "1.5" -requiredversion "2.0" -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Find Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
		(find-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -minimumversion "1.5" -maximumversion "1.0" -EA silentlycontinue) | should throw
	}
}

Describe "Chocolatey: Save-Package" {
	# make sure packagemanagement is loaded
	import-packagemanagement

	it "EXPECTED: Saves 'Python' Package To Packages Directory" {
		(save-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -path $destination)
		(test-path $destination\Python*) | should be $true
		if (Test-Path $destination\python*) {
			rm $destination\python*
		}
	}

	it "EXPECTED: Saves Various Packages With Various Version Parameters To Packages Directory" {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				(save-package -name $x -source $chocolateySource -Provider "chocolatey" -minimumversion $y -maximumversion $z -Path $destination)
				(Test-Path -Path $destination\$x*) | should be $true
				if (Test-Path -Path $destination\$x*) {
					rm $destination\$x*
					}
				}
			}
		}
	}

	It "EXPECTED: Saves 'Python' Package After Having The Provider Piped" {
	(find-package -Name "Python" -Provider "chocolatey" -source $chocolateySource | save-package -Path $destination)
	(Test-Path -Path $destination\Python*) | should be $true
	if (Test-Path -Path $destination\Python*) {
		rm $destination\Python*
		}
	}

	It "EXPECTED: -FAILS- To Save Package Due To Too Long Of Name" {
		(save-package -name $longName -Provider "chocolatey" -source $chocolateySource -Path $destination -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Save Package Due To Invalid Name" {
		(save-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -Provider "chocolatey" -source $chocolateySource -Path $destination -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Save Package Due To Negative Maximum Version Parameter" {
		(save-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -maximumversion "-1.5" -Path $destination -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Save Package Due To Negative Minimum Version Parameter" {
		(save-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -minimumversion "-1.5" -Path $destination -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Save Package Due To Negative Required Version Parameter" {
		(save-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -requiredversion "-1.5" -Path $destination -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Save Package Due To Out Of Bounds Required Version Parameter" {
		(save-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -minimumversion "1.0" -maximumversion "1.5" -requiredversion "2.0" -Path $destination -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Save Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
		(save-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -minimumversion "1.5" -maximumversion "1.0" -Path $destination -EA silentlycontinue) | should throw
	}
}

Describe "Chocolatey: Install-Package" {
	# make sure packagemanagement is loaded
	import-packagemanagement

	it "EXPECTED: Installs 'Python' Package To Packages Directory" {
		(install-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -force)
		(test-path $destination\Python*) | should be $true
		if (Test-Path $destination\Python*) {
			(Remove-Item -Recurse -Force -Path $destination\Python*)
		}
	}

	it "EXPECTED: Installs Various Packages With Various Version Parameters To Packages Directory" {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				(install-package -name $x -source $chocolateySource -Provider "chocolatey" -minimumversion $y -maximumversion $z -force)
				(Test-Path -Path $destination\$x*) | should be $true
				if (Test-Path -Path $destination\$x*) {
					(Remove-Item -Recurse -Force -Path $destination\$x*)
					}
				}
			}
		}
	}

	It "EXPECTED: Installs 'Python' Package After Having The Provider Piped" {
	(find-package -Name "Python" -Provider "chocolatey" -source $chocolateySource | install-package -force)
	(Test-Path -Path $destination\Python*) | should be $true
	if (Test-Path -Path $destination\Python*) {
		(Remove-Item -Recurse -Force -Path $destination\Python*)
		}
	}

	It "EXPECTED: -FAILS- To Install Package Due To Too Long Of Name" {
		(install-package -name $longName -Provider "chocolatey" -source $chocolateySource -force -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Install  Package Due To Invalid Name" {
		(install-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -Provider "chocolatey" -source $chocolateySource -force -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Install Package Due To Negative Maximum Version Parameter" {
		(install-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -maximumversion "-1.5" -force -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Install Package Due To Negative Maximum Version Parameter" {
		(install-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -minimumversion "-1.5" -force -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Install Package Due To Negative Maximum Version Parameter" {
		(install-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -requiredversion "-1.5" -force -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Install Package Due To Out Of Bounds Required Version Parameter" {
		(install-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -minimumversion "1.0" -maximumversion "1.5" -requiredversion "2.0" -force -EA silentlycontinue) | should throw
	}

	It "EXPECTED: -FAILS- To Install Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
		(install-package -Name "Python" -Provider "chocolatey" -source $chocolateySource -minimumversion "1.5" -maximumversion "1.0" -force -EA silentlycontinue) | should throw
	}
}

Describe "Chocolatey: Get-Package" {
	# make sure packagemanagement is loaded
	import-packagemanagement

	it "EXPECTED: Gets The 'ResolveAlias' Package After Installing" {
		(install-package -name "ResolveAlias" -Provider "chocolatey" -source $chocolateySource -force)
		(get-package -name "ResolveAlias" -Provider "chocolatey").name | should be "ResolveAlias"
		if (Test-Path $destination\ResolveAlias*) {
			(Remove-Item -Recurse -Force -Path $destination\ResolveAlias*)
		}
	}

<# this makes no sense
	it "EXPECTED: Gets Various Packages With Various Version Parameters From Packages Directory After Installing" {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				(install-package -name $x -source $chocolateySource -Provider "chocolatey" -minimumversion $y -maximumversion $z -force)
				(Get-Package -name $x -Provider "chocolatey" -minimumversion $y -maximumversion $z).name | should be $x
				if (Test-Path -Path $destination\$x*) {
					(Remove-Item -Recurse -Force -Path $destination\$x*)
					}
				}
			}
		}
	}
#>

	It "EXPECTED: Gets The 'ResolveAlias' Package After Installing And After Piping The Provider" {
		(install-package -name "ResolveAlias" -Provider "chocolatey" -source $chocolateySource -force)
		(get-packageprovider -name "chocolatey" | get-package "ResolveAlias" ).name | should be "ResolveAlias"
		if (Test-Path -Path $destination\ResolveAlias*) {
			(Remove-Item -Recurse -Force -Path $destination\ResolveAlias*)
		}
	}

	It "EXPECTED: -FAILS- To Get Package Due To Too Long Of Name" {
		(install-package -name "ResolveAlias" -Provider "chocolatey" -source $chocolateySource -force)
		(get-package -name $longName -Provider "chocolatey" -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\ResolveAlias*) {
			(Remove-Item -Recurse -Force -Path $destination\ResolveAlias*)
		}
	}

	It "EXPECTED: -FAILS- To Get Package Due To Invalid Name" {
		(install-package -name "ResolveAlias" -Provider "chocolatey" -source $chocolateySource -force)
		(get-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -Provider "chocolatey" -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\ResolveAlias*) {
			(Remove-Item -Recurse -Force -Path $destination\ResolveAlias*)
		}
	}

	It "EXPECTED: -FAILS- To Get Package Due To Out Of Bounds Required Version Parameter" {
		(install-package -name "ResolveAlias" -Provider "chocolatey" -source $chocolateySource -force)
		(get-package -name "ResolveAlias" -Provider "chocolatey" -maximumversion "4.0" -minimumversion "1.0" -requiredversion "5.0" -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\ResolveAlias*) {
			(Remove-Item -Recurse -Force -Path $destination\ResolveAlias*)
		}
	}

	It "EXPECTED: -FAILS- To Get Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
		(install-package -name "ResolveAlias" -Provider "chocolatey" -source $chocolateySource -force)
		(get-package -name "ResolveAlias" -Provider "chocolatey" -maximumversion "3.0" -minimumversion "4.0" -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\ResolveAlias*) {
			(Remove-Item -Recurse -Force -Path $destination\ResolveAlias*)
		}
	}
}

Describe "Chocolatey: Uninstall-Package" {
	# make sure packagemanagement is loaded
	import-packagemanagement

	it "EXPECTED: Uninstalls The'ResolveAlias' Package From The Packages Directory" {
		(install-package -name "ResolveAlias" -Provider "chocolatey" -source $chocolateySource -force)
		(uninstall-package -name "ResolveAlias" -Provider "chocolatey" -force)
		(Test-Path -Path $destination\ResolveAlias*) | should be $false
	}

<# makes no sense
	it "EXPECTED: Uninstalls Various Packages With Various Versions From The Packages Directory" {
		foreach ($x in $packageNames) {
			foreach ($y in $minimumVersions) {
				foreach ($z in $maximumVersions) {
				(install-package -name $x -source $chocolateySource -Provider "chocolatey" -minimumversion $y -maximumversion $z -force)
				(uninstall-package -name $x -Provider "chocolatey" -minimumversion $y -maximumversion $z -force)
				(Test-Path -Path $destination\$x*) | should be $false
				}
			}
		}
	}
#>

	It "EXPECTED: Uninstalls The'ResolveAlias' Package From The Packages Directory After Having The Package Piped" {
		(install-package -name "ResolveAlias" -Provider "chocolatey" -source $chocolateySource -force)
		(get-package -name "ResolveAlias" -Provider "chocolatey" | uninstall-package -force)
		(Test-Path -Path $destination\ResolveAlias*) | should be $false	
	}

	It "EXPECTED: -FAILS- To Uninstall Package Due To Too Long Of Name" {
		(install-package -name "ResolveAlias" -Provider "chocolatey" -source $chocolateySource -force)
		(uninstall-package -name $longName -Provider "chocolatey" -force -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\ResolveAlias*) {
			(Remove-Item -Recurse -Force -Path $destination\ResolveAlias*)
		}
	}

	It "EXPECTED: -FAILS- To Uninstall Package Due To Invalid Name" {
		(install-package -name "ResolveAlias" -Provider "chocolatey" -source $chocolateySource -force)
		(uninstall-package -name "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER" -Provider "chocolatey" -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\ResolveAlias*) {
			(Remove-Item -Recurse -Force -Path $destination\ResolveAlias*)
		}
	}

	It "EXPECTED: -FAILS- To Uninstall Package Due To Out Of Bounds Required Version Parameter" {
		(install-package -name "ResolveAlias" -Provider "chocolatey" -source $chocolateySource -force)
		(uninstall-package -name "ResolveAlias" -Provider "chocolatey" -maximumversion "4.0" -minimumversion "1.0" -requiredversion "5.0" -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\ResolveAlias*) {
			(Remove-Item -Recurse -Force -Path $destination\ResolveAlias*)
		}
	}

	It "EXPECTED: -FAILS- To Uninstall Package Due To Minimum Version Parameter Greater Than Maximum Version Parameter" {
		(install-package -name "ResolveAlias" -Provider "chocolatey" -source $chocolateySource -force)
		(uninstall-package -name "ResolveAlias" -Provider "chocolatey" -maximumversion "3.0" -minimumversion "4.0" -EA silentlycontinue) | should throw
		if (Test-Path -Path $destination\ResolveAlias*) {
			(Remove-Item -Recurse -Force -Path $destination\ResolveAlias*)
		}
	}
}

Describe "Chocolatey: Get-PackageProvider" {
	# make sure that packagemanagement is loaded
    import-packagemanagement

	it "EXPECTED: Gets The 'Chocolatey' Package Provider" {
		(get-packageprovider -name "chocolatey" -forcebootstrap -force).name | should be "chocolatey"
	}
}

Describe "Chocolatey: Register-PackageSource" {
    # make sure that packagemanagement is loaded
    import-packagemanagement

	it "EXPECTED: Registers A New Package Source 'ChocolateyTest'" {
		(register-packagesource -name "ChocolateyTest" -Provider "chocolatey" -location "http://chocolatey.org/api/v2" -SkipValidate).name | should be "ChocolateyTest"
		(unregister-packagesource -name "ChocolateyTest")
	}

	it "EXPECTED: Registers Multiple Package Sources" {
		foreach ($x in $pkgSources) {
			(register-packagesource -name $x -Provider "chocolatey" -location "http://chocolatey.org/api/v2" -SkipValidate).name | should be $x
			(unregister-packagesource -name $x)
		}
	}
	it "EXPECTED: Registers A 'ChocolateyTest' Package Source After Having The Provider Piped" {
		(get-packageprovider -name "chocolatey" | register-packagesource -name "ChocolateyTest" -location "http://chocolatey.org/api/v2").ProviderName | should be "chocolatey"
		(unregister-packagesource -name "ChocolateyTest")
	}
}

Describe "Chocolatey: Unregister-PackageSource" {
    # make sure that packagemanagement is loaded
    import-packagemanagement

	it "EXPECTED: Unregisters The 'ChocolateyTest' Package Source" {
		(register-packagesource -name "ChocolateyTest" -Provider "chocolatey" -location "http://chocolatey.org/api/v2" -SkipValidate)
		(unregister-packagesource -name "ChocolateyTest").name | should BeNullOrEmpty
	}

	it "EXPECTED: Unregisters Multiple Package Sources" {
		foreach ($x in $pkgSources) {
			(register-packagesource -name $x -Provider "chocolatey" -location "http://chocolatey.org/api/v2" -SkipValidate)
			(unregister-packagesource -name $x).name | should BeNullOrEmpty
		}
	}
	it "EXPECTED: Unregisters A 'ChocolateyTest' Package Source After Having The Provider Piped" {
		(get-packageprovider -name "chocolatey" | register-packagesource -name "ChocolateyTest" -location "http://chocolatey.org/api/v2" -SkipValidate)
		(unregister-packagesource -name "ChocolateyTest").name | should BeNullOrEmpty
	}
}

Describe "Chocolatey: Set-PackageSource" {
    # make sure that packagemanagement is loaded
    import-packagemanagement

	it "EXPECTED: Sets The 'ChocolateyTest' Package Source to 'ChocolateyTest2'" {
		(register-packagesource -name "ChocolateyTest" -Provider "chocolatey" -location "https://www.nuget.org/api/v2" -SkipValidate)
		(set-packagesource -name "ChocolateyTest" -Provider "chocolatey" -location "https://www.nuget.org/api/v2" -newname "ChocolateyTest2" -newlocation "https://www.nuget.org/api/v2").name | should be "ChocolateyTest2"
		(unregister-packagesource -name "ChocolateyTest2")
	}
	
	it "EXPECTED: Sets The 'ChocolateyTest' Package Source to 'ChocolateyTest2' After Piping The Provider Then Piping The Entire Source" {
		(get-packageprovider -name "chocolatey" | register-packagesource -SkipValidate -name "ChocolateyTest" -location "https://www.nuget.org/api/v2" | set-packagesource -newname "ChocolateyTest2" -newlocation "https://www.nuget.org/api/v2").name | should be "ChocolateyTest2"
		(unregister-packagesource -name "ChocolateyTest2")
	}

	it "EXPECTED: Sets Multiple Package Sources To New Names" {
		(register-packagesource -SkipValidate -name "ChocolateyTest" -Provider "chocolatey" -location "https://www.nuget.org/api/v2")
		(set-packagesource -name "ChocolateyTest" -Provider "chocolatey" -location "https://www.nuget.org/api/v2" -newname "ChocolateyTest2" -newlocation "https://www.nuget.org/api/v2").name | should be "ChocolateyTest2"
		(unregister-packagesource -name "ChocolateyTest2")

		(register-packagesource -SkipValidate -name "ChocolateyTest3" -Provider "chocolatey" -location "https://www.nuget.org/api/v2")
		(set-packagesource -name "ChocolateyTest3" -Provider "chocolatey" -location "https://www.nuget.org/api/v2" -newname "ChocolateyTest4" -newlocation "https://www.nuget.org/api/v2").name | should be "ChocolateyTest4"
		(unregister-packagesource -name "ChocolateyTest4")
	}
}
