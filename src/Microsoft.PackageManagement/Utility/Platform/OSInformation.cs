
using Microsoft.Win32;

namespace Microsoft.PackageManagement.Internal.Utility.Platform
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;

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

                var psHomePath = RunPowerShellCommand(_dollarPSHome);

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
                    var sodoUserId = RunPowerShellCommand("id -u");
                    if (!string.IsNullOrWhiteSpace(sodoUserId) && sodoUserId.Equals("0", StringComparison.OrdinalIgnoreCase))
                    {
                        _isSudoUser = true;
                    }
                    else
                    {
                        //give it another try
                        var sodoUser = RunPowerShellCommand(_sudoUser);
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
            var iis = InitialSessionState.CreateDefault2();
            using (Runspace rs = RunspaceFactory.CreateRunspace(iis))
            {
                using (PowerShell powershell = PowerShell.Create())
                {
                    rs.Open();
                    powershell.Runspace = rs;
                    powershell.AddScript(commandName);
                    var retval = powershell.Invoke().FirstOrDefault();
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
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Lsa\FipsAlgorithmPolicy"))
                    {
                        _isFipsEnabled = (key != null) && ((int) key.GetValue("Enabled", 0) == 1);
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
