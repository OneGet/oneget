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

    public class NugetProviderTest : TestBase {
        private const string TempFolder = @"C:\\tempTestDirectoryZXY";

        private const string LongName =
            "THISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERSTHISISOVER255CHARACTERS";

        private string NuGetPath {
            get {
                var systemBase = KnownFolders.GetFolderPath(KnownFolder.CommonApplicationData);
                Assert.False(string.IsNullOrEmpty(systemBase), "Known Folder CommonApplicationData is null");

                var nugets = NugetsInPath(Path.Combine(systemBase, "oneget", "ProviderAssemblies"));
                var first = nugets.FirstOrDefault();
                if (first != null) {
                    return first;
                }

                var userBase = KnownFolders.GetFolderPath(KnownFolder.LocalApplicationData);
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

        [Fact]
        public void TestGetPackageProvider() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing, "Failed to get package provider.");
        }

        [Fact]
        public void TestFindPackage() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing, "Failed to find package with nuget provider.");
        }

        [Fact]
        public void TestFindPackagePipeProvider() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(Name: "Nuget", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.FindPackage(result, Name: "adept.nugetrunner", IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result2.IsFailing, "Failed to pipe provider to Find Package.");

        }

        [Fact]
        public void TestFindPackageLongName() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: LongName, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Found an invalid name.");
        }

       // [Fact] //TODO, Figure out how to properly do null.
        public void TestFindPackageNullName() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: null, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing);
        }

        public void TestFindPackageEmptyStringName() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "", IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing);
        }

        [Fact]
        public void TestFindPackageValidName() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing, "Failed to find package with valid name parameter.");
        }

        [Fact]
        public void TestFindPackageInvalidName() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Found package with invalid name.");
        }

        [Fact]
        public void TestFindPackageValidProvider() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing, "Did not manage to find nuget provider.");
        }

        [Fact(Skip = "impossible to check.")]
        //TODO
        //Has warning, not error, impossible to check at the moment. [BUG]
        public void TestFindPackageInvalidProvider() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.Success, "Found package with invalid name.");
        }

        [Fact]
        public void TestFindPackageMaxVersion() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", MaximumVersion: "1.5", IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing, "Failed to find package with max version parameter.");
        }

        [Fact]
        public void TestFindPackageNegMaxVersion() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", MaximumVersion: "-1.5", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to find package with negative MaximumVersion paramater.");
        }

        [Fact]
        public void TestFindPackageMinVersion() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", Version: "1.5", IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing, "Failed to find package with MinimumVersion parameter.");
        }

        [Fact]
        public void TestFindPackageNegMinVersion() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", Version: "-1.5", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to find package with negative MinimumVersion.");
        }

        [Fact]
        public void TestFindPackageReqVersion() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", RequiredVersion: "1.5", IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing, "Failed to find package with RequiredVersion parameter.");
        }

        [Fact]
        public void TestFindPackageNegReqVersion() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", RequiredVersion: "-1.5", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to find package with negative RequiredVersion.");
        }

        [Fact]
        public void TestFindPackageAllVersion() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", AllVersions: true, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing, "Failed to find package with AllVersion parameter.");
        }

        [Fact]
        public void TestFindPackageRequiredVersionFail() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", Version: "1.0", MaximumVersion: "1.5", RequiredVersion: "2.0", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to find package with invalid RequiredVersion parameter.");
        }

        [Fact]
        public void TestFindPackageAllVersionFail() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", Version: "1.0", MaximumVersion: "1.5", AllVersion: "2.0", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to find package with invalid AllVersion parameter.");
        }

        [Fact]
        public void TestFindPackageMismatchedVersions() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.FindPackage(Provider: "Nuget", Name: "Nuget", Version: "1.5", MaximumVersion: "1.0", IsTesting: true);
            Assert.True(result.IsFailing, "Managed to find package with invalid Max/Min version parameter combination.");
        }

        [Fact]
        public void TestSavePackage() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(File.Exists("C:\\tempTestDirectoryZXY\\Adept.NuGetRunner.1.0.0.2.nupkg"), "Save failed, package not found.");
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestSavePackagePipeName() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.FindPackage(Name: "adept.Nuget", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.SavePackage(result, Provider: "nuget", Destination: TempFolder, IsTesting: true);
            result2.WaitForCompletion();
            Assert.True(File.Exists("C:\\tempTestDirectoryZXY\\Adept.NuGetRunner.1.0.0.2.nupkg"), "Save failed, package not found.");
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestSavePackageLongName()
        {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: LongName, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);
            Directory.Delete(TempFolder, true);
        }

      //  [Fact] //TODO NULL
        public void TestSavePackageNullName()
        {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: null, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestSavePackageMinVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", Destination: TempFolder, Version: "1.0", IsTesting: true);
            result.WaitForCompletion();

            Assert.True(File.Exists("C:\\tempTestDirectoryZXY\\Adept.NuGetRunner.1.0.0.2.nupkg"), "Save failed, package with MinimumVersion parameter not found.");
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestSavePackageMaxVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", Destination: TempFolder, MaximumVersion: "1.0", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(File.Exists("C:\\tempTestDirectoryZXY\\Adept.NuGetRunner.1.0.nupkg"), "Save failed, package with MaximumVersion parameter not found.");
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestSavePackageReqVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", Destination: TempFolder, RequiredVersion: "1.0", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(File.Exists("C:\\tempTestDirectoryZXY\\Adept.NuGetRunner.1.0.nupkg"), "Save failed, package with RequiredVersion parameter not found.");
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestSavePackageMismatchedVersions() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", Destination: TempFolder, MaximumVersion: "1.0", Version: "1.0.0.2", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to save package with invalid Min/Max Version combination.");
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestSavePackageNegReqVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", Destination: TempFolder, RequiredVersion: "-1.0.0.2", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to save package with invalid RequiredVersion parameter.");
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestSavePackageNegMaxVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", Destination: TempFolder, MaximumVersion: "-1.0.0.2", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to save package with invalid MaximumVersion parameter.");
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestSavePackageNegMinVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", Destination: TempFolder, Version: "-1.0.0.2", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing, "Managed to save package with invalid MinimumVersion parameter.");
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestSavePackageDestination() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.NugetRunner", Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(File.Exists("C:\\tempTestDirectoryZXY\\Adept.NuGetRunner.1.0.nupkg"), "Failed to save package in destination.");
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestSavePackageInvalidDestination() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.SavePackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Destination: "c:\\failLocation", IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing); //TODO
        }

        [Fact]
        public void TestInstallPackage() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestInstallPackagePipeName() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.FindPackage(Name: "adept.nugetrunner", Provider: "Nuget", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.InstallPackage(result, Destination: TempFolder, Force: true, IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing);

            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestInstallPackageLongName()
        {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: LongName, Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }

        //[Fact] //TODO, NULL
        public void TestInstallPackageNullName()
        {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: null, Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestInstallPackageMaxVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestInstallPackageMinVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinimumVersion: "1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }
        
        [Fact]
        public void TestInstallPackageBigMinVersion()
        {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinimumVersion: "999", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }
        [Fact]
        public void TestInstallPackageReqVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: "1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestInstallPackageNegMaxVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "-1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestInstallPackageNegMinVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinVersion: "-1.0.0.2", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestInstallPackageNegReqVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: "-1.0", Force: true,Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);

            Directory.Delete(TempFolder, true);
        }

       // [Fact] //TODO : HOW TO INSERT NULL AND MAKE IT WORK
        public void TestInstallPackageNullReqVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: null, Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.Success);

            Directory.Delete(TempFolder, true);
        }


        //TODO, Uninstall Package does not work at the moment.
        [Fact]
        public void TestUninstallPackage() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Destination: TempFolder, Force: true, IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestUninstallPackageLongName() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: LongName, Destination: TempFolder, Force: true, IsTesting: true);
            Assert.True(result2.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        //[Fact]
        public void TestUninstallPackageNullName()
        {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: null, Destination: TempFolder, Force: true, IsTesting: true);
            Assert.True(result2.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestUninstallPackageMaxVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestUninstallPackageMinVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinimumVersion: "1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinimumVersion: "1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestUninstallPackageReqVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: "1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: "1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestUninstallPackageNegMaxVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "-1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result2.WaitForCompletion();
            Assert.True(result2.IsFailing);
            ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Force: true, IsTesting: true);
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestUninstallPackageNegMinVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinimumVersion: "1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MinimumVersion: "-1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result2.WaitForCompletion();
            Assert.True(result2.IsFailing);
            ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Force: true, IsTesting: true);
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestUninstallPackageNegReqVersion() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: "1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: "-1.0", Destination: TempFolder, Force: true, IsTesting: true);
            result2.WaitForCompletion();
            Assert.True(result2.IsFailing);
            ps.UninstallPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.0", Force: true, IsTesting: true);
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestGetPackage() {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "nuget", Name: "Adept.Nugetrunner", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.GetPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Force: true, Destination: TempFolder, IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing, "failed to get package.");
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestGetPackageLongName()
        {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result2 = ps.GetPackage(Provider: "Nuget", Name: LongName, Force: true, Destination: TempFolder, IsTesting: true);
            result2.WaitForCompletion();
            Assert.True(result2.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        //[Fact]
        public void TestGetPackageNullName()
        {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result2 = ps.GetPackage(Provider: "Nuget", Name: null, Force: true, Destination: TempFolder, IsTesting: true);
            result2.WaitForCompletion();
            Assert.True(result2.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestGetPackageMaxVersion()
        {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.1", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.GetPackage(Provider: "nuget", Name: "Adept.Nugetrunner", MaximumVersion: "1.1", Force: true, Destination: TempFolder, IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing, "failed to get package with maximum version parameter.");
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestGetPackageMinVersion()
        {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "nuget", Name: "Adept.Nugetrunner", MinimumVersion: "1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.GetPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", Version: "1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing, "failed to get package with minimum version parameter.");
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestGetPackageReqVersion()
        {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.InstallPackage(Provider: "nuget", Name: "Adept.Nugetrunner", RequiredVersion: "1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.GetPackage(Provider: "Nuget", Name: "Adept.Nugetrunner", RequiredVersion: "1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing, "failed to get package with required version parameter.");
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestGetPackageNegMaxVersion()
        {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.GetPackage(Provider: "Nuget", Name: "adept.nugetrunner", MaximumVersion: "-1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestGetPackageNegMinVersion()
        {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.GetPackage(Provider: "Nuget", Name: "adept.nugetrunner", Version: "-1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestGetPackageNegReqVersion()
        {
            var ps = NewPowerShellSession;

            Directory.CreateDirectory(TempFolder);
            DynamicPowershellResult result = ps.GetPackage(Provider: "Nuget", Name: "adept.nugetrunner", RequiredVersion: "-1.0", Force: true, Destination: TempFolder, IsTesting: true);
            result.WaitForCompletion();
            Assert.True(result.IsFailing);
            Directory.Delete(TempFolder, true);
        }

        [Fact]
        public void TestGetPackageProviderName() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(Name: "NuGet", ForceBootstrap: true, Force: true, IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing, "Failed to get package provider.");

            var items = result.ToArray();
            Assert.Equal(1, items.Length);
        }

        [Fact]
        public void TestRegisterPackageSource() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.RegisterPackageSource(Name: "nugettest.org", Provider: "nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result.WaitForCompletion();
            Assert.False(result.IsFailing);
            DynamicPowershellResult result2 = ps.UnregisterPackageSource(Name: "nugettest.org", Provider: "Nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result2.WaitForCompletion();
        }

        [Fact]
        public void TestRegisterPackageSourcePipe() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.GetPackageProvider(Name: "nuget", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.RegisterPackageSource(result, Name: "nugettest.org", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing);
            DynamicPowershellResult result3 = ps.UnregisterPackageSource(Name: "nugettest.org", Provider: "Nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result3.WaitForCompletion();
        }

        [Fact]
        public void TestUnregisterPackageSource() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.RegisterPackageSource(Name: "nugettest.org", Provider: "nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.UnregisterPackageSource(Name: "nugettest.org", Provider: "Nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing);
        
        }

        [Fact]
        public void TestUnregisterPackageSourcePipe()
        {
            var ps = NewPowerShellSession;
            DynamicPowershellResult result = ps.GetPackageProvider(Name: "nuget", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.RegisterPackageSource(Provider: "Nuget", Name: "nugettest.org", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result2.WaitForCompletion();
            DynamicPowershellResult result3 = ps.UnregisterPackageSource(result, Name: "Nugettest.org", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            Assert.False(result3.IsFailing);
        }

        //[Fact] // DOES NOT WORK TODO
        public void TestSetPackageSource() {
            var ps = NewPowerShellSession;

            DynamicPowershellResult result = ps.RegisterPackageSource(Name: "nugettest.org", Provider: "nuget", Location: "https://www.nuget.org/api/v2/", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.SetPackageSource(NewName: "nugettest2.org", Provider: "nuget", Location: "https://www.nuget.org/api/v2/", NewLocation: "https://www.nuget.org/api/v2/", IsTesting: true);
            result2.WaitForCompletion();
            Assert.False(result2.IsFailing);
            DynamicPowershellResult result3 = ps.UnRegisterPackageSource(Name: "Nugettest.org", Provider: "Nuget", Location: "https://www.nuget.org/api/v2/");
            result3.WaitForCompletion();
        }


        private void DeleteAllNuget() {
            const string filesToDelete = "*NuGet*";
            var fileList = Directory.GetFiles(TempFolder, filesToDelete);
            foreach (var file in fileList) {
                File.Delete(file);
            }
        }

        private bool IsDllOrExe(string path) {
            return path.ToLower().EndsWith(".exe") || path.ToLower().EndsWith(".dll");
        }

        private void PrintItems(dynamic items) {
            foreach (var i in items) {
                Console.WriteLine(i.Name);
            }
        }

        private IEnumerable<string> FilenameContains(IEnumerable<string> paths, string value) {
            return paths.Where(item => item.IndexOf(value, StringComparison.OrdinalIgnoreCase) > -1);
        }

        private IEnumerable<string> NugetsInPath(string folder) {
            if (Directory.Exists(folder)) {
                var files = Directory.EnumerateFiles(folder).ToArray();

                return FilenameContains(files, "nuget").Where(IsDllOrExe);
            }
            return Enumerable.Empty<string>();
        }

        private void DeleteNuGet() {
            var systemBase = KnownFolders.GetFolderPath(KnownFolder.CommonApplicationData);
            Assert.False(string.IsNullOrEmpty(systemBase));

            var nugets = NugetsInPath(Path.Combine(systemBase, "oneget", "ProviderAssemblies"));
            foreach (var nuget in nugets) {
                nuget.TryHardToDelete();
            }

            var userBase = KnownFolders.GetFolderPath(KnownFolder.LocalApplicationData);
            Assert.False(string.IsNullOrEmpty(userBase));

            nugets = NugetsInPath(Path.Combine(userBase, "oneget", "ProviderAssemblies"));
            foreach (var nuget in nugets) {
                nuget.TryHardToDelete();
            }
        }

        [Fact]
        public void TestBootstrapNuGet() {
            // delete any copies of nuget if they are installed.
            if (IsNuGetInstalled) {
                DeleteNuGet();
            }

            // verify that nuget is not installed.
            Assert.False(IsNuGetInstalled, "NuGet is still installed at :".format(NuGetPath));

            var ps = NewPowerShellSession;

            // ask onget for the nuget package provider, bootstrapping if necessary
            DynamicPowershellResult result = ps.GetPackageProvider(Name: "NuGet", ForceBootstrap: true, IsTesting: true);
            Assert.False(result.IsFailing);

            // did we get back one item?
            var items = result.ToArray();
            Assert.Equal(1, items.Length);

            // and is the nuget.exe where we expect it?
            Assert.True(IsNuGetInstalled);
        }
    }
}