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

Describe -Name  "PackageManagement Test-TargetResource Basic Tests" -Tags "BVT"{

    BeforeEach {

        #Remove all left over files if exists
        Remove-Item "$($DestinationPath)" -Recurse -Force -ErrorAction SilentlyContinue
    }

    AfterEach {

    }

      
    Context "PackageManagement Test-TargetResource with Mandatory Parameters" {

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

        It "Test-TargetResource: Check False" {

            # Because 'BeforeEach' removes all packages, there is no package left in the $DestinationPath. 
            # It is expected Test-Target* returns false for ensure='Present'
            #
            # Calling Test-TargetResource in the NugetPackage resource 
            $result = MSFT_PackageManagement\Test-TargetResource -Name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray 

            # Validate the result
            $result | should be $false
        }

        It "Test-TargetResource: Check True" {

            # Calling Test-TargetResource in the NugetPackage resource 
            $result = MSFT_PackageManagement\Test-TargetResource -Name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray -Ensure Absent 

            # Validate the result
            $result | should be $true
        }

        It "Test-TargetResource with RequiredVersion: Check True" {
            
            Set-TargetResource -name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray  -RequiredVersion "12.0.1" -Ensure "Present" -Verbose

            $result = MSFT_PackageManagement\Test-TargetResource -Name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray  -RequiredVersion "12.0.1" 

            #Validate the returned results
            $result | should be $true
        }

        
        It "Test-TargetResource with InstalledVersion 12.0.1 but RequiredVersion 12.0.1.1: Check False" {
            
            Set-TargetResource -name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray  -RequiredVersion "12.0.1" -Ensure "Present" -Verbose

            #The requiredVersion does not exist, expect Ensure=Absent
            $result = MSFT_PackageManagement\Test-TargetResource -Name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray -RequiredVersion "12.0.1.1" 

            #Validate the returned results
            $result | should be $false 
        }

        It "Test-TargetResource with InstalledVersion 12.0.1.1 but RequiredVersion 12.0.1: Check False" {
            
            Set-TargetResource -name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray  -RequiredVersion "12.0.1.1" -Ensure "Present" -Verbose

            #Provide a req version does not exist, expect Ensure=Absent
            $result = MSFT_PackageManagement\Test-TargetResource -Name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray  -RequiredVersion "12.0.1" 

            #Validate the returned results
            $result | should be $false 
        }

        It "Test-TargetResource with MaximumVersion: Check True" {
            
            Set-TargetResource -name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray -RequiredVersion "12.0.1.1" -Ensure "Present" -Verbose
            Set-TargetResource -name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray -RequiredVersion "12.0.1" -Ensure "Present" -Verbose
            Set-TargetResource -name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray -RequiredVersion "15.2.1" -Ensure "Present" -Verbose

            $result = MSFT_PackageManagement\Test-TargetResource -Name "MyTestPackage" -AdditionalParameters $AdditionalParameterCimInstanceArray -MaximumVersion "12.9.9"

            $result | should be $true
        }
    }#context

}#Describe

