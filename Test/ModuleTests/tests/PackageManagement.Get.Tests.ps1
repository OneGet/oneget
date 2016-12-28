#
# Copyright (c) Microsoft Corporation.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.
#
$CurrentDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path

.  "$CurrentDirectory\..\OneGetTestHelper.ps1"

#
# Pre-Requisite: MyTestPackage.12.0.1.1, MyTestPackage.12.0.1, MyTestPackage.15.2.1 packages are available under the $LocalRepositoryPath. 
# It's been taken care of by SetupPackageManagementTest
#
 
# Calling the setup function 
SetupPackageManagementTest

$AdditionalParameters = @{"Destination" = $DestinationPath}
$AdditionalParameterCimInstanceArray = ConvertHashtableToArryCimInstance $AdditionalParameters

Describe -Name "PackageManagement Get-TargetResource Basic Test" -Tags "BVT" {

    BeforeEach {    
        #Remove all left over files if exists
        Remove-Item "$($DestinationPath)" -Recurse -Force -ErrorAction SilentlyContinue
    }

    AfterEach {
 
    }     

    Context "PackageManagement Get-TargetResource BVT" {

        #Mock Set-TargetResource/PackageManagement DSC Resource. The tests under this context use the below mock function
        
        Mock Set-TargetResource  {

            #Nuget package folder name format: MyTestPackage.12.0.1, i.e., name + version
            $package = $name + "." + $RequiredVersion+ ".nupkg"

            #MyTestPackage.12.0.1.1
            $path = "$($DestinationPath)\$($name).$($RequiredVersion)" 

            if ($Ensure -ieq "Present") {
         
                if (!(Test-Path -path $path)) {New-Item $path -Type Directory}


                #Copy the $package folder to your destination folder
                Copy-Item -Path "$($LocalRepositoryPath)\$package" -Destination "$($path)" -Recurse -Force
            }
            else {

                #Delete the $package folder
                Remove-Item "$($path)\$package" -Recurse -Force  -ErrorAction SilentlyContinue
            }            
        }

        It "Get-TargetResource with the Mandatory Parameters: Check Absent" {

            # Calling Get-TargetResource in the NugetPackage resource 
            $result = MSFT_PackageManagement\Get-TargetResource -Name "MyTestPackage" -Verbose

            # Validate the result
            $result.Ensure | should be "Absent"
        }

        It "Get-TargetResource with the Mandatory Parameters: Check Present" {
         
            Set-TargetResource -name "MyTestPackage" -RequiredVersion "12.0.1" -Ensure "Present" -AdditionalParameters $AdditionalParameterCimInstanceArray -Verbose

            $result = MSFT_PackageManagement\Get-TargetResource -Name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray  -ErrorVariable ev

            #Validate the returned results
            $result.Ensure | should be "Present"
            $result.Name | should be "MyTestPackage"
            $result.ProviderName | should be "NuGet"
            $result.RequiredVersion | should be "12.0.1"
            ($result.Source).StartsWith($DestinationPath) | should be $true
        }

        It "Get-TargetResource with RequiredVersion: Check Present" {
            
            Set-TargetResource -name "MyTestPackage" -RequiredVersion "12.0.1" -Ensure "Present" -AdditionalParameters $AdditionalParameterCimInstanceArray -Verbose

            #provide a req version that exists, expect ensure=Present
            $result = MSFT_PackageManagement\Get-TargetResource -Name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray -RequiredVersion "12.0.1" -ErrorVariable ev

            #Validate the returned results
            $result.Ensure | should be "Present"
            $result.Name | should be "MyTestPackage"
            $result.RequiredVersion | should be "12.0.1"    
        }

        It "Get-TargetResource with Non-exist RequiredVersion: Check Absent" {
            
            Set-TargetResource -name "MyTestPackage" -RequiredVersion "12.0.1" -Ensure "Present" -Verbose -AdditionalParameters $AdditionalParameterCimInstanceArray

            #Provide a req version does not exist, expect Ensure=Absent
            $result = MSFT_PackageManagement\Get-TargetResource -Name "MyTestPackage" -RequiredVersion "10.11.12" -ErrorVariable ev -AdditionalParameters $AdditionalParameterCimInstanceArray

            #Validate the returned results
            $result.Ensure | should be "Absent"  
        }

        It "Get-TargetResource with MaximumVersion: Check Present" {
            
            Set-TargetResource -name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray -RequiredVersion "12.0.1.1" -Ensure "Present" -Verbose
            Set-TargetResource -name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray -RequiredVersion "12.0.1" -Ensure "Present" -Verbose
            Set-TargetResource -name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray -RequiredVersion "15.2.1" -Ensure "Present" -Verbose

            $result = MSFT_PackageManagement\Get-TargetResource -Name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray  -MaximumVersion "19.9" -ErrorVariable ev

            $result.Ensure | should be "Present"
            $result.RequiredVersion | should be "15.2.1"  #1.8.2 is the only package -le maximumversion 1.9.9
        }

        It "Get-TargetResource MinimumVersion: Check Present" {
            
            Set-TargetResource -name "MyTestPackage" -RequiredVersion "12.0.1.1" -Ensure "Present" -Verbose -AdditionalParameters $AdditionalParameterCimInstanceArray
            Set-TargetResource -name "MyTestPackage" -RequiredVersion "12.0.1" -Ensure "Present" -Verbose -AdditionalParameters $AdditionalParameterCimInstanceArray
            Set-TargetResource -name "MyTestPackage" -RequiredVersion "15.2.1" -Ensure "Present" -Verbose -AdditionalParameters $AdditionalParameterCimInstanceArray

            $result = MSFT_PackageManagement\Get-TargetResource -Name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray  -MinimumVersion "12.0.1"

            $result.Ensure | should be "Present"
            $result.RequiredVersion | should be "15.2.1"  #Get-package will return the latest version
        }

        It "Get-TargetResource MinimumVersion and MaximumVersion: Check Present" {
            
            Set-TargetResource -name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray -RequiredVersion "12.0.1.1" -Ensure "Present" -Verbose
            Set-TargetResource -name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray -RequiredVersion "12.0.1" -Ensure "Present" -Verbose
            Set-TargetResource -name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray -RequiredVersion "15.2.1" -Ensure "Present" -Verbose

            #will return the latest, ie 15.2.1
            $result = MSFT_PackageManagement\Get-TargetResource -Name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray  -MinimumVersion "15.0"  -MaximumVersion "19.0"

            $result.Ensure | should be "Present"
            $result.RequiredVersion | should be "15.2.1"  
        }
        
    }#context
}#Describe

