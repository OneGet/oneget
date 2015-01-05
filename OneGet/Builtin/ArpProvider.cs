// 
//  Copyright (c) Microsoft Corporation. All rights reserved. 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  

namespace Microsoft.OneGet.Builtin {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.AccessControl;
    using Implementation;
    using Packaging;
    using Utility.Extensions;
    using Utility.Platform;
    using Utility.Plugin;
    using Win32;
    using IRequestObject = System.Object;

    public class ArpProvider {
        /// <summary>
        ///     The name of this Package Provider
        /// </summary>
        internal const string ProviderName = "ARP";

        private static readonly Dictionary<string, string[]> _features = new Dictionary<string, string[]>();

        /// <summary>
        ///     Returns the name of the Provider.
        /// </summary>
        /// <returns>The name of this proivder (uses the constant declared at the top of the class)</returns>
        public string GetPackageProviderName() {
            return ProviderName;
        }

        /// <summary>
        ///     Performs one-time initialization of the PROVIDER.
        /// </summary>
        /// <param name="requestObject">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void InitializeProvider(IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::InitializeProvider'", ProviderName);
                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine("Unexpected Exception thrown in '{0}::InitializeProvider' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        ///     Returns a collection of strings to the client advertizing features this provider supports.
        /// </summary>
        /// <param name="requestObject">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void GetFeatures(IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::GetFeatures' ", ProviderName);

                    foreach (var feature in _features) {
                        request.Yield(feature);
                    }
                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine("Unexpected Exception thrown in '{0}::GetFeatures' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        ///     Returns dynamic option definitions to the HOST
        /// </summary>
        /// <param name="category">The category of dynamic options that the HOST is interested in</param>
        /// <param name="requestObject">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void GetDynamicOptions(string category, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::GetDynamicOptions' '{1}'", ProviderName, category);

                    switch ((category ?? string.Empty).ToLowerInvariant()) {
                        case "install":
                            // options required for install/uninstall/getinstalledpackages
                            break;

                        case "provider":
                            // options used with this provider. Not currently used.
                            break;

                        case "source":
                            // options for package sources
                            break;

                        case "package":
                            // options used when searching for packages 
                            request.YieldDynamicOption("IncludeWindowsInstaller", OptionType.Switch.ToString(), false);
                            break;
                    }
                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine("Unexpected Exception thrown in '{0}::GetDynamicOptions' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="requestObject">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void GetInstalledPackages(string name, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::GetInstalledPackages' '{1}'", ProviderName, name);

                    // dump out results.
                    var includeWindowsInstaller = request.GetOptionValue("IncludeWindowsInstaller").IsTrue();
                    if (Environment.Is64BitOperatingSystem) {
                        using (var hklm64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ReadKey)) {
                            if (!YieldPackages("hklm64", hklm64, name, includeWindowsInstaller, request)) {
                                return;
                            }
                        }

                        using (var hkcu64 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false)) {
                            if (!YieldPackages("hkcu64", hkcu64, name, includeWindowsInstaller, request)) {
                                return;
                            }
                        }
                    }

                    using (var hklm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false)) {
                        if (!YieldPackages("hklm32", hklm32, name, includeWindowsInstaller, request)) {
                            return;
                        }
                    }

