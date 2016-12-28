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
#Helper functions for PackageManagement DSC Resouces

Import-LocalizedData -BindingVariable LocalizedData -filename PackageManagementDscUtilities.strings.psd1


 Function ExtractArguments
{
    <#
    .SYNOPSIS

    This is a helper function that extract the parameters from a given table. 

    .PARAMETER FunctionBoundParameters
    Specifies the hashtable containing a set of parameters to be extracted

    .PARAMETER ArgumentNames
    Specifies A list of arguments you want to extract
    #>

    Param
    (
        [parameter(Mandatory = $true)]
        [System.Collections.Hashtable]
        $FunctionBoundParameters,

        #A list of arguments you want to extract
        [parameter(Mandatory = $true)]
        [System.String[]]$ArgumentNames
    )

    Write-Verbose -Message ($LocalizedData.CallingFunction -f $($MyInvocation.mycommand))

    $returnValue=@{}

    foreach ($arg in $ArgumentNames)
    {
        if($FunctionBoundParameters.ContainsKey($arg))
        {
            #Found an argument we are looking for, so we add it to return collection
            $returnValue.Add($arg,$FunctionBoundParameters[$arg])
        }
    }

    return $returnValue
 }

function ThrowError
{
    <#
    .SYNOPSIS

    This is a helper function that throws an error. 

    .PARAMETER ExceptionName
    Specifies the type of errors, e.g. System.ArgumentException

    .PARAMETER ExceptionMessage
    Specifies the exception message

    .PARAMETER ErrorId
    Specifies an identifier of the error

    .PARAMETER ErrorCategory
    Specifies the error category, e.g., InvalidArgument defined in System.Management.Automation. 

    #>

    param
    (        
        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.String]        
        $ExceptionName,

        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.String]
        $ExceptionMessage,      
        
        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.String]
        $ErrorId,

        [parameter(Mandatory = $true)]
        [ValidateNotNull()]
        [System.Management.Automation.ErrorCategory]
        $ErrorCategory
    )
    
    Write-Verbose -Message ($LocalizedData.CallingFunction -f $($MyInvocation.mycommand))
        
    $exception   = New-Object -TypeName $ExceptionName -ArgumentList $ExceptionMessage;
    $errorRecord = New-Object -TypeName System.Management.Automation.ErrorRecord -ArgumentList ($exception, $ErrorId, $ErrorCategory, $null)    
    throw $errorRecord
}

