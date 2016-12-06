# Writing OneGet provider

## Prerequisites
- If you are working on Windows 10, Windows Server 2016 or later, you are good to go.
- If you are working on earlier version of Windows OS, you need to install WMF package. For this tutorial, let's install [WMF5.1][WMF51].
- If you are working on Linux or Mac, follow the [PowerShell installation instructions][setupps].


[WMF51]: https://www.microsoft.com/en-us/download/details.aspx?id=53347
[setupps]:https://github.com/PowerShell/PowerShell/blob/master/docs/learning-powershell/README.md

## Getting Started With OneGet Provider

In this tutorial, we are going to build a OneGet provider in PowerShell.

### Step 1: Create a PowerShell module
  - Let's say we want to build a provider, named as `MyAlbum`.

  ``` PowerShell
  New-ModuleManifest -path .\MyAlbum.psd1
  ```

- Open the `MyAlbum.psd1` in your favorite editor, add/change the following fields in the MyAlbum.psd1.

  Since the business logic will be implemented in MyAlbum.psm1 module, set RootModule to `MyAlbum.psm1`.
  ``` PowerShell
  RootModule = 'MyAlbum.psm1'
  ```
  Because we need to let OneGet know which module to load, set the info within `PrivateData` section:
  ``` PowerShell
  PrivateData = @{"PackageManagementProviders" = 'MyAlbum.psm1' ...
  ```
  In order to identify MyAlbum is not only a PowerShell module but a OneGet provider, we need to add below line in the PSData section:
  ``` PowerShell
  Tags = @("PackageManagement","Provider")
  ```  

  Finally the MyAlbum.psd1 will be something like below:
``` PowerShell
  @{

    RootModule = 'MyAlbum.psm1'
    ModuleVersion = '1.0.0'
    GUID = 'ae72ced2-5c91-46e2-9081-e272df6282ef'
    Author = 'Contoso Corporation'
    CompanyName = 'Contoso Corporation'
    Copyright = '© Contoso Corporation. All rights reserved.'
    Description = 'MyAlbum provider discovers the photos in your remote file repository and installs them to your local folder.'
    PowerShellVersion = '3.0'
    FunctionsToExport = @()
    PrivateData = @{
      "PackageManagementProviders" = 'MyAlbum.psm1'
      PSData = @{

        # Tags applied to this module to indicate this is a PackageManagement Provider.
        Tags = @("PackageManagement","Provider")

        # A URL to the license for this module, for example,
        LicenseUri = 'https://github.com/OneGet/MyAlbum-Sample-Provider/blob/master/LICENSE'

        # A URL to the main website for this project. For example,
        ProjectUri = 'https://github.com/OneGet/MyAlbum-Sample-Provider'

        # ReleaseNotes of this module
        ReleaseNotes = 'This is a sample PackageManagement provider. It discovers photos in your remote file repository and installs them to your local folder.'
        } # End of PSData
    }
 }
```

### Step 2: Implement OneGet Mandatory Methods

There are two methods considered as mandatory:

- function Get-PackageProviderName

  It returns the name of your provider. In this case you can simply return "MyAlbum". For example,

``` PowerShell
  function Get-PackageProviderName {
      return "MyAlbum"
  }

  ```

- function Initialize-Provider

 This function allows your provider to do initialization before performing any actions.
 In this example, we do not have anything to be initialized.

  ``` PowerShell

  function Initialize-Provider {
      Write-Debug ("Initialize-Provider")
  }

  ```

- First attempt

  Let's copy the MyAlbum.psd1 and MyAlbum.psm1 from the current folder (say e:\oneget\test) to the PowerShell module folder. In this tutorial, let's copy it to $env:programfiles\WindowsPowerShell\Modules folder. If the Myalbum module exists under $env:programfiles\WindowsPowerShell\Modules, rename it to something else before starting the copying.

``` PowerShell
    cd $env:programfiles\WindowsPowerShell\Modules
    mkdir MyAlbum
    cd MyAlbum
    Copy-Item e:\OneGet\test\* . -Recurse
    PS C:\Program Files\WindowsPowerShell\Modules\myalbum> dir


        Directory: C:\Program Files\WindowsPowerShell\Modules\myalbum


    Mode                LastWriteTime         Length Name
    ----                -------------         ------ ----
    -a----       11/30/2016   4:08 PM           2400 MyAlbum.psd1
    -a----       11/30/2016   3:59 PM           7184 MyAlbum.psm1

    PS C:\> Get-PackageProvider -Name Myalbum -ListAvailable

    Name                     Version          DynamicOptions
    ----                     -------          --------------
    MyAlbum                  1.0.0.0

  ```
  The above means that OneGet is able to successfully discover the MyAlbum provider you just created.

