﻿// 
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

namespace Microsoft.PackageManagement.Test.Core.Service {
    using System.Linq;
    using Implementation;
    using Support;
    using Xunit;
    using Xunit.Abstractions;

    public class Chained1Tests : BasePmsServiceTests {
        public Chained1Tests(ITestOutputHelper outputHelper)
            : base(outputHelper) {
        }

        private PackageProvider _provider;

        public PackageProvider Provider {
            get {
                if (_provider == null) {
                    var packageProviders = PackageManagementService.SelectProviders("Chained1", new BasicHostImpl()).ToArray();
                    Assert.NotNull(packageProviders);
                    Assert.Equal(1, packageProviders.Length);
                    _provider = packageProviders[0];
                }
                Assert.NotNull(_provider);
                return _provider;
            }
        }

        [Fact]
        public void FindPackageTest() {
            using (CaptureConsole) {
                var packages = Provider.FindPackage("zlib", null, null, null, new BasicHostImpl()).ToArray();

                Assert.Equal(1, packages.Length);
                var pkg1 = packages[0];

                foreach (var pkg in packages) {
                    Console.WriteLine("PKG : {0}", pkg.SwidTagText);
                }

                Assert.Equal(1, pkg1.Dependencies.Count());

                packages = PackageManagementService.FindPackageByCanonicalId(pkg1.Dependencies.FirstOrDefault(), new BasicHostImpl()).ToArray();
                Assert.Equal(1, packages.Length);

                var pkg2 = packages[0];

                foreach (var pkg in packages) {
                    Console.WriteLine("PKG : {0}", pkg.SwidTagText);
                }
            }
        }

#if grab_what_you_need
        [Fact]
        public void TestInitialized() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void TestFeatures() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void DynamicOptionsTest() {
            using (CaptureConsole) {

            }
        }

       [Fact]
        public void ResolvePackageSourceTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void AddPackageSourceTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void RemovePackageSourceTest() {
            using (CaptureConsole) {

            }
        }


        [Fact]
        public void FindPackageByUriTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void FindPackageByFileTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void GetInstalledPackagesTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void InstallPackageTest() {
            using (CaptureConsole) {

            }
        }


        [Fact]
        public void UninstallPackageTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void IsSupportedFile() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void IsSupportedSchemeTest() {
            using (CaptureConsole) {

            }
        }

#endif
    }
}