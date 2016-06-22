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

$ProgramProviderInstalledPath = "$Env:ProgramFiles\PackageManagement\ProviderAssemblies"

$LocalAppData = [Environment]::GetFolderPath("LocalApplicationData")
$UserProviderInstalledPath = "$($LocalAppData)\PackageManagement\ProviderAssemblies"

$ProgramModulePath = "$Env:ProgramFiles\WindowsPowerShell\Modules"

$mydocument = [Environment]::GetFolderPath("MyDocuments")
$UserModuleFolder = "$($mydocument)\WindowsPowerShell\Modules"

$InternalGallery = "https://www.powershellgallery.com/api/v2/"
$InternalSource = 'OneGetTestSource'

#make sure the package repository exists
$a=Get-PackageSource -ForceBootstrap| select Name, Location, ProviderName
    
$found = $false
foreach ($item in $a)
{       
    #name contains "." foo.bar for example for the registered sources internally
    if($item.ProviderName -eq "PowerShellGet")
    {
        if ($item.Location -eq $InternalGallery) {
            Unregister-PackageSource $item.Name -Provider "PowerShellGet" -ErrorAction SilentlyContinue
        }
    }
}

Register-PackageSource -Name $InternalSource -Location $InternalGallery -ProviderName 'PowerShellGet' -Trusted -ForceBootstrap -ErrorAction SilentlyContinue
# ------------------------------------------------------------------------------
# Actual Tests:

Describe "import-packageprovider" -Tags @('BVT', 'DRT'){
    # make sure that packagemanagement is loaded
    import-packagemanagement
    
    It "Import -force 'PowerShellGet', a builtin package provider, Expect succeed" {
        #avoid popup for installing nuget-any.exe
        Find-PackageProvider -force
        (Import-PackageProvider  'PowerShellGet' -verbose -force).name | should match "PowerShellGet"
    }
              
        
    It "Import a PowerShell package provider Expect succeed" {
        (get-packageprovider -name "OneGetTest" -list).name | should match "OneGetTest"
        $x = PowerShell '(Import-PackageProvider  OneGetTest -WarningAction SilentlyContinue).Name'
        $x | should match "OneGetTest"

        $x = PowerShell '(Import-PackageProvider  OneGetTest -WarningAction SilentlyContinue -force).Name'
        $x | should match "OneGetTest"
    } 

    It "Import 'OneGetTestProvider' CSharp package provider with filePath from programs folder, Expect succeed" {
    
        $path = "$($ProgramProviderInstalledPath)\Microsoft.PackageManagement.OneGetTestProvider.dll" 
        $path | should Exist

        $job=Start-Job -ScriptBlock {
            param($path) import-packageprovider -name $path;
         } -ArgumentList @($path)

        $a= $job | Receive-Job -Wait
        $a.Name | should match "OneGetTestProvider"

    } 
          
    It "Import 'PSChained1Provider' PowerShell package provider with filePath from programfiles folder, Expect succeed" {
 
        $path = "$($ProgramModulePath)\PSChained1Provider.psm1" 
        $path | should Exist

        $job=Start-Job -ScriptBlock {
            param($path) import-packageprovider -name $path; 
         } -ArgumentList @($path)

        $a= $job | Receive-Job -Wait
        $a.Name | should match "PSChained1Provider"
    }   

          
    It "Import a CSharp package provider with filePath from user folder -force, Expect succeed" {
        $path = "$($UserProviderInstalledPath)\Microsoft.PackageManagement.OneGetTestProvider.dll" 
        $path | should Exist         
        
        $job=Start-Job -ScriptBlock {
            param($path) import-packageprovider -name $path; 
         } -ArgumentList @($path)

        $a= $job | Receive-Job -Wait
        $a.Name | should match "OneGetTestProvider"
    }

    It "Import a PowerShell package provider with filePath from user folder -force, Expect succeed" {

         $path = "$($UserModuleFolder)\PSChained1Provider.psm1"
         $path  | should Exist

         $job=Start-Job -ScriptBlock {
            param($path) import-packageprovider -name $path; 
            } -ArgumentList @($path)

        $a= $job | Receive-Job -Wait
        $a.Name | should match "PSChained1Provider"
    }

    It "Import a PowerShell package provider with -force, Expect succeed" {

         $path = "$($UserModuleFolder)\PSChained1Provider.psm1"
         $path  | should Exist

         $newPath = "$($UserModuleFolder)\MyTest.psm1"
         Copy-Item -Path $path  -Destination $newPath -Force
                  

         $job=Start-Job -ScriptBlock {
            param($newPath) import-packageprovider -name $newPath -Force; 
            } -ArgumentList @($newPath)

        $a= $job | Receive-Job -Wait
        $a.Name | should match "PSChained1Provider"
         
        $job=Start-Job -ScriptBlock {

            param($newPath)

                import-packageprovider -name $newPath -Force            
                Set-Content -Path $newPath -Value "#test"
                import-packageprovider -name $newPath -Force 

            } -ArgumentList @($newPath)


        Receive-Job -Wait -Job $job -ErrorVariable theError 2>&1
        $theError.FullyQualifiedErrorId | should be "FailedToImportProvider,Microsoft.PowerShell.PackageManagement.Cmdlets.ImportPackageProvider"      
    }
    It "Import 'OneGetTest' PowerShell package provider that has multiple versions, Expect succeed" {
        #check all version of OneGetTest is listed
        $x = get-packageprovider "OneGetTest" -ListAvailable
       

        $x | ?{ $_.Version.ToString() -eq "9.9.0.0" } | should not BeNullOrEmpty          
        $x | ?{ $_.Version.ToString() -eq "3.5.0.0" } | should not BeNullOrEmpty          
        $x | ?{ $_.Version.ToString() -eq "1.1.0.0" } | should not BeNullOrEmpty   
        
        #latest one is imported
        $y = powershell '(import-packageprovider -name "OneGetTest").Version.Tostring()' 
        $y | should match  "9.9.0.0"
    } 
}