### Step3: Implement Find-Package

To support `find-package -provider MyAlbum` cmdlet, you need to implement the following:

``` PowerShell
function Find-Package {
    param(
        [string] $name,            #Name of a package
        [string] $requiredVersion, #Version of a package
        [string] $minimumVersion,  #Mini version of a package
        [string] $maximumVersion   #Max version of a package
    )

```

Assuming that you have your photos saved under c:\test\1.0.1 folder, you want to use find-package to discover them.  We can implement this logic something like below:

``` PowerShell

function Find-Package {
    param(
        [string] $name,
        [string] $requiredVersion,
        [string] $minimumVersion,
        [string] $maximumVersion
    )

    Write-Verbose ("Find-Package")

    $location = "c:\test\1.0.1"                       
    $files = Get-ChildItem -Path $location -Filter '*.png' -Recurse  | `
            Where-Object { ($_.PSIsContainer -eq $false) -and  ( $_.Name -like "*$name*") }

    foreach($file in $files)
    {
        if($request.IsCanceled) { return }  

            $swidObject = @{
                FastPackageReference = $file.FullName;
                Name = $file.Name;
                Version = New-Object System.Version ("1.0.1");
                versionScheme  = "MultiPartNumeric";
                summary = "Add the summary of your package provider here";
                Source = $location;              
            }

            $sid = New-SoftwareIdentity @swidObject              
            Write-Output -InputObject $sid               
    }    
}
```
In the above example, we enumerate files with extension .png,
check if the file name matches what a user is looking for ($name), construct the swidtag object and return it to OneGet.

`New-SoftwareIdentity` is the utility function for you to use. It's defined the PackageProviderFunctions.psm1. You can find it under  $env:ProgramFiles\WindowsPowerShell\Modules\PackageManagement. SoftwareIdentity object is the protocol that provider and OneGet communicate. It is also used as the output object of OneGet cmdlets.

As you can see, SoftwareIdentity object contains name and version of package, source location where the package comes from, and summary description, etc.
Ignore FastPackageReference for now. We will discuss it later.
In the above sample code, we hard-coded the package version to 1.0.1.
In the real world scenario, you need to fill in the actual package version.

Now let's try it. Under the c:\test\1.0.1 folder, I have three files: seattle.png, happy.png and nice.png.

``` PowerShell
PS C:\Test> Get-PackageProvider -list -name Myalbum

Name                     Version          DynamicOptions
----                     -------          --------------
MyAlbum                  1.0.0.0


PS C:\Test> Import-PackageProvider Myalbum -force

Name                     Version          DynamicOptions
----                     -------          --------------
MyAlbum                  1.0.0.0

PS C:\Test> Find-Package -ProviderName Myalbum

Name                           Version          Source           Summary
----                           -------          ------           -------
Happy.png                      1.0.1            c:\test\1.0.1    Add the summary of your package provider here
Nice.png                       1.0.1            c:\test\1.0.1    Add the summary of your package provider here
Seattle.png                    1.0.1            c:\test\1.0.1    Add the summary of your package provider here

PS C:\Test> Find-Package -ProviderName Myalbum -verbose -Name Seattle
VERBOSE: Using the provider 'MyAlbum' for searching packages.
VERBOSE: Find-Package

Name                           Version          Source           Summary
----                           -------          ------           -------
Seattle.png                    1.0.1            c:\test\1.0.1    Add the summary of your package provider here


PS C:\Test> (Find-Package -ProviderName Myalbum -name Seattle).GetType()

IsPublic IsSerial Name                                     BaseType
-------- -------- ----                                     --------
True     False    SoftwareIdentity                         Microsoft.PackageManagement.Internal.Packaging.Swidtag

```

### Step4: $FastPackageReference Explained

OneGet follows a stateless model, meaning no communication information or status is maintained within the OneGet among its cmdlet calls such as find-package, install-package etc.
For example, when a user calls `install-package -name foobar`, OneGet calls provider's
find-package first to ensure the package indeed exists and then calls provider's install-package. Between these two calls,
some package information such as name, version, source location, etc. needs to be communicated between OneGet and providers and between find-package and install-package cmdlets.
Due to the nature of stateless design, $FastPackageReference string object is used to pass around package information between find-package and install-package calls.

$FastPackageReference is used across multiple calls such as Find-package, Install-package, UnInstall-Package and Download-Package.
because of that,  the format of $FastPackageReference needs to be consistent within your provider. It usually contains package Name, Version and Source.

In this MyAlbum tutorial, we choose the file full path as $FastPackageReference for the Simplicity. In the real world case, you need to construct the package name, version, source etc. information returned by find-package and pass back the information to OneGet. For example, [NanoServerPackage provider][nano], it uses '|#|' as a separator, construct a string something like below as a $FastPacakgereference string in find-package and then pass it to OneGet.
PackageName|#|PackageVersion|#|PackageSource|#|Culture|#|NanoServerVersion|#|.

OneGet then pass it back to the provider in Install-Package for provider to use.

[nano]: https://github.com/OneGet/NanoServerPackage/blob/master/NanoServerPackage/NanoServerPackage.psm1



### Step5: Implement Install-Package

The signature of Install-Package is as bellow:
``` PowerShell
function Install-Package
{
   [CmdletBinding()]
   param
   (
       [Parameter(Mandatory=$true)]
       [ValidateNotNullOrEmpty()]
       [string]
       $fastPackageReference
   )
   ...
 }