Describe -Name "PackageManagement Get-Dscconfiguration Error Cases" -Tags "RI" {


    BeforeEach {

        #Remove all left over files if exists
        Remove-Item "$($DestinationPath)" -Recurse -Force -ErrorAction SilentlyContinue
    }

    AfterEach {

    }

    #Mock Set-TargetResource/NugetPackage DSC Resource 
    Mock Set-TargetResource  {

        #Nuget package folder name format: MyTestPackage.12.0.1, i.e., name + version
        $package = $Name + "." + $RequiredVersion

        if ($Ensure -ieq "Present") {

            #Copy the $package folder to your destination folder
            Copy-Item -Path "$($LocalRepositoryPath)\$package" -Destination "$($DestinationPath)\$package" -Recurse -Force
        }
        else {

            #Delete the $package folder
            Remove-Item "$($DestinationPath)\$package" -Recurse -Force -ErrorAction SilentlyContinue
        }
            
    }

    It "Get-TargetResource with Max, Req and Min Verion: Check Error" {

       $result = MSFT_PackageManagement\Get-TargetResource -Name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray  `
                           -MinimumVersion "2.1.3" -RequiredVersion "1.1.1" -MaximumVersion "2.3.5" `
                           -ErrorVariable ev

      ($ev -ne $null) | should be $true
      $ev[0].FullyQualifiedErrorId | should be "VersionRangeAndRequiredVersionCannotBeSpecifiedTogether,Microsoft.PowerShell.PackageManagement.Cmdlets.GetPackage"
    }
    
    It "Get-TargetResource with Max and Min Verion: Check Error" {
    
      $result = MSFT_PackageManagement\Get-TargetResource -Name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray  `
                          -MinimumVersion "5.0" -MaximumVersion "2.5" `
                          -ErrorVariable ev

      ($ev -ne $null) | should be $true
      $ev[0].FullyQualifiedErrorId | should be "NoMatchFound,Microsoft.PowerShell.PackageManagement.Cmdlets.GetPackage"
    }
}