                    using (var hkcu32 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false)) {
                        if (!YieldPackages("hkcu32", hkcu32, name, includeWindowsInstaller, request)) {
                        }
                    }
                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine("Unexpected Exception thrown in '{0}::GetInstalledPackages' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        private bool YieldPackages(string hive, RegistryKey regkey, string name, bool includeWindowsInstaller, Request request) {
            if (regkey != null) {
                foreach (var key in regkey.GetSubKeyNames()) {
                    var subkey = regkey.OpenSubKey(key);
                    if (subkey != null) {
                        var properties = subkey.GetValueNames().ToDictionaryNicely(each => each.ToString(), each => (subkey.GetValue(each) ?? string.Empty).ToString(), StringComparer.OrdinalIgnoreCase);

                        if (includeWindowsInstaller || (!properties.ContainsKey("WindowsInstaller") || properties["WindowsInstaller"] != "1")) {
                            var productName = "";

                            if (!properties.TryGetValue("DisplayName", out productName)) {
                                // no product name?
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(name) || productName.IndexOf(name, StringComparison.OrdinalIgnoreCase) > -1) {
                                var productVersion = properties.Get("DisplayVersion") ?? "";
                                var publisher = properties.Get("Publisher") ?? "";
                                var uninstallString = properties.Get("QuietUninstallString") ?? properties.Get("UninstallString") ?? "";
                                var comments = properties.Get("Comments") ?? "";

                                var fp = hive + @"\" + subkey;
                                if (request.YieldSoftwareIdentity(fp, productName, productVersion, "unknown", comments, "", name, "", "")) {
                                    if (properties.Keys.Where(each => !string.IsNullOrWhiteSpace(each)).Any(k => !request.YieldSoftwareMetadata(fp, k.MakeSafeFileName(), properties[k]))) {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        private bool YieldPackage(string path, string searchKey, Dictionary<string, string> properties, Request request) {
            var productName = properties.Get("DisplayName") ?? "";
            var productVersion = properties.Get("DisplayVersion") ?? "";
            var publisher = properties.Get("Publisher") ?? "";
            var uninstallString = properties.Get("QuietUninstallString") ?? properties.Get("UninstallString") ?? "";
            var comments = properties.Get("Comments") ?? "";

            if (request.YieldSoftwareIdentity(path, productName, productVersion, "unknown", comments, "", searchKey, "", "")) {
                if (properties.Keys.Where(each => !string.IsNullOrWhiteSpace(each)).Any(k => !request.YieldSoftwareMetadata(path, k.MakeSafeFileName(), properties[k]))) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     Uninstalls a package
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="requestObject">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void UninstallPackage(string fastPackageReference, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    request.Debug("Calling '{0}::UninstallPackage' '{1}'", ProviderName, fastPackageReference);

                    // Nice-to-have put a debug message in that tells what's going on.

                    var path = fastPackageReference.Split(new[] {'\\'}, 3);
                    var uninstallCommand = string.Empty;
                    Dictionary<string, string> properties = null;
                    if (path.Length == 3) {
                        switch (path[0].ToLowerInvariant()) {
                            case "hklm64":
                                using (var product = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(path[2], RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ReadKey)) {
                                    properties = product.GetValueNames().ToDictionaryNicely(each => each.ToString(), each => (product.GetValue(each) ?? string.Empty).ToString(), StringComparer.OrdinalIgnoreCase);
                                    uninstallCommand = properties.Get("QuietUninstallString") ?? properties.Get("UninstallString") ?? "";
                                }
                                break;
                            case "hkcu64":
                                using (var product = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).OpenSubKey(path[2], RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ReadKey)) {
                                    properties = product.GetValueNames().ToDictionaryNicely(each => each.ToString(), each => (product.GetValue(each) ?? string.Empty).ToString(), StringComparer.OrdinalIgnoreCase);
                                    uninstallCommand = properties.Get("QuietUninstallString") ?? properties.Get("UninstallString") ?? "";
                                }
                                break;
                            case "hklm32":
                                using (var product = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(path[2], RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ReadKey)) {
                                    properties = product.GetValueNames().ToDictionaryNicely(each => each.ToString(), each => (product.GetValue(each) ?? string.Empty).ToString(), StringComparer.OrdinalIgnoreCase);
                                    uninstallCommand = properties.Get("QuietUninstallString") ?? properties.Get("UninstallString") ?? "";
                                }
                                break;
                            case "hkcu32":
                                using (var product = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(path[2], RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ReadKey)) {
                                    properties = product.GetValueNames().ToDictionaryNicely(each => each.ToString(), each => (product.GetValue(each) ?? string.Empty).ToString(), StringComparer.OrdinalIgnoreCase);
                                    uninstallCommand = properties.Get("QuietUninstallString") ?? properties.Get("UninstallString") ?? "";
                                }
                                break;
                        }

                        if (!string.IsNullOrWhiteSpace(uninstallCommand)) {
                            do {
                                if (File.Exists(uninstallCommand)) {
                                    ExecStandalone(request,uninstallCommand);
                                    break;
                                }

                                // not a single file.
                                // check if it's just quoted.
                                var c = uninstallCommand.Trim('\"');
                                if (File.Exists(c)) {
                                    ExecStandalone(request,c);
                                    break;
                                }

                                if (uninstallCommand.IndexOf("msiexec", StringComparison.OrdinalIgnoreCase) > -1) {
                                    MsiUninstall(request,uninstallCommand);
                                    break;
                                }

                                if (uninstallCommand.IndexOf("rundll32", StringComparison.OrdinalIgnoreCase) > -1) {
                                    RunDll32(request, uninstallCommand);
                                    break;
                                }

                                if (uninstallCommand.IndexOf("cmd.exe", StringComparison.OrdinalIgnoreCase) == 0) {
                                    CmdCommand(request, uninstallCommand);
                                    continue;
                                }

                                if (uninstallCommand.IndexOf("cmd ", StringComparison.OrdinalIgnoreCase) == 0) {
                                    CmdCommand(request, uninstallCommand);
                                    continue;
                                }

                                if (uninstallCommand[0] == '"') {
                                    var p = uninstallCommand.IndexOf('"', 1);
                                    if (p > 0) {
                                        var file = uninstallCommand.Substring(1, p - 1);
                                        var args = uninstallCommand.Substring(p + 1);
                                        if (File.Exists(file)) {
                                            CommandWithParameters(request, file, args);
                                        }
                                    }
                                } else {
                                    var p = uninstallCommand.IndexOf(' ');
                                    if (p > 0) {
                                        var file = uninstallCommand.Substring(0, p);
                                        var args = uninstallCommand.Substring(p + 1);
                                        if (File.Exists(file)) {
                                            CommandWithParameters(request, file, args);
                                            continue;
                                        }

                                        var s = 0;
                                        do {
                                            s = uninstallCommand.IndexOf(' ', s + 1);
                                            if (s == -1) {
                                                break;
                                            }
                                            file = uninstallCommand.Substring(0, s);
                                            if (File.Exists(file)) {
                                                args = uninstallCommand.Substring(s + 1);
                                                CommandWithParameters(request, file, args);
                                                break;
                                            }
                                        } while (s > -1);

                                        if (s == -1) {
                                            // never found a way to parse the command :(
                                            request.Error(ErrorCategory.InvalidOperation, properties["DisplayName"], Constants.Messages.UnableToUninstallPackage);
                                            return;
                                        }
                                    }
                                }
                            } while (false);
                            YieldPackage(fastPackageReference, fastPackageReference, properties, request);
                            return;
                        }
                        request.Error(ErrorCategory.InvalidOperation, properties["DisplayName"], Constants.Messages.UnableToUninstallPackage);
                    }
                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine("Unexpected Exception thrown in '{0}::UninstallPackage' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        private void RunDll32(Request request,string uninstallCommand) {
            
        }

        private void MsiUninstall(Request request, string uninstallCommand) {
        }

        private void CommandWithParameters(Request request, string file, string args) {
        }

        private void CmdCommand(Request request, string args) {
        }

        private void ExecStandalone(Request request, string uninstallCommand) {
            // we could examine the EXE a bit here to see if it's a NSIS installer and if it is, tack on a /S to get it to go silently.

            // uninstall via standalone EXE
            var proc = AsyncProcess.Start(new ProcessStartInfo {
                FileName = uninstallCommand,
            });

            proc.WaitForExit();
        }
    }
}