```
As mentioned above, we set $fastPackageReference in the find-package and pass it to OneGet via SoftwareIdentity object. In this Install-Package function, we get the same $fastPackageReference back from OneGet. In the MyAlbum example, we set $fastPackageReference to full file path for the simplicity purpose. In the Install-Package function, we just copy the source file from $fastPackageReference to other location a user wants. So the implementation looks something like below:
``` PowerShell
function Install-Package
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $fastPackageReference
    )

    Write-Debug -Message ('Install-Package')

    $force = $false
    $options = $request.Options

    if($options -and $options.ContainsKey('Destination'))
    {        
        $location = $($options['Destination'])
    }
    if($options.ContainsKey('Force'))
    {
        $force = $options['Force']
    }

    $sourceFilePath = $fastPackageReference
    Write-Verbose "sourceFilePath=$sourceFilePath, Location=$location"

    Copy-Item -Path $sourceFilePath -Destination "" -Force:$force -Verbose

    if(-not $Location -or -not (Test-Path $Location)){
        ThrowError -ExceptionName "System.ArgumentException" `
                    -ExceptionMessage "Path ''$location' is not found" `
                    -ErrorId "PathNotFound" `
                    -CallerPSCmdlet $PSCmdlet `
                    -ErrorCategory InvalidArgument `
                    -ExceptionObject $Location
    }

    $swidObject = @{
                    FastPackageReference = $fastPackageReference;
                    Name = [System.IO.Path]::GetFileName($fastPackageReference);
                    Version = New-Object System.Version ("1.0.1");  
                    versionScheme  = "MultiPartNumeric";              
                    summary = "Summary of your package provider";
                    Source =   [System.IO.Path]::GetDirectoryName($fastPackageReference)         
                   }
    $swidTag = New-SoftwareIdentity @swidObject
    Write-Output -InputObject $swidTag    
}

# Utility to throw an errorrecord
function ThrowError
{
    param
    (        
        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.PSCmdlet]
        $CallerPSCmdlet,

        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.String]        
        $ExceptionName,

        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.String]
        $ExceptionMessage,

        [System.Object]
        $ExceptionObject,

        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.String]
        $ErrorId,

        [parameter(Mandatory = $true)]
        [ValidateNotNull()]
        [System.Management.Automation.ErrorCategory]
        $ErrorCategory
    )

    $exception = New-Object $ExceptionName $ExceptionMessage;
    $errorRecord = New-Object System.Management.Automation.ErrorRecord $exception, $ErrorId, $ErrorCategory, $ExceptionObject    
    $CallerPSCmdlet.ThrowTerminatingError($errorRecord)
}
```

If you run the above code, you will get the following error:
```PowerShell
PS C:\test> Import-PackageProvider myalbum -force

Name                     Version          DynamicOptions
----                     -------          --------------
MyAlbum                  1.0.0.0


PS C:\test> install-package -name seattle -provider MyAlbum -force -verbose
VERBOSE: Using the provider 'MyAlbum' for searching packages.
VERBOSE: Find-Package
VERBOSE: Performing the operation "Install Package" on target "Package 'Seattle.png' version '1.0.1' from 'c:\test\1.0.1'.".
VERBOSE: sourceFilePath=C:\test\1.0.1\Seattle.png, Location=
install-package : 'Path ' is not found
At line:1 char:1
+ install-package -name seattle -provider MyAlbum -force -verbose
+ ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    + CategoryInfo          : InvalidArgument: (Microsoft.Power....InstallPackage:InstallPackage) [Install-Package], Exception
    + FullyQualifiedErrorId : PathNotFound,Install-Package,Microsoft.PowerShell.PackageManagement.Cmdlets.InstallPackage


```
This is because the destination path is set.
Install-Package only knows the file source but does not know where it should copy the file to.
Now Get-DynamicOptions function comes to play.

### Step6: Get-DynamicOptions
In the above section, we got an error in the install-package because the destination file path was empty.
We need to find a way to let a user to pass in the file destination path such as -destination "c:\test".
To achieve that, we introduced Get-DynamicOptions function.
It allows your provider to define your provider specific cmdlet parameters.
For example,

```PowerShell
function Get-DynamicOptions
{
    param
    (
        [Microsoft.PackageManagement.MetaProvider.PowerShell.OptionCategory]
        $category
    )

    Write-Debug ("Get-DynamicOptions")      
    switch($category)
    {
        Install
        {
            Write-Output -InputObject (New-DynamicOption -Category $category -Name "Destination" -ExpectedType String -IsRequired $true)
        }
    }
}
```

`New-DynamicOption` is the utility function defined the PackageProviderFunctions.psm1. It creates a new instance of a Dynamic parameter object.
You can find it under  $env:ProgramFiles\WindowsPowerShell\Modules\PackageManagement.
The code looks like below:

``` PowerShell
function New-DynamicOption {
	param(
		[Parameter(Mandatory=$true)]
    [Microsoft.PackageManagement.MetaProvider.PowerShell.OptionCategory]
    $category,

		[Parameter(Mandatory=$true)]
    [string]
    $name,

		[Parameter(Mandatory=$true)]
    [Microsoft.PackageManagement.MetaProvider.PowerShell.OptionType]
    $expectedType,

		[Parameter(Mandatory=$true)]
    [bool]
    $isRequired,

		[System.Collections.ArrayList] $permittedValues = $null
	)

	if( -not $permittedValues ) {
		return New-Object -TypeName Microsoft.PackageManagement.MetaProvider.PowerShell.DynamicOption -ArgumentList $category,$name,  $expectedType, $isRequired
	}
	return New-Object -TypeName Microsoft.PackageManagement.MetaProvider.PowerShell.DynamicOption -ArgumentList $category,$name,  $expectedType, $isRequired, $permittedValues.ToArray()
}
```

- $category : here are available categories defined by PackageManagement:
  - Package  - for searching for packages
  - Source   - for package sources
  - Install  - for Install/Uninstall/Get-InstalledPackage

  For example, if you want to define a dynamic parameter used for install-package,
you can add specify $category it as Install.
  If you want to add filer for find-package, you can specify the $category as Package.

- $name - define the name of dynamic parameter.
  In this example, `Destination` is the name of parameter we defined.

- $expectedType - specify type of your dynamic parameter
  Only basic .net primitive types are supported.  For sample, string, int, bool or switch.
- $isRequired - whether the dynamic parameter is mandatory.
- $permittedValues - allowed values of the dynamic parameter. For example, a user can specify `-scope AllUser or CurrentUser`. You can define AllUser or CurrentUser as a string array here.

Now we can understand that, in the above example, we just defined a dynamic parameter named as `Destination` as string parameter. It is mandatory.

Let's copy the above Get-DynamicOptions to your MyAlbum.psm1 and then try it out. Note that no need to copy New-DynamicOption because it exists already on your machine.

```PowerShell
PS C:\Test> Import-PackageProvider -name Myalbum -force

Name                     Version          DynamicOptions
----                     -------          --------------
MyAlbum                  1.0.0.0          Destination


PS C:\Test> install-package -name seattle -provider MyAlbum -force -verbose
install-package : The action with the specified provider 'MyAlbum' is missing one or more required parameters: Destination.
At line:1 char:1
+ install-package -name seattle -provider MyAlbum -force -verbose
+ ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    + CategoryInfo          : InvalidArgument: (Microsoft.Power....InstallPackage:InstallPackage) [Install-Package], Exception
    + FullyQualifiedErrorId : SpecifiedProviderMissingRequiredOption,Microsoft.PowerShell.PackageManagement.Cmdlets.InstallPackage
```

Since we defined Destination as a mandatory parameter in the Get-DynamicOptions, we have to pass in Destination as bellow,

```PowerShell
PS C:\test> install-package -name seattle -provider MyAlbum -force -verbose -Destination c:\test\
VERBOSE: Using the provider 'MyAlbum' for searching packages.
VERBOSE: Find-Package
VERBOSE: Performing the operation "Install Package" on target "Package 'Seattle.png' version '1.0.1' from 'c:\test\1.0.1'.".
VERBOSE: sourceFilePath=C:\test\1.0.1\Seattle.png, Location=c:\test\

Name                           Version          Source           Summary
----                           -------          ------           -------
Seattle.png                    1.0.1              C:\test\1.0.1    Summary of your package provider

PS C:\test> dir c:\test\Seattle.png


    Directory: C:\test


Mode                LastWriteTime         Length Name
----                -------------         ------ ----
-a----       10/30/2015   6:07 PM           9749 Seattle.png

```

You can find more information regarding PowerShell Dynamic Parameters such as:
- [Dynamic Parameters in PowerShell][psm] by PowerShellMagazine.com
[psm]:http://www.powershellmagazine.com/2014/05/29/dynamic-parameters-in-powershell/

### Step7: $request object
In the above `install-package` function, you may have noticed we used $request.Options.
$request is the object implemented by OneGet.
It is a communication channel between provider and OneGet.
Before OneGet calls into powershell.Invoke(), it sets the request object as a PowerShell variable for providers to use.

What $request supports?

Here are list of commonly used members of $request object:
- IsCanceled

  Your provider possibly wants to periodically check if the operation gets cancelled such as a user typed "Ctrl+C" and exit the process in your provider as soon as you can.
  Sample usage:
``` PowerShell
    $request.IsCanceled
```
- PackageSources

  Your provider can get the value of package source parameter, e.g., "-Source `local`" from user's commandline input.
  Sample usage:
``` PowerShell
    $SourceName = $request.PackageSources
```
  where $SourceName will be "local"

- Options

  It allows you go get dynamic option parameter values. In the above example, we defined `Destination` in Get-DynamicOptions method. In the provider, we needc to get the value of -Destination in the install-package cmdlet. Here is an example:
```PowerShell
   if($options -and $options.ContainsKey('Destination'))
   {
          $path = $($options['Destination'])
          ...          
   }
```
  If your provider needs to know whether a user specify -force for example,
  you can do:
```PowerShell
    $force = $false
    $options = $request.Options
    if($options.ContainsKey('Force'))
    {
        $force = $options['Force']
    }
```

- Credential

  It returns as PSCredential object.
  Usage: $request.Credential.
  User input: -Credential from the OneGet cmdlet.

- ShouldContinue
  It works as PSCmdlet ShouldContinue.
  Usage: $request.ShouldContinue.
  Sample Usage: [PowerShellGet][psget]

[psget]: https://github.com/PowerShell/PowerShellGet/blob/development/PowerShellGet/PSModule.psm1


### Step8: Implement Get-Package

A user may want to know what packages are installed on his system.
In the install-package we passed in "-Destination c:\test" for example.
In the Get-Package, we expect a user can do something like below:

```PowerShell
Get-Package -ProviderName MyAlbum -Destination c:\test  

```

The API to be implemented for Get-Package is `Get-InstalledPackage`.
Below is the sample implementation.

```PowerShell
function Get-InstalledPackage
{
    [CmdletBinding()]
    param
    (
        [Parameter()]
        [string]
        $Name,

        [Parameter()]
        [string]
        $RequiredVersion,

        [Parameter()]
        [string]
        $MinimumVersion,

        [Parameter()]
        [string]
        $MaximumVersion
    )

    Write-Debug -Message ('Get-InstalledPackage')

    $options = $request.Options
    if($options -and $options.ContainsKey('Destination'))
    {        
        $location = $($options['Destination'])
    }

    if (Test-Path -Path $location)
    {
        # Find the photos
        $files = Get-ChildItem -Path $location -Filter '*.png' -Recurse  | `
                        Where-Object { ($_.PSIsContainer -eq $false) -and  ( $_.Name -like "*$Name*") }

        foreach($file in $files)
        {
            if($request.IsCanceled) { return }

            $swidObject = @{
                FastPackageReference = $file.FullName;
                Name = $file.Name;
                Version = New-Object System.Version ("1.0.1");
                versionScheme  = "MultiPartNumeric";
                summary = "Summary of your package provider";
                Source = $file.FullName;               
            }
            $swidTag = New-SoftwareIdentity @swidObject
            Write-Output -InputObject $swidTag               
        }
     }
}
```

Copy the above code and try it out as follows.

``` PowerShell
Import-PackageProvider myAlbum -force
PS E:\Test> Get-Package -provider myalbum -Destination c:\test\test -verbose

