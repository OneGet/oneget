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

namespace OneGet.PowerShell.Module.Test {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Utility.Extensions;
    using Microsoft.OneGet.Utility.Platform;
    using Microsoft.OneGet.Utility.PowerShell;
    using Xunit;

    public class NugetProviderTest : TestBase {
        public static int Count = 0;
        public static string Source = "https://www.nuget.org/api/v2";

        private string TempFolder {
            get {
                string result = @"c:\tempTestDirectory_" + (Count++);
                result.TryHardToDelete();
                Directory.CreateDirectory(result);

                return result;
            }
        }

        private const string LongName =
            "THISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERS";

        private readonly string[] _workingMaximumVersions = {
            "2.0",
            "2.5",
            "3.0"
        };

        private readonly string[] _workingMinimumVersions = {
            "1.0",
            "1.3",
            "1.5"
        };

        private readonly string[] _workingNames = {
            //"BlackBirdPie",
            "AzureContrib",
            "AWSSDK"
        };

        private readonly string[] _workingSourceNames = {
            "NUGETTEST101.org",
            "NUGETTEST202.org"
        };

        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* -----------------------------------------------------------------------------     PRIMARY TESTS     ----------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */

        [Fact(Timeout = 180000), Trait("Test", "Primary")]
        public void TestWhatIfAllCmdlets() {
            dynamic ps = NewPowerShellSession;
            string testFolder = TempFolder;
            try {
                DynamicPowershellResult save = ps.SavePackage(Name: "Adept.Nugetrunner", Provider: "Nuget", DestinationPath: testFolder, WhatIf: true, Source: Source, IsTesting: true);
                save.WaitForCompletion();
                Assert.False(save.ContainsErrors);
                DynamicPowershellResult install = ps.InstallPackage(Name: "Adept.Nugetrunner", Provider: "Nuget", Destination: testFolder, WhatIf: true, Source: Source, IsTesting: true);
                install.WaitForCompletion();
                Assert.False(install.ContainsErrors);

                DynamicPowershellResult install2 = ps.InstallPackage(Name: "Adept.Nugetrunner", Provider: "Nuget", Destination: testFolder, Force: true, IsTesting: true);
                install2.WaitForCompletion();

                DynamicPowershellResult uninstall = ps.UninstallPackage(Name: "Adept.Nugetrunner", Provider: "Nuget", Destination: testFolder, WhatIf: true, IsTesting: true);
                uninstall.WaitForCompletion();
                Assert.False(uninstall.ContainsErrors);

                DynamicPowershellResult register = ps.RegisterPackageSource(Name: "nugettest55.org", Provider: "Nuget", Location: "Http://www.nuget.org/api/v2", WhatIf: true, IsTesting: true);
                register.WaitForCompletion();
                Assert.False(register.ContainsErrors);

                DynamicPowershellResult unregister = ps.UnregisterPackageSource(Name: "nuget", Provider: "Nuget", Location: "Http://www.nuget.org/api/v2", WhatIf: true, IsTesting: true);
                unregister.WaitForCompletion();
                Assert.False(unregister.ContainsErrors);
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 180000, Priority = 0, Skip = "Disabled"), Trait("Test", "Primary")]
        public void TestBootstrapNuGet() {
            // delete any copies of nuget if they are installed.
            if (IsNuGetInstalled) {
                DeleteNuGet();
            }

            // verify that nuget is not installed.
            Assert.False(IsNuGetInstalled, "NuGet is still installed at :".format(NuGetPath));

            dynamic ps = NewPowerShellSession;
            // ask onget for the nuget package provider, bootstrapping if necessary
            DynamicPowershellResult result = ps.GetPackageProvider(Name: "NuGet", ForceBootstrap: true, Source: Source, IsTesting: true);
            Assert.False(result.ContainsErrors);

            // did we get back one item?
            object[] items = result.ToArray();
            Assert.Equal(1, items.Length);

            // and is the nuget.exe where we expect it?
            Assert.True(IsNuGetInstalled);
            UnloadOneGet();
        }

        [Fact(Timeout = 120000, Priority = 1, Skip = "Disabled. SetPackageSource does not work right now."), Trait("Test", "Primary")]
        public void TestSetPackageSourceSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;

