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

# This sample configuration does the following
# 1. Registers PowerShellGallery if it is not already
# 2. Downloads GistProvider OneGet provider using Install-Package
# 3. Downloads a Gist from source DFinke using Gist Provider
configuration Sample_Install_Package
{
    param
    (
        #Target nodes to apply the configuration
        [string[]]$NodeName = 'localhost'
    )


    Import-DscResource -Module PackageManagement -ModuleVersion 1.1.3.0

    Node $NodeName
    {               
        #register package source       
        PackageManagementSource PSGallery
        {

            Ensure      = "Present"
            Name        = "psgallery"
            ProviderName= "PowerShellGet"
            SourceLocation   = "https://www.powershellgallery.com/api/v2/"  
            InstallationPolicy ="Trusted"
        }

        #Install a package from the Powershell gallery
        PackageManagement GistProvider
        {
            Ensure            = "present" 
            Name              = "gistprovider"
            Source            = "PSGallery"
            DependsOn         = "[PackageManagementSource]PSGallery"
        }             
        
        PackageManagement PowerShellTeamOSSUpdateInfo
        {
            Ensure   = "present"
            Name     = "Get-PSTOss.ps1"
            ProviderName = "Gist"
            Source   = "dfinke"
            DependsOn = "[PackageManagement]GistProvider"
        }                  
    } 
}


#Compile it
Sample_Install_Package 

#Run it
Start-DscConfiguration -path .\Sample_Install_Package -wait -Verbose -force 
