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
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Utility.Extensions;
    using Microsoft.OneGet.Utility.PowerShell;
    using Xunit;

    public class ChocolateyProviderTest : TestBase {
        public static int Count = 0;
        public static string Source = "https://www.chocolatey.org/api/v2";

        private const string ChocoDirectory = @"c:\chocolatey\lib";

        private const string LongName =
            "THISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERS";

        private readonly string[] _workingMaximumVersions = {
            "3.5",
            "4.0"
        };

        private readonly string[] _workingMinimumVersions = {
            "1.0",
            "1.3",
            "1.5"
        };

        private readonly string[] _workingNames = {
            "ResolveAlias",
            "python"
        };

        private readonly string[] _workingSourceNames = {
            "CHOCOLATEY101.org",
            "CHOCOLATEY202.org",
            "CHOCOLATEY303.org"
        };

        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* -----------------------------------------------------------------------------     PRIMARY TESTS     ----------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */

        [Fact(Timeout = 60000, Priority = 500), Trait("Test", "Chocolatey")]
        public void TestSavePackage() {
            dynamic ps = NewPowerShellSession;

            try {
                DynamicPowershellResult result = ps.SavePackage(Name: "python", Provider: "Chocolatey", MinimumVersion: "1.0", MaximumVersion: "2.0", DestinationPath: ChocoDirectory, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.False(IsDirectoryEmpty(ChocoDirectory));
            } finally {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 501), Trait("Test", "Chocolatey")]
        public void TestInstallPackage() {
            dynamic ps = NewPowerShellSession;
            try {
                TrustChocolatey();
                DynamicPowershellResult result = ps.InstallPackage(Name: "python", Provider: "Chocolatey", Source: Source, Force: true, IsTesting: true);
                result.WaitForCompletion();
                Assert.False(IsDirectoryEmpty(ChocoDirectory));
            } finally {
                DynamicPowershellResult result2 = ps.UninstallPackage(Name: "python", Provider: "Chocolatey", isTesting: true);
                result2.WaitForCompletion();
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000), Trait("Test", "Chocolatey")]
        public void TestFindPackage() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Name: "python", Provider: "Chocolatey", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            var x = (from dynamic source in result select source.Name).ToList();
            Assert.True(x.Contains("python"));
        }

        [Fact(Timeout = 180000, Skip = "Disabled. SetPackageSource Currently Fails -WhatIf."), Trait("Test", "Chocolatey")]
        public void TestWhatIfAllCmdlets()
        {
            dynamic ps = NewPowerShellSession;
            try
            {
                DynamicPowershellResult save = ps.SavePackage(Name: "ResolveAlias", Provider: "Chocolatey", DestinationPath: ChocoDirectory, WhatIf: true, Source: Source, IsTesting: true);
                save.WaitForCompletion();
                Assert.False(save.ContainsErrors);
                DynamicPowershellResult install = ps.InstallPackage(Name: "ResolveAlias", Provider: "Chocolatey", WhatIf: true, Source: Source, IsTesting: true);
                install.WaitForCompletion();
                Assert.False(install.ContainsErrors);

                DynamicPowershellResult install2 = ps.InstallPackage(Name: "ResolveAlias", Provider: "Chocolatey", Force: true, IsTesting: true);
                install2.WaitForCompletion();

                DynamicPowershellResult uninstall = ps.UninstallPackage(Name: "ResolveAlias", Provider: "Chocolatey", Destination: ChocoDirectory, WhatIf: true, IsTesting: true);
                uninstall.WaitForCompletion();
                Assert.False(uninstall.ContainsErrors);

                DynamicPowershellResult register = ps.RegisterPackageSource(Name: "chocolateytest55.org", Provider: "Chocolatey", Location: "Http://www.chocolatey.org/api/v2", WhatIf: true, IsTesting: true);
                register.WaitForCompletion();
                Assert.False(register.ContainsErrors);

                DynamicPowershellResult setPs = ps.SetPackageSource(Name: "Chocolatey", NewName: "ChocolateyTest", WhatIf: true, isTesting: true);
                setPs.WaitForCompletion();
                Assert.False(setPs.ContainsErrors);

                DynamicPowershellResult unregister = ps.UnregisterPackageSource(Name: "Chocolatey", Provider: "Chocolatey", Location: "Http://www.chocolatey.org/api/v2", WhatIf: true, IsTesting: true);
                unregister.WaitForCompletion();
                Assert.False(unregister.ContainsErrors);


            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 120000, Priority = 502), Trait("Test", "Chocolatey")]
        public void TestSetPackageSourceSuccessfulCombinations()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.RegisterPackageSource(Name: "chocolateytest.org", provider: "chocolatey", Location: "https://www.chocolatey.org/api/v2/", IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.SetPackageSource(Name: "chocolateytest.org", NewName: "chocolateytest2.org", provider: "chocolatey", Location: "https://www.chocolatey.org/api/v2/", NewLocation: "https://www.chocolatey.org/api/v2/", IsTesting: true);
                result2.WaitForCompletion();
                DynamicPowershellResult result3 = ps.GetPackageSource(IsTesting: true);
                result3.WaitForCompletion();
                List<dynamic> x = (from dynamic source in result3 select source.Name).ToList();
                Assert.True(x.Contains("chocolateytest2.org"));

                DynamicPowershellResult result4 = ps.RegisterPackageSource(Name: "chocolateytest3.org", provider: "chocolatey", Location: "https://www.chocolatey.org/api/v2/", IsTesting: true);
                result4.WaitForCompletion();
                DynamicPowershellResult result5 = ps.SetPackageSource(Name: "chocolateytest3.org", NewName: "chocolateytest4.org", provider: "chocolatey", Location: "https://www.chocolatey.org/api/v2/", NewLocation: "https://www.chocolatey.org/api/v2/", IsTesting: true);
                result5.WaitForCompletion();
                DynamicPowershellResult result6 = ps.GetPackageSource(isTesting: true);
                result6.WaitForCompletion();
                List<dynamic> y = (from dynamic source in result6 select source.Name).ToList();
                Assert.True(y.Contains("chocolateytest4.org"));

            }
            finally
            {
                DynamicPowershellResult result7 = ps.UnregisterPackageSource(Name: "chocolateytest.org", provider: "chocolatey", Location: "https://www.chocolatey.org/api/v2/", IsTesting: true);
                result7.WaitForCompletion();
                DynamicPowershellResult result8 = ps.UnregisterPackageSource(Name: "chocolateytest2.org", provider: "chocolatey", Location: "https://www.chocolatey.org/api/v2/", IsTesting: true);
                result8.WaitForCompletion();
                DynamicPowershellResult result9 = ps.UnregisterPackageSource(Name: "chocolateytest3.org", provider: "chocolatey", Location: "https://www.chocolatey.org/api/v2/", IsTesting: true);
                result9.WaitForCompletion();
                DynamicPowershellResult result10 = ps.UnregisterPackageSource(Name: "chocolateytest4.org", provider: "chocolatey", Location: "https://www.chocolatey.org/api/v2/", IsTesting: true);
                result10.WaitForCompletion();
            }
        }
        [Fact(Timeout = 120000, Priority = 503), Trait("Test", "Chocolatey")]
        public void TestFindPackageSuccessfulCombinations()
        {
            dynamic ps = NewPowerShellSession;
            int i = 0;
            foreach (string packageName in _workingNames)
            {
                foreach (string minVersion in _workingMinimumVersions)
                {
                    foreach (string maxVersion in _workingMaximumVersions)
                    {
                        i++;
                        DynamicPowershellResult result = ps.FindPackage(Name: packageName, Provider: "chocolatey", MaximumVersion: maxVersion, version: minVersion, allVersions: true, IsTesting: true);
                        result.WaitForCompletion();
                        foreach (object pkg in result)
                        {
                            var package = pkg as SoftwareIdentity;
                            if (package == null)
                            {
                                Console.WriteLine(@"ERROR: No Package Found.");
                            }
                            else
                            {
                                Console.WriteLine(@"{0}: Found {1} - {2} -- {3}", i, package.Name, package.Version, package.PackageFilename);
                            }
                        }
                        Assert.False(result.ContainsErrors);
                    }
                }
            }
        }

        [Fact(Timeout = 120000, Priority = 504), Trait("Test", "Chocolatey")]
        public void TestSavePackageSuccessfulCombinations()
        {
            dynamic ps = NewPowerShellSession;
            int i = 0;
            try
            {
                foreach (string packageName in _workingNames)
                {
                    foreach (string minVersion in _workingMinimumVersions)
                    {
                        foreach (string maxVersion in _workingMaximumVersions)
                        {
                            i++;
                            Console.WriteLine(@"{0}: Saving {1} {2}", i, packageName, "");
                            DynamicPowershellResult result = ps.SavePackage(Name: packageName, provider: "chocolatey", MaximumVersion: maxVersion, minimumversion: minVersion, Destination: ChocoDirectory, Force: true, Source: Source,
                                IsTesting: true);
                            result.WaitForCompletion();

                            foreach (object pkg in result)
                            {
                                var package = pkg as SoftwareIdentity;
                                if (package == null)
                                {
                                    Console.WriteLine(@"ERROR: NO PACKAGE FOUND.");
                                }
                                else
                                {
                                    Console.WriteLine(@"{0}: Saved {1} - {2} -- {3}", i, package.Name, package.Version, package.PackageFilename);
                                }
                            }
                            Assert.False(IsDirectoryEmpty(ChocoDirectory));
                            CleanFolder(ChocoDirectory);
                        }
                    }
                }
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 120000, Priority = 505), Trait("Test", "Chocolatey")]
        public void TestInstallPackageSuccessfulCombinations()
        {
            dynamic ps = NewPowerShellSession;
            int i = 0;
            TrustChocolatey();
            try
            {
                foreach (string packageName in _workingNames)
                {
                    foreach (string minVersion in _workingMinimumVersions)
                    {
                        foreach (string maxVersion in _workingMaximumVersions)
                        {
                            i++;
                            DynamicPowershellResult result = ps.InstallPackage(Name: packageName, Provider: "Chocolatey", MaximumVersion: maxVersion, minimumversion: minVersion, Force: true, Source: Source,
                                IsTesting: true);
                            result.WaitForCompletion();
                            foreach (object pkg in result)
                            {
                                var package = pkg as SoftwareIdentity;
                                if (package == null)
                                {
                                    Console.WriteLine(@"ERROR: NO PACKAGE FOUND.");
                                }
                                else
                                {
                                    Console.WriteLine(@"{0}: Installed {1} - {2} -- {3}", i, package.Name, package.Version, package.PackageFilename);
                                }
                            }
                            Assert.False(IsDirectoryEmpty(ChocoDirectory));
                            CleanFolder(ChocoDirectory);
                        }
                    }
                }
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 120000, Priority = 506), Trait("Test", "Chocolatey")]
        public void TestUninstallPackageSuccessfulCombinations()
        {
            dynamic ps = NewPowerShellSession;
            int i = 0;
            try
            {
                foreach (string packageName in _workingNames)
                {
                    foreach (string minVersion in _workingMinimumVersions)
                    {
                        foreach (string maxVersion in _workingMaximumVersions)
                        {
                            i++;
                            DynamicPowershellResult result = ps.InstallPackage(Name: packageName, Provider: "Chocolatey", MaximumVersion: maxVersion, minimumversion: minVersion, Force: true, IsTesting: true);
                            result.WaitForCompletion();
                            foreach (object pkg in result)
                            {
                                var package = pkg as SoftwareIdentity;
                                if (package == null)
                                {
                                    Console.WriteLine(@"ERROR: NO PACKAGE FOUND.");
                                }
                                else
                                {
                                    Console.WriteLine(@"{0}: Installed {1} - {2} -- {3}", i, package.Name, package.Version, package.PackageFilename);
                                }
                            }
                            DynamicPowershellResult result2 = ps.UninstallPackage(Name: packageName, Provider: "Chocolatey", MaximumVersion: maxVersion, minimumversion: minVersion, Force: true, IsTesting: true);
                            result2.WaitForCompletion();
                            Console.WriteLine(@"{0}: Uninstalling {1}", i, packageName);
                            Assert.True(IsDirectoryEmpty(ChocoDirectory));
                        }
                    }
                }
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 120000, Priority = 507), Trait("Test", "Chocolatey")]
        public void TestGetPackageSuccessfulCombinations()
        {
            dynamic ps = NewPowerShellSession;
            int i = 0;
            try
            {
                foreach (string packageName in _workingNames)
                {
                    foreach (string minVersion in _workingMinimumVersions)
                    {
                        foreach (string maxVersion in _workingMaximumVersions)
                        {
                            i++;
                            DynamicPowershellResult result = ps.InstallPackage(Name: packageName, Provider: "Chocolatey", MaximumVersion: maxVersion, minimumversion: minVersion, Force: true, IsTesting: true);
                            result.WaitForCompletion();
                            foreach (object pkg in result)
                            {
                                var package = pkg as SoftwareIdentity;
                                if (package == null)
                                {
                                    Console.WriteLine(@"ERROR: NO PACKAGE FOUND.");
                                }
                                else
                                {
                                    Console.WriteLine(@"{0}: Installed {1} - {2} -- {3}", i, package.Name, package.Version, package.PackageFilename);
                                }
                            }
                            DynamicPowershellResult result2 = ps.GetPackage(Name: packageName, Provider: "Chocolatey", MaximumVersion: maxVersion, minimumversion: minVersion, IsTesting: true);
                            result2.WaitForCompletion();
                            Assert.False(result2.ContainsErrors);
                            foreach (dynamic source in result2)
                            {
                                dynamic name = source.Name;
                                dynamic provider = source.ProviderName;
                                Console.WriteLine("Getting Package: {0} - {1}", name, provider);
                            }
                            CleanFolder(ChocoDirectory);
                        }
                    }
                }
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 120000, Priority = 508), Trait("Test", "Chocolatey")]
        public void TestRegisterPackageSourceSuccessfulCombinations()
        {
            dynamic ps = NewPowerShellSession;
            try
            {
                foreach (string sourceName in _workingSourceNames)
                {
                    DynamicPowershellResult result = ps.RegisterPackageSource(Name: sourceName, Provider: "chocolatey", Location: "http://www.chocolatey.org/api/v2", IsTesting: true);
                    result.WaitForCompletion();
                    DynamicPowershellResult result2 = ps.GetPackageSource(IsTesting: true);
                    result2.WaitForCompletion();
                    List<dynamic> x = (from dynamic source in result2 select source.Name).ToList();
                    Assert.True(x.Contains(sourceName));
                    DynamicPowershellResult result3 = ps.UnregisterPackageSource(Name: sourceName, Provider: "chocolatey", Location: "http://www.chocolatey.org/api/v2", IsTesting: true);
                    result3.WaitForCompletion();
                }
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 120000, Priority = 509), Trait("Test", "Chocolatey")]
        public void TestUnregisterPackageSourceSuccessfulCombinations()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                foreach (string sourceName in _workingSourceNames)
                {
                    DynamicPowershellResult result = ps.RegisterPackageSource(Name: sourceName, provider: "chocolatey", Location: "http://www.chocolatey.org/api/v2", IsTesting: true);
                    result.WaitForCompletion();
                    DynamicPowershellResult result2 = ps.GetPackageSource(isTesting: true);
                    result2.WaitForCompletion();
                    List<dynamic> x = (from dynamic source in result2 select source.Name).ToList();
                    Assert.True(x.Contains(sourceName));
                    DynamicPowershellResult result3 = ps.UnregisterPackageSource(Name: sourceName, provider: "chocolatey", Location: "http://www.chocolatey.org/api/v2", IsTesting: true);
                    result3.WaitForCompletion();
                    DynamicPowershellResult result4 = ps.GetPackageSource(isTesting: true);
                    List<dynamic> y = (from dynamic source in result4 select source.Name).ToList();
                    Assert.False(y.Contains(sourceName));
                    Console.WriteLine(@"UNREGISTERED: {0} ", sourceName);
                }
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 510), Trait("Test", "Chocolatey")]
        public void TestGetPackageProvider()
        {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(Name: "Chocolatey", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.Any());
        }

        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* ----------------------------------------------------------------------------     PIPELINE TESTS     ----------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */

        [Fact(Timeout = 120000, Priority = 511), Trait("Test", "Chocolatey")]
        public void TestFindPackagePipeProvider()
        {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(Name: "Chocolatey", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.FindPackage(result, Name: "ResolveAlias", IsTesting: true);
            result.WaitForCompletion();
            List<dynamic> x = (from dynamic source in result2 select source.Name).ToList();
            Assert.True(x.Contains("ResolveAlias"));
        }

        [Fact(Timeout = 60000, Priority = 512), Trait("Test", "Chocolatey")]
        public void TestSetPackageSourcePipeProvider()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.GetPackageProvider(Name: "chocolatey", isTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.RegisterPackageSource(result, Name: "chocolateytest.org", Location: "https://www.chocolatey.org/api/v2", isTesting: true);
                result2.WaitForCompletion();
                DynamicPowershellResult result3 = ps.SetPackageSource(result2, NewName: "chocolateytest2.org", NewLocation: "https://www.chocolatey.org/api/v2", isTesting: true);
                result3.WaitForCompletion();
                DynamicPowershellResult result4 = ps.GetPackageSource(isTesting: true);
                result4.WaitForCompletion();
                var x = (from dynamic source in result4 select source.Name).ToList();
                Assert.True(x.Contains("chocolateytest2.org"));
            }
            finally
            {
                DynamicPowershellResult result5 = ps.UnregisterPackageSource(Name: "chocolateytest.org", isTesting: true);
                result5.WaitForCompletion();
                DynamicPowershellResult result6 = ps.UnregisterPackageSource(Name: "chocolateytest2.org", isTesting: true);
                result6.WaitForCompletion();
            }
        }

        [Fact(Timeout = 60000, Priority = 513), Trait("Test", "Chocolatey")]
        public void TestSavePackagePipeName()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.FindPackage(Name: "ResolveAlias", ProviderName: "Chocolatey", IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.SavePackage(result, DestinationPath: ChocoDirectory, IsTesting: true);
                result2.WaitForCompletion();
                Assert.False(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 600000, Priority = 514), Trait("Test", "Chocolatey")]
        public void TestInstallPackagePipeName()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.FindPackage(Name: "ResolveAlias", Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.InstallPackage(result, Force: true, IsTesting: true);
                result2.WaitForCompletion();
                Assert.False(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 515), Trait("Test", "Chocolatey")]
        public void TestRegisterPackageSourcePipe()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.GetPackageProvider(Name: "chocolatey", IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.RegisterPackageSource(result, Name: "chocolateytest.org", Location: "https://www.chocolatey.org/api/v2/", IsTesting: true);
                result2.WaitForCompletion();
                DynamicPowershellResult result3 = ps.getPackageSource(isTesting: true);
                result3.WaitForCompletion();
                List<dynamic> x = (from dynamic source in result3 select source.Name).ToList();
                Assert.True(x.Contains("chocolateytest.org"));
            }
            finally
            {
                DynamicPowershellResult result3 = ps.UnregisterPackageSource(Name: "chocolateytest.org", Provider: "chocolatey", Location: "https://www.chocolatey.org/api/v2/", IsTesting: true);
                result3.WaitForCompletion();
            }
        }

        [Fact(Timeout = 60000, Priority = 516), Trait("Test", "Chocolatey")]
        public void TestUnregisterPackageSourcePipe()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.GetPackageProvider(Name: "Chocolatey", Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.RegisterPackageSource(Provider: "Chocolatey", Name: "ChocolateyTest.org", Location: "https://www.chocolatey.org/api/v2/", IsTesting: true);
                result2.WaitForCompletion();
                DynamicPowershellResult result3 = ps.getPackageSource(isTesting: true);
                result3.WaitForCompletion();
                List<dynamic> x = (from dynamic source in result3 select source.Name).ToList();
                Assert.True(x.Contains("ChocolateyTest.org"));
                DynamicPowershellResult result4 = ps.UnregisterPackageSource(result, Name: "ChocolateyTest.org", Location: "https://www.chocolatey.org/api/v2/", IsTesting: true);
                result4.WaitForCompletion();
                DynamicPowershellResult result5 = ps.getPackageSource(isTesting: true);
                result5.WaitForCompletion();
                List<dynamic> y = (from dynamic source in result5 select source.Name).ToList();
                Assert.False(y.Contains("ChocolateyTest.org"));
            }
            finally
            {
                DynamicPowershellResult result6 = ps.UnregisterPackageSource(Name: "ChocolateyTest.org", Location: "https://www.chocolatey.org/api/v2", IsTesting: true);
                result6.WaitForCompletion();
            }
        }

        [Fact(Timeout = 60000, Priority = 517), Trait("Test", "Chocolatey")]
        public void TestGetPackagePipeProvider()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.InstallPackage(Name: "ResolveAlias", Provider: "Chocolatey", Force: true, isTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackageProvider(Name: "Chocolatey", IsTesting: true);
                result2.WaitForCompletion();
                DynamicPowershellResult result3 = ps.GetPackage(result, IsTesting: true);
                result3.WaitForCompletion();
                List<dynamic> x = (from dynamic source in result3 select source.ProviderName).ToList();
                Assert.True(x.Contains("Chocolatey"));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 518), Trait("Test", "Chocolatey")]
        public void TestUninstallPackagePipeName()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.FindPackage(Name: "ResolveAlias", Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.InstallPackage(result, Force: true, IsTesting: true);
                result2.WaitForCompletion();
                DynamicPowershellResult result3 = ps.GetPackage(Name: "ResolveAlias", Provider: "Chocolatey", IsTesting: true);
                DynamicPowershellResult result4 = ps.UninstallPackage(result3, Force: true, IsTesting: true);
                result4.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [
            Fact(Timeout = 60000, Priority = 519), Trait("Test", "Chocolatey")]
        public void TestGetPackageSourcePipe()
        {
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
        /* ---------------------------------------------------------------------------     SECONDARY TESTS     ----------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */

        [Fact(Timeout = 60000), Trait("Test", "Chocolatey")]
        public void TestFindPackageLongName()
        {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(provider: "chocolatey", Name: LongName, Source: Source, IsTesting: true);
            result.WaitForCompletion();
            var x = (from dynamic source in result select source.Name).ToList();
            Assert.False(x.Contains(LongName));
        }

        [Fact(Timeout = 60000, Skip = "Disabled. Cannot handle null names.")]
        public void TestFindPackageNullName()
        {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(provider: "chocolatey", Name: null, Source: Source, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.ContainsErrors);
        }

        [Fact(Timeout = 60000), Trait("Test", "Chocolatey")]
        public void TestFindPackageInvalidName()
        {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(provider: "chocolatey", Name: "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            var x = (from dynamic source in result select source.Name).ToList();
            Assert.False(x.Contains("1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER"));
        }

        [Fact(Timeout = 60000), Trait("Test", "Chocolatey")]
        public void TestFindPackageNegMaxVersion()
        {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(provider: "chocolatey", Name: "chocolatey", MaximumVersion: "-1.5", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            var x = (from dynamic source in result select source.Name).ToList();
            Assert.False(x.Contains("chocolatey"));
        }

        [Fact(Timeout = 60000), Trait("Test", "Chocolatey")]
        public void TestFindPackageNegMinVersion()
        {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(provider: "chocolatey", Name: "chocolatey", Version: "-1.5", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            var x = (from dynamic source in result select source.Name).ToList();
            Assert.False(x.Contains("chocolatey"));
        }

        [Fact(Timeout = 60000), Trait("Test", "Chocolatey")]
        public void TestFindPackageNegReqVersion()
        {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(provider: "chocolatey", Name: "chocolatey", RequiredVersion: "-1.5", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            var x = (from dynamic source in result select source.Name).ToList();
            Assert.False(x.Contains("chocolatey"));
        }

        [Fact(Timeout = 60000), Trait("Test", "Chocolatey")]
        public void TestFindPackageRequiredVersionFail()
        {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(provider: "chocolatey", Name: "ResolveAlias", Version: "1.0", MaximumVersion: "1.5", RequiredVersion: "2.0", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            var x = (from dynamic source in result select source.Name).ToList();
            Assert.False(x.Contains("ResolveAlias"));
        }

        [Fact(Timeout = 60000), Trait("Test", "Chocolatey")]
        public void TestFindPackageAllVersionFail()
        {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(provider: "chocolatey", Name: "ResolveAlias", Version: "1.0", MaximumVersion: "1.5", AllVersion: "2.0", Source: Source, IsTesting: true);
            result.WaitForCompletion();
            var x = (from dynamic source in result select source.Name).ToList();
            Assert.False(x.Contains("ResolveAlias"));
        }

        [Fact(Timeout = 300000), Trait("Test", "Chocolatey")]
        public void TestFindPackageMismatchedVersions()
        {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(provider: "chocolatey", Name: "ResolveAlias", Version: "1.5", MaximumVersion: "1.0", Source: Source, IsTesting: true);
            var x = (from dynamic source in result select source.Name).ToList();
            Assert.False(x.Contains("ResolveAlias"));
        }

        [Fact(Timeout = 120000, Priority = 520), Trait("Test", "Chocolatey")]
        public void TestSavePackageLongName()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.SavePackage(Provider: "Chocolatey", Name: LongName, DestinationPath: ChocoDirectory, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Skip = "Disabled. Cannot handle null names.")]
        public void TestSavePackageNullName()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.SavePackage(Provider: "Chocolatey", Name: null, DestinationPath: ChocoDirectory, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 120000, Priority = 521), Trait("Test", "Chocolatey")]
        public void TestSavePackageMismatchedVersions()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.SavePackage(Provider: "Chocolatey", Name: "ResolveAlias", DestinationPath: ChocoDirectory, MaximumVersion: "1.0", Version: "2.0", IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 120000, Priority = 522), Trait("Test", "Chocolatey")]
        public void TestSavePackageNegReqVersion()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.SavePackage(Provider: "Chocolatey", Name: "ResolveAlias", DestinationPath: ChocoDirectory, RequiredVersion: "-1.0.0.2", Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 120000, Priority = 523), Trait("Test", "Chocolatey")]
        public void TestSavePackageNegMaxVersion()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.SavePackage(Provider: "Chocolatey", Name: "ResolveAlias", DestinationPath: ChocoDirectory, MaximumVersion: "-1.0.0.2", Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 120000, Priority = 524), Trait("Test", "Chocolatey")]
        public void TestSavePackageNegMinVersion()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.SavePackage(Provider: "Chocolatey", Name: "ResolveAlias", DestinationPath: ChocoDirectory, Version: "-1.0.0.2", Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 525), Trait("Test", "Chocolatey")]
        public void TestInstallPackageLongName()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: LongName, Destination: ChocoDirectory, Force: true, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Skip = "Disabled. Cannot handle null names.")]
        public void TestInstallPackageNullName()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: null, Destination: ChocoDirectory, Force: true, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 526), Trait("Test", "Chocolatey")]
        public void TestInstallPackageBigMinVersion()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: "ResolveAlias", MinimumVersion: "999", Force: true, Destination: ChocoDirectory, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 527), Trait("Test", "Chocolatey")]
        public void TestInstallPackageNegMaxVersion()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: "ResolveAlias", MaximumVersion: "-1.0", Force: true, Destination: ChocoDirectory, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 528), Trait("Test", "Chocolatey")]
        public void TestInstallPackageNegMinVersion() {
            dynamic ps = NewPowerShellSession;

            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: "ResolveAlias", MinVersion: "-1.0.0.2", Force: true, Destination: ChocoDirectory, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(result.ContainsErrors);
            } finally {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 529), Trait("Test", "Chocolatey")]
        public void TestInstallPackageNegReqVersion() {
            dynamic ps = NewPowerShellSession;

            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: "ResolveAlias", RequiredVersion: "-1.0", Force: true, Destination: ChocoDirectory, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(ChocoDirectory));
            } finally {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Skip = "Disabled. Cannot handle null names.")]
        public void TestInstallPackageNullReqVersion() {
            dynamic ps = NewPowerShellSession;

            try {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: "ResolveAlias", RequiredVersion: null, Force: true, Destination: ChocoDirectory, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(ChocoDirectory));
            } finally {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 530), Trait("Test", "Chocolatey")]
        public void TestUninstallPackage()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: "ResolveAlias", Force: true, Destination: ChocoDirectory, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Chocolatey", Name: "ResolveAlias", Destination: ChocoDirectory, Force: true, IsTesting: true);
                result2.WaitForCompletion();
                Assert.True(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 531), Trait("Test", "Chocolatey")]
        public void TestUninstallPackageLongName()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: "python", Force: true, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Chocolatey", Name: LongName, Force: true, IsTesting: true);
                result2.WaitForCompletion();
                Assert.False(IsDirectoryEmpty(ChocoDirectory));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Skip = "Disabled. Cannot handle null names.")]
        public void TestUninstallPackageNullName()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: "ResolveAlias", Destination: ChocoDirectory, Force: true, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Chocolatey", Name: null, Destination: ChocoDirectory, Force: true, Source: Source, IsTesting: true);
                Assert.True(result2.ContainsErrors);
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }


        [Fact(Timeout = 60000, Priority = 532), Trait("Test", "Chocolatey")]
        public void TestGetPackage()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: "ResolveAlias", Force: true, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackage(Provider: "Chocolatey", Name: "ResolveAlias", Force: true, IsTesting: true);
                result2.WaitForCompletion();
                List<dynamic> x = (from dynamic source in result2 select source.Name).ToList();
                Assert.True(x.Contains("ResolveAlias"));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 533), Trait("Test", "Chocolatey")]
        public void TestGetPackageLongName()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: "ResolveAlias", Force: true, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackage(Provider: "Chocolatey", Name: LongName, Force: true, IsTesting: true);
                result2.WaitForCompletion();
                List<dynamic> x = (from dynamic source in result2 select source.Name).ToList();
                Assert.False(x.Contains(LongName));
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Skip = "Disabled. Cannot handle null names.")]
        public void TestGetPackageNullName()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: "ResolveAlias", Force: true,  IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackage(Provider: "Chocolatey", Name: null, Force: true, IsTesting: true);
                result2.WaitForCompletion();
                Assert.True(result2.ContainsErrors);
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 534), Trait("Test", "Chocolatey")]
        public void TestGetPackageNegMaxVersion()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: "ResolveAlias", Force: true, Destination: ChocoDirectory, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackage(Provider: "Chocolatey", Name: "ResolveAlias", MaximumVersion: "-1.0", Force: true, Destination: ChocoDirectory, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(result2.ContainsErrors);
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 535), Trait("Test", "Chocolatey")]
        public void TestGetPackageNegMinVersion()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: "ResolveAlias", Force: true, Destination: ChocoDirectory, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackage(Provider: "Chocolatey", Name: "ResolveAlias", Version: "-1.0", Force: true, Destination: ChocoDirectory, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(result2.ContainsErrors);
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 536), Trait("Test", "Chocolatey")]
        public void TestGetPackageNegReqVersion()
        {
            dynamic ps = NewPowerShellSession;

            try
            {
                DynamicPowershellResult result = ps.InstallPackage(Provider: "Chocolatey", Name: "ResolveAlias", Force: true, Destination: ChocoDirectory, IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackage(Provider: "Chocolatey", Name: "ResolveAlias", RequiredVersion: "-1.0", Force: true, Destination: ChocoDirectory, Source: Source, IsTesting: true);
                result.WaitForCompletion();
                Assert.True(result2.ContainsErrors);
            }
            finally
            {
                CleanFolder(ChocoDirectory);
            }
        }

        [Fact(Timeout = 60000, Priority = 537), Trait("Test", "Chocolatey")]
        public void TestGetPackageProviderName()
        {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(Name: "Chocolatey", ForceBootstrap: true, Force: true, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.ContainsErrors, "Failed to get package provider.");

            object[] items = result.ToArray();
            Assert.Equal(1, items.Length);
        }

        [Fact(Timeout = 60000, Priority = 538), Trait("Test", "Chocolatey")]
        public void TestRegisterPackageSource()
        {
            dynamic ps = NewPowerShellSession;
            try
            {
                DynamicPowershellResult result = ps.RegisterPackageSource(Name: "ChocolateyTest.org", Provider: "Chocolatey", Location: "https://www.Chocolatey.org/api/v2/", IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackageSource(isTesting: true);
                List<dynamic> x = (from dynamic source in result2 select source.Name).ToList();
                Assert.True(x.Contains("ChocolateyTest.org"));
            }
            finally
            {
                DynamicPowershellResult result3 = ps.UnregisterPackageSource(Name: "ChocolateyTest.org", Provider: "Chocolatey", Location: "https://www.Chocolatey.org/api/v2/", IsTesting: true);
                result3.WaitForCompletion();
            }
        }

        [Fact(Timeout = 60000, Priority = 539), Trait("Test", "Chocolatey")]
        public void TestUnregisterPackageSource()
        {
            dynamic ps = NewPowerShellSession;
            try
            {
                DynamicPowershellResult result = ps.RegisterPackageSource(Name: "ChocolateyTest.org", Provider: "Chocolatey", Location: "https://www.Chocolatey.org/api/v2/", IsTesting: true);
                result.WaitForCompletion();
                DynamicPowershellResult result2 = ps.GetPackageSource(isTesting: true);
                List<dynamic> x = (from dynamic source in result2 select source.Name).ToList();
                Assert.True(x.Contains("ChocolateyTest.org"));
                DynamicPowershellResult result3 = ps.UnregisterPackageSource(Name: "ChocolateyTest.org", Provider: "Chocolatey", Location: "https://www.Chocolatey.org/api/v2/", IsTesting: true);
                result3.WaitForCompletion();
                List<dynamic> y = (from dynamic source in result3 select source.Name).ToList();
                Assert.False(y.Contains("ChocolateyTest.org"));
            }
            finally
            {
                DynamicPowershellResult result4 = ps.UnregisterPackageSource(Name: "ChocolateyTest.org", Provider: "Chocolatey", Location: "https://www.Chocolatey.org/api/v2/", IsTesting: true);
                result4.WaitForCompletion();
            }
        }

        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* ---------------------------------------------------------------------------     HELPER FUNCTIONS     ---------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */

        private void TrustChocolatey() {
            dynamic ps = NewPowerShellSession;

            DynamicPowershellResult trust = ps.SetPackageSource(Name: "Chocolatey", Provider: "Chocolatey", Trusted: true, isTesting: true);
            trust.WaitForCompletion();
        }
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
    }
}