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
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Xml;
    using Microsoft.OneGet.Utility.Extensions;
    using Microsoft.OneGet.Utility.Platform;
    using Microsoft.OneGet.Utility.PowerShell;
    using Xunit;

    public class TestDynamicPowerShell {
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
                Assert.False(string.IsNullOrEmpty(userBase));

                nugets = NugetsInPath(Path.Combine(userBase, "oneget", "ProviderAssemblies"));
                first = nugets.FirstOrDefault();
                if (first != null) {
                    return first;
                }

                return null;
            }
        }

        [Fact]
        public void TestGetPackageProvider() {
            var PS = NewPowerShellSession;

            DynamicPowershellResult result = PS.GetPackageProvider(IsTesting:true);
            var i = result.ToArray();
            PrintItems(i);
        }

        [Fact]
        public void TestFindPackage() {
            var PS = NewPowerShellSession;

            DynamicPowershellResult result = PS.FindPackage(IsTesting:true);
            var i = result.ToArray();
            PrintItems(i);
        }

        [Fact]
        public void TestFindPackageValidName() {
            var PS = NewPowerShellSession;

            DynamicPowershellResult result = PS.FindPackage(Name: "Nuget", IsTesting: true);
            var i = result.ToArray();
            PrintItems(i);
        }

        [Fact]
        public void TestFindPackageInvalidName() {
            var PS = NewPowerShellSession;

            DynamicPowershellResult result = PS.FindPackage(Name: "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER", IsTesting:true);
            Assert.True(result.IsFailing);
        }

        [Fact]
        public void TestFindPackageValidProvider() {
            var PS = NewPowerShellSession;

            DynamicPowershellResult result = PS.FindPackage(ProviderName: "Nuget", IsTesting: true); 
            var i = result.ToArray();
            PrintItems(i);
        }

        private bool IsNuGetInstalled {
            get {
                return NuGetPath != null;
            }
        }

        private dynamic NewPowerShellSession {
            get {
                dynamic p = new DynamicPowershell();
                DynamicPowershellResult result = p.ImportModule(".\\oneget.psd1");
                Assert.False(result.IsFailing, "unable to import '.\\oneget.psd1  (PWD:'{0}')".format(Environment.CurrentDirectory));
                return p;
            }
        }

        [Fact]
        //TODO
        //Has warning, not error, impossible to check at the moment. [BUG]
        public void TestFindPackageInvalidProvider() {
            var PS = NewPowerShellSession;

            DynamicPowershellResult result = PS.FindPackage(ProviderName: "1THIS_3SHOULD_5NEVER_7BE_9FOUND_11EVER", IsTesting: true);
            Assert.True(result.Success); 
           
        }

        [Fact]
        public void TestFindPackageMaxVersion() {
            var PS = NewPowerShellSession;

            DynamicPowershellResult maxResult = PS.FindPackage(Name: "Nuget", MaximumVersion: "1.5", IsTesting: true);
            Assert.False(maxResult.IsFailing);
        }

        [Fact]
        public void TestFindPackageNegMaxVersion() {
            var PS = NewPowerShellSession;

            DynamicPowershellResult maxResult = PS.FindPackage(Name: "Nuget", MaximumVersion: "-1.5", IsTesting: true);
            Assert.True(maxResult.IsFailing);
        }

        [Fact]
        public void TestFindPackageMinVersion() {
            var PS = NewPowerShellSession;

            DynamicPowershellResult minResult = PS.FindPackage(Name: "Nuget", Version: "1.5", IsTesting: true);
            Assert.False(minResult.IsFailing);
        }

        [Fact]
        public void TestFindPackageNegMinVersion()
        {
            var PS = NewPowerShellSession;

            DynamicPowershellResult minResult = PS.FindPackage(Name: "Nuget", Version: "-1.5", IsTesting: true);
            Assert.True(minResult.IsFailing);
        }

        [Fact]
        public void TestFindPackageReqVersion() {
            var PS = NewPowerShellSession;

            DynamicPowershellResult reqResult = PS.FindPackage(Name: "Nuget", RequiredVersion: "1.5", IsTesting: true);
            Assert.False(reqResult.IsFailing);
        }

        [Fact]
        public void TestFindPackageNegReqVersion()
        {
            var PS = NewPowerShellSession;

            DynamicPowershellResult reqResult = PS.FindPackage(Name: "Nuget", RequiredVersion: "-1.5", IsTesting: true);
            Assert.True(reqResult.IsFailing);
        }

        [Fact]
        public void TestFindPackageAllVersion() {
            var PS = NewPowerShellSession;

            DynamicPowershellResult allResult = PS.FindPackage(Name: "Nuget", AllVersions: true, IsTesting: true);
            Assert.False(allResult.IsFailing);
        }

        [Fact]
        public void TestFindPackageRequiredVersionFail() {
            var PS = NewPowerShellSession;
            
            DynamicPowershellResult result = PS.FindPackage(Name: "Nuget", Version: "1.0", MaximumVersion: "1.5", RequiredVersion: "2.0", IsTesting: true);
            Assert.True(result.IsFailing);
        }

        [Fact]
        public void TestFindPackageAllVersionFail() {
            var PS = NewPowerShellSession;
            
            DynamicPowershellResult result = PS.FindPackage(Name: "Nuget", Version: "1.0", MaximumVersion: "1.5", AllVersion: "2.0", IsTesting: true);
            Assert.True(result.IsFailing);
        }
        [Fact]
        public void TestFindPackageMismatchedVersions() {
            var PS = NewPowerShellSession;

            DynamicPowershellResult result = PS.FindPackage(Name: "Nuget", Version: "1.5", MaximumVersion: "1.0", IsTesting: true);
            Assert.True(result.IsFailing);
        }

        [Fact]
        public void TestSavePackage() {
            var PS = NewPowerShellSession;
            DeleteAdeptNuget();
            DynamicPowershellResult result = PS.SavePackage(Name: "Adept.NugetRunner", Destination: "c:\\temp", IsTesting: true);
            Assert.True(File.Exists("C:\\temp\\Adept.NuGetRunner.1.0.0.2.nupkg"));
            DeleteAdeptNuget();
        }

        [Fact]
        public void TestSavePackageMinVersion()
        {
            var PS = NewPowerShellSession;
            DeleteAdeptNuget();
            DynamicPowershellResult result = PS.SavePackage(Name: "Adept.NugetRunner", Destination: "c:\\temp", Version: "1.0", IsTesting: true);
            Assert.True(File.Exists("C:\\temp\\Adept.NuGetRunner.1.0.0.2.nupkg"));
            DeleteAdeptNuget();
        }

        [Fact]
        public void TestSavePackageMaxVersion()
        {
            var PS = NewPowerShellSession;
            DeleteAdeptNuget();
            DynamicPowershellResult result = PS.SavePackage(Name: "Adept.NugetRunner", Destination: "c:\\temp", MaximumVersion: "1.0", IsTesting: true);
            Assert.True(File.Exists("C:\\temp\\Adept.NuGetRunner.1.0.nupkg"));
            DeleteAdeptNuget();
        }

        [Fact]
        public void TestSavePackageReqVersion()
        {
            var PS = NewPowerShellSession;
            DeleteAdeptNuget();
            DynamicPowershellResult result = PS.SavePackage(Name: "Adept.NugetRunner", Destination: "c:\\temp", RequiredVersion: "1.0", IsTesting: true);
            Assert.True(File.Exists("C:\\temp\\Adept.NuGetRunner.1.0.nupkg"));
            DeleteAdeptNuget();
        }

        [Fact]
        public void TestSavePackageMismatchedVersions()
        {
            var PS = NewPowerShellSession;
            DeleteAdeptNuget();
            DynamicPowershellResult result = PS.SavePackage(Name: "Adept.NugetRunner", Destination: "c:\\temp", MaximumVersion: "1.0", Version: "1.0.0.2", IsTesting: true);
            Assert.True(result.IsFailing);
        }

        [Fact]
        public void TestGetPackageProviderName() {
            var PS = NewPowerShellSession;

            DynamicPowershellResult result = PS.GetPackageProvider(Name: "NuGet", ForceBootstrap: true, IsTesting: true);
            Assert.False(result.IsFailing);

            var items = result.ToArray();
            Assert.Equal(1, items.Length);
        }


        private void DeleteAdeptNuget() {
            const string folderPath = @"C:\\temp";
            const string filesToDelete = "*NuGet*";
            string[] fileList = Directory.GetFiles(folderPath, filesToDelete);
            foreach (string file in fileList)
            {
                File.Delete(file);
            }
        }
        private bool IsDllOrExe(string path) {
            return path.ToLower().EndsWith(".exe") || path.ToLower().EndsWith(".dll");
        }

        private void PrintItems(dynamic items) {
            foreach (var i in items)
            {
                Console.WriteLine(i.Name);
            }
        }

        private IEnumerable<string> FilenameContains(IEnumerable<string> paths, string value) {
            foreach (var item in paths) {
                if (item.IndexOf(value, StringComparison.OrdinalIgnoreCase) > -1) {
                    yield return item;
                }
            }
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

            var PS = NewPowerShellSession;

            // ask onget for the nuget package provider, bootstrapping if necessary
            DynamicPowershellResult result = PS.GetPackageProvider(Name: "NuGet", ForceBootstrap: true, IsTesting: true);
            Assert.False(result.IsFailing);

            // did we get back one item?
            var items = result.ToArray();
            Assert.Equal(1, items.Length);

            // and is the nuget.exe where we expect it?
            Assert.True(IsNuGetInstalled);
        }
    }
}