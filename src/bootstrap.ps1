# Use the .NET Core APIs to determine the current platform; if a runtime
# exception is thrown, we are on FullCLR, not .NET Core.
try {
    $Runtime = [System.Runtime.InteropServices.RuntimeInformation]
    $OSPlatform = [System.Runtime.InteropServices.OSPlatform]

    $IsCoreCLR = $true
    $IsLinux = $Runtime::IsOSPlatform($OSPlatform::Linux)
    $IsOSX = $Runtime::IsOSPlatform($OSPlatform::OSX)
    $IsWindows = $Runtime::IsOSPlatform($OSPlatform::Windows)
} catch {
    # If these are already set, then they're read-only and we're done
    try {
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
        [string]$Channel = "rel-1.0.0",
        #[string]$Version = "latest",
        # we currently pin dotnet-cli version, because tool
        # is currently migrating to msbuild toolchain
        # and requires constant updates to our build process.
        [string]$Version = "1.0.0-preview3-003930"              
    )

    # Install ours and .NET's dependencies
    $Deps = @()
    if ($IsUbuntu) {
        # Build tools
        $Deps += "curl", "g++", "cmake", "make"

        # .NET Core required runtime libraries
        $Deps += "libunwind8"
        if ($IsUbuntu14) { $Deps += "libicu52" }
        elseif ($IsUbuntu16) { $Deps += "libicu55" }

        # Install dependencies
        sudo apt-get install -y -qq $Deps
    } elseif ($IsCentOS) {
        # Build tools
        $Deps += "which", "curl", "gcc-c++", "cmake", "make"

        # .NET Core required runtime libraries
        $Deps += "libicu", "libunwind"

        # Install dependencies
        sudo yum install -y -q $Deps
    } elseif ($IsOSX) {

        # Build tools
        $Deps += "curl", "cmake"

        # .NET Core required runtime libraries
        $Deps += "openssl"

        # Install dependencies
        brew install $Deps
    }


    # this url is temporarely workaround because of https://github.com/dotnet/cli/issues/4715
 
    #$obtainUrl = "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain"

    $obtainUrl = "https://raw.githubusercontent.com/dotnet/cli/9855dc0088cf7e56e24860c734f33fe8353f38a6/scripts/obtain"
	  

    # Install for Linux and OS X
    if ($IsLinux -or $IsOSX) {
        # Uninstall all previous dotnet packages
        $uninstallScript = if ($IsUbuntu) {
            "dotnet-uninstall-debian-packages.sh"
        } elseif ($IsOSX) {
            "dotnet-uninstall-pkgs.sh"
        }

        if ($uninstallScript) {
            curl -s $obtainUrl/uninstall/$uninstallScript -o $uninstallScript
            chmod +x $uninstallScript
            sudo ./$uninstallScript
        } else {
            Write-Warning "This script only removes prior versions of dotnet for Ubuntu 14.04 and OS X"
        }

        # Install new dotnet 1.0.0 preview packages
        $installScript = "dotnet-install.sh"
        curl -s $obtainUrl/$installScript -o $installScript
        chmod +x $installScript
        bash ./$installScript -c $Channel -v $Version

        # .NET Core's crypto library needs brew's OpenSSL libraries added to its rpath
        if ($IsOSX) {
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
        & ./$installScript -c $Channel -v $Version

    } elseif ($IsWindows) {
        Write-Warning "Start-PSBootstrap cannot be run in Core PowerShell on Windows (need Invoke-WebRequest!)"
    }
}

Start-DotnetBootstrap
