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
configuration Sample_PSModule
{
    param
    (
    #Target nodes to apply the configuration
        [string[]]$NodeName = 'localhost',

        #The name of the module
        [Parameter(Mandatory)]
        [string]$Name,

        #The required version of the module
        [string]$RequiredVersion,

        #Repository name  
        [string]$Repository,

        #Whether you trust the repository
        [string]$InstallationPolicy
    )


    Import-DscResource -Module PackageManagement -ModuleVersion 1.1.1.0

    Node $NodeName
    {               
        #Install a package from the Powershell gallery
        PSModule MyPSModule
        {
            Ensure            = "present" 
            Name              = $Name
            RequiredVersion   = "0.2.16.3"  
            Repository        = "PSGallery"
            InstallationPolicy="trusted"     
        }                               
    } 
}


#Compile it
Sample_PSModule -Name "xjea" 

#Run it
Start-DscConfiguration -path .\Sample_PSModule -wait -Verbose -force 