Name                           Version          Source                           ProviderName
----                           -------          ------                           ------------
Seattle.png                    1.0.1              C:\test\test\Seattle.png         MyAlbum

```

### Step9: Implement Uninstall-Package
Similar to Install-Package,
OneGet calls Find-Package first to ensure a particular package to be installed exists in repository.
For Uninstall-Package,
OneGet calls Get-InstalledPackage first to ensure a particular package to be uninstalled exists in the local machine.
OneGet uses $fastPackageReference parameter to pass down the package information returned from Get-InstalledPackage to Uninstall-Package function.

To support UnInstall-Package, your provider needs to implement UnInstall-Package method.

```PowerShell

function UnInstall-Package
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $fastPackageReference
    )

    Write-Debug -Message ("Uninstall-Package")
    $fileFullName = $fastPackageReference
    if(Test-Path -Path $fileFullName)
    {
        Remove-Item $fileFullName -Force -WhatIf:$false -Confirm:$false

        $swidObject = @{
            FastPackageReference = $fileFullName;                        
            Name = [System.IO.Path]::GetFileName($fileFullName);
            Version = New-Object System.Version ("1.0.1");     
            versionScheme  = "MultiPartNumeric";              
            summary = "Summary of your package provider";
            Source =   [System.IO.Path]::GetDirectoryName($fileFullName)                             
        }

        $swidTag = New-SoftwareIdentity @swidObject
        Write-Output -InputObject $swidTag
    }	 
}
```

Copy the above code and try it out as follows.

``` powershell

