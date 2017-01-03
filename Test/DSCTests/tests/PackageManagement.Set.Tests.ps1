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

if (-not (IsAdmin))
{
  throw "This test script requires to be run from an elevated PowerShell session. Launch an elevated PowerShell session and try again."
}

#
# Pre-Requisite: MyTestModule 1.1, 1.1.2, 3.2.1 modules are available under the $LocalRepositoryPath for testing purpose only.
# It's been taken care of by SetupPackageManagementTest
#
 
# Calling the setup function 
SetupPackageManagementTest -SetupPSModuleRepository
 
Describe -Name  "PackageManagement Set-TargetResource Basic Test" -Tags "BVT" {

    BeforeAll {
        $script:OriginalRepository = CleanupRepository
    }
 
    BeforeEach {

        #Remove all left over files if exists
        Remove-Item "$PSModuleBase\MyTestModule" -Recurse -Force  -ErrorAction SilentlyContinue      
        Remove-Item "$PSModuleBase\MyTestModule" -Recurse -Force  -ErrorAction SilentlyContinue     
    }

    AfterEach {
        
    }

    AfterAll {
        # Remove all left over files if exists
        Remove-Item "$PSModuleBase\MyTestModule" -Recurse -Force  -ErrorAction SilentlyContinue      
        Remove-Item "$PSModuleBase\MyTestModule" -Recurse -Force  -ErrorAction SilentlyContinue     
     
        RestoreRepository $script:OriginalRepository
    }

    Context "PackageManagement Set-TargetResource Basic Test" {

        It "Set, Test-TargetResource with Trusted Source, No Versions Specified: Check Installed" {
           
            #Register a local module repository to make the test run faster
            RegisterRepository -Name "LocalRepository" -InstallationPolicy Trusted -Ensure Present

            # 'BeforeEach' removes all specific modules under the $module path, so it is expected Set-Target* should success in the installation
            MSFT_PackageManagement\Set-TargetResource -name "MyTestModule" -Source $LocalRepository  -Ensure Present -Verbose

            # Validate the module is installed
            Test-Path -Path "$PSModuleBase\MyTestModule\3.2.1" | should be $true
            
            # Uninstalling the module
            MSFT_PackageManagement\Set-TargetResource -name "MyTestModule" -Source $LocalRepository  -Ensure Absent -Verbose

            # Validate the module is uninstalled
            $result = MSFT_PackageManagement\Test-TargetResource -name "MyTestModule" -Source $LocalRepository  -Ensure Absent
            $result| should be $true

            Test-Path -Path "$PSModuleBase\MyTestModule\3.2.1" | should be $false
        }

        It "Set, Test-TargetResource with Trusted Source, No respository Specified: Check Installed" {
           
            #Register a local module repository to make the test run faster
            RegisterRepository -Name "LocalRepository" -InstallationPolicy Trusted -Ensure Present

            # 'BeforeEach' removes all specific modules under the $module path, so it is expected Set-Target* should success in the installation
            MSFT_PackageManagement\Set-TargetResource -name "MyTestModule" -Ensure Present -Verbose

            # Validate the module is installed
            Test-Path -Path "$PSModuleBase\MyTestModule\3.2.1" | should be $true

            # Uninstalling the module
            MSFT_PackageManagement\Set-TargetResource -name "MyTestModule" -Ensure Absent -Verbose

            # Validate the module is uninstalled
            $result = MSFT_PackageManagement\Test-TargetResource -name "MyTestModule" -Ensure Absent

            $result| should be $true
        }

        It "Set, Test-TargetResource with multiple sources and versions of a modules: Check Installed" {
           
            # Registering multiple source

            $returnVal = $null

            try
            {
                $returnVal = CleanupRepository
                
                RegisterRepository -Name "LocalRepository1" -InstallationPolicy Untrusted -Ensure Present -SourceLocation $LocalRepositoryPath1 -PublishLocation $LocalRepositoryPath1

                RegisterRepository -Name "LocalRepository2" -InstallationPolicy Trusted -Ensure Present -SourceLocation $LocalRepositoryPath2 -PublishLocation $LocalRepositoryPath2

                RegisterRepository -Name "LocalRepository3" -InstallationPolicy Untrusted -Ensure Present -SourceLocation $LocalRepositoryPath3 -PublishLocation $LocalRepositoryPath3
                
                # User's installation policy is untrusted
                MSFT_PackageManagement\Set-TargetResource -name "MyTestModule" -Ensure "Present" -Verbose -Source "LocalRepository2"

                # The module from the trusted source should be installed
                Get-InstalledModule MyTestModule | % Repository | should be "LocalRepository2"
            }
            finally
            {
                RestoreRepository -RepositoryInfo $returnVal
                # Unregistering the repository sources
            
                RegisterRepository -Name "LocalRepository1" -Ensure Absent -SourceLocation $LocalRepositoryPath1 -PublishLocation $LocalRepositoryPath1

                RegisterRepository -Name "LocalRepository2" -Ensure Absent -SourceLocation $LocalRepositoryPath2 -PublishLocation $LocalRepositoryPath2

                RegisterRepository -Name "LocalRepository3" -Ensure Absent -SourceLocation $LocalRepositoryPath3 -PublishLocation $LocalRepositoryPath3
            }
        }  
                    
    }#context

    
    Context "PackageManagement Set-TargetResource Error Cases" {

        #Register a local module repository to make the test run faster
        RegisterRepository -Name "LocalRepository" -InstallationPolicy Trusted -Ensure Present

        It "Set-TargetResource with module not found for the install: Check Error" {

            try
            {
                # The module does not exist
                MSFT_PackageManagement\Set-TargetResource -name "NonExistModule" -Ensure Present -ErrorAction SilentlyContinue  2>&1
            }
            catch
            {
                #Expect fail to install.
                $_.FullyQualifiedErrorId | should be "NoMatchFoundForCriteria,Microsoft.PowerShell.PackageManagement.Cmdlets.InstallPackage"
                return
            }
   
            Throw "Expected 'ModuleNotFoundInRepository' exception did not happen"  
        }

        
        It "Set , Test-TargetResource: Check Absent and False" {

            # Calling Set-TargetResource to uninstall the MyTestModule module
            try
            {
                MSFT_PackageManagement\Set-TargetResource -name "MyTestModule" -Source $LocalRepository -RequiredVersion "1.1.2" -Ensure "Absent" -Verbose
            }
            catch
            {
                if ($_.FullyQualifiedErrorId -ieq "NoMatchFound,Microsoft.PowerShell.PackageManagement.Cmdlets.UninstallPackage")
                {
                    #The module is not installed. Ignore the error
                }
                else
                {
                    throw
                }
            }

            # Calling Get-TargetResource in the PSModule resource 
            $result = MSFT_PackageManagement\Test-TargetResource -Name "MyTestModule" -Source $LocalRepository -RequiredVersion "1.1.2"

            # Validate the result
            $result | should be $false

        }
       
    }#context
}#Describe
