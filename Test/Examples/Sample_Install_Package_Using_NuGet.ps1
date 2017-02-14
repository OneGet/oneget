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

# DSC configuration example for generice PackageManagement DSC resource
# which uses NuGet Provider

configuration Sample_Install_Package_Using_NuGet
{
    param
    (
        #Target nodes to apply the configuration
        [string[]]$NodeName = 'localhost',

        #Name of the package to be installed
        [Parameter(Mandatory)]
        [string]$Name,

        #Destination path for the package
        [Parameter(Mandatory)]
        [string]$DestinationPath,
        
        #Version of the package to be installed
        [string]$RequiredVersion,

        #Source location where the package download from
        [string]$Source,

        #Whether the source is Trusted or Untrusted
        [string]$InstallationPolicy
    )

    Import-DscResource -Module PackageManagement -ModuleVersion 1.1.3.0

    Node $NodeName
    {
        
        #register package source       
        PackageManagementSource SourceRepository
        {

            Ensure      = "Present"
            Name        = "MyNuget"
            ProviderName= "Nuget"
            SourceLocation   = "http://nuget.org/api/v2/"  
            InstallationPolicy ="Trusted"
        }   
        
        #Install a package from Nuget repository
        PackageManagement NugetPackage
        {
            Ensure               = "Present" 
            Name                 = $Name
            AdditionalParameters = @{"Destination" = $DestinationPath}
            RequiredVersion      = "2.0.1"
            DependsOn            = "[PackageManagementSource]SourceRepository"
        }                               
    } 
}


#Compile it
Sample_Install_Package_Using_NuGet -Name "JQuery" -DestinationPath "$env:HomeDrive\test\test"

#Run it
Start-DscConfiguration -path .\Sample_Install_Package_Using_NuGet -wait -Verbose -force 
