
# OneGet
<hr><hr>

### What's New
Follow our [News Panel](https://github.com/OneGet/oneget/wiki/News-Panel).

### Get Started!

OneGet is shipped inside PowerShell in Win10 Client RTM! For downlevel OS, OneGet is included in WMF 5.0. Download the latest official release [here](http://www.microsoft.com/en-us/download/details.aspx?id=48729) -- currently it's in the WMF 5.0 Production Preview.

You can follow [@PSOneGet on Twitter](http://twitter.com/PSOneGet) to be notified of every new build.


* Learn how to [use the powershell cmdlets](https://github.com/OneGet/oneget/wiki/cmdlets) 
* Read our [General Q and A](https://github.com/OneGet/oneget/wiki/Q-and-A)
* Learn about the [8 Laws of Software Installation](https://github.com/OneGet/oneget/wiki/8-Laws-of-Software-Installation)
* See the [documentation](https://github.com/OneGet/oneget/wiki) tab for more info
* Check out [install note](https://github.com/OneGet/oneget/issues/68).



## Building the code

#### Required Tools
- Visual Studio 2013
- Powershell Tools for Visual Studio : http://visualstudiogallery.msdn.microsoft.com/c9eb3ba8-0c59-4944-9a62-6eee37294597
- XUnit ( I currently use 2.0.0.0 ) : http://xunit.codeplex.com/releases

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

# run test
build the packagemanagment.sln
copy the files generated from the build to x:\Program Files\WindowsPowerShell\Modules\PackageManagement
You may need to update the PowerShellGet files that are in the inbox folder.
cd to the test folder
.\run-test.ps1
```


## Understanding the OneGet code repository

OneGet is under rapid development, and so you get to see just how the sausage is being made. I try to keep the master branch clean and buildable, but my own working branch can get pretty damn wild and I make no bones about some of this. I work fast, I make big changes, and I try to keep my eye on the target.

Feel free to clone the repository, and check out the different branches:

### Branches

There are currently three branches in the git repository:

| Branch | Purpose |
| ------- | ---------------------------|
|`master`|  The `master` branch is where the daily builds of OneGet will be made from.  |
|`ctp`|  The `ctp` branch is a snapshot of the code that was released as the WMF 5.0 CTP back in April. This branch is frozen and here only for completion sake. No pull requests/patches will be taken for this, as it's a dead end. The new `master` branch is where all the new action is taking place. Fun to read, but a bit hacky in places. |
|`wip`|  The `wip` branch is where the current **unstable** and **not-likely-working** coding is taking place. This lets you see where I'm at before stuff actually hits the master branch. Fun to read, but really, the wild-west of code branches. |

## Contributing to OneGet

Contributions to the OneGet project will require the signing of a CLA -- contact @fearthecowboy for details...

In the immediate time frame, we won't be taking pull requests to the core itself, as we still have many masters at Microsoft to keep happy, and I have a lot of release process stuff I have to go thru to make them happy. 

There are some exceptions to the where I can take Pull Requests immediately: 

> Pull Requests to the not-in-core Package Providers (Chocolatey, NuGet, etc) are instantly welcome 

> Any unit tests, BVT tests or -Edge only features, we can take pull requests for as well

> Docs, Wiki, content, designs, bugs -- everything gleefully accepted :D
  

## Participating in the OneGet Community

I'm eager to work with anyone who wants to help shape the future of Package Management on Windows -- your opinions, feedback and code can help everyone. 

### Online Meeeting 

We have an online monthly meeting at the beginning of each month on Tuesday from 10am - 11am (PST). Each month may have slight shift of either the first week or the second week.  We will twitter the exact time as well as put a note on GitHub site.  (everyone welcome!)

You can see archives of the previous meetings available on [YouTube](https://www.youtube.com/playlist?list=PLeKWr5Ekac1SEEvHqIh3g051OyioFwOXN&feature=c4-feed-u)

<hr><hr>
## What is OneGet?

OneGet is a unified interface to package management systems and aims to make Software Discovery, Installation and Inventory (SDII) work via a common set of cmdlets (and eventually a set of APIs). Regardless of the installation technology underneath, users can use these common cmdlets to install/uninstall packages, add/remove/query package repositories, and query a system for the software installed. Included in this CTP is a prototype implementation of a Chocolatey-compatible package manager that can install existing Chocolatey packages.

With OneGet, you can
* Manage a list of software repositories in which packages can be searched, acquired, and installed
* Search and filter your repositories to find the packages you need
* Seamlessly install and uninstall packages from one or more repositories with a single PowerShell command



We'll also be publishing standalone builds, once the source code is published.


[Follow us on Twitter](https://twitter.com/PSOneGet)

<hr><hr>
## Team Members

| Branch | Purpose |
| ------- | ---------------------------|
|@fearthecowboy|  Godfather of OneGet project, has moved on to some other exciting project. But will always stand by with OneGet to give us supports. |
|@Xumin|  Program Manager on OneGet. Xumin is the sheriff, trying to keep the law. If there are rules that we need to play by, Xumin make us follow them.   |
|@Jianyun|  New engineer owner on OneGet. |
|@Krishna|  Our engineer manager on OneGet, also owner for PowerShell Gallery.   |
|@Quoc|  Our newest and freshest engineer.   |
