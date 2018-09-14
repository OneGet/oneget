﻿#
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
# ------------------ PackageManagement Test  -----------------------------------

$InternalGallery = "https://dtlgalleryint.cloudapp.net/api/v2/"
$InternalSource = 'OneGetTestSource'


Describe "PackageManagement Acceptance Test" -Tags "Feature" {
 
 BeforeAll{
    Register-PackageSource -Name Nugettest -provider NuGet -Location https://www.nuget.org/api/v2 -force
    Register-PackageSource -Name $InternalSource -Location $InternalGallery -ProviderName 'PowerShellGet' -Trusted -ErrorAction SilentlyContinue

 }
    It "get-packageprovider" {
       
        $gpp = Get-PackageProvider
        
        $gpp | ?{ $_.name -eq "NuGet" } | should not BeNullOrEmpty
   
        $gpp | ?{ $_.name -eq "PowerShellGet" } | should not BeNullOrEmpty   
    }


    It "find-packageprovider PowerShellGet" {
        $fpp = (Find-PackageProvider -Name "PowerShellGet" -force).name 
        $fpp -contains "PowerShellGet" | should be $true
    }

     It "install-packageprovider, Expect succeed" {
        $ipp = (install-PackageProvider -name gistprovider -force -source $InternalSource -Scope CurrentUser).name 
        $ipp -contains "gistprovider" | should be $true      
    }
       

    it "Find-package"  {
        $f = Find-Package -ProviderName NuGet -Name jquery -source Nugettest
        $f.Name -contains "jquery" | should be $true
	}

    it "Install-package"  {
        $i = install-Package -ProviderName NuGet -Name jquery -force -source Nugettest -Scope CurrentUser 
        $i.Name -contains "jquery" | should be $true
	}

    it "Get-package"  {
        $g = Get-Package -ProviderName NuGet -Name jquery
        $g.Name -contains "jquery" | should be $true
	}

    it "save-package"  {
        $s = save-Package -ProviderName NuGet -Name jquery -path $TestDrive -force -source Nugettest
        $s.Name -contains "jquery" | should be $true
	}

    it "uninstall-package"  {
        $u = uninstall-Package -ProviderName NuGet -Name jquery
        $u.Name -contains "jquery" | should be $true
	}
}
