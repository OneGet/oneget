# OneGet

> *OneGet Online Meeting Friday mornings @ 10:00PDT [via Lync](https://join.microsoft.com/meet/garretts/HZ96LF57)* (everyone welcome!)

OneGet is a unified interface to package management systems and aims to make Software Discovery, Installation and Inventory (SDII) work via a common set of cmdlets (and eventually a set of APIs). Regardless of the installation technology underneath, users can use these common cmdlets to install/uninstall packages, add/remove/query package repositories, and query a system for the software installed. Included in this CTP is a prototype implementation of a Chocolatey-compatible package manager that can install existing Chocolatey packages.

With OneGet, you can
* Manage a list of software repositories in which packages can be searched, acquired, and installed
* Search and filter your repositories to find the packages you need
* Seamlessly install and uninstall packages from one or more repositories with a single PowerShell command


### Get Started!

Download  [here](http://www.microsoft.com/en-us/download/details.aspx?id=42316) -- currently it's in the WMF 5.0 CTP (WMF is where you get PowerShell!)  -- once the source is up, we'll post a build of just the OneGet bits.

* Learn how to [use the powershell cmdlets](https://github.com/OneGet/oneget/wiki/cmdlets) 
* Read our [General Q and A](https://github.com/OneGet/oneget/wiki/Q-and-A)
* Learn about the [8 Laws of Software Installation](https://github.com/OneGet/oneget/wiki/8-Laws-of-Software-Installation)
* See the [documentation](https://github.com/OneGet/oneget/wiki) tab for more info.

OneGet should be shipping inside future versions of PowerShell, and by extension, would ship in future versions of Windows.

We'll also be publishing standalone builds, once the source code is published.

And *yes*, we're going to work with the Chocolatey project to have one single chocolatey plugin that meets everyone's needs : [see Rob's post](https://groups.google.com/forum/#!topic/chocolatey/a8WdEoF-M58)

[Follow us on Twitter](https://twitter.com/PSOneGet)
