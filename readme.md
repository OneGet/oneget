## Announcement 

OneGet is in a stable state and is expected to recieve only high-priority bug fixes from Microsoft in the future. We continue to welcome updates and improvements from the community.  

### Build Status - Master

| AppVeyor (Windows)       | Travis CI (Linux / macOS) |
|--------------------------|--------------------------|
| [![av-image][]][av-site] | [![tv-image][]][tv-site] |


[tv-image]: https://travis-ci.org/OneGet/oneget.svg?branch=master
[tv-site]: https://travis-ci.org/OneGet/oneget/branches
[av-image]: https://ci.appveyor.com/api/projects/status/0q1frhm84pp83syh/branch/master?svg=true
[av-site]: https://ci.appveyor.com/project/jianyunt/oneget

### Build Status - Nightly Builds
|AppVeyor (Windows)                  |
|------------------------------------|
| [![av-nightimage][]][av-nightsite] |

[av-nightimage]: https://ci.appveyor.com/api/projects/status/87d07mj8s9eyhfst/branch/master?svg=true
[av-nightsite]:https://ci.appveyor.com/project/jianyunt/oneget-weumx

# PackageManagement (aka OneGet)


### What's New
PackageManagement is supported in Windows, Linux and MacOS now.
We periodically make binary drops to [PowerShellCore][pscore],
meaning PackageManagement is a part of PowerShell Core releases.
Also PackageManagement and PowershellGet Modules are regularly updated in [PowerShellGallery.com](https://www.PowerShellGallery.com).

Thus check out the latest version from PowerShellGallery.com.

[pscore]: https://github.com/PowerShell/PowerShell

### Get Started!

OneGet is shipped in Win10 and Windows Server 2016! For downlevel OS, you can install the [WMF 5.1][WMF5RTM] and then start using the OneGet.

You can follow [@PSOneGet on Twitter](http://twitter.com/PSOneGet) to be notified of every new build.


* Learn how to [use the PowerShell OneGet cmdlets](https://github.com/OneGet/oneget/wiki/cmdlets) and [try some samples](https://github.com/PowerShell/PowerShell-Docs/blob/staging/wmf/5.0/oneget_cmdlets.md)
* Read our [General Q and A](https://github.com/OneGet/oneget/wiki/Q-and-A)
* Read [Writing OneGet Provider Guidelines](./docs/writepowershellbasedprovider.md)
* Learn about the [8 Laws of Software Installation](https://github.com/OneGet/oneget/wiki/8-Laws-of-Software-Installation)
* [General Troubleshooting](https://github.com/OneGet/oneget/wiki/General-Troubleshooting)
* Check out more help information [in our wiki page](https://github.com/oneget/oneget/wiki)


[WMF5RTM]: https://aka.ms/wmf5download

### What is PackageManagement (OneGet)?

OneGet is a Windows package manager, renamed as PackageManagement. It is a unified interface to package management systems and aims to make Software Discovery, Installation, and Inventory (SDII) work via a common set of cmdlets (and eventually a set of APIs). Regardless of the installation technology underneath, users can use these common cmdlets to install/uninstall packages, add/remove/query package repositories, and query a system for the software installed.

With OneGet, you can
* Manage a list of software repositories in which packages can be searched, acquired, and installed
* Search and filter your repositories to find the packages you need
* Seamlessly install and uninstall packages from one or more repositories with a single PowerShell command

#### PackageManagement Architecture

![Image](./assets/OneGetArchitecture.PNG?raw=true)

<br/>


### Let's Try it

#### Prerequisites
 - Windows 10, Windows Server 2016, or down-level Windows OS + WMF5
 - Linux or Mac with the [PowerShellCore][pscore]


#### Working with PowerShellGallery.com

 ```powershell
 # 1.check available providers

 PS E:\> get-packageprovider

Name                     Version          DynamicOptions
----                     -------          --------------
msi                      3.0.0.0          AdditionalArguments
msu                      3.0.0.0
PowerShellGet            1.1.0.0          PackageManagementProvider, Type...
Programs                 3.0.0.0          IncludeWindowsInstaller,...

# 2. find a module from the PowerShell gallery, for example, xjea

PS E:\> find-module xjea

NuGet provider is required to continue
PowerShellGet requires NuGet provider version '2.8.5.201' or newer to interact with NuGet-based repositories. The NuGet provider must be available in 'C:\Program
Files\PackageManagement\ProviderAssemblies' or 'C:\Users\jianyunt\AppData\Local\PackageManagement\ProviderAssemblies'. You can also install the NuGet provider by
running 'Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force'. Do you want PowerShellGet to install and import the NuGet provider now?
[Y] Yes  [N] No  [S] Suspend  [?] Help (default is "Y"): y

Version    Name           Repository           Description
-------    ----           ----------           -----------
0.3.0.0    xJea           PSGallery             Module with DSC Resources for Just Enough...

# 3. install a module from the PowerShell gallery

PS E:\> Install-Module xjea

Untrusted repository
You are installing the modules from an untrusted repository. If you trust this repository, change its InstallationPolicy value by running the Set-PSRepository cmdlet. Are
you sure you want to install the modules from 'gallery'?
[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "N"): y

# 4. Find out if a module is installed

PS E:\> Get-InstalledModule -name xjea

Version    Name        Repository      Description
-------    ----        ----------       -----------
0.3.0.0    xJea        gallery          Module with DSC Resources for Just Enough Admin (JEA)..

# 5. Uninstall a module

PS E:\> Uninstall-Module -name xjea
```

#### Working with http://www.NuGet.org repository

```powershell

# find a package from the nuget repository

PS E:\> find-package -name jquery -provider Nuget -Source https://www.nuget.org/api/v2

Name           Version          Source           Summary
----           -------          ------           -------
jQuery          3.1.1            nuget.org        jQuery is a new kind of JavaScript Library....

# install a package from NuGet repository

PS E:\> install-package -name jquery -provider Nuget -Source https://www.nuget.org/api/v2

The package(s) come(s) from a package source that is not marked as trusted.
Are you sure you want to install software from 'nuget.org'?
[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "N"): y

Name             Version          Source           Summary
----             -------          ------           -------
jQuery           3.1.1            nuget.org        jQuery is a new kind of JavaScript Library....

# Uninstall the package

PS E:\> uninstall-package jquery

Name            Version          Source           Summary
----            -------          ------           -------
jQuery          3.1.1            C:\Program Fi... jQuery is a new kind of JavaScript Library....

# Register a package Source

PS E:\> Register-PackageSource -name test -ProviderName NuGet -Location https://www.nuget.org/api/v2

Name             ProviderName     IsTrusted  Location
----              ------------     ---------  --------
test              NuGet            False      https://www.nuget.org/api/v2

# find a package from the registered package Source

PS E:\> find-package -Source test -name jquery

Name                Version          Source           Summary
----                -------          ------           -------
jQuery              3.1.1            test             jQuery is a new kind of JavaScript Library....
```

### Try the latest PackageManagement (OneGet)

You can run `install-module PowerShellGet` to install the latest PackageManagment and PowerShellGet from [PowerShellGallery](https://www.powershellgallery.com).

### Downloading the Source Code
OneGet repo has a number of other repositories embeded as submodules. To make things easy, you can just clone recursively:
```powershell
git clone --recursive https://github.com/OneGet/oneget.git
```
If you already cloned but forgot to use `--recursive`, you can update submodules manually:
```powershell
git submodule update --init
```

### Building the code

``` powershell
# After cloning this repository, go to the project folder:
> cd oneget
> cd src

# download the dotnet cli tool
> .\bootstrap.ps1

# building OneGet for fullclr
> .\build.ps1 net452

#building OneGet for coreclr
> .\build.ps1 netstandard2.0
```

If successfully built above, you should be able to see a folder:
`oneget\src\out\PackageManagement\` whose layout looks like below:

 * `coreclr`
 * `fullclr`
 * `PackageManagement.format.ps1xml`
 * `PackageManagement.psd1`
 * `PackageManagement.psm1`
 * `PackageProviderFunctions.psm1`

### Deploying it

#### Generate PackageManagement.nupkg
We can use `publish-module` to create a .nupkg. Assuming you want to put the generated .nupkg in c:\test folder.  You can do something like below. Note I cloned to E:\OneGet folder.
 ```powershell
cd E:\OneGet\oneget\src\out\PackageManagement
Register-PSRepository -name local -SourceLocation c:\test
Get-PSRepository
Publish-Module -path .\ -Repository local
PS E:\OneGet\oneget\src\out\PackageManagement> dir c:\test\PackageManagement*.nupkg

    Directory: C:\test


Mode                LastWriteTime         Length Name
----                -------------         ------ ----
-a----        11/4/2016   4:15 PM        1626335 PackageManagement.1.1.0.0.nupkg
```
Then you can do
```powershell
find-module -Repository local
install-module -Repository local -Name PackageManagement
```
to get the newly built PackageManagement on your machines.

#### Manual copy
You can also manually copy the OneGet binaries. For example, copy the entire `E:\OneGet\oneget\src\out\PackageManagement` folder you just built to your
`$env:Programfiles\WindowsPowerShell\Modules\PackageManagement\#onegetversion\`

If you are running within PowerShellCore,
similarily drop the PackageManagement folder to your `$env:Programfiles\PowerShell\#psversion\Modules\PackageManagement\#onegetversion\`,

or copy to `/opt/microsoft/powershell/<psversion>/Modules/PackageManagement/#onegetversion/`,
if you are running on Linux or Mac.

**Note**: OneGet version number can be found from the PackageManagement.psd1 file.

### Testing the code
```PowerShell
> cd oneget
> cd Test
> & '.\run-tests.ps1' fullclr
> & '.\run-tests.ps1' coreclr
```

### Understanding the OneGet code repository

OneGet is under rapid development, so you get to see just how the sausage is being made. I try to keep the master branch clean and buildable, but my own working branch can get pretty damn wild and I make no bones about some of this. I work fast, I make big changes, and I try to keep my eye on the target.

Feel free to clone the repository and check out the different branches:

#### Branches

There are currently three branches in the git repository:

| Branch/Tag | Purpose |
| ------- | ---------------------------|
|`master`|  The `master` branch is where the daily builds of OneGet will be made from.  |
|`WMF5_RTM`|  The `WMF5_RTM` tag is to mark the WMF 5.0 RTM release point. |
|`TP5`|  The `TP5` tag is to mark the TP5 release point. |
|`wip`|  The `wip` branch is where the current **unstable** and **not-likely-working** coding is taking place. This lets you see where I'm at before stuff actually hits the master branch. Fun to read, but really, the wild-west of code branches. |


### Team Members

| Branch | Purpose |
| ------- | ---------------------------|
|@sydneyhsmith |  Program Manager on OneGet.  |
|@jianyunt|  Engineer owner on OneGet & its providers. |
|@edyoung|  Our engineer manager on OneGet.   |
|@alerickson|  Engineer on the team.   |

[Follow us on Twitter](https://twitter.com/PSOneGet)

### More Resources
- [NuGet Provider](https://github.com/OneGet/NuGetProvider)
- [PowerShellGet Provider](https://github.com/PowerShell/PowerShellGet)
- [MicrosoftDockerProvider](https://github.com/OneGet/MicrosoftDockerProvider)
- [NanoServerPackage](https://github.com/OneGet/NanoServerPackage)
- Check out OneGet providers from our Community such as Gistprovider, OfficeProvider, 0Install and more from powershellgallery.com or simply run [Find-PackageProvider cmdlet](https://msdn.microsoft.com/en-us/powershell/gallery/psget/oneget/packagemanagement_cmdlets)
- Want to write a provider? Check out our [sample provider](https://www.powershellgallery.com/packages/MyAlbum/)
- Want to download packages from http://Chocolatey.org, try out [ChocolateyGet provider](https://www.powershellgallery.com/packages/ChocolateyGet)
- Want to control which packages to use and where to get them from based on your organization? Check out [PSL provider](https://github.com/OneGet/PSLProvider)
