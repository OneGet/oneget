namespace Microsoft.PackageManagement.Internal.Utility.Platform
{
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;

    /// <summary>
    /// These are platform abstractions and platform specific implementations
    /// </summary>
    public static class OSInformation
    {
        private static readonly string _dollarPSHome = "$PSHome";
        private static readonly string _sudoUser = "whoami";
        private static bool? _isWindows = null;
        private static bool? _isWindowsPowerShell = null;
        private static bool? _isSudoUser = null;
        private static bool? _isFipsEnabled = null;
        private static string _allUserhomeDirectory = null;

        /// <summary>
        /// True if the current platform is Windows.
        /// </summary>
        public static bool IsWindows
        {
            get
            {
                if (_isWindows.HasValue) { return _isWindows.Value; }

#if CORECLR
                try
                {
                    _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                }
                catch
                {
                    _isWindows = false;
                }
#else
                _isWindows = true;
#endif
                return _isWindows.Value;
            }
        }

        /// <summary>
        /// true, on Windows FullCLR or Nano Server
        /// false, on PowerShellCore which can be Linux, Windows PowerShellCore.
        ///
        /// Is CORECLR will be 'true' for both PowerShellCore and Nano
        ///
        /// </summary>
        public static bool IsWindowsPowerShell
        {
            get
            {
                if (_isWindowsPowerShell.HasValue) { return _isWindowsPowerShell.Value; }

                string psHomePath = RunPowerShellCommand(_dollarPSHome);

                if (!string.IsNullOrWhiteSpace(psHomePath) &&
                    psHomePath.TrimEnd(new char[] { '\\' })
                        .EndsWith(@"\WindowsPowerShell\v1.0", StringComparison.OrdinalIgnoreCase))
                {
                    _isWindowsPowerShell = true;
                }
                else
                {
                    _isWindowsPowerShell = false;
                }
                return _isWindowsPowerShell.Value;
            }
        }

        //
        //Summary of pathes we are using
        //On Windows ---- Current user:

        //NuGet.config is under               %appdata%\NuGet\NuGet.config
        //NuGet packages installation path:   %localappdata%\PackageManagement\NuGet\packages
        //OneGet bootstrapping provider path: %localappdata%\PackageManagement\ProviderAssemblies

        //On Windows ---- AllUsers:

        //NuGet packages installation path:    %programfiles\PackageManagement\NuGet\packages
        //OneGet bootstrapping provider path:  %programfiles\PackageManagement\ProviderAssemblies

        //On Linux/Mac --- Current User:

        //NuGet.config path:                    /$home/.config/PackageManagement/NuGet/NuGet.config
        //NuGet packages install path:          /$home/.local/share/PackageManagement/NuGet/Packages
        //OneGet bootrapping provider path:     disabled on non-windows

        //On Linux/Mac - AllUsers:

        //NuGet packages install path:          /usr/local/share/PackageManagement/NuGet/Packages
        //OneGet bootstrapping provider path:   disabled on non-windows

        public static string ConfigLocation => SelectDirectory(XDG_Type.CONFIG);

        public static string DataHomeLocation => SelectDirectory(XDG_Type.DATA);

        public static string AllUserLocation => SelectDirectory(XDG_Type.ALL_USER);

        //~ alluser, e.g., programfiles
        internal static string AllUserHomeDirectory
        {
            get
            {
                if (_allUserhomeDirectory != null) { return _allUserhomeDirectory; }

                _allUserhomeDirectory = Path.Combine(OSInformation.IsWindows ? Environment.GetEnvironmentVariable("ProgramFiles") : AllUserLocation);
                _allUserhomeDirectory = _allUserhomeDirectory ?? string.Empty;
                return _allUserhomeDirectory;
            }
        }

        public static string ProgramFilesDirectory => AllUserHomeDirectory;

        //~ currentuser
        public static string LocalAppDataDirectory
        {
            get
            {
                string dataHome = Environment.GetEnvironmentVariable("localappdata");
                if (!IsWindows)
                {
                    dataHome = DataHomeLocation;
                }
                return dataHome;
            }
        }

        // Note: Watch out any path changes in /src/System.Management.Automation/CoreCLR/CorePsPlatform.cs

        /// <summary>
        /// X Desktop Group configuration type enum.
        /// </summary>
        public enum XDG_Type
        {
            /// <summary> XDG_CONFIG_HOME/powershell </summary>
            CONFIG,

            /// <summary> XDG_DATA_HOME/powershell </summary>
            DATA,

            /// <summary>/usr/local/share/</summary>
            ALL_USER
        }

        //Note: Keep in sync with /src/System.Management.Automation/CoreCLR/CorePsPlatform.cs
        internal static string SelectDirectory(XDG_Type dirpath)
        {
            string xdgconfighome = System.Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            string xdgdatahome = System.Environment.GetEnvironmentVariable("XDG_DATA_HOME");

            //this is equivalent to $env:appdata. NuGet.config is under $env:appdata\NuGet\Config on Windows
            string xdgConfigHomeDefault = Path.Combine(System.Environment.GetEnvironmentVariable("HOME"), ".config");

            //$HOME/.local/share/powershell is equivalent to MyDucument folder, i.e. "$Home\Document\WindowsPowerShell"
            string xdgDataHomeDefault = Path.Combine(System.Environment.GetEnvironmentVariable("HOME"), ".local", "share");

            switch (dirpath)
            {
                case XDG_Type.CONFIG:
                    //the user has set XDG_CONFIG_HOME corresponding to profile path
                    if (string.IsNullOrEmpty(xdgconfighome))
                    {
                        //xdg values have not been set, use the default config
                        return xdgConfigHomeDefault;
                    }
                    else
                    {
                        return Path.Combine(xdgconfighome);
                    }

                case XDG_Type.DATA:
                    //equivalent to MyDocument folder

                    if (string.IsNullOrEmpty(xdgdatahome))
                    {
                        // create the xdg folder if needed
                        if (!Directory.Exists(xdgDataHomeDefault))
                        {
                            Directory.CreateDirectory(xdgDataHomeDefault);
                        }
                        return xdgDataHomeDefault;
                    }
                    else
                    {
                        return Path.Combine(xdgdatahome);
                    }

                case XDG_Type.ALL_USER:
                    //equivalent to programfiles folder
                    // shared_modules: "/usr/local/share/powershell/Modules";
                    return "/usr/local/share";

                default:
                    return string.Empty;
            }
        }

        public static bool IsSudoUser
        {
            get
            {
                if (_isSudoUser.HasValue) { return _isSudoUser.Value; }
                if (IsWindows)
                {
                    _isSudoUser = false;
                }
                else
                {
                    //See some articles from LINUX forum: sudo users have id eqaul to 0
                    // https://ubuntuforums.org/showthread.php?t=479255
                    // http://stackoverflow.com/questions/18215973/how-to-check-if-running-as-root-in-a-bash-script
                    //
                    string sodoUserId = RunPowerShellCommand("id -u");
                    if (!string.IsNullOrWhiteSpace(sodoUserId) && sodoUserId.Equals("0", StringComparison.OrdinalIgnoreCase))
                    {
                        _isSudoUser = true;
                    }
                    else
                    {
                        //give it another try
                        string sodoUser = RunPowerShellCommand(_sudoUser);
                        if (!string.IsNullOrWhiteSpace(sodoUser) && sodoUser.Equals("root", StringComparison.OrdinalIgnoreCase))
                        {
                            _isSudoUser = true;
                        }
                        else
                        {
                            _isSudoUser = false;
                        }
                    }
                }
                return _isSudoUser.Value;
            }
        }

        private static string RunPowerShellCommand(string commandName)
        {
            InitialSessionState iis = InitialSessionState.CreateDefault2();
            using (Runspace rs = RunspaceFactory.CreateRunspace(iis))
            {
                using (PowerShell powershell = PowerShell.Create())
                {
                    rs.Open();
                    powershell.Runspace = rs;
                    powershell.AddScript(commandName);
                    PSObject retval = powershell.Invoke().FirstOrDefault();
                    if (retval != null)
                    {
                        return retval.ToString();
                    }
                }
            }
            return string.Empty;
        }

        internal static bool IsFipsEnabled
        {
            get
            {
                if (_isFipsEnabled.HasValue) { return _isFipsEnabled.Value; }

                if (OSInformation.IsWindows)
                {
                    // see here about FIPS: https://blogs.msdn.microsoft.com/shawnfa/2005/05/16/enforcing-fips-certified-cryptography/
                    // FIPS enabled,  HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa\FipsAlgorithmPolicy\Enabled is 1
                    // otherwise, it is 0.
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Lsa\FipsAlgorithmPolicy"))
                    {
                        _isFipsEnabled = (key != null) && ((int)key.GetValue("Enabled", 0) == 1);
                    }
                }
                else
                {
                    _isFipsEnabled = false;
                }

                return _isFipsEnabled.Value;
            }
        }
    }
}