Import-PackageProvider myAlbum -force

PS E:\Test> uninstall-package -ProviderName MyAlbum -Name Seattle -verbose -Destination "c:\test\test"
VERBOSE: Performing the operation "Uninstall Package." on target "Package 'Seattle.png' with version '1.0.1'.".

Name                           Version          Source           Summary
----                           -------          ------           -------
Seattle.png                    1.0.1            C:\test\test     Summary of your package provider

```

### Step8: Implement Save-Package
There is no difference between Save-Package and Install-Package for our MyAlbum example provider.
For same cases like PowerShellGet provider, a user may not want to install PowerShell modules to the default PowerShell module folder (e..g, $env:programfiles\WindowsPowerShell\modules).
Instead, the user may want to download modules to some other folder first and do some security scan for example before actual using the module.

In terms of Save-Package implementation, it is very similar to Install-Package.
In the Install-Package, we get Destination location via $request object.
In the Save-Package, as OneGet defines a cmdlet parameter called `Path`,
which is directly passed in to the Save-Package as an input parameter `Location`.
The API to be implemented is `Download-Package`.  See sample implementation.

``` PowerShell
function Download-Package
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $FastPackageReference,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Location
    )

    Write-Debug ("Download-Package")

    $force = $false
    $options = $request.Options

    if($options.ContainsKey('Force'))
    {
        $force = $options['Force']
    }

    $sourceFilePath = $fastPackageReference
    Write-Verbose "sourceFilePath=$sourceFilePath, Location=$location"

    Copy-Item -Path $sourceFilePath -Destination $Location -Force:$force -Verbose


    if(-not $Location -or -not (Test-Path $location)){

       ThrowError -ExceptionName "System.ArgumentException" `
                    -ExceptionMessage "Path '$Location' is not found" `
                    -ErrorId "PathNotFound" `
                    -CallerPSCmdlet $PSCmdlet `
                    -ErrorCategory InvalidArgument `
                    -ExceptionObject $Location

    }

    $swidObject = @{
                    FastPackageReference = $fastPackageReference;
                    Name = [System.IO.Path]::GetFileName($fastPackageReference);
                    Version = New-Object System.Version ("1.0.1");  # Note: You need to fill in a proper package version    
                    versionScheme  = "MultiPartNumeric";              
                    summary = "Summary of your package provider";
                    Source =   [System.IO.Path]::GetDirectoryName($fastPackageReference)         
                   }
    $swidTag = New-SoftwareIdentity @swidObject
    Write-Output -InputObject $swidTag    
}

```


Copy and paste the above code to your provider and try it as follows.

```PowerShell

Import-PackageProvider myAlbum -force

PS E:\Test> save-package -provider myalbum -name seattle  -force -verbose -Path "c:\test\test"
VERBOSE: Using the provider 'MyAlbum' for searching packages.
VERBOSE: Find-Package
VERBOSE: Performing the operation "Save Package" on target "'Seattle.png' to location 'C:\test\test'".
VERBOSE: sourceFilePath=C:\test\1.0.1\Seattle.png, Location=C:\test\test

Name                           Version          Source           Summary
----                           -------          ------           -------
Seattle.png                    1.0.1            C:\test\1.0.1    Summary of your package provider

```
### Step9: Register Package Source

Let's take a look at the Find-Package code again,
you will see we set the package source to a hard-coded file path, c:\test\1.0.1.
This is certainly not good coding practice. Also not portable.

One way to let a user set package source location/repostiory is to register the package source.
OneGet has introduced a cmdlet named as `Register-PackageSource` to allow a user to specify where the package come from.
The package source can be file location, repository, Azure storage, or anywhere you provider understands for querying or downloading packages.

In order to make the register package source to work, following methods need to be implemented by your provider.

- Resolve-PackageSource
- Register-PackageSource
- Unregister-PackageSource

Let's start with Resolve-PackageSource method.

#### Resolve-PackageSource

This function returns the registered package sources that a provider can handle.
OneGet calls this function to identify which provider can handle the packages from a particular source location.
The function gets called during find-package, install-package, get-packagesource etc.

Below is the sample implementation. Assuming the registered package source information is saved in a local config file, we define as follows.

``` PowerShell

$script:LocalPath="$env:LOCALAPPDATA\Contoso\$script:ProviderName"
$script:RegisteredPackageSourcesFilePath = Microsoft.PowerShell.Management\Join-Path -Path $script:LocalPath -ChildPath "MyAlbumPackageSource.xml"
$script:RegisteredPackageSources = $null
# Wildcard pattern matching configuration
$script:wildcardOptions = [System.Management.Automation.WildcardOptions]::CultureInvariant -bor `
                          [System.Management.Automation.WildcardOptions]::IgnoreCase
                          
function Resolve-PackageSource
{
    Write-Debug ("Resolve-PackageSource")      
    $SourceName = $request.PackageSources

    # get Sources from the registered config file
    Set-PackageSourcesVariable

    if(-not $SourceName)
    {
        $SourceName = "*"
    }

    foreach($src in $SourceName)
    {
        if($request.IsCanceled) { return }

        # Get the sources that registered before
        $wildcardPattern = New-Object System.Management.Automation.WildcardPattern $src,$script:wildcardOptions
        $sourceFound = $false

        $script:RegisteredPackageSources.GetEnumerator() |
            Microsoft.PowerShell.Core\Where-Object {$wildcardPattern.IsMatch($_.Key)} |
                Microsoft.PowerShell.Core\ForEach-Object {
                    $source = $script:RegisteredPackageSources[$_.Key]
                    $packageSource = New-PackageSourceAndYield -Source $source
                    Write-Output -InputObject $packageSource
                    $sourceFound = $true
                }

        # If a user does specify -Source but not registered
        if(-not $sourceFound)
        {
            Write-Error -Message "Package source not found" -ErrorId "PackageSourceNotFound" -Category InvalidOperation -TargetObject $src
            break
        }
    }
}

# Utility function - Read the registered package sources from its configuration file
function Set-PackageSourcesVariable
{
    if(-not $script:RegisteredPackageSources)
    {
        if(Microsoft.PowerShell.Management\Test-Path $script:RegisteredPackageSourcesFilePath)
        {
            $script:RegisteredPackageSources = Import-Clixml -Path $script:RegisteredPackageSourcesFilePath
        }
        else
        {
            $script:RegisteredPackageSources = [ordered]@{}
        }
    }   
}

# Utility function - Yield the package source to OneGet
function New-PackageSourceAndYield
{
    param
    (
        [Parameter(Mandatory)]
        $Source
    )

    # create a new package source
    $src =  New-PackageSource -Name $Source.Name `
                              -Location $Source.SourceLocation `
                              -Trusted $Source.Trusted `
                              -Registered $Source.Registered `

    # return the package source object.
    Write-Output -InputObject $src
}

```

#### Register-PackageSource

To support Register-PackageSource, an API to be implemented is `Add-PackageSource`.
This method gets called by OneGet when a user is registering a package source.

Sample usage:
``` PowerShell
Register-PackageSource -Name demo -Location  C:\CameraRoll -ProviderName MyAlbum

```
In the below example, we save the package source name, source location, whether the source is trusted, etc. info to the local file `MyAlbumPackageSource.xml`.

``` PowerShell
function Add-PackageSource
{
    [CmdletBinding()]
    param
    (
        [string]
        $Name,

        [string]
        $Location,

        [bool]
        $Trusted
    )     

    Write-Debug ("Add-PackageSource")  
    Set-PackageSourcesVariable -Force  

    # Add new package source
    $packageSource = Microsoft.PowerShell.Utility\New-Object PSCustomObject -Property ([ordered]@{
            Name = $Name
            SourceLocation = $Location.TrimEnd("\")
            Trusted=$Trusted
            Registered= $true          
        })    

    $script:RegisteredPackageSources.Add($Name, $packageSource)   

    # yield the package source to OneGet
    Write-Verbose "$packageSource"

    # Persist the package sources
    Save-PackageSources

    # yield the package source to OneGet
    Write-Output -InputObject (New-PackageSourceAndYield -Source $packageSource)
}

# Utility function - save the package source to the configuration file
function Save-PackageSources
{
    if($script:RegisteredPackageSources)
    {
        if(-not (Microsoft.PowerShell.Management\Test-Path $script:LocalPath))
        {
            $null = Microsoft.PowerShell.Management\New-Item `
                    -Path $script:LocalPath `
                    -ItemType Directory `
                    -Force `
                    -ErrorAction SilentlyContinue `
                    -WarningAction SilentlyContinue `
                    -Confirm:$false -WhatIf:$false
        }

        Microsoft.PowerShell.Utility\Export-Clixml `
            -Path $script:RegisteredPackageSourcesFilePath `
            -Force `
            -InputObject ($script:RegisteredPackageSources)
   }   
}
```

#### Unregister-PackageSource

To support Unregister-PackageSource, an API to be implemented is `Remove-PackageSource`.
This method gets called by OneGet when a user is unregistering a package source.

Sample usage:
``` PowerShell
UnRegister-PackageSource -Name demo -ProviderName MyAlbum

```
In the following sample implementation, we delete the entry from memory and update the package source config file.

```PowerShell
function Remove-PackageSource
{
    param
    (
        [string]
        $Name
    )

    Write-Debug ('Remove-PackageSource')

    Set-PackageSourcesVariable -Force

    if(-not $script:RegisteredPackageSources.Contains($Name))
    {
        Write-Error -Message "Package $Name not found" -ErrorId "PackageSourceNotFound" -Category InvalidOperation -TargetObject $Name
        return
    }

    $source = $script:RegisteredPackageSources[$Name]

    #Remove it from memory cache
    $script:RegisteredPackageSources.Remove($Name)

    Write-Verbose "$source"
    # Persist the package sources
    Save-PackageSources        
}
```

Copy/paste the above code to your provider. Try it out.

``` PowerShell
PS C:\Test> Import-PackageProvider myAlbum -force

Name                     Version          DynamicOptions
----                     -------          --------------
MyAlbum                  1.0.0.0          Destination

PS C:\Test> get-packagesource

Name                             ProviderName     IsTrusted  Location
----                             ------------     ---------  --------
PSGallery                        PowerShellGet    False      https://www.powershellgallery.com/api/v2/


PS C:\Test> Register-PackageSource -name test -provider myalbum -Location c:\test\1.0.1 -verbose
WARNING: Package source not found
VERBOSE: Performing the operation "Register Package Source." on target "Package Source 'test' (c:\test\1.0.1) in provider 'myalbum'.".
VERBOSE: @{Name=test; SourceLocation=C:\test\1.0.1; Trusted=False; Registered=True}

Name                             ProviderName     IsTrusted  Location
----                             ------------     ---------  --------
test                             MyAlbum          False      C:\test\1.0.1


PS C:\Test> get-packagesource

Name                             ProviderName     IsTrusted  Location
----                             ------------     ---------  --------
PSGallery                        PowerShellGet    False      https://www.powershellgallery.com/api/v2/
test                             MyAlbum          False      C:\test\1.0.1


PS C:\Test> unregister-packagesource -name test
PS C:\Test> get-packagesource

Name                             ProviderName     IsTrusted  Location
----                             ------------     ---------  --------
PSGallery                        PowerShellGet    False      https://www.powershellgallery.com/api/v2/

```

## Testing your provider
We recommend using [Pester][pester] as a test framework to write test cases for your provider testing.

See [sample test cases.][tc]

[pester]: https://github.com/pester/Pester
[tc]: https://github.com/OneGet/MyAlbum-Sample-Provider/blob/master/Test/MyAlbum.Tests.ps1


## Debugging your provider

For debugging PowerShell based OneGet your provider see [guidelines here.][debug]
[debug]: https://onedrive.live.com/?authkey=%21ABDm4cpGA4aEQ0U&cid=EF4B329A5EB9EA4D&id=EF4B329A5EB9EA4D%21130&parId=root&o=OneUp

Please note that to keep the code logic simple, I removed some necessary error checking on the above sample code.
For more completed the sample provider, see the published version of [MyAlbum.](https://github.com/OneGet/MyAlbum-Sample-Provider)

Congratulations! You have successfully created a OneGet provider!

Let's publish it to https://www.PowerShellGallery.com.
For how to publish module, see [Publish-Module instructions](https://msdn.microsoft.com/en-us/powershell/reference/5.1/powershellget/publish-module).

## Recommended Training and Reading
- [OneGet.org Overview](https://www.oneget.org)
- [Sample provider](https://github.com/OneGet/MyAlbum-Sample-Provider)
- [PowerShellGet Provider](https://github.com/PowerShell/PowerShellGet)
- [MicrosoftDockerProvider](https://github.com/OneGet/MicrosoftDockerProvider)
