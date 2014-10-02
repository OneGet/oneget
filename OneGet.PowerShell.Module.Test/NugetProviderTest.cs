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
    using System.IO;
    using System.Linq;
    using Microsoft.OneGet.Utility.Extensions;
    using Microsoft.OneGet.Utility.Platform;
    using Microsoft.OneGet.Utility.PowerShell;
    using Xunit;
    using System.Diagnostics;
    public class NugetProviderTest : TestBase {
        private const string TempFolder = @"C:\\tempTestDirectoryZXY";

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

        //[Fact(Timeout = 180000)] //TODO Save --> Install not functional
        public void TestWhatIfAllCmdlets() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult save = ps.SavePackage(Name: "Adept.Nugetrunner", Provider: "Nuget", Destination: TempFolder, WhatIf: true, IsTesting: true);
            save.WaitForCompletion();
           // Assert.False(save.IsFailing);
            DynamicPowershellResult install = ps.InstallPackage(Name: "Adept.Nugetrunner", Provider: "Nuget", Destination: TempFolder, WhatIf: true, IsTesting: true);
            install.WaitForCompletion();
            Assert.False(install.IsFailing);

            DynamicPowershellResult install2 = ps.InstallPackage(Name: "Adept.Nugetrunner", Provider: "Nuget", Destination: TempFolder, Force: true, IsTesting: true);
            install2.WaitForCompletion();

            DynamicPowershellResult uninstall = ps.UninstallPackage(Name: "Adept.Nugetrunner", Provider: "Nuget", Destination: TempFolder, WhatIf: true, IsTesting: true);
            uninstall.WaitForCompletion();
            Assert.False(uninstall.IsFailing);

            DynamicPowershellResult register = ps.RegisterPackageSource(Name: "nugettest55.org", Provider: "Nuget", Location: "Http://www.nuget.org/api/v2", WhatIf: true, IsTesting: true);
            register.WaitForCompletion();
            Assert.False(register.IsFailing);

            DynamicPowershellResult unregister = ps.UnregisterPackageSource(Name: "nugettest55.org", Provider: "Nuget", Location: "Http://www.nuget.org/api/v2", WhatIf: true, IsTesting: true);
            unregister.WaitForCompletion();
            Assert.False(unregister.IsFailing);
        }

        [Fact(Timeout = 180000, Priority = 0), Trait("Test", "Primary")]
        public void TestBootstrapNuGet()
        {
            // delete any copies of nuget if they are installed.
            if (IsNuGetInstalled)
            {
                DeleteNuGet();
            }

            // verify that nuget is not installed.
            Assert.False(IsNuGetInstalled, "NuGet is still installed at :".format(NuGetPath));

            dynamic ps = NewPowerShellSession;
            // ask onget for the nuget package provider, bootstrapping if necessary
            DynamicPowershellResult result = ps.GetPackageProvider(Name: "NuGet", ForceBootstrap: true, IsTesting: true);
            Assert.False(result.IsFailing);

            // did we get back one item?
            object[] items = result.ToArray();
            Assert.Equal(1, items.Length);

            // and is the nuget.exe where we expect it?
            Assert.True(IsNuGetInstalled);
            UnloadOneGet();
        }

        [Fact(Timeout = 120000, Priority = 1), Trait("Test", "Primary")]
        public void TestSetPackageSourceSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.RegisterPackageSource(Name: "nugettest.org", Provider: "nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.SetPackageSource(Name: "nugettest.org", NewName: "nugettest3.org", Provider: "nuget", Location: "https://www.nuget.org/api/v2/", NewLocation: "https://www.nuget.org/api/v2/", IsTesting: true);
            result2.WaitForCompletion();
            DynamicPowershellResult result3 = ps.GetPackageSource();
            result3.WaitForCompletion();
            foreach (dynamic source in result3) {
                dynamic name = source.Name;
                dynamic provider = source.Provider;
                dynamic location = source.Location;
                Console.WriteLine("Name: " + name + "            Provider: " + provider + "            Location: " + location);
                Assert.False(result2.IsFailing);
            }
            DynamicPowershellResult result4 = ps.UnregisterPackageSource(Name: "Nugettest.org", Provider: "Nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result4.WaitForCompletion();
        }

        [Fact(Timeout = 120000, Priority = 2), Trait("Test", "Primary")]
        public void TestFindPackageSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;
            int i = 1;
            foreach (string packageName in _workingNames) {
                foreach (string minVersion in _workingMinimumVersions) {
                    foreach (string maxVersion in _workingMaximumVersions) {
                        DynamicPowershellResult result = ps.FindPackage(Name: packageName, Provider: "Nuget", MaximumVersion: maxVersion, RequiredVersion: "1.5", version: minVersion, allVersions: true, isTesting: true);
                        result.WaitForCompletion();
                        Console.WriteLine(i++ + @": " + @" PackageName: " + packageName + @" MinVersion: " + minVersion + @" MaxVersion: " + maxVersion + @" RequiredVersion: 1.5" + @" AllVersions: true");
                        Assert.False(result.IsFailing);
                    }
                }
            }
        }

        [Fact(Timeout = 120000, Priority = 3), Trait("Test", "Primary")]
        public void TestSavePackageSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;
            int i = 1;
            Directory.CreateDirectory(TempFolder);
            foreach (string packageName in _workingNames) {
                foreach (string minVersion in _workingMinimumVersions) {
                    foreach (string maxVersion in _workingMaximumVersions) {
                        DynamicPowershellResult result = ps.SavePackage(Name: packageName, Provider: "Nuget", MaximumVersion: maxVersion, RequiredVersion: "1.5", minimumversion: minVersion, Destination: TempFolder, isTesting: true);
                        result.WaitForCompletion();
                        Console.WriteLine(i++ + @": " + @" PackageName: " + packageName + @" MinVersion: " + minVersion + @" MaxVersion: " + maxVersion + @" RequiredVersion: 1.5" + @" AllVersions: true");
                        Assert.False(Directory.GetFiles(TempFolder).Length == 0);
                        DeleteAllNuget();
                    }
                }
            }
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 120000, Priority = 4), Trait("Test", "Primary")]
        public void TestInstallPackageSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;
            int i = 1;
            Directory.CreateDirectory(TempFolder);
            foreach (string packageName in _workingNames) {
                foreach (string minVersion in _workingMinimumVersions) {
                    foreach (string maxVersion in _workingMaximumVersions) {
                        DynamicPowershellResult result = ps.InstallPackage(Name: packageName, Provider: "Nuget", MaximumVersion: maxVersion, minimumversion: minVersion, RequiredVersion: "1.5", Destination: TempFolder, Force: true, isTesting: true);
                        result.WaitForCompletion();
                        Console.WriteLine(i++ + @": " + @" PackageName: " + packageName + @" MinVersion: " + minVersion + @" MaxVersion: " + maxVersion + @" RequiredVersion: 1.5" + @" AllVersions: true");
                        Assert.False(IsDirectoryEmpty(TempFolder));
                        DeleteAllNuget();
                    }
                }
            }
            Directory.Delete(TempFolder, true);
        }

        [Fact(Priority = 5), Trait("Test", "Primary")]
        public void TestUninstallPackageSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;
            int i = 1;
            Directory.CreateDirectory(TempFolder);
            foreach (string packageName in _workingNames) {
                foreach (string minVersion in _workingMinimumVersions) {
                    foreach (string maxVersion in _workingMaximumVersions) {
                        DynamicPowershellResult result = ps.InstallPackage(Name: packageName, Provider: "Nuget", MaximumVersion: maxVersion, minimumversion: minVersion, RequiredVersion: "1.5", Destination: TempFolder, Force: true, isTesting: true);
                        result.WaitForCompletion();
                        Console.WriteLine(i++ + @": " + @" PackageName: " + packageName + @" MinVersion: " + minVersion + @" MaxVersion: " + maxVersion + @" RequiredVersion: 1.5" + @" AllVersions: true");
                        DynamicPowershellResult result2 = ps.UninstallPackage(Name: packageName, Provider: "Nuget", MaximumVersion: maxVersion, minimumversion: minVersion, RequiredVersion: "1.5", Destination: TempFolder, Force: true, isTesting: true);
                        result2.WaitForCompletion();
                        Assert.True(IsDirectoryEmpty(TempFolder));
                    }
                }
            }
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 120000, Priority = 6), Trait("Test", "Primary")]
        public void TestGetPackageSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;
            Directory.CreateDirectory(TempFolder);
            foreach (string packageName in _workingNames) {
                foreach (string minVersion in _workingMinimumVersions) {
                    foreach (string maxVersion in _workingMaximumVersions) {
                        DynamicPowershellResult result = ps.InstallPackage(Name: packageName, Provider: "Nuget", MaximumVersion: maxVersion, minimumversion: minVersion, RequiredVersion: "1.5", Destination: TempFolder, Force: true, isTesting: true);
                        result.WaitForCompletion();
                        DynamicPowershellResult result2 = ps.GetPackage(Name: packageName, Provider: "Nuget", MaximumVersion: maxVersion, minimumversion: minVersion, RequiredVersion: "1.5", Destination: TempFolder, Force: true, isTesting: true);
                        result2.WaitForCompletion();
                        Assert.False(result2.IsFailing);
                        foreach (dynamic source in result2) {
                            dynamic name = source.Name;
                            dynamic provider = source.ProviderName;
                            Console.WriteLine("Name: " + name + " Provider: " + provider);
                        }
                    }
                }
            }
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 120000, Priority = 7), Trait("Test", "Primary")]
        public void TestRegisterPackageSourceSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;
            Directory.CreateDirectory(TempFolder);
            foreach (string packageName in _workingSourceNames) {
                DynamicPowershellResult result = ps.RegisterPackageSource(Name: packageName, Provider: "Nuget", Location: "http://www.nuget.org/api/v2", isTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackageSource();
                result2.WaitForCompletion();
                Assert.False(result.IsFailing);
                foreach (dynamic source in result2) {
                    dynamic name = source.Name;
                    dynamic provider = source.ProviderName;
                    Console.WriteLine("Name: " + name + " Provider: " + provider);
                }
                DynamicPowershellResult result3 = ps.UnregisterPackageSource(Name: packageName, Provider: "Nuget", Location: "http://www.nuget.org/api/v2", isTesting: true);
                result3.WaitForCompletion();
            }
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 120000, Priority = 8), Trait("Test", "Primary")]
        public void TestUnregisterPackageSourceSuccessfulCombinations() {
            dynamic ps = NewPowerShellSession;
            Directory.CreateDirectory(TempFolder);
            foreach (string packageName in _workingSourceNames) {
                DynamicPowershellResult result = ps.RegisterPackageSource(Name: packageName, Provider: "Nuget", Location: "http://www.nuget.org/api/v2", isTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackageSource();
                result2.WaitForCompletion();
                foreach (dynamic source in result2) {
                    dynamic name = source.Name;
                    dynamic provider = source.ProviderName;
                    Console.WriteLine("Name: " + name + " Provider: " + provider);
                }
                DynamicPowershellResult result3 = ps.UnregisterPackageSource(Name: packageName, Provider: "Nuget", Location: "http://www.nuget.org/api/v2", isTesting: true);
                result3.WaitForCompletion();
                Assert.False(result3.IsFailing);
            }
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000), Trait("Test", "Primary")]
        public void TestGetPackageProvider() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing, "Failed to get package provider.");
        }

        [Fact(Timeout = 120000, Priority = 9), Trait("Test", "Primary")]
        public void TestFindPackagePipeProvider() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(Name: "Nuget", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.FindPackage(result, Name: "adept.nugetrunner", IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result2.IsFailing, "Failed to pipe provider to Find Package.");
        }

        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* ----------------------------------------------------------------------------     PIPELINE TESTS     ----------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */

        //[Fact(Priority = 10)]
        public void TestSavePackagePipeName()
        {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.FindPackage(Name: "adept.Nuget", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.SavePackage(result, Provider: "Nuget", Destination: TempFolder, IsTesting: true);
            result2.WaitForCompletion();
            Assert.True(File.Exists("C:\\tempTestDirectoryZXY\\Adept.NuGetRunner.1.0.0.2.nupkg"), "Save failed, package not found.");
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Priority = 11), Trait("Test", "Primary")]
        public void TestInstallPackagePipeName()
        {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.FindPackage(Name: "adept.nugetrunner", Provider: "Nuget", MaximumVersion: "1.5", MinimumVersion: "1.0.0.1", RequiredVersion: "1.0.0.2", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.InstallPackage(result, Destination: TempFolder, Force: true, IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(IsDirectoryEmpty(TempFolder));

            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Priority = 12), Trait("Test", "Primary")]
        public void TestRegisterPackageSourcePipe()
        {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(Name: "nuget", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.RegisterPackageSource(result, Name: "nugettest.org", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing);
            DynamicPowershellResult result3 = ps.UnregisterPackageSource(Name: "nugettest.org", Provider: "Nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result3.WaitForCompletion();
        }

        [Fact(Timeout = 60000, Priority = 13), Trait("Test", "Primary")]
        public void TestUnregisterPackageSourcePipe()
        {
            dynamic ps = NewPowerShellSession;
            DynamicPowershellResult result = ps.GetPackageProvider(Name: "nuget", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.RegisterPackageSource(Provider: "Nuget", Name: "nugettest.org", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result2.WaitForCompletion();
            DynamicPowershellResult result3 = ps.UnregisterPackageSource(result, Name: "Nugettest.org", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            Assert.False(result3.IsFailing);
        }

        [Fact(Timeout = 60000, Priority = 14), Trait("Test", "Primary")]
        public void TestGetPackagePipeProvider() {
            dynamic ps = NewPowerShellSession;
            DynamicPowershellResult result = ps.GetPackageProvider(Name: "Nuget", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.GetPackage(result);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing);
            foreach (dynamic source in result2) {
                var x = source.Name;
                var y = source.Version;
                var z = source.Status;
                Console.WriteLine(x + y + z);
            }
        }

        [Fact(Timeout = 60000, Priority = 15), Trait("Test", "Primary")]
        public void TestUninstallPackagePipeName()
        {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.FindPackage(Name: "adept.nugetrunner", Provider: "Nuget", MaximumVersion: "1.5", MinimumVersion: "1.0.0.1", RequiredVersion: "1.0.0.2", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.InstallPackage(result, Destination: TempFolder, Force: true, IsTesting: true);
            result2.WaitForCompletion();
            DynamicPowershellResult result3 = ps.GetPackage(Name: "adept.nugetrunner", Provider: "nuget", Destination: TempFolder, Force: true, IsTesting: true);
            DynamicPowershellResult result4 = ps.UninstallPackage(result3, Force: true, IsTesting: true);
            result4.WaitForCompletion();
            Assert.True(IsDirectoryEmpty(TempFolder));

            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Priority = 16), Trait("Test", "Primary")]
        public void TestSetPackageSourcePipe() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.RegisterPackageSource(Name: "nugettest.org", Location: "https://www.nuget.org/api/v2/", Provider: "nuget", Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.GetPackageSource(Provider: "Nuget", Name: "nugettest.org", Location: "https://www.nuget.org/api/v2", IsTesting: true);
            result2.WaitForCompletion();
            DynamicPowershellResult result3 = ps.SetPackageSource(result2, NewName: "nugettest3.org", NewLocation: "https://www.nuget.org/api/v2/", IsTesting: true);
            result3.WaitForCompletion();
            Assert.False(result3.IsFailing);
            DynamicPowershellResult result4 = ps.UnregisterPackageSource(Name: "nugettest.org", IsTesting: true);
            result4.WaitForCompletion();
            DynamicPowershellResult result5 = ps.UnregisterPackageSource(Name: "nugettest3.org", IsTesting: true);
            result5.WaitForCompletion();
        }

        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* ----------------------------------------------------------------------------     SCENARIO TESTS     ----------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */

        [Fact(Timeout = 60000, Priority = 17), Trait("Test", "Primary")]

        public void TestScenarioOne() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);

            DynamicPowershellResult register = ps.RegisterPackageSource(Name: "nugettest.org", Location: "https://www.nuget.org/api/v2/", Provider: "Nuget", Force: true, IsTesting: true);
            register.WaitForCompletion();
            DynamicPowershellResult getP = ps.GetPackageSource(register, Force: true, IsTesting: true);
            getP.WaitForCompletion();
            Assert.False(getP.IsFailing);
            foreach (dynamic source in getP) {
                Console.WriteLine("Name: " + source.Name + " Location: " + source.Location);
            }
            DynamicPowershellResult set = ps.SetPackageSource(Name: "Nugettest.org", Location: "https://www.nuget.org/api/v2/", Provider: "Nuget", NewName: "RenamedNugetTest.org", NewLocation: "https://www.nuget.org/api/v2", Force: true, IsTesting: true);
            set.WaitForCompletion();
            Assert.False(set.IsFailing);
            foreach (dynamic source in set) {
                Console.WriteLine("Name: " + source.Name + " Location: " + source.Location);
            }

            DynamicPowershellResult getPp = ps.GetPackageProvider(Name: "Nuget", IsTesting: true);
            getPp.WaitForCompletion();
            Assert.False(getPp.IsFailing);
            foreach (dynamic source in getPp) {
                Console.WriteLine("GetPP Provider Name: " + source.Name);
            }
            DynamicPowershellResult find = ps.FindPackage(Name: "adept.nugetrunner", Provider: "Nuget", MaximumVersion: "1.0.0.2", IsTesting: true);
            find.WaitForCompletion();
            Assert.False(find.IsFailing);

            DynamicPowershellResult install = ps.InstallPackage(find, Destination: TempFolder, Force: true, IsTesting: true);
            install.WaitForCompletion();
            Assert.False(IsDirectoryEmpty(TempFolder));

            DynamicPowershellResult get = ps.GetPackage(Destination: TempFolder, IsTesting: true);
            get.WaitForCompletion();
            foreach (dynamic source in get) {
                Console.WriteLine("Name: " + source.Name + " Status: " + source.Status);
            }
            Assert.False(get.IsFailing);

            DynamicPowershellResult uninstall = ps.UninstallPackage(Name: "Adept.Nugetrunner", Destination: TempFolder, IsTesting: true);
            uninstall.WaitForCompletion();
            Assert.True(IsDirectoryEmpty(TempFolder));

            DynamicPowershellResult unregister = ps.UnregisterPackageSource(Name: "RenamedNugetTest.org", Location: "https://www.nuget.org/api/v2", Provider: "Nuget", IsTesting: true);
            unregister.WaitForCompletion();
            Assert.False(unregister.IsFailing);

            Directory.Delete(TempFolder);
        }



        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* ---------------------------------------------------------------------------     SECONDARY TESTS     ----------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */


        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestFindPackageLongName() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: LongName, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Found an invalid name.");
        }

        [Fact(Timeout = 60000, Skip = "Cannot handle null names yet.")]
        public void TestFindPackageNullName() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: null, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing);
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestFindPackageInvalidName() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Found package with invalid name.");
        }

        [Fact(Timeout = 60000, Skip = "impossible to check, only doing nuget")]
        public void TestFindPackageInvalidProvider() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.Success, "Found package with invalid name.");
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestFindPackageNegMaxVersion() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", MaximumVersion: "-1.5", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to find package with negative MaximumVersion paramater.");
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestFindPackageNegMinVersion() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", Version: "-1.5", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to find package with negative MinimumVersion.");
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestFindPackageNegReqVersion() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", RequiredVersion: "-1.5", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to find package with negative RequiredVersion.");
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestFindPackageRequiredVersionFail() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "adept.Nuget", Version: "1.0", MaximumVersion: "1.5", RequiredVersion: "2.0", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to find package with invalid RequiredVersion parameter.");
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestFindPackageAllVersionFail() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", Version: "1.0", MaximumVersion: "1.5", AllVersion: "2.0", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to find package with invalid AllVersion parameter.");
        }

        [Fact(Timeout = 300000), Trait("Test", "Secondary")]
        public void TestFindPackageMismatchedVersions() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", Version: "1.5", MaximumVersion: "1.0", IsTesting: true);
            Assert.True(result.IsFailing, "Managed to find package with invalid Max/Min version parameter combination.");
        }

        [Fact(Timeout = 120000, Priority = 100), Trait("Test", "Secondary")]
        public void TestSavePackageLongName() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: LongName, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Skip = "Cannot handle null names yet.")]
        public void TestSavePackageNullName() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: null, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 120000, Priority = 101), Trait("Test", "Secondary")]
        public void TestSavePackageMismatchedVersions() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", Destination: TempFolder, MaximumVersion: "1.0", Version: "1.0.0.2", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to save package with invalid Min/Max Version combination.");
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 120000, Priority = 102), Trait("Test", "Secondary")]
        public void TestSavePackageNegReqVersion() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", Destination: TempFolder, RequiredVersion: "-1.0.0.2", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to save package with invalid RequiredVersion parameter.");
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 120000, Priority = 103), Trait("Test", "Secondary")]
        public void TestSavePackageNegMaxVersion() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", Destination: TempFolder, MaximumVersion: "-1.0.0.2", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to save package with invalid MaximumVersion parameter.");
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 120000, Priority = 104), Trait("Test", "Secondary")]
        public void TestSavePackageNegMinVersion() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", Destination: TempFolder, Version: "-1.0.0.2", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to save package with invalid MinimumVersion parameter.");
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Skip = "Unsure how to do.")]
        public void TestSavePackageInvalidDestination() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Destination: "c:\\failLocation", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);
        }

        [Fact(Timeout = 60000, Priority = 105), Trait("Test", "Secondary")]
        public void TestInstallPackageLongName() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: LongName, Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Skip = "Cannot handle null names yet.")]
        public void TestInstallPackageNullName() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: null, Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Priority = 106), Trait("Test", "Secondary")]
        public void TestInstallPackageBigMinVersion() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinimumVersion: "999", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Priority = 107), Trait("Test", "Secondary")]
        public void TestInstallPackageNegMaxVersion() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "-1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Priority = 108), Trait("Test", "Secondary")]
        public void TestInstallPackageNegMinVersion() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinVersion: "-1.0.0.2", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Priority = 109), Trait("Test", "Secondary")]
        public void TestInstallPackageNegReqVersion() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: "-1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Skip = "Cannot handle null names yet.")]
        public void TestInstallPackageNullReqVersion() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: null, Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.Success);

            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Priority = 110), Trait("Test", "Secondary")]
        public void TestUninstallPackage() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Destination: TempFolder, Force: true, IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Priority = 111), Trait("Test", "Secondary")]
        public void TestUninstallPackageLongName() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: LongName, Destination: TempFolder, Force: true, IsTesting: true);
            Assert.True(result2.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Skip = "Cannot do null names yet.")]
        public void TestUninstallPackageNullName() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: null, Destination: TempFolder, Force: true, IsTesting: true);
            Assert.True(result2.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Priority = 112), Trait("Test", "Secondary")]
        public void TestUninstallPackageNegMaxVersion() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "-1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result2.WaitForCompletion();
            Assert.True(result2.IsFailing);
            ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Force: true, IsTesting: true);
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Priority = 113), Trait("Test", "Secondary")]
        public void TestUninstallPackageNegMinVersion() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinimumVersion: "1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinimumVersion: "-1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result2.WaitForCompletion();
            Assert.True(result2.IsFailing);
            ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Force: true, IsTesting: true);
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Priority = 114), Trait("Test", "Secondary")]
        public void TestUninstallPackageNegReqVersion() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: "1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: "-1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result2.WaitForCompletion();
            Assert.True(result2.IsFailing);
            ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Force: true, IsTesting: true);
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Priority = 115), Trait("Test", "Secondary")]
        public void TestGetPackage() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "nuget", Name: "Adept.Nugetrunner", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.GetPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Force: true, Destination: TempFolder, IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing, "failed to get package.");
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestGetPackageLongName() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result2 = ps.GetPackage(Provider: "Nuget", Name: LongName, Force: true, Destination: TempFolder, IsTesting: true);
            result2.WaitForCompletion();
            Assert.True(result2.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000, Skip = "Null does not work at the moment.")]
        public void TestGetPackageNullName() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result2 = ps.GetPackage(Provider: "Nuget", Name: null, Force: true, Destination: TempFolder, IsTesting: true);
            result2.WaitForCompletion();
            Assert.True(result2.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestGetPackageNegMaxVersion() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.GetPackage(Provider: "Nuget", Name: "adept.nugetrunner", MaximumVersion: "-1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestGetPackageNegMinVersion() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.GetPackage(Provider: "Nuget", Name: "adept.nugetrunner", Version: "-1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestGetPackageNegReqVersion() {
            dynamic ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.GetPackage(Provider: "Nuget", Name: "adept.nugetrunner", RequiredVersion: "-1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact(Timeout = 60000), Trait("Test", "Secondary")]
        public void TestGetPackageProviderName() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(Name: "NuGet", ForceBootstrap: true, Force: true, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing, "Failed to get package provider.");

            object[] items = result.ToArray();
            Assert.Equal(1, items.Length);
        }

        [Fact(Timeout = 60000, Priority = 116), Trait("Test", "Secondary")]
        public void TestRegisterPackageSource() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.RegisterPackageSource(Name: "nugettest.org", Provider: "nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing);
            DynamicPowershellResult result2 = ps.UnregisterPackageSource(Name: "nugettest.org", Provider: "Nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result2.WaitForCompletion();
        }


        [Fact(Timeout = 60000, Priority = 117), Trait("Test", "Secondary")]
        public void TestUnregisterPackageSource() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.RegisterPackageSource(Name: "nugettest.org", Provider: "nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UnregisterPackageSource(Name: "nugettest.org", Provider: "Nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing);
        }

        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* ------------------------------------------------------------------------------     PERFORMANCE TESTS     ------------------------------------------------------------------------------ */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */

        [Fact(Timeout = 60000), Trait("Test", "Performance1")]
        public void TestFindPackageSelectObjectPipe() {
            dynamic ps = NewPowerShellSession;

            var watch = new Stopwatch();
            DynamicPowershellResult warmup = ps.FindPackage(ProviderName: "Nuget", Source: "https://msconfiggallery.cloudapp.net/api/v2/", IsTesting: true);
            warmup.WaitForCompletion();
            watch.Start();
            DynamicPowershellResult result = ps.FindPackage(ProviderName: "Nuget", Source: "https://msconfiggallery.cloudapp.net/api/v2/", IsTesting: true);
            // ReSharper disable once UnusedVariable
            var first = result.FirstOrDefault();
            watch.Stop();

            
            Console.WriteLine(@"Time elapsed: {0}", watch.Elapsed);
        }

        [Fact(Timeout = 60000), Trait("Test", "Performance2")]
        public void TestFindPackageAppDomainConfig()
        {
            dynamic ps = NewPowerShellSession;

            var watch = new Stopwatch();
            DynamicPowershellResult warmup = ps.FindPackage(Provider: "Nuget", Source: "https://msconfiggallery.cloudapp.net/api/v2/", Name: "AppDomainConfig", IsTesting: true);
            warmup.WaitForCompletion();
            watch.Start();
            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Source: "https://msconfiggallery.cloudapp.net/api/v2/", Name: "AppDomainConfig", IsTesting: true);
            result.WaitForCompletion();
            watch.Stop();
            Console.WriteLine(@"Time elapsed: {0}", watch.Elapsed);
            Assert.False(result.IsFailing);
        }

        [Fact(Timeout = 60000), Trait("Test", "Performance3")]
        public void TestFindPackageSelectObjectPipePsModule()
        {
            dynamic ps = NewPowerShellSession;

            var watch = new Stopwatch();
            DynamicPowershellResult warmup = ps.FindPackage(Provider: "PSModule", Source: "PSGallery", Name: "AppDomainConfig", IsTesting: true);
            warmup.WaitForCompletion();
            watch.Start();
            DynamicPowershellResult result = ps.FindPackage(Provider: "PSModule", Source: "PSGallery", Name: "AppDomainConfig", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.SelectObject(result, First: "1");
            result2.WaitForCompletion();
            watch.Stop();
            Console.WriteLine(@"Time elapsed: {0}", watch.Elapsed);
            Assert.False(result2.IsFailing);
        }

        [Fact(Timeout = 60000), Trait("Test", "Performance4")]
        public void TestFindPackagePsModule()
        {
            dynamic ps = NewPowerShellSession;

            var watch = new Stopwatch();
            DynamicPowershellResult warmup = ps.FindPackage(Provider: "PSModule", Source: "PSGallery", Name: "AppDomainConfig", IsTesting: true);
            warmup.WaitForCompletion();
            watch.Start();
            DynamicPowershellResult result = ps.FindPackage(Provider: "PSModule", Source: "PSGallery", Name: "AppDomainConfig", IsTesting: true);
            result.WaitForCompletion();
            watch.Stop();
            Console.WriteLine(@"Time elapsed: {0}", watch.Elapsed);
            Assert.False(result.IsFailing);
        }

        [Fact(Timeout = 60000), Trait("Test", "Performance5")]
        public void TestFindPackageSelectObjectRepositoryPsGalleryPipe()
        {
            dynamic ps = NewPowerShellSession;

            var watch = new Stopwatch();
            DynamicPowershellResult warmup = ps.FindModule(Repository: "PSGallery", IsTesting: true);
            warmup.WaitForCompletion();
            watch.Start();
            DynamicPowershellResult result = ps.FindModule(Repository: "PSGallery", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.SelectObject(result, First: "1");
            result2.WaitForCompletion();
            watch.Stop();
            Console.WriteLine(@"Time elapsed: {0}", watch.Elapsed);
            Assert.False(result2.IsFailing);
        }

        [Fact(Timeout = 60000), Trait("Test", "Performance6")]
        public void TestFindPackageRepositoryPsGallery()
        {
            dynamic ps = NewPowerShellSession;

            var watch = new Stopwatch();
            DynamicPowershellResult warmup = ps.FindModule(Repository: "PSGallery", Name: "AppDomainConfig");
            warmup.WaitForCompletion();
            watch.Start();
            DynamicPowershellResult result = ps.FindModule(Repository: "PSGallery", Name: "AppDomainConfig");
            result.WaitForCompletion();
            watch.Stop();
            Console.WriteLine(@"Time elapsed: {0}", watch.Elapsed);
            Assert.False(result.IsFailing);
        }

        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* ---------------------------------------------------------------------------     HELPER FUNCTIONS     ---------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
    
        private void DeleteAllNuget() {
            const string filesToDelete = "*.nupkg*";
            string[] fileList = Directory.GetFiles(TempFolder, filesToDelete);
            foreach (string file in fileList) {
                File.Delete(file);
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
        private string NuGetPath
        {
            get
            {
                string systemBase = KnownFolders.GetFolderPath(KnownFolder.CommonApplicationData);
                Assert.False(string.IsNullOrEmpty(systemBase), "Known Folder CommonApplicationData is null");

                IEnumerable<string> nugets = NugetsInPath(Path.Combine(systemBase, "oneget", "ProviderAssemblies"));
                string first = nugets.FirstOrDefault();
                if (first != null)
                {
                    return first;
                }

                string userBase = KnownFolders.GetFolderPath(KnownFolder.LocalApplicationData);
                Assert.False(string.IsNullOrEmpty(userBase), "Known folder LocalApplicationData is null");

                nugets = NugetsInPath(Path.Combine(userBase, "oneget", "ProviderAssemblies"));
                first = nugets.FirstOrDefault();
                if (first != null)
                {
                    return first;
                }

                return null;
            }
        }
        private bool IsNuGetInstalled
        {
            get
            {
                return NuGetPath != null;
            }
        }
    }

    public class ChocolateyProviderTest : TestBase {
        [Fact(Priority = 0)]
        public void TestChocolateyProperInstall() {
            //TODO
        }
    }
}