Describe "import-packageprovider Error Cases" -Tags @('BVT', 'DRT'){
    # make sure that packagemanagement is loaded
    import-packagemanagement

     It "Expected error when importing wildcard chars 'OneGetTest*" {
        $Error.Clear()
        $msg = powershell 'import-packageprovider -name OneGetTest* -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "InvalidParameter,Microsoft.PowerShell.PackageManagement.Cmdlets.ImportPackageProvider"     
    }

  It "EXPECTED:  returns an error when inputing a bad version format" {
        $Error.Clear()
        $msg = powershell 'import-packageprovider -name Gistprovider -RequiredVersion BOGUSVERSION -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "InvalidVersion,Microsoft.PowerShell.PackageManagement.Cmdlets.ImportPackageProvider"
    }

  It "EXPECTED:  returns an error when asking for a provider that does not exist" {
        $Error.Clear()
        $msg = powershell 'import-packageprovider -name NOT_EXISTS  -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "NoMatchFoundForCriteria,Microsoft.PowerShell.PackageManagement.Cmdlets.ImportPackageProvider"
    }
   
   It "EXPECTED:  returns an error when asking for a provider with file full path and version" {
        $Error.Clear()
        $msg = powershell {import-packageprovider -name "$($ProgramModulePath)\PSChained1Provider.psm1" -RequiredVersion 9.9.9  -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId}
        $msg | should be "FullProviderFilePathVersionNotAllowed,Microsoft.PowerShell.PackageManagement.Cmdlets.ImportPackageProvider"
    }
 

   It "EXPECTED:  returns an error when asking for a provider with RequiredVersoin and MinimumVersion" {
        $Error.Clear()
        $msg = powershell 'import-packageprovider -name PowerShellGet -RequiredVersion 1.0 -MinimumVersion 2.0  -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "VersionRangeAndRequiredVersionCannotBeSpecifiedTogether,Microsoft.PowerShell.PackageManagement.Cmdlets.ImportPackageProvider"
    }

   It "EXPECTED:  returns an error when asking for a provider with RequiredVersoin and MaximumVersion" {
        $Error.Clear()
        $msg = powershell 'import-packageprovider -name PowerShellGet -RequiredVersion 1.0 -MaximumVersion 2.0  -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "VersionRangeAndRequiredVersionCannotBeSpecifiedTogether,Microsoft.PowerShell.PackageManagement.Cmdlets.ImportPackageProvider"
    }

   It "EXPECTED:  returns an error when asking for a provider with a MinimumVersion greater than MaximumVersion" {
        $Error.Clear()
        $msg = powershell 'import-packageprovider -name PowerShellGet -MaximumVersion 1.0 -MinimumVersion 2.0 -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "MinimumVersionMustBeLessThanMaximumVersion,Microsoft.PowerShell.PackageManagement.Cmdlets.ImportPackageProvider"
    }

   It "EXPECTED:  returns an error when asking for a provider with MinimumVersion that does not exist" {
        $Error.Clear()
        $msg = powershell 'Import-packageprovider -name OneGetTest -MinimumVersion 20.2 -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "NoMatchFoundForCriteria,Microsoft.PowerShell.PackageManagement.Cmdlets.ImportPackageProvider"
    }

   It "EXPECTED:  returns an error when asking for a provider with MaximumVersion that does not exist" {
        $Error.Clear()
        $msg = powershell 'Import-packageprovider -name OneGetTest -MaximumVersion 0.2 -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "NoMatchFoundForCriteria,Microsoft.PowerShell.PackageManagement.Cmdlets.ImportPackageProvider"
    }

   It "EXPECTED:  returns an error when asking for a provider that has name with wildcard and version" {
        $Error.Clear()
        $msg = powershell 'Import-packageprovider -name "OneGetTest*" -RequiredVersion 4.5 -force -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "MultipleNamesWithVersionNotAllowed,Microsoft.PowerShell.PackageManagement.Cmdlets.ImportPackageProvider"
    }
    
}


