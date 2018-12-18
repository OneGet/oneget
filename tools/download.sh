#!/usr/bin/env bash

# Let's quit on interrupt of subcommands
trap '
  trap - INT # restore default INT handler
  echo "Interrupted"
  kill -s INT "$$"
' INT

get_url() {
    release=v6.1.1
    echo "https://github.com/PowerShell/PowerShell/releases/download/$release/$1"
}

# Get OS specific asset ID and package name
case "$OSTYPE" in
    linux*)
        source /etc/os-release
        # Install curl and wget to download package
        case "$ID" in
            centos*)
                if ! hash curl 2>/dev/null; then
                    echo "curl not found, installing..."
                    sudo yum install -y curl
                fi
                
                package=powershell-6.1.1-1.rhel.7.x86_64.rpm
                ;;
            ubuntu)
                if ! hash curl 2>/dev/null; then
                    echo "curl not found, installing..."
                    sudo apt-get install -y curl
                fi

                case "$VERSION_ID" in
                    14.04)
                    https://github.com/PowerShell/PowerShell/releases/download/v6.1.1/powershell_6.1.1-1.ubuntu.14.04_amd64.deb
                        package=powershell_6.1.1-1.ubuntu.14.04_amd64.deb
                        ;;
                    16.04)
                        package=powershell_6.1.1-1.ubuntu.16.04_amd64.deb
                        ;;
                    *)
                        echo "Ubuntu $VERSION_ID is not supported!" >&2
                        exit 2
                esac
                ;;
            *)
                echo "$NAME is not supported!" >&2
                exit 2
        esac
        ;;
    darwin*)
        # We don't check for curl as macOS should have a system version
        package=powershell-6.1.1-osx-x64.pkg
        ;;
    *)
        echo "$OSTYPE is not supported!" >&2
        exit 2
        ;;
esac

curl -L -o "$package" $(get_url "$package")

if [[ ! -r "$package" ]]; then
    echo "ERROR: $package failed to download! Aborting..." >&2
    exit 1
fi

# Installs PowerShell package
case "$OSTYPE" in
    linux*)
        source /etc/os-release
        # Install dependencies
        echo "Installing PowerShell with sudo..."
        case "$ID" in
            centos)
                # yum automatically resolves dependencies for local packages
                sudo yum install "./$package"
                ;;
            ubuntu)
                # dpkg does not automatically resolve dependencies, but spouts ugly errors
                sudo dpkg -i "./$package" &> /dev/null
                # Update packages that need updating
                sudo apt-get update
                # Resolve dependencies
                sudo apt-get install -f
                ;;
            *)
        esac
        ;;
    darwin*)
        patched=0
        if hash brew 2>/dev/null; then
            if [[ ! -d $(brew --prefix openssl) ]]; then
               echo "Installing OpenSSL with brew..."
               if ! brew install openssl; then
                   echo "ERROR: OpenSSL failed to install! Crypto functions will not work..." >&2
                   # Don't abort because it is not fatal
               elif ! brew install curl --with-openssl; then
                   echo "ERROR: curl failed to build against OpenSSL; SSL functions will not work..." >&2
                   # Still not fatal
               else
                   # OpenSSL installation succeeded; remember to patch System.Net.Http after PowerShell installation
                   patched=1
               fi
            fi

        else
            echo "ERROR: brew not found! OpenSSL may not be available..." >&2
            # Don't abort because it is not fatal
        fi

        echo "Installing $package with sudo ..."
        sudo installer -pkg "./$package" -target /
        if [[ $patched -eq 1 ]]; then
            echo "Patching System.Net.Http for libcurl and OpenSSL..."
            find /usr/local/microsoft/powershell -name System.Net.Http.Native.dylib | xargs sudo install_name_tool -change /usr/lib/libcurl.4.dylib /usr/local/opt/curl/lib/libcurl.4.dylib
        fi
        ;;
esac

pwsh -noprofile -c '"Congratulations! PowerShell is installed at $PSHOME"'
success=$?

if [[ "$success" != 0 ]]; then
    echo "ERROR: PowerShell failed to install!" >&2
    exit "$success"
fi

: '
case "$OSTYPE" in
    linux*)
        # Install OMI and DSC for Linux
        get_url() {
            release=v1.1.0-0
            echo "https://github.com/Microsoft/omi/releases/download/$release/$1"
        }

        source /etc/os-release
        case "$ID" in
            centos*)
                if ! hash curl 2>/dev/null; then
                    echo "curl not found, installing..."
                    sudo yum install -y curl
                fi

                package=omi-1.1.0.ssl_100.x64.rpm
                ;;
            ubuntu)
                if ! hash curl 2>/dev/null; then
                    echo "curl not found, installing..."
                    sudo apt-get install -y curl
                fi
                        
                package=omi-1.1.0.ssl_100.x64.deb
                ;;
        esac
        echo "Downloading $package"
        curl -L -o "$package" $(get_url "$package")

        if [[ ! -r "$package" ]]; then
            echo "ERROR: $package failed to download! Aborting..." >&2
            exit 1
        fi

        source /etc/os-release
        echo "Installing OMI with sudo..."
        case "$ID" in
            centos*)
                # yum automatically resolves dependencies for local packages
                sudo yum install "./$package"
                ;;
            ubuntu)
                # dpkg does not automatically resolve dependencies, but spouts ugly errors
                sudo dpkg -i "./$package" &> /dev/null
                # Resolve dependencies
                sudo apt-get install -f
                ;;
            *)
        esac

        success=$?

        if [[ "$success" != 0 ]]; then
            echo "ERROR: OMI failed to install!" >&2
            exit "$success"
        fi

        # Install DSC
        get_url() {
            release=v1.1.1-294
            echo "https://github.com/Microsoft/PowerShell-DSC-for-Linux/releases/download/$release/$1"
        }

        source /etc/os-release
        case "$ID" in
            centos*)
                if ! hash curl 2>/dev/null; then
                    echo "curl not found, installing..."
                    sudo yum install -y curl
                fi

                package=dsc-1.1.1-294.ssl_100.x64.rpm
                ;;
            ubuntu)
                if ! hash curl 2>/dev/null; then
                    echo "curl not found, installing..."
                    sudo apt-get install -y curl
                fi
                        
                package=dsc-1.1.1-294.ssl_100.x64.deb
                ;;
        esac
        echo "Downloading $package"
        curl -L -o "$package" $(get_url "$package")

        if [[ ! -r "$package" ]]; then
            echo "ERROR: $package failed to download! Aborting..." >&2
            exit 1
        fi

        source /etc/os-release
        echo "Installing DSC with sudo..."
        case "$ID" in
            centos)
                # yum automatically resolves dependencies for local packages
                sudo yum install "./$package"
                ;;
            ubuntu)
                # dpkg does not automatically resolve dependencies, but spouts ugly errors
                sudo dpkg -i "./$package" &> /dev/null
                success=$?

                if [[ "$success" != 0 ]]; then
                    echo "ERROR: DSC failed to install!" >&2
                    exit "$success"
                fi

                # Resolve dependencies
                sudo apt-get install -f
                ;;
            *)
        esac

        success=$?

        if [[ "$success" != 0 ]]; then
            echo "ERROR: DSC failed to install!" >&2
            exit "$success"
        fi

        pwsh -noprofile -c '"Congratulations! PowerShell DSC for Linux is installed!"'
        ;;
    darwin*)
        # TODO: Need to do anything here?
esac'