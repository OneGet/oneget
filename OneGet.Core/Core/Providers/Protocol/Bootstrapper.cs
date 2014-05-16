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

namespace Microsoft.OneGet.Core.Providers.Protocol {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Api;
    using Extensions;
    using Package;
    using Platform;
    using Service;
    using Tasks;
    using Versions;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    internal class Bootstrapper {
        private const string NuGetExe = "NuGet.exe";
        private const string NuGetDll = "NuGet.dll";
        private static string _exePath;
        private static string _dllPath;

        private static readonly FourPartVersion _minimumNuGetVersion = "2.8.1";

        // "http://www.nuget.org/api/v2/package/Nuget.CommandLine/2.8.2", // theoretical known good version

        private static string[] NuGetPackageDownloadLocations = {
            "http://www.nuget.org/api/v2/package/Nuget.CommandLine", // latest released version
            "http://www.nuget.org/api/v2/package/Nuget.CommandLine/2.8.2+alpha" // short-term known good version
        };

        private static string[] NuGetExeDownloadLocations = {
            "http://www.nuget.org/nuget.exe", // latest released version
            "http://downloads.coapp.org/files/nuget.exe" // short-term known good version
        };

        private Callback _callback;
        private ServiceApiImpl _serviceApi;

        private Bootstrapper(ServiceApiImpl serviceApi, Callback c) {
            _callback = c;
            _serviceApi = serviceApi;

            if (IsElevated) {
                EnsurePath(SystemBin, EnvironmentVariableTarget.Machine);
            }

            EnsurePath(UserBin, EnvironmentVariableTarget.User);
        }

        private static string SystemBin {
            get {
                return Path.Combine(KnownFolders.GetFolderPath(KnownFolder.CommonApplicationData), "bin");
            }
        }

        private static string UserBin {
            get {
                return Path.Combine(KnownFolders.GetFolderPath(KnownFolder.LocalApplicationData), "bin");
            }
        }

        private bool IsElevated {
            get {
                return AdminPrivilege.IsElevated;
            }
        }

        private string ExePath {
            get {
                foreach (var file in PeExecutablesInPath(NuGetExe).Where(each => each.Version > _minimumNuGetVersion).OrderByDescending(each => each.Version.ToULong())) {
                    return file.Path;
                }
                Event<Verbose>.Raise(Messages.Miscellaneous.NuGetNotFound);

                // not found -- try to bootstrap it.
                var targetPath = IsElevated ? Path.Combine(SystemBin, NuGetExe) : Path.Combine(UserBin, NuGetExe);

                // first, ask permission
                // todo:get the right text here.
                if ((bool)_callback.DynamicInvoke<AskPermission>("The NuGet Package Manager is required to continue. Can we please go get it?")) {
                    // try and get the packaged version first.
                    foreach (var location in NuGetPackageDownloadLocations) {
                        var downloadedFile = _serviceApi.DownloadFile(location, FilesystemExtensions.TempPath);

                        if (downloadedFile != null && downloadedFile.FileExists()) {
                            // unpack this file
                            try {
                                var files = _serviceApi.UnzipFileIncremental(downloadedFile, Path.GetFileName(downloadedFile).GenerateTemporaryFilename());

                                // grab the NuGet EXE from it
                                foreach (var f in files) {
                                    if (f.EndsWith(NuGetExe, StringComparison.OrdinalIgnoreCase)) {
                                        // check to see if it's new enough

                                        var ver = (FourPartVersion)FileVersionInfo.GetVersionInfo(f).ProductVersion;
                                        if (ver > _minimumNuGetVersion) {
                                            // awesome. good enough.
                                            // copy this to the target path...
                                            if (_serviceApi.CopyFile(f, targetPath)) {
                                                // we got it.
                                                return targetPath;
                                            }
                                        }
                                        // if it's not the right version, we're clearly not in the right package.
                                    }
                                }
                            } catch {
                                // meh. No good to us.
                                // unable to open the zip file. drop this one and move on.
                            }
                            // it didn't take.
                        }
                    }

                    // fallback to getting the EXE
                    foreach (var location in NuGetExeDownloadLocations) {
                        var downloadedFile = _serviceApi.DownloadFile(location, FilesystemExtensions.TempPath);

                        if (downloadedFile != null && downloadedFile.FileExists()) {
                            // check to see if it's new enough
                            var ver = (FourPartVersion)FileVersionInfo.GetVersionInfo(downloadedFile).ProductVersion;
                            if (ver > _minimumNuGetVersion) {
                                // awesome. good enough.
                                // copy this to the target path...
                                if (_serviceApi.CopyFile(downloadedFile, targetPath)) {
                                    // we got it.
                                    return targetPath;
                                }
                            }
                        }
                    }

                    // still nothing?
                }
                return null;
            }
        }

        private IEnumerable<string> PathFolders {
            get {
                var processPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process) ?? string.Empty;
                return processPath.Split(new[] {
                    Path.PathSeparator
                }, StringSplitOptions.RemoveEmptyEntries).Where(Directory.Exists);
            }
        }