Describe "Import-PackageProvider with OneGetTest that has 3 versions: 1.1, 3.5, and 9.9." -Tags @('BVT', 'DRT') {
    # make sure that packagemanagement is loaded
    import-packagemanagement
    $ps = [PowerShell]::Create()
    $ps.AddScript("Import-PackageProvider -Name OneGetTest -RequiredVersion 9.9; Find-Package -ProviderName OneGetTest")
    $ps.Invoke()


    It "EXPECTED: success 'import OneGetTest -requiredVersion 3.5'" {
        powershell '(Import-packageprovider -name OneGetTest -requiredVersion 3.5 -WarningAction SilentlyContinue).Version.ToString()' | should match "3.5.0.0"

        # test that if we call a function with error, powershell does not hang for the provider
        $warningMsg = powershell '(Import-packageprovider -name OneGetTest -requiredVersion 3.5) | Out-Null; Get-Package -ProviderName OneGetTest 3>&1'
        $result = $warningMsg[0]

        if ($PSCulture -eq "en-US") {
            foreach($w in $warningMsg) 
            { 
                if($w -match 'WARNING: Cannot bind parameter')
                {
                    $result = $w
                }
        
            }
            $result.StartsWith('WARNING: Cannot bind parameter') | should be $true
        }
    }

    It "EXPECTED: success 'Import OneGetTest -requiredVersion 3.5 and then 9.9 -force'" {
        $a = powershell {(Import-packageprovider -name OneGetTest -requiredVersion 3.5) > $null; (Import-packageprovider -name OneGetTest -requiredVersion 9.9 -force)} 
        $a.Version.ToString()| should match "9.9.0.0"
    }

    It "EXPECTED: success 'import OneGetTest with MinimumVersion and MaximumVersion'" {
        powershell '(Import-packageprovider -name OneGetTest -MinimumVersion 1.2 -MaximumVersion 5.0 -WarningAction SilentlyContinue).Version.ToString()' | should match "3.5.0.0"
    }
    
    It "EXPECTED: success 'OneGetTest with MaximumVersion'" {
        powershell '(Import-packageprovider -name OneGetTest -MaximumVersion 3.5 -WarningAction SilentlyContinue).Version.ToString()' | should match "3.5.0.0"
    }
    
    It "EXPECTED: success 'OneGetTest with MinimumVersion'" {
        powershell '(Import-packageprovider -name OneGetTest -MinimumVersion 2.2 -WarningAction SilentlyContinue).Version.ToString()' | should match "9.9.0.0"
    }

    It "EXPECTED: success 'OneGetTest Find-Package with Progress'" -skip {
        $ps.Streams.Progress.Count | Should be 6
        $ps.Streams.Progress[1].ActivityId | Should Be 1
        $ps.Streams.Progress[1].Activity | Should match "Getting some progress"

        $ps.Streams.Progress[2].RecordType -eq [System.Management.Automation.ProgressRecordType]::Processing | should be true
        $ps.Streams.Progress[2].PercentComplete | should be 22

        $ps.Streams.Progress[5].RecordType -eq [System.Management.Automation.ProgressRecordType]::Completed | should be true
        $ps.Streams.Progress[5].PercentComplete | should be 100
    }

    It "EXPECTED: success 'OneGetTest Find-Package returns correct TagId'" -skip {
        $ps = [PowerShell]::Create()
        $ps.AddScript("Import-PackageProvider -Name OneGetTest -RequiredVersion 9.9; Find-Package -ProviderName OneGetTest")
        $result = $ps.Invoke() | Select -First 1

        $result.TagId | should match "MyVeryUniqueTagId"
    }

    It "EXPECTED: success 'OneGetTest Get-Package returns correct package object using swidtag'" -skip {
        $ps = [PowerShell]::Create()
        $null = $ps.AddScript("`$null = Import-PackageProvider -Name OneGetTest -RequiredVersion 9.9; Get-Package -ProviderName OneGetTest")
        $result = $ps.Invoke() 
        
        $result.Count | should Be 2

        ($result | Select -Last 1).TagId | should match "jWhat-jWhere-jWho-jQuery"
    }
}

<#
Describe "Import-PackageProvider with OneGetTestProvider that has 2 versions: 4.5, 6.1" -Tags @('BVT', 'DRT') {
    # make sure that packagemanagement is loaded
    import-packagemanagement

    # install onegettestprovider
    Install-PackageProvider -Name OneGetTestProvider -RequiredVersion 4.5.0.0 -Source $InternalGallery
    Install-PackageProvider -Name OneGetTestProvider -RequiredVersion 6.1.0.0 -Source $InternalGallery

    It "EXPECTED: Get-PackageProvider -ListAvailable succeeds" {
        $providers = Get-PackageProvider -ListAvailable
        ($providers | Where-Object {$_.Name -eq 'OneGetTest'}).Count | should match 3
        ($providers | Where-Object {$_.Name -eq 'OneGetTestProvider'}).Count | should match 2
    }

    It "EXPECTED: Get-PackageProvider -ListAvailable succeeds even after importing gist provider" {
	Install-PackageProvider GistProvider -Source $InternalGallery
        Import-PackageProvider Gist
        $providers = Get-PackageProvider -ListAvailable
        ($providers | Where-Object {$_.Name -eq 'OneGetTest'}).Count | should match 3
        ($providers | Where-Object {$_.Name -eq 'OneGetTestProvider'}).Count | should match 2
    }

    It "EXPECTED: success 'import OneGetTestProvider -requiredVersion 4.5'" {
        Import-PackageProvider -Name OneGetTestProvider -RequiredVersion 4.5 -Force
        (Get-PackageProvider OneGetTestProvider).Version.ToString() | should match '4.5.0.0'
    }

    It "EXPECTED: success 'import OneGetTestProvider with MinimumVersion and MaximumVersion'" {
        Import-packageprovider -name OneGetTestProvider -MinimumVersion 4.6 -MaximumVersion 6.2 -Force
        (Get-PackageProvider OneGetTestProvider).Version.ToString() | should match '6.1.0.0'
    }
    
    It "EXPECTED: success 'import OneGetTestProvider with MaximumVersion'" {
        Import-packageprovider -name OneGetTestProvider -MaximumVersion 4.6 -Force
        (Get-PackageProvider OneGetTestProvider).Version.ToString() | should match '4.5.0.0'
    }
    
    It "EXPECTED: success 'OneGetTestProvider with MinimumVersion'" {
        Import-packageprovider -name OneGetTestProvider -MinimumVersion 6.0.5 -Force

        (Get-PackageProvider OneGetTestProvider).Version.ToString() | should match '6.1.0.0'
    }

    It "EXPECTED: success 'Import OneGetTestProvider -requiredVersion 4.5 and then 6.1 -force'" {
        Import-PackageProvider -Name OneGetTestProvider -RequiredVersion 4.5 -Force;
        Import-PackageProvider -Name OneGetTestProvider -RequiredVersion 6.1 -Force;
        (Get-PackageProvider OneGetTestProvider).Version.ToString() | should match '6.1.0.0'
    }

    It "EXPECTED: success 'Import OneGetTestProvider -MinimumVersion 4.5 and then MaximumVersion 5.0 -force'" {
        Import-PackageProvider -Name OneGetTestProvider -MinimumVersion 4.5 -Force;
        (Get-PackageProvider OneGetTestProvider).Version.ToString() | should match '6.1.0.0'
        Import-PackageProvider -Name OneGetTestProvider -MaximumVersion 5.0 -Force;
        (Get-PackageProvider OneGetTestProvider).Version.ToString() | should match '4.5.0.0'
    }
}
#>