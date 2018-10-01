# Use the .NET Core APIs to determine the current platform; if a runtime
# exception is thrown, we are on FullCLR, not .NET Core.
try {
    $Runtime = [System.Runtime.InteropServices.RuntimeInformation]
    $OSPlatform = [System.Runtime.InteropServices.OSPlatform]

    Write-Output("runtime: " + $Runtime)
    Write-Output("OSPlatform: " + $OSPlatform)
    Write-Output($Runtime::IsOSPlatform($OSPlatform::OSX))
    Write-Output($PSVersionTable.PSVersion)
    
    $IsCoreCLR = ($PSVersionTable.ContainsKey('PSEdition')) -and ($PSVersionTable.PSEdition -eq 'Core')
    $IsOSX = $Runtime::IsOSPlatform($OSPlatform::OSX)
    Write-Output ("is osx in try block: " + $IsOSX)
    $IsLinux = $Runtime::IsOSPlatform($OSPlatform::Linux)
    $IsWindows = $Runtime::IsOSPlatform($OSPlatform::Windows)
} catch {
    # If these are already set, then they're read-only and we're done
    try {
        Write-Output("IsOSX is getting set to false")
        $IsCoreCLR = $false
        $IsLinux = $false
        $IsOSX = $false
        $IsWindows = $true
    }
    catch { }
}

if ($IsLinux) {
    $LinuxInfo = Get-Content /etc/os-release | ConvertFrom-StringData

    $IsUbuntu = $LinuxInfo.ID -match 'ubuntu'
    $IsUbuntu14 = $IsUbuntu -and $LinuxInfo.VERSION_ID -match '14.04'
    $IsUbuntu16 = $IsUbuntu -and $LinuxInfo.VERSION_ID -match '16.04'
    $IsCentOS = $LinuxInfo.ID -match 'centos' -and $LinuxInfo.VERSION_ID -match '7'
}

function Start-DotnetBootstrap {
    [CmdletBinding(
        SupportsShouldProcess=$true,
        ConfirmImpact="High")]
    param(
        [string]$Channel = "preview",
        #[string]$Version = "latest",
        # we currently pin dotnet-cli version, because tool
        # is currently migrating to msbuild toolchain
        # and requires constant updates to our build process.
        [string]$Version = "2.1.4"
    )

    # Install ours and .NET's dependencies
    $Deps = @()
    if ($IsUbuntu) {
        # Build tools
        $Deps += "curl", "g++", "cmake", "make"

        # .NET Core required runtime libraries
        $Deps += "libunwind8-dev"
        if ($IsUbuntu14) { $Deps += "libicu52" }
        elseif ($IsUbuntu16) { $Deps += "libicu55" }

        # Install dependencies
        sudo apt-get update
        sudo apt-get install -y -qq $Deps
    } elseif ($IsCentOS) {
        # Build tools
        $Deps += "which", "curl", "gcc-c++", "cmake", "make"

        # .NET Core required runtime libraries
        $Deps += "libicu", "libunwind"

        # Install dependencies
        sudo yum install -y -q $Deps
    } elseif ($IsOSX) {

        Write-Output("Debugging Mac image 1")
        # Build tools
        $Deps += "curl", "cmake"
        Write-Output("Debugging Mac image 2")

        # .NET Core required runtime libraries
        $Deps += "openssl"
        Write-Output("Debugging Mac image 3")

        # Install dependencies
        brew install $Deps
        Write-Output("Debugging Mac image 4")
    }

    Write-Output("is osx?: ")
    Write-Output($IsOSX)
    Write-Output("Debugging Mac image 5")
    $obtainUrl = "https://raw.githubusercontent.com/dotnet/cli/master/scripts/obtain"

    # Install for Linux and OS X
    if ($IsLinux -or $IsOSX) {
        Write-Output("Debugging Mac image 6")
        # Uninstall all previous dotnet packages
        $uninstallScript = if ($IsUbuntu) {
            "dotnet-uninstall-debian-packages.sh"
        } elseif ($IsOSX) {
            Write-Output("Debugging Mac image 7")
            "dotnet-uninstall-pkgs.sh"
        }

        if ($uninstallScript) {
            curl -s $obtainUrl/uninstall/$uninstallScript -o $uninstallScript
            chmod +x $uninstallScript
            sudo ./$uninstallScript
        } else {
            Write-Warning "This script only removes prior versions of dotnet for Ubuntu 14.04 and OS X"
        }

        Write-Output("Debugging Mac image 8")
        # Install new dotnet packages
        $installScript = "dotnet-install.sh"
        curl -s $obtainUrl/$installScript -o $installScript
        chmod +x $installScript
        bash ./$installScript -c $Channel -v $Version
        Write-Output("Debugging Mac image 9")

        # .NET Core's crypto library needs brew's OpenSSL libraries added to its rpath
        if ($IsOSX) {
            Write-Output("Debugging Mac image 10")
            # This is the library shipped with .NET Core
            # This is allowed to fail as the user may have installed other versions of dotnet
            Write-Warning ".NET Core links the incorrect OpenSSL, correcting .NET CLI libraries..."
            find $env:HOME/.dotnet -name System.Security.Cryptography.Native.dylib | xargs sudo install_name_tool -add_rpath /usr/local/opt/openssl/lib
        }
    }

    # Install for Windows
    if ($IsWindows -and -not $IsCoreCLR) {
        Remove-Item -ErrorAction SilentlyContinue -Recurse -Force ~\AppData\Local\Microsoft\dotnet
        $installScript = "dotnet-install.ps1"
        Invoke-WebRequest -Uri $obtainUrl/$installScript -OutFile $installScript
        & ./$installScript -c $Channel -Version $Version

    } elseif ($IsWindows) {
        Write-Warning "Start-PSBootstrap cannot be run in Core PowerShell on Windows (need Invoke-WebRequest!)"
    }
}

Start-DotnetBootstrap
