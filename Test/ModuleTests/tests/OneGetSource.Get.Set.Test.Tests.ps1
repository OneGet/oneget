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

#Calling the setup function 
SetupOneGetSourceTest

Describe -Name  "PackageManagementSource Get.Set.Test-TargetResource Basic Test" -Tags "BVT" {

    BeforeEach {

        #Unregister the source if already registered 
        UnRegisterSource -Name "MyNuget" -providerName "Nuget" -SourceUri $LocalRepositoryPath
    }

    AfterEach {
    }     
    
    Context "PackageManagementSource Get.Set.Test-TargetResource Basic Test" {

        It "Get.Set.Test-TargetResource: Check Present" {
            
            #Register the package source
            MSFT_PackageManagementSource\Set-TargetResource -name "MyNuget" -providerName "Nuget" -SourceUri $LocalRepositoryPath -Ensure Present -Verbose

            #Test it to make sure Set-TargetResource is successfully register the source
            $testResult = MSFT_PackageManagementSource\Test-TargetResource -Name "MyNuget" -providerName "Nuget" -SourceUri $LocalRepositoryPath -Verbose

            $testResult | should be $true

            #Validate the returned Get results
            $getResult = MSFT_PackageManagementSource\Get-TargetResource -Name "MyNuget" -providerName "Nuget" -SourceUri $LocalRepositoryPath -Verbose

            $getResult.Ensure | should be "Present"
            $getResult.Name | should be "MyNuget"
            $getResult.SourceUri | should be $LocalRepositoryPath
            $getResult.InstallationPolicy | should be "Untrusted"  #default is untrusted
            $getResult.Providername | should be "Nuget"  
        }

        
        It "Get.Set.Test-TargetResource: Check Absent" {
            
 
            #Test it to make sure the source is unregistered
            $testResult = MSFT_PackageManagementSource\Test-TargetResource -Name "MyNuget" -providerName "Nuget" -SourceUri $LocalRepositoryPath -Verbose

            $testResult | should be $false

            #Validate the returned Get results
            $getResult = MSFT_PackageManagementSource\Get-TargetResource -Name "MyNuget" -providerName "Nuget" -SourceUri $LocalRepositoryPath -Verbose

            $getResult.Ensure | should be "Absent"
            $getResult.Name | should be "MyNuget"
            $getResult.Providername | should be "Nuget" 
            $getResult.SourceUri | should BeNullOrEmpty
            $getResult.InstallationPolicy | should BeNullOrEmpty 
        }

     
        It "Get.Set.Test-TargetResource with the multiple Sources" {

            #Unregister the source if already registered 
            UnRegisterSource -Name "MyNuget1" -providerName "Nuget" -SourceUri $LocalRepositoryPath1
            UnRegisterSource -Name "MyNuget2" -providerName "Nuget" -SourceUri $LocalRepositoryPath2

            Try
            {
                #Register the package source
                MSFT_PackageManagementSource\Set-TargetResource -name "MyNuget" -providerName "Nuget" -SourceUri $LocalRepositoryPath -Ensure Present -Verbose
                MSFT_PackageManagementSource\Set-TargetResource -name "MyNuget1" -providerName "Nuget" -SourceUri $LocalRepositoryPath1 -Ensure Present -Verbose
                MSFT_PackageManagementSource\Set-TargetResource -name "MyNuget2" `
                                                     -providerName "Nuget" `
                                                     -SourceUri $LocalRepositoryPath2 `
                                                     -Ensure Present `
                                                     -InstallationPolicy Trusted `
                                                     -Verbose                                                 

            
                $testResult = MSFT_PackageManagementSource\Test-TargetResource -Name "MyNuget" `
                                                     -providerName "Nuget"`
                                                     -SourceUri $LocalRepositoryPath `
                                                     -InstallationPolicy Trusted `
                                                     -Verbose
                                                    
                #We registered a source with untrusted installation policy but test-targetresource uses trusted, so it's a false
                $testResult | should be $false

                $testResult = MSFT_PackageManagementSource\Test-TargetResource -Name "MyNuget" `
                                                     -providerName "Nuget"`
                                                     -SourceUri $LocalRepositoryPath1 `
                                                     -InstallationPolicy Untrusted `
                                                     -Verbose
                                                    
                #We registered a source with $LocalRepositoryPath but test-targetresource uses $LocalRepositoryPath1 , so it's a false
                $testResult | should be $false

                $testResult = MSFT_PackageManagementSource\Test-TargetResource -Name "MyNuget2" `
                                                     -providerName "Nuget"`
                                                     -SourceUri $LocalRepositoryPath2 `
                                                     -InstallationPolicy Trusted `
                                                     -Verbose
                                                    
                # The properties in Test and Set all match, should return true
                $testResult | should be $true
            }
            finally
            {
                #Unregister the source if already registered 
                UnRegisterSource -Name "MyNuget" -providerName "Nuget" -SourceUri $LocalRepositoryPath
                UnRegisterSource -Name "MyNuget1" -providerName "Nuget" -SourceUri $LocalRepositoryPath1
                UnRegisterSource -Name "MyNuget2" -providerName "Nuget" -SourceUri $LocalRepositoryPath2
            }
        }
       
               
        It "Get.Set.Test-TargetResource with SourceCredential: Check Registered" {
           
            $credential = (CreateCredObject -Name ".\Administrator" -PSCode "MassRules!")

            MSFT_PackageManagementSource\Set-TargetResource -name "MyNuget" `
                                                     -providerName "Nuget" `
                                                     -SourceUri $LocalRepositoryPath `
                                                     -Ensure Present `
                                                     -InstallationPolicy Trusted `
                                                     -SourceCredential $credential `
                                                     -Verbose  


            # Validate the package is installed
            $testResult = MSFT_PackageManagementSource\Test-TargetResource -Name "MyNuget" `
                                                     -providerName "Nuget" `
                                                     -SourceUri $LocalRepositoryPath `
                                                     -Ensure Present `
                                                     -InstallationPolicy Trusted `
                                                     -SourceCredential $credential `
                                                     -Verbose 
                                                    
            # The properties in Test and Set all match, should return true
            $testResult | should be $true

        }

        It "Set-TargetResource to change installationpolicy from untrusted to trusted: Check Installed" {
            
            #Register the package source
            MSFT_PackageManagementSource\Set-TargetResource -name "MyNuget" -providerName "Nuget" -SourceUri $LocalRepositoryPath -Ensure Present -InstallationPolicy Untrusted -Verbose 

            #Test it to make sure Set-TargetResource is successfully unregister the source
            $testResult = MSFT_PackageManagementSource\Test-TargetResource -Name "MyNuget" -providerName "Nuget" -SourceUri $LocalRepositoryPath

            $testResult | should be $true
                    
            
            #register it with the same name but different source uri
            MSFT_PackageManagementSource\Set-TargetResource -name "MyNuget" -providerName "Nuget" -SourceUri $LocalRepositoryPath -Ensure Present -InstallationPolicy Trusted -Verbose 
            
            $testResult = MSFT_PackageManagementSource\Test-TargetResource -Name "MyNuget" -providerName "Nuget" -SourceUri $LocalRepositoryPath -InstallationPolicy Trusted

            $testResult | should be $true
                          
        }
        
    }#context
   
    Context "PackageManagementSource Get.Set.Test-TargetResource Error Case" {  

        It "Get-TargetResource to unregistered a source that does not exist: Check Error" {

            try
            {
                MSFT_PackageManagementSource\Set-TargetResource -name "MyNuget" -providerName "Nuget" -SourceUri $LocalRepositoryPath -Ensure Absent -Verbose 2>&1
            }
            catch
            {
                $_.FullyQualifiedErrorId -ieq "UnRegisterFailed" | should be $true
                return
            }
            
            Throw "Expected Error 'UnRegisterFailed' does not happen"
        }

     } #context

}#Describe