            try {
                DynamicPowershellResult result = ps.RegisterPackageSource(Name: "nugettest.org", Provider: "nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.SetPackageSource(Name: "nugettest.org", NewName: "nugettest3.org", Provider: "nuget", Location: "https://www.nuget.org/api/v2/", NewLocation: "https://www.nuget.org/api/v2/", IsTesting: true);
                result2.WaitForCompletion();
                DynamicPowershellResult result3 = ps.GetPackageSource(isTesting: true);
                result3.WaitForCompletion();
                List<dynamic> x = (from dynamic source in result3 select source.Name).ToList();
                Assert.True(x.Contains("nugettest3.org"));
            } finally {
                DynamicPowershellResult result4 = ps.UnregisterPackageSource(Name: "nugettest.org", Provider: "nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
                result4.WaitForCompletion();
                DynamicPowershellResult result5 = ps.UnregisterPackageSource(Name: "nugettest3.org", Provider: "nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
                result5.WaitForCompletion();
            }
        }

        [Fact(Timeout = 120000, Priority = 2), Trait("Test", "Primary")]
        public void TestFindPackageSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;
            int i = 0;
            foreach (string packageName in _workingNames) {
                foreach (string minVersion in _workingMinimumVersions) {
                    foreach (string maxVersion in _workingMaximumVersions) {
                        i++;
                        DynamicPowershellResult result = ps.FindPackage(Name: packageName, Provider: "Nuget", MaximumVersion: maxVersion, RequiredVersion: "1.5", version: minVersion, allVersions: true, Source: Source, IsTesting: true);
                        result.WaitForCompletion();
                        foreach (object pkg in result) {
                            var package = pkg as SoftwareIdentity;
                            if (package == null) {
                                Console.WriteLine(@"ERROR: No Package Found.");
                            } else {
                                Console.WriteLine(@"{0}: Found {1} - {2} -- {3}", i, package.Name, package.Version, package.PackageFilename);
                            }
                        }
                        Assert.False(result.ContainsErrors);
                    }
                }
            }
        }

        [Fact(Timeout = 120000, Priority = 3), Trait("Test", "Primary")]
        public void TestSavePackageSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;
            int i = 0;
            string testFolder = TempFolder;
            try {
                foreach (string packageName in _workingNames) {
                    foreach (string minVersion in _workingMinimumVersions) {
                        foreach (string maxVersion in _workingMaximumVersions) {
                            i++;
                            Console.WriteLine(@"{0}: Saving {1} {2}", i, packageName, "");
                            DynamicPowershellResult result = ps.SavePackage(Name: packageName, Provider: "Nuget", MaximumVersion: maxVersion, minimumversion: minVersion, RequiredVersion: "1.5", Destination: testFolder, Force: true, Source: Source,
                                IsTesting: true);
                            result.WaitForCompletion();

                            foreach (object pkg in result) {
                                var package = pkg as SoftwareIdentity;
                                if (package == null) {
                                    Console.WriteLine(@"ERROR: NO PACKAGE FOUND.");
                                } else {
                                    Console.WriteLine(@"{0}: Saved {1} - {2} -- {3}", i, package.Name, package.Version, package.PackageFilename);
                                }
                            }
                            Assert.False(IsDirectoryEmpty(testFolder));
                            CleanFolder(testFolder);
                        }
                    }
                }
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 120000, Priority = 4), Trait("Test", "Primary")]
        public void TestInstallPackageSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;
            int i = 0;
            string testFolder = TempFolder;
            try {
                foreach (string packageName in _workingNames) {
                    foreach (string minVersion in _workingMinimumVersions) {
                        foreach (string maxVersion in _workingMaximumVersions) {
                            i++;
                            DynamicPowershellResult result = ps.InstallPackage(Name: packageName, Provider: "Nuget", MaximumVersion: maxVersion, minimumversion: minVersion, RequiredVersion: "1.5", Destination: testFolder, Force: true, Source: Source,
                                IsTesting: true);
                            result.WaitForCompletion();
                            foreach (object pkg in result) {
                                var package = pkg as SoftwareIdentity;
                                if (package == null) {
                                    Console.WriteLine(@"ERROR: NO PACKAGE FOUND.");
                                } else {
                                    Console.WriteLine(@"{0}: Installed {1} - {2} -- {3}", i, package.Name, package.Version, package.PackageFilename);
                                }
                            }
                            Assert.False(IsDirectoryEmpty(testFolder));
                            CleanFolder(testFolder);
                        }
                    }
                }
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Priority = 5), Trait("Test", "Primary")]
        public void TestUninstallPackageSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;
            int i = 0;
            string testFolder = TempFolder;
            try {
                foreach (string packageName in _workingNames) {
                    foreach (string minVersion in _workingMinimumVersions) {
                        foreach (string maxVersion in _workingMaximumVersions) {
                            i++;
                            DynamicPowershellResult result = ps.InstallPackage(Name: packageName, Provider: "Nuget", MaximumVersion: maxVersion, minimumversion: minVersion, RequiredVersion: "1.5", Destination: testFolder, Force: true, Source: Source,
                                IsTesting: true);
                            result.WaitForCompletion();
                            foreach (object pkg in result) {
                                var package = pkg as SoftwareIdentity;
                                if (package == null) {
                                    Console.WriteLine(@"ERROR: NO PACKAGE FOUND.");
                                } else {
                                    Console.WriteLine(@"{0}: Installed {1} - {2} -- {3}", i, package.Name, package.Version, package.PackageFilename);
                                }
                            }
                            DynamicPowershellResult result2 = ps.UninstallPackage(Name: packageName, Provider: "Nuget", MaximumVersion: maxVersion, minimumversion: minVersion, RequiredVersion: "1.5", Destination: testFolder, Force: true, IsTesting: true);
                            result2.WaitForCompletion();
                            Console.WriteLine(@"{0}: Uninstalling {1}", i, packageName);
                            Assert.True(IsDirectoryEmpty(testFolder));
                        }
                    }
                }
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 120000, Priority = 6), Trait("Test", "Primary")]
        public void TestGetPackageSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;
            string testFolder = TempFolder;
            int i = 0;
            try {
                foreach (string packageName in _workingNames) {
                    foreach (string minVersion in _workingMinimumVersions) {
                        foreach (string maxVersion in _workingMaximumVersions) {
                            i++;
                            DynamicPowershellResult result = ps.InstallPackage(Name: packageName, Provider: "Nuget", MaximumVersion: maxVersion, minimumversion: minVersion, RequiredVersion: "1.5", Destination: testFolder, Force: true, Source: Source,
                                IsTesting: true);
                            result.WaitForCompletion();
                            foreach (object pkg in result) {
                                var package = pkg as SoftwareIdentity;
                                if (package == null) {
                                    Console.WriteLine(@"ERROR: NO PACKAGE FOUND.");
                                } else {
                                    Console.WriteLine(@"{0}: Installed {1} - {2} -- {3}", i, package.Name, package.Version, package.PackageFilename);
                                }
                            }
                            DynamicPowershellResult result2 = ps.GetPackage(Name: packageName, Provider: "Nuget", MaximumVersion: maxVersion, minimumversion: minVersion, RequiredVersion: "1.5", Destination: testFolder, Force: true, IsTesting: true);
                            result2.WaitForCompletion();
                            Assert.False(result2.ContainsErrors);
                            foreach (dynamic source in result2) {
                                dynamic name = source.Name;
                                dynamic provider = source.ProviderName;
                                Console.WriteLine("Getting Package: {0} - {1}", name, provider);
                            }
                            CleanFolder(testFolder);
                        }
                    }
                }
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 120000, Priority = 7), Trait("Test", "Primary")]
        public void TestRegisterPackageSourceSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;
            string testFolder = TempFolder;
            try {
                foreach (string packageName in _workingSourceNames) {
                    DynamicPowershellResult result = ps.RegisterPackageSource(Name: packageName, Provider: "Nuget", Location: "http://www.nuget.org/api/v2", IsTesting: true);
                    result.WaitForCompletion();
                    DynamicPowershellResult result2 = ps.GetPackageSource(IsTesting: true);
                    result2.WaitForCompletion();
                    List<dynamic> x = (from dynamic source in result2 select source.Name).ToList();
                    Assert.True(x.Contains(packageName));
                    DynamicPowershellResult result3 = ps.UnregisterPackageSource(Name: packageName, Provider: "Nuget", Location: "http://www.nuget.org/api/v2", IsTesting: true);
                    result3.WaitForCompletion();
                }
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 120000, Priority = 8), Trait("Test", "Primary")]
        public void TestUnregisterPackageSourceSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;
            string testFolder = TempFolder;
            try {
                foreach (string packageName in _workingSourceNames) {
                    DynamicPowershellResult result = ps.RegisterPackageSource(Name: packageName, Provider: "Nuget", Location: "http://www.nuget.org/api/v2", IsTesting: true);
                    result.WaitForCompletion();
                    DynamicPowershellResult result2 = ps.GetPackageSource(isTesting: true);
                    result2.WaitForCompletion();
                    List<dynamic> x = (from dynamic source in result2 select source.Name).ToList();
                    Assert.True(x.Contains(packageName));
                    DynamicPowershellResult result3 = ps.UnregisterPackageSource(Name: packageName, Provider: "Nuget", Location: "http://www.nuget.org/api/v2", IsTesting: true);
                    result3.WaitForCompletion();
                    DynamicPowershellResult result4 = ps.GetPackageSource(isTesting: true);
                    List<dynamic> y = (from dynamic source in result4 select source.Name).ToList();
                    Assert.False(y.Contains(packageName));
                    Console.WriteLine(@"UNREGISTERED: {0} ", packageName);
                }
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000), Trait("Test", "Primary")]
        public void TestGetPackageProvider() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.Any());
        }

        [Fact(Timeout = 120000, Priority = 9), Trait("Test", "Primary")]
        public void TestFindPackagePipeProvider() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(Name: "Nuget", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.FindPackage(result, Name: "adept.nugetrunner", IsTesting: true);
            result.WaitForCompletion();
            List<dynamic> x = (from dynamic source in result2 select source.Name).ToList();
            Assert.True(x.Contains("Adept.NuGetRunner"));
        }

        [Fact(Timeout = 60000, Priority = 19, Skip = "Disabled. Cannot Save -> Install using Nuget. "), Trait("Test", "Primary")]
        public void TestSaveThenInstallPackage() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.SavePackage(Name: "adept.nuget", Provider: "nuget", DestinationPath: testFolder, Force: true, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.InstallPackage(Name: "adept.nuget", Provider: "Nuget", Destination: testFolder, Force: true, Source: Source, IsTesting: true);
                result2.WaitForCompletion();
                Assert.False(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* ----------------------------------------------------------------------------     PIPELINE TESTS     ----------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */

        [Fact(Timeout = 60000, Priority = 18), Trait("Test", "Primary")]
        public void TestSavePackage() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.SavePackage(Name: "adept.nuget", Provider: "nuget", MinimumVersion: "1.0", MaximumVersion: "2.0", DestinationPath: testFolder, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.False(result.ContainsErrors);
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 20), Trait("Test", "Primary")]
        public void TestInstallPackage() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Name: "adept.nuget", Provider: "nuget", MinimumVersion: "1.0", MaximumVersion: "2.0", Destination: testFolder, Source: Source, Force: true, IsTesting: true);
                result.WaitForCompletion();
                Assert.False(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 21), Trait("Test", "Primary")]
        public void TestFindPackage() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Name: "adept.nugetrunner", Provider: "nuget", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            var x = (from dynamic source in result select source.Name).ToList();
            Assert.True(x.Contains("Adept.NuGetRunner"));
        }
        [Fact(Timeout = 60000, Priority = 10), Trait("Test", "Primary")]
        public void TestSavePackagePipeName() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.FindPackage(Name: "adept.Nuget", IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.SavePackage(result, DestinationPath: testFolder, IsTesting: true);
                result2.WaitForCompletion();
                Assert.False(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 600000, Priority = 11), Trait("Test", "Primary")]
        public void TestInstallPackagePipeName() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.FindPackage(Name: "adept.nugetrunner", Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.InstallPackage(result, Destination: testFolder, Force: true, IsTesting: true);
                result2.WaitForCompletion();
                Assert.False(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 12), Trait("Test", "Primary")]
        public void TestRegisterPackageSourcePipe() {
            dynamic ps = NewPowerShellSession;

            try {
                DynamicPowershellResult result = ps.GetPackageProvider(Name: "nuget", IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.RegisterPackageSource(result, Name: "nugettest.org", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
                result2.WaitForCompletion();
                DynamicPowershellResult result3 = ps.getPackageSource(isTesting: true);
                result3.WaitForCompletion();
                List<dynamic> x = (from dynamic source in result3 select source.Name).ToList();
                Assert.True(x.Contains("nugettest.org"));
            } finally {
                DynamicPowershellResult result3 = ps.UnregisterPackageSource(Name: "nugettest.org", Provider: "Nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
                result3.WaitForCompletion();
            }
        }

        [Fact(Timeout = 60000, Priority = 13), Trait("Test", "Primary")]
        public void TestUnregisterPackageSourcePipe() {
            dynamic ps = NewPowerShellSession;

            try {
                DynamicPowershellResult result = ps.GetPackageProvider(Name: "nuget", Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.RegisterPackageSource(Provider: "nuget", Name: "nugettest.org", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
                result2.WaitForCompletion();
                DynamicPowershellResult result3 = ps.getPackageSource(isTesting: true);
                result3.WaitForCompletion();
                List<dynamic> x = (from dynamic source in result3 select source.Name).ToList();
                Assert.True(x.Contains("nugettest.org"));
                DynamicPowershellResult result4 = ps.UnregisterPackageSource(result, Name: "nugettest.org", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
                result4.WaitForCompletion();
                DynamicPowershellResult result5 = ps.getPackageSource(isTesting: true);
                result5.WaitForCompletion();
                List<dynamic> y = (from dynamic source in result5 select source.Name).ToList();
                Assert.False(y.Contains("nugettest.org"));
            } finally {
                DynamicPowershellResult result6 = ps.UnregisterPackageSource(Name: "nugettest.org", Location: "https://www.nuget.org/api/v2", IsTesting: true);
                result6.WaitForCompletion();
            }
        }

        [Fact(Timeout = 60000, Priority = 14), Trait("Test", "Primary")]
        public void TestGetPackagePipeProvider() {
            dynamic ps = NewPowerShellSession;
            string testFolder = TempFolder;

            try {
                DynamicPowershellResult result = ps.InstallPackage(Name: "adept.nuget", Provider: "NuGet", Destination: testFolder, Force: true, isTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackageProvider(Name: "NuGet", IsTesting: true);
                result2.WaitForCompletion();
                DynamicPowershellResult result3 = ps.GetPackage(result, Destination: testFolder, IsTesting: true);
                result3.WaitForCompletion();
                List<dynamic> x = (from dynamic source in result3 select source.ProviderName).ToList();
                Assert.True(x.Contains("NuGet"));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 15), Trait("Test", "Primary")]
        public void TestUninstallPackagePipeName() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.FindPackage(Name: "adept.nugetrunner", Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.InstallPackage(result, Destination: testFolder, Force: true, IsTesting: true);
                result2.WaitForCompletion();
                DynamicPowershellResult result3 = ps.GetPackage(Name: "adept.nugetrunner", Provider: "nuget", Destination: testFolder, IsTesting: true);
                DynamicPowershellResult result4 = ps.UninstallPackage(result3, Force: true, IsTesting: true);
                result4.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        //[Fact(Timeout = 60000, Priority = 16), Trait("Test", "Primary")]
        public void TestSetPackageSourcePipe() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.RegisterPackageSource(Name: "nugettest.org", Location: "https://www.nuget.org/api/v2/", Provider: "nuget", Force: true, Source: Source, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.GetPackageSource(Provider: "Nuget", Name: "nugettest.org", Location: "https://www.nuget.org/api/v2", Source: Source, IsTesting: true);
            result2.WaitForCompletion();
            DynamicPowershellResult result3 = ps.SetPackageSource(result2, Provider: "nuget", NewName: "nugettest3.org", NewLocation: "https://www.nuget.org/api/v2/", Force: true, Source: Source, IsTesting: true);
            result3.WaitForCompletion();
            Assert.False(result3.ContainsErrors);
            DynamicPowershellResult result4 = ps.UnregisterPackageSource(Name: "nugettest.org", Source: Source, IsTesting: true);
            result4.WaitForCompletion();
            DynamicPowershellResult result5 = ps.UnregisterPackageSource(Name: "nugettest3.org", Source: Source, IsTesting: true);
            result5.WaitForCompletion();
        }

        [Fact(Timeout = 60000, Priority = 17), Trait("Test", "Primary")]
        public void TestGetPackageSourcePipe() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(Source: Source, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.GetPackageSource(result, Source: Source, IsTesting: true);
            result2.WaitForCompletion();
            DynamicPowershellResult result3 = ps.GetPackageSource(Source: Source, IsTesting: true);
            result3.WaitForCompletion();
            List<dynamic> x = (from dynamic source in result2 select source.Name).ToList();
            List<dynamic> y = (from dynamic source in result3 select source.Name).ToList();
            Assert.Equal(x, y);
        }

        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* ----------------------------------------------------------------------------     SCENARIO TESTS     ----------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */

        //[Fact(Timeout = 60000, Priority = 17), Trait("Test", "Primary")]

        public void TestScenarioOne() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;

            try {
                DynamicPowershellResult register = ps.RegisterPackageSource(Name: "nugettest.org", Location: "https://www.nuget.org/api/v2/", Provider: "Nuget", Force: true, Source: Source, IsTesting: true);
                register.WaitForCompletion();
                DynamicPowershellResult getP = ps.GetPackageSource(register, Force: true, Source: Source, IsTesting: true);
                getP.WaitForCompletion();
                var x = (from dynamic source in getP select source.Name).ToList();
                Assert.True(x.Contains("nugettest.org"));

                DynamicPowershellResult set = ps.SetPackageSource(Name: "Nugettest.org", Location: "https://www.nuget.org/api/v2/", Provider: "Nuget", NewName: "RenamedNugetTest.org", NewLocation: "https://www.nuget.org/api/v2", Force: true, Source: Source,
                    IsTesting: true);
                set.WaitForCompletion();
                Assert.False(set.ContainsErrors);
                foreach (dynamic source in set) {
                    Console.WriteLine("Name: " + source.Name + " Location: " + source.Location);
                }

                DynamicPowershellResult getPp = ps.GetPackageProvider(Name: "Nuget", Source: Source, IsTesting: true);
                getPp.WaitForCompletion();
                Assert.False(getPp.ContainsErrors);
                foreach (dynamic source in getPp) {
                    Console.WriteLine("GetPP Provider Name: " + source.Name);
                }
                DynamicPowershellResult find = ps.FindPackage(Name: "adept.nugetrunner", Provider: "Nuget", MaximumVersion: "1.0.0.2", Source: Source, IsTesting: true);
                find.WaitForCompletion();
                Assert.False(find.ContainsErrors);

                DynamicPowershellResult install = ps.InstallPackage(find, DestinationPath: testFolder, Force: true, Source: Source, IsTesting: true);
                install.WaitForCompletion();
                Assert.False(IsDirectoryEmpty(testFolder));

                DynamicPowershellResult get = ps.GetPackage(DestinationPath: TempFolder, Source: Source, IsTesting: true);
                get.WaitForCompletion();
                foreach (dynamic source in get) {
                    Console.WriteLine("Name: " + source.Name + " Status: " + source.Status);
                }
                Assert.False(get.ContainsErrors);

                DynamicPowershellResult uninstall = ps.UninstallPackage(Name: "Adept.Nugetrunner", DestinationPath: testFolder, Source: Source, IsTesting: true);
                uninstall.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(testFolder));

                DynamicPowershellResult unregister = ps.UnregisterPackageSource(Name: "RenamedNugetTest.org", Location: "https://www.nuget.org/api/v2", Provider: "Nuget", Source: Source, IsTesting: true);
                unregister.WaitForCompletion();
                Assert.False(unregister.ContainsErrors);
           
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* ---------------------------------------------------------------------------     SECONDARY TESTS     ----------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestFindPackageLongName() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: LongName, Source: Source, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.ContainsErrors, "Found an invalid name.");
        }

        [Fact(Timeout = 60000, Skip = "Disabled. Cannot handle null names.")]
        public void TestFindPackageNullName() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: null, Source: Source, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.ContainsErrors);
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestFindPackageInvalidName() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.ContainsErrors, "Found package with invalid name.");
        }

        [Fact(Timeout = 60000, Skip = "impossible to check, only doing nuget")]
        public void TestFindPackageInvalidProvider() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.Success, "Found package with invalid name.");
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestFindPackageNegMaxVersion() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", MaximumVersion: "-1.5", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.ContainsErrors, "Managed to find package with negative MaximumVersion paramater.");
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestFindPackageNegMinVersion() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", Version: "-1.5", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.ContainsErrors, "Managed to find package with negative MinimumVersion.");
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestFindPackageNegReqVersion() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", RequiredVersion: "-1.5", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.ContainsErrors, "Managed to find package with negative RequiredVersion.");
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestFindPackageRequiredVersionFail() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "adept.Nuget", Version: "1.0", MaximumVersion: "1.5", RequiredVersion: "2.0", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.ContainsErrors, "Managed to find package with invalid RequiredVersion parameter.");
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestFindPackageAllVersionFail() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", Version: "1.0", MaximumVersion: "1.5", AllVersion: "2.0", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.ContainsErrors, "Managed to find package with invalid AllVersion parameter.");
        }

        [Fact(Timeout = 300000), Trait("Test", "Secondary")]
        public void TestFindPackageMismatchedVersions() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", Version: "1.5", MaximumVersion: "1.0", Source: Source, IsTesting: true);
            Assert.True(result.ContainsErrors, "Managed to find package with invalid Max/Min version parameter combination.");
        }

        [Fact(Timeout = 120000, Priority = 100), Trait("Test", "Secondary")]
        public void TestSavePackageLongName() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: LongName, DestinationPath: testFolder, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(result.ContainsErrors);
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Skip = "Disabled. Cannot handle null names.")]
        public void TestSavePackageNullName() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: null, DestinationPath: testFolder, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 120000, Priority = 101), Trait("Test", "Secondary")]
        public void TestSavePackageMismatchedVersions() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", DestinationPath: testFolder, MaximumVersion: "1.0", Version: "1.0.0.2", Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 120000, Priority = 102), Trait("Test", "Secondary")]
        public void TestSavePackageNegReqVersion() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", DestinationPath: testFolder, RequiredVersion: "-1.0.0.2", Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 120000, Priority = 103), Trait("Test", "Secondary")]
        public void TestSavePackageNegMaxVersion() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", DestinationPath: testFolder, MaximumVersion: "-1.0.0.2", Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 120000, Priority = 104), Trait("Test", "Secondary")]
        public void TestSavePackageNegMinVersion() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", DestinationPath: testFolder, Version: "-1.0.0.2", Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 105), Trait("Test", "Secondary")]
        public void TestInstallPackageLongName() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: LongName, Destination: testFolder, Force: true, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Skip = "Disabled. Cannot handle null names.")]
        public void TestInstallPackageNullName() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: null, Destination: testFolder, Force: true, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 106), Trait("Test", "Secondary")]
        public void TestInstallPackageBigMinVersion() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinimumVersion: "999", Force: true, Destination: testFolder, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 107), Trait("Test", "Secondary")]
        public void TestInstallPackageNegMaxVersion() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "-1.0", Force: true, Destination: testFolder, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 108), Trait("Test", "Secondary")]
        public void TestInstallPackageNegMinVersion() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinVersion: "-1.0.0.2", Force: true, Destination: testFolder, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(result.ContainsErrors);
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 109), Trait("Test", "Secondary")]
        public void TestInstallPackageNegReqVersion() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: "-1.0", Force: true, Destination: testFolder, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Skip = "Disabled. Cannot handle null names.")]
        public void TestInstallPackageNullReqVersion() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: null, Force: true, Destination: testFolder, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 110), Trait("Test", "Secondary")]
        public void TestUninstallPackage() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "nuget", Name: "Adept.Nugetrunner", Force: true, Destination: testFolder, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Destination: testFolder, Force: true, IsTesting: true);
                result2.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 111), Trait("Test", "Secondary")]
        public void TestUninstallPackageLongName() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Destination: testFolder, Force: true, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: LongName, Destination: testFolder, Force: true, IsTesting: true);
                result2.WaitForCompletion();
                Assert.False(IsDirectoryEmpty(testFolder));
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Skip = "Disabled. Cannot handle null names.")]
        public void TestUninstallPackageNullName() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Destination: testFolder, Force: true, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: null, Destination: testFolder, Force: true, Source: Source, IsTesting: true);
                Assert.True(result2.ContainsErrors);
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 112, Skip = "Disabled. "), Trait("Test", "Secondary")]
        public void TestUninstallPackageNegMaxVersion() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Destination: testFolder, Force: true, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "-1.0", Destination: testFolder, Force: true, Source: Source, IsTesting: true);
                result2.WaitForCompletion();
                Assert.True(result2.ContainsErrors);
                ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Force: true, Source: Source, IsTesting: true);
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 113, Skip = "Disabled."), Trait("Test", "Secondary")]
        public void TestUninstallPackageNegMinVersion() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinimumVersion: "1.0", Destination: testFolder, Force: true, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinimumVersion: "-1.0", Destination: testFolder, Force: true, Source: Source, IsTesting: true);
                result2.WaitForCompletion();
                Assert.True(result2.ContainsErrors);
                ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Force: true, Source: Source, IsTesting: true);
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 114, Skip = "Disabled."), Trait("Test", "Secondary")]
        public void TestUninstallPackageNegReqVersion() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: "1.0", Destination: testFolder, Force: true, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: "-1.0", Destination: testFolder, Force: true, Source: Source, IsTesting: true);
                result2.WaitForCompletion();
                Assert.True(result2.ContainsErrors);
                ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Force: true, Source: Source, IsTesting: true);
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Priority = 115), Trait("Test", "Secondary")]
        public void TestGetPackage() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "nuget", Name: "Adept.Nugetrunner", Force: true, Destination: testFolder, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Force: true, Destination: testFolder, IsTesting: true);
                result2.WaitForCompletion();
                List<dynamic> x = (from dynamic source in result2 select source.Name).ToList();
                Assert.True(x.ToArray().Length > 0);
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestGetPackageLongName() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "nuget", Name: "Adept.Nugetrunner", Force: true, Destination: testFolder, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackage(Provider: "Nuget", Name: LongName, Force: true, Destination: testFolder, IsTesting: true);
                result2.WaitForCompletion();
                Assert.True(result2.ContainsErrors);
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000, Skip = "Disabled. Cannot handle null names.")]
        public void TestGetPackageNullName() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "nuget", Name: "Adept.Nugetrunner", Force: true, Destination: testFolder, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackage(Provider: "Nuget", Name: null, Force: true, Destination: testFolder, Source: Source, IsTesting: true);
                result2.WaitForCompletion();
                Assert.True(result2.ContainsErrors);
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestGetPackageNegMaxVersion() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "nuget", Name: "Adept.Nugetrunner", Force: true, Destination: testFolder, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackage(Provider: "Nuget", Name: "adept.nugetrunner", MaximumVersion: "-1.0", Force: true, Destination: testFolder, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(result2.ContainsErrors);
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestGetPackageNegMinVersion() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "nuget", Name: "Adept.Nugetrunner", Force: true, Destination: testFolder, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackage(Provider: "Nuget", Name: "adept.nugetrunner", Version: "-1.0", Force: true, Destination: testFolder, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(result2.ContainsErrors);
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestGetPackageNegReqVersion() {
            dynamic ps = NewPowerShellSession;

            string testFolder = TempFolder;
            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "nuget", Name: "Adept.Nugetrunner", Force: true, Destination: testFolder, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackage(Provider: "Nuget", Name: "adept.nugetrunner", RequiredVersion: "-1.0", Force: true, Destination: testFolder, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(result2.ContainsErrors);
            } finally {
                testFolder.TryHardToDelete();
            }
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestGetPackageProviderName() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(Name: "NuGet", ForceBootstrap: true, Force: true, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.ContainsErrors, "Failed to get package provider.");

            object[] items = result.ToArray();
            Assert.Equal(1, items.Length);
        }

        [Fact(Timeout = 60000, Priority = 116), Trait("Test", "Secondary")]
        public void TestRegisterPackageSource() {
            dynamic ps = NewPowerShellSession;
            try {
                DynamicPowershellResult result = ps.RegisterPackageSource(Name: "nugettest.org", Provider: "nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackageSource(isTesting: true);
                List<dynamic> x = (from dynamic source in result2 select source.Name).ToList();
                Assert.True(x.Contains("nugettest.org"));
            } finally {
                DynamicPowershellResult result3 = ps.UnregisterPackageSource(Name: "nugettest.org", Provider: "Nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
                result3.WaitForCompletion();
            }
        }

        [Fact(Timeout = 60000, Priority = 117), Trait("Test", "Secondary")]
        public void TestUnregisterPackageSource() {
            dynamic ps = NewPowerShellSession;
            try {
                DynamicPowershellResult result = ps.RegisterPackageSource(Name: "nugettest.org", Provider: "nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackageSource(isTesting: true);
                List<dynamic> x = (from dynamic source in result2 select source.Name).ToList();
                Assert.True(x.Contains("nugettest.org"));
                DynamicPowershellResult result3 = ps.UnregisterPackageSource(Name: "nugettest.org", Provider: "Nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
                result3.WaitForCompletion();
                List<dynamic> y = (from dynamic source in result3 select source.Name).ToList();
                Assert.False(y.Contains("nugettest.org"));
            } finally {
                DynamicPowershellResult result4 = ps.UnregisterPackageSource(Name: "nugettest.org", Provider: "Nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
                result4.WaitForCompletion();
            }
        }

        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* ------------------------------------------------------------------------------     PERFORMANCE TESTS     ------------------------------------------------------------------------------ */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */

        [Fact(Timeout = 60000), Trait("Test", "Performance")]
        public void TestPerformanceFindPackageSelectObjectPipe() {
            dynamic ps = NewPowerShellSession;

            var watch = new Stopwatch();
            DynamicPowershellResult warmup = ps.FindPackage(ProviderName: "Nuget", Source: "https://msconfiggallery.cloudapp.net/api/v2/", IsTesting: true);
            warmup.WaitForCompletion();
            watch.Start();
            DynamicPowershellResult result = ps.FindPackage(ProviderName: "Nuget", Source: "https://msconfiggallery.cloudapp.net/api/v2/", IsTesting: true);
            // ReSharper disable once UnusedVariable
            object first = result.FirstOrDefault();
            watch.Stop();

            Console.WriteLine(@"Time elapsed: {0}", watch.Elapsed);
        }

        [Fact(Timeout = 60000), Trait("Test", "Performance")]
        public void TestPerformanceFindPackageAppDomainConfig() {
            dynamic ps = NewPowerShellSession;

            var watch = new Stopwatch();
            DynamicPowershellResult warmup = ps.FindPackage(Provider: "Nuget", Source: "https://msconfiggallery.cloudapp.net/api/v2/", Name: "AppDomainConfig", IsTesting: true);
            warmup.WaitForCompletion();
            watch.Start();
            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Source: "https://msconfiggallery.cloudapp.net/api/v2/", Name: "AppDomainConfig", IsTesting: true);
            result.WaitForCompletion();
            watch.Stop();
            Console.WriteLine(@"Time elapsed: {0}", watch.Elapsed);
            Assert.False(result.ContainsErrors);
        }

       

        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* ---------------------------------------------------------------------------     HELPER FUNCTIONS     ---------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */

        private void CleanFolder(String path) {
            foreach (string file in Directory.GetFiles(path)) {
                file.TryHardToDelete();
            }
            foreach (string directory in Directory.GetDirectories(path)) {
                directory.TryHardToDelete();
            }
        }

        private bool IsDirectoryEmpty(string path) {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        private bool IsDllOrExe(string path) {
            return path.ToLower().EndsWith(".exe") || path.ToLower().EndsWith(".dll");
        }

        private IEnumerable<string> FilenameContains(IEnumerable<string> paths, string value) {
            return paths.Where(item => item.IndexOf(value, StringComparison.OrdinalIgnoreCase) > -1);
        }

        private IEnumerable<string> NugetsInPath(string folder) {
            if (Directory.Exists(folder)) {
                string[] files = Directory.EnumerateFiles(folder).ToArray();

                return FilenameContains(files, "nuget").Where(IsDllOrExe);
            }
            return Enumerable.Empty<string>();
        }

        private void DeleteNuGet() {
            string systemBase = KnownFolders.GetFolderPath(KnownFolder.CommonApplicationData);
            Assert.False(string.IsNullOrEmpty(systemBase));

            IEnumerable<string> nugets = NugetsInPath(Path.Combine(systemBase, "oneget", "ProviderAssemblies"));
            foreach (string nuget in nugets) {
                nuget.TryHardToDelete();
            }

            string userBase = KnownFolders.GetFolderPath(KnownFolder.LocalApplicationData);
            Assert.False(string.IsNullOrEmpty(userBase));

            nugets = NugetsInPath(Path.Combine(userBase, "oneget", "ProviderAssemblies"));
            foreach (string nuget in nugets) {
                nuget.TryHardToDelete();
            }
        }

        private string NuGetPath {
            get {
                string systemBase = KnownFolders.GetFolderPath(KnownFolder.CommonApplicationData);
                Assert.False(string.IsNullOrEmpty(systemBase), "Known Folder CommonApplicationData is null");

                IEnumerable<string> nugets = NugetsInPath(Path.Combine(systemBase, "oneget", "ProviderAssemblies"));
                string first = nugets.FirstOrDefault();
                if (first != null) {
                    return first;
                }

                string userBase = KnownFolders.GetFolderPath(KnownFolder.LocalApplicationData);
                Assert.False(string.IsNullOrEmpty(userBase), "Known folder LocalApplicationData is null");

                nugets = NugetsInPath(Path.Combine(userBase, "oneget", "ProviderAssemblies"));
                first = nugets.FirstOrDefault();
                if (first != null) {
                    return first;
                }

                return null;
            }
        }

        private bool IsNuGetInstalled {
            get {
                return NuGetPath != null;
            }
        }
    }
}