Function ValidateArgument
{
    <#
    .SYNOPSIS

    This is a helper function that validates the arguments. 

    .PARAMETER Argument
    Specifies the argument to be validated.

    .PARAMETER Type
    Specifies the type of argument.
    #>

    [CmdletBinding()]
    param
    (
        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$Argument,

        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]$Type,

        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]$ProviderName
    )

    Write-Verbose -Message ($LocalizedData.CallingFunction -f $($MyInvocation.mycommand))

    switch ($Type)
    {

        "SourceUri"
        {
            # Checks whether given URI represents specific scheme
            # Most common schemes: file, http, https, ftp        
            $scheme =@('http', 'https', 'file', 'ftp')
 
            $newUri = $Argument -as [System.URI]  
            $returnValue = ($newUri -and $newUri.AbsoluteURI -and ($scheme -icontains $newuri.Scheme)) 

            if ($returnValue -eq $false)
            {                
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage ($LocalizedData.InValidUri -f $Argument)`
                            -ErrorId "InValidUri" `
                            -ErrorCategory InvalidArgument
            }
            
            #Check whether it's a valid uri. Wait for the response within 2mins.
            <#$result = Invoke-WebRequest $newUri -TimeoutSec 120 -UseBasicParsing -ErrorAction SilentlyContinue

            if ($null -eq (([xml]$result.Content).service ))
            {
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage ($LocalizedData.InValidUri -f $Argument)`
                            -ErrorId "InValidUri" `
                            -ErrorCategory InvalidArgument
            }#>
                                         
        }
        "DestinationPath"
        {
            $returnValue = Test-Path -Path $Argument
            if ($returnValue -eq $false)
            {
                ThrowError  -ExceptionName "System.ArgumentException" `
                            -ExceptionMessage ($LocalizedData.PathDoesNotExist -f $Argument)`
                            -ErrorId "PathDoesNotExist" `
                            -ErrorCategory InvalidArgument
            }
        }
        "PackageSource"
        {      
            #Argument can be either the package source Name or source Uri.  
            
            #Check if the source is a uri 
            $uri = $Argument -as [System.URI]  

            if($uri -and $uri.AbsoluteURI) 
            {
                # Check if it's a valid Uri
                ValidateArgument -Argument $Argument -Type "SourceUri" -ProviderName $ProviderName
            }
            else
            {
                #Check if it's a registered package source name                                                             
                $source = PackageManagement\Get-PackageSource -Name $Argument -ProviderName $ProviderName -verbose -ErrorVariable ev
                if ((-not $source) -or $ev) 
                {
                    #We do not need to throw error here as Get-PackageSource does already
                    Write-Verbose -Message ($LocalizedData.SourceNotFound -f $source)                
                }
            }
        }
        default
        {
            ThrowError  -ExceptionName "System.ArgumentException" `
                        -ExceptionMessage ($LocalizedData.UnexpectedArgument -f $Type)`
                        -ErrorId "UnexpectedArgument" `
                        -ErrorCategory InvalidArgument
        }
     }           
}

Function ValidateVersionArgument
{
    <#
    .SYNOPSIS

    This is a helper function that does the version validation. 

    .PARAMETER RequiredVersion
    Provides the required version.

    .PARAMETER MaximumVersion
    Provides the maximum version.

    .PARAMETER MinimumVersion
    Provides the minimum version.
    #>

    [CmdletBinding()]
    param
    (
        [string]$RequiredVersion,
        [string]$MinimumVersion,
        [string]$MaximumVersion

    )
         
    Write-Verbose -Message ($LocalizedData.CallingFunction -f $($MyInvocation.mycommand))

    $isValid = $false
         
    #Case 1: No further check required if a user provides either none or one of these: minimumVersion, maximumVersion, and requiredVersion
    if ($PSBoundParameters.Count -le 1)
    {
        return $true
    }

    #Case 2: #If no RequiredVersion is provided 
    if (-not $PSBoundParameters.ContainsKey('RequiredVersion'))
    {
        #If no RequiredVersion, both MinimumVersion and MaximumVersion are provided. Otherwise fall into the Case #1
        $isValid = $PSBoundParameters['MinimumVersion'] -le $PSBoundParameters['MaximumVersion']
    }
    
    #Case 3: RequiredVersion is provided. 
    #        In this case  MinimumVersion and/or MaximumVersion also are provided. Otherwise fall in to Case #1.
    #        This is an invalid case. When RequiredVersion is provided, others are not allowed. so $isValid is false, which is already set in the init

    if ($isValid -eq $false)
    {        
        ThrowError  -ExceptionName "System.ArgumentException" `
                    -ExceptionMessage ($LocalizedData.VersionError)`
                    -ErrorId "VersionError" `
                    -ErrorCategory InvalidArgument
    }
}

Function Get-InstallationPolicy
{
    <#
    .SYNOPSIS

    This is a helper function that retrives the InstallationPolicy from the given repository. 

    .PARAMETER RepositoryName
    Provides the repository Name.

    #>

    Param
    (
        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.String]$RepositoryName
    )

    Write-Verbose -Message ($LocalizedData.CallingFunction -f $($MyInvocation.mycommand))

    $repositoryobj = PackageManagement\Get-PackageSource -Name $RepositoryName -ErrorAction SilentlyContinue -WarningAction SilentlyContinue

    if ($repositoryobj)
    {      
        return $repositoryobj.IsTrusted
    }                  
}
