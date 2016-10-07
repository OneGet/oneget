### Build status - Master

| AppVeyor (Windows)       | Travis CI (Linux / macOS) |
|--------------------------|--------------------------|
| [![av-image][]][av-site] | [![tv-image][]][tv-site] |

[tv-image]: https://travis-ci.org/OneGet/oneget.svg?branch=master
[tv-site]: https://travis-ci.org/OneGet/oneget/branches
[av-image]: https://ci.appveyor.com/api/projects/status/0q1frhm84pp83syh/branch/master?svg=true
[av-site]: https://ci.appveyor.com/project/OneGet/oneget

# PackageManagement (aka OneGet)


### What's New
Follow our [News Panel](https://github.com/OneGet/oneget/wiki/News-Panel).

Check out the PackageManagement and PowershellGet MSI package [March release for downlevel OSs!] (https://www.microsoft.com/en-us/download/details.aspx?id=51451)


### Get Started!

OneGet is shipped in Win10 Client RTM! For downlevel OS, you can install the [WMF 5.0 RTM] (https://www.microsoft.com/en-us/download/details.aspx?id=50395) and then start using the OneGet.

You can follow [@PSOneGet on Twitter](http://twitter.com/PSOneGet) to be notified of every new build.


* Learn how to [use the powershell cmdlets](https://github.com/OneGet/oneget/wiki/cmdlets), [try some samples] (https://github.com/PowerShell/PowerShell-Docs/blob/staging/wmf/5.0/oneget_cmdlets.md), or read [our MSDN Technet docs] (https://technet.microsoft.com/en-us/library/mt422622.aspx)
* Read our [General Q and A](https://github.com/OneGet/oneget/wiki/Q-and-A)
* Learn about the [8 Laws of Software Installation](https://github.com/OneGet/oneget/wiki/8-Laws-of-Software-Installation)
* [General Troubleshooting] (https://github.com/OneGet/oneget/wiki/General-Troubleshooting)
* Check out more help information [in our wiki page] (https://github.com/oneget/oneget/wiki)

#### What is PackageManagement (OneGet)?

OneGet is a Windows package manager, renamed as PackageManagement. It is a unified interface to package management systems and aims to make Software Discovery, Installation and Inventory (SDII) work via a common set of cmdlets (and eventually a set of APIs). Regardless of the installation technology underneath, users can use these common cmdlets to install/uninstall packages, add/remove/query package repositories, and query a system for the software installed. 

With OneGet, you can
* Manage a list of software repositories in which packages can be searched, acquired, and installed
* Search and filter your repositories to find the packages you need
* Seamlessly install and uninstall packages from one or more repositories with a single PowerShell command

#####PackageManagement Architecture#####

![Image](./assets/OneGetArchitecture.PNG?raw=true)


### Building the code

#### Required Tools
- Visual Studio 2013
- Powershell Tools for Visual Studio : http://visualstudiogallery.msdn.microsoft.com/c9eb3ba8-0c59-4944-9a62-6eee37294597
- XUnit ( I currently use 2.0.0.0 ) : http://xunit.codeplex.com/releases
- You may need to manually install Windows SDK for getting tools like mt.exe.

#### Optional Tools
- Resharper - http://www.jetbrains.com/resharper/
- Resharper xUnit test runner - http://resharper-plugins.jetbrains.com/packages/xunitcontrib/2.0.0
- Wix 3.9 : http://wixtoolset.org (only if you want to build the MSI and Installer)

check out the source code
``` powershell

#clone this repository
> git clone --recurse-submodules https://github.com/OneGet/oneget.git

# go to the project folder
> cd oneget

# optional: switch to the wip branch
> git checkout wip

# get the submodules for this branch
> git submodule update --init

# BUILD using Visual Studio, or from the command line:

> msbuild PackageManagement.sln /p:Configuration=Release "/p:Platform=Any CPU"

# If you want to send me changes, you should fork the project into your own
# account first, and use that URL to clone it.
# If you fork it later you can just change the origin by:

# move the old origin out of the way. You could delete it if you want.
> git remote rename origin original

# add your repo url as the origin:
# e.g. git@github.com:fearthecowboy/OneGet.git
> git remote add origin <your-repo-url>

# build & deploy binaries and run test
build the packagemanagment.sln:

    msbuild PackageManagement.sln /p:Configuration=Release "/p:Platform=Any CPU"

cd to the test folder
.\run-test.ps1  will copy the files generated from the build to x:\Program Files\WindowsPowerShell\Modules\PackageManagement
and update the PowerShellGet to x:\Program Files\WindowsPowerShell\Modules\PowerShellGet. Also run the tests.

```


### Understanding the OneGet code repository

OneGet is under rapid development, and so you get to see just how the sausage is being made. I try to keep the master branch clean and buildable, but my own working branch can get pretty damn wild and I make no bones about some of this. I work fast, I make big changes, and I try to keep my eye on the target.

Feel free to clone the repository, and check out the different branches:

#### Branches

There are currently three branches in the git repository:

| Branch/Tag | Purpose |
| ------- | ---------------------------|
|`master`|  The `master` branch is where the daily builds of OneGet will be made from.  |
|`WMF5_RTM`|  The `WMF5_RTM` tag is to mark the WMF 5.0 RTM release point. |
|`TP5`|  The `TP5` tag is to mark the TP5 release point. |
|`wip`|  The `wip` branch is where the current **unstable** and **not-likely-working** coding is taking place. This lets you see where I'm at before stuff actually hits the master branch. Fun to read, but really, the wild-west of code branches. |


### Contributing to OneGet

Contributions to the OneGet project will require the signing of a CLA -- contact @jianyunt for details...

In the immediate time frame, we won't be taking pull requests to the core itself, as we still have many masters at Microsoft to keep happy, and I have a lot of release process stuff I have to go thru to make them happy.

There are some exceptions to the where I can take Pull Requests immediately:

> Pull Requests to the Package Providers are instantly welcome

> Any unit tests, BVT tests or -Edge only features, we can take pull requests for as well

> Docs, Wiki, content, designs, bugs -- everything gleefully accepted :D


### Participating in the OneGet Community

I'm eager to work with anyone who wants to help shape the future of Package Management on Windows -- your opinions, feedback and code can help everyone.


### Online Meeting

We have an online monthly meeting at the beginning of each month on Tuesday from 10am - 11am (PST). Each month may have slight shift of either the first week or the second week.  We will twitter the exact time as well as put a note on GitHub site.  (everyone welcome!)

You can see archives of the previous meetings available on

All meeting notes are recorded under [OneDrive PackageManagement](https://onedrive.live.com/?authkey=%21ABehsy6i3rzQdxE&id=EF4B329A5EB9EA4D%21127&cid=EF4B329A5EB9EA4D)


### Project Dashboard

You can see issues, pull requests, backlog items, etc. in the [OneGet Dashboard](https://waffle.io/oneget/oneget)

[![Stories in Progress](https://badge.waffle.io/oneget/oneget.svg?label=Bug&title=Bug)](http://waffle.io/OneGet/OneGet)
[![Stories in Progress](https://badge.waffle.io/oneget/oneget.svg?label=Investigate&title=Investigate)](http://waffle.io/OneGet/OneGet)
[![Stories in Progress](https://badge.waffle.io/oneget/oneget.svg?label=Discussion&title=Discussion)](http://waffle.io/OneGet/OneGet)
[![Stories in Progress](https://badge.waffle.io/oneget/oneget.svg?label=New%20Feature&title=New%20Feature)](http://waffle.io/OneGet/OneGet)
[![Stories in Progress](https://badge.waffle.io/oneget/oneget.svg?label=PowerShellGet&title=PowerShellGet)](http://waffle.io/OneGet/OneGet)

Throughput Graph

[![Throughput Graph](https://graphs.waffle.io/OneGet/oneget/throughput.svg)](https://waffle.io/OneGet/oneget/metrics)


### Team Members

| Branch | Purpose |
| ------- | ---------------------------|
|@Xumin|  Program Manager on OneGet. Xumin is the sheriff, trying to keep the law. If there are rules that we need to play by, Xumin make us follow them.   |
|@Jianyun|  Engineer owner on OneGet & its providers. |
|@Krishna|  Our engineer manager on OneGet, also owner for PowerShell Gallery.   |
|@Quoc|  Engineer on the team.   |

[Follow us on Twitter](https://twitter.com/PSOneGet)