        private string DllPath {
            get {
                foreach (var file in PeExecutablesInPath(NuGetDll).Where(each => each.Version > _minimumNuGetVersion).OrderByDescending(each => each.Version.ToULong())) {
                    return file.Path;
                }

                // doesn't look like we have it installed.
                var exe = ExePath;

                // see if we can bootstrap the EXE first.
                if (exe == null || !exe.FileExists()) {
                    return null;
                }

                // ok, now we can copy it to a DLL
                var tempFile = NuGetDll.GenerateTemporaryFilename();
                _serviceApi.CopyFile(exe, tempFile);

                // and patch it:
                // TODO : SUPER CHEAT! THIS NEEDS TO GET FIXED ASAP AFTER THE CTP.
                // I know that the offset for the corflags tha I need to fix is at 0x210 
                // and the byte needs to change from 0x03 to 0x01 
                // I feel so terrible for doing this, but I'm in a hurry.
                using (var stream = File.Open(tempFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
                    stream.Seek(0x210, SeekOrigin.Begin);
                    if (stream.ReadByte() == 0x03) {
                        stream.Seek(0x210, SeekOrigin.Begin);
                        stream.WriteByte(0x01);
                    }
                    stream.Close();
                }

                var targetPath = IsElevated ? Path.Combine(SystemBin, NuGetExe) : Path.Combine(UserBin, NuGetDll);

                // and copy it into place.
                if (_serviceApi.CopyFile(tempFile, targetPath)) {
                    return targetPath;
                }

                return null;
            }
        }

        public static string GetNuGetExePath(ServiceApiImpl serviceApi, Callback c) {
            return _exePath ?? (_exePath = new Bootstrapper(serviceApi, c).ExePath);
        }

        public static string GetNuGetDllPath(ServiceApiImpl serviceApi, Callback c) {
            return _dllPath ?? (_dllPath = new Bootstrapper(serviceApi, c).DllPath);
        }

        private string AddFolderToPath(string path, string folder) {
            foreach (var f in path.Split(new[] {
                Path.PathSeparator
            }, StringSplitOptions.RemoveEmptyEntries)) {
                try {
                    var p = Path.GetFullPath(f);
                    if (p.Equals(folder, StringComparison.OrdinalIgnoreCase)) {
                        return path;
                    }
                } catch {
                    // just continue.
                }
            }

            return path + Path.PathSeparator + folder;
        }

        private void EnsurePath(string location, EnvironmentVariableTarget target) {
            if (!Directory.Exists(location)) {
                Directory.CreateDirectory(location);
            }

            var path = Environment.GetEnvironmentVariable("PATH", target) ?? string.Empty;
            var newPath = AddFolderToPath(path, location);
            if (path != newPath) {
                // add it to the system path.
                path += Path.PathSeparator + location;
                Environment.SetEnvironmentVariable("PATH", newPath, target);
            }

            // add it to the current environment's path.
            var processPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process) ?? path;
            newPath = AddFolderToPath(processPath, location);
            if (processPath != newPath) {
                Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Process);
            }
        }

        private IEnumerable<FileWithVersion> PeExecutablesInPath(string PeExecutable) {
            return from folder in PathFolders
                let fullPath = Path.Combine(folder, PeExecutable)
                where fullPath.FileExists()
                select new FileWithVersion {
                    Path = fullPath,
                    Version = (FourPartVersion)FileVersionInfo.GetVersionInfo(fullPath).ProductVersion
                };
        }

        // ensure that %allusersprofile%\bin is in the System PATH
        // ensure that %appdata%\bin is in the User's PATH

        // search PATH all nuget versions > 2.8.1 (minimum required)
        // if we have one, then that's the nuget exe

        // search path for the 
        // check if NuGet.EXE is in user folder
        // check if NuGet.EXE is in system folder
    }

    internal class FileWithVersion {
        internal string Path {get; set;}
        internal FourPartVersion Version {get; set;}
    }
}