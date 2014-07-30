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

namespace Microsoft.OneGet.Test {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    // using MetaProvider.PowerShell;
    using Api;
    using Packaging;
    using PowerShell.OneGet.CmdLets;
    using Providers.Package;
    using Utility.Extensions;
    using Utility.Plugin;
    using Xunit;
    using PackageSource = Packaging.PackageSource;

    public class PackageManagementServiceTest : MarshalByRefObject {

        public class Req {

            public bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
                return true;
            }

            public bool ShouldProcessPackageInstall(string packageName, string version, string source) {
                return true;
            }

            public bool ShouldProcessPackageUninstall(string packageName, string version) {
                return true;
            }

            public bool ShouldContinueAfterPackageInstallFailure(string packageName, string version, string source) {
                return true;
            }

            public bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source) {
                return true;
            }

            public bool ShouldContinueRunningInstallScript(string packageName, string version, string source, string scriptLocation) {
                return true;
            }

            public bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation) {
                return true;
            }

            public bool AskPermission(string permission) {
                return true;
            }

            public bool Warning(string messageText) {
                Console.WriteLine("WARNING: {0}", messageText);
                return true;
            }

            public bool Error(string messageText) {
                Console.WriteLine("ERROR: {0}", messageText);
                return true;
            }

            public bool Message(string messageText) {
                Console.WriteLine("MESSAGE: {0}", messageText);
                return true;
            }

            public bool Verbose(string messageText) {
                Console.WriteLine("VERBOSE: {0}", messageText);
                return true;
            }

            public bool Debug(string messageText) {
                Console.WriteLine("DEBUG: {0}", messageText);
                return true;
            }

            public string GetMessageString(string messageText) {
                return messageText;
            }

            private static int count;
            public int StartProgress(int parentActivityId, string messageText) {
                Console.WriteLine("STARTPROGRESS {0} // {1}", parentActivityId, string.Format(messageText));
                return count++;
            }

            public bool Progress(int activityId, int progressPercentage, string messageText) {
                Console.WriteLine("PROGRESS {0} // {1}% // {2}", activityId, progressPercentage, string.Format(messageText));
                return true;
            }

            public bool CompleteProgress(int activityId, bool isSuccessful) {
                Console.WriteLine("COMPLETEPROGRESS {0}", activityId);
                return isSuccessful;
            }

            public bool IsCancelled() {
                return false;
            }
        }

        public override object InitializeLifetimeService() {
            return null;
        }

       private static object _lock = new object();

        /// <summary>
        ///     The provider can query to see if the operation has been cancelled.
        ///     This provides for a gentle way for the caller to notify the callee that
        ///     they don't want any more results.
        /// </summary>
        /// <returns>returns TRUE if the operation has been cancelled.</returns>
        public bool IsCancelled() {
            return false;
        }

        [Fact]
        public void Load() {
            System.Diagnostics.Debug.WriteLine("=================");
            //System.Diagnostics.Debug.Listeners.Add(new DefaultTraceListener());

            var providers = Service.SelectProviders(null);
            Console.WriteLine("Provider Count {0}",providers.Count());
        }

        [Fact]
        public void GetProviders() {

            var providerNames = Service.ProviderNames.ToArray();
            foreach (var n in providerNames) {
                Console.WriteLine("Package Provider Loaded: {0}",n );
            }

            var expectedProviders = new string[] {
                "NuGet"
            };

            var missing = expectedProviders.Where(each => !providerNames.Contains(each)).ToArray();
            foreach (var m in missing) {
                Console.WriteLine("Missing Provider {0}",m);
            }
            Assert.Empty( missing );
        }

        [Fact]
        public void GetSources() {

        }

        private static IPackageManagementService _service;
        private static PackageProvider _testPsProvider;

        public static IPackageManagementService Service {
            get {
                if (_service == null) {
                    _service = new PackageManagementService().Instance;
                    _service.Initialize(new Req(), false);
                }
                return _service;
            }
        }

        public PackageManagementServiceTest() {
            Assert.NotNull(Service);
        }

        public static PackageProvider TestPSProvider {
            get {
                if (_testPsProvider == null) {
                    _testPsProvider = Service.SelectProviders("TestPSProvider").FirstOrDefault();
                    Assert.NotNull(_testPsProvider);
                }

                return _testPsProvider;
            }
        }

        [Fact]
        public void Provider_IsMethodImplemented() {

            
        }

        private Req Request {
            get {
                return new Req();
            }
        }

        [Fact]
        public void Provider_AddRemovePackageSource() {
            lock (_lock) {
                var sources = TestPSProvider.ResolvePackageSources(Request).ToArray();
                Assert.Equal(3, sources.Count());

                TestPSProvider.AddPackageSource("sampleName", "http://foo/bar/test", false, Request );

                sources = TestPSProvider.ResolvePackageSources(Request).ToArray();

                Assert.Equal(4,sources.Count());

                TestPSProvider.RemovePackageSource("sampleName", Request);

                sources = TestPSProvider.ResolvePackageSources(Request).ToArray();
                Assert.Equal(3, sources.Count());


                TestPSProvider.AddPackageSource("sampleName", "http://foo/bar/test", false, Request);

                sources = TestPSProvider.ResolvePackageSources(Request).ToArray();
                Assert.Equal(4, sources.Count());

                TestPSProvider.RemovePackageSource("http://foo/bar/test", Request);
                sources = TestPSProvider.ResolvePackageSources(Request).ToArray();
                Assert.Equal(3, sources.Count());
            }
        }

      
        [Fact]
        public void Provider_FindPackageByUri() {
            var pkgs = TestPSProvider.FindPackageByUri(new Uri("http://foo/bar/x.testpkg"),0, Request).ToArray();
            Assert.Equal(1, pkgs.Length);

        }

        [Fact]
        public void Provider_FindPackageByFile() {
            var pkgs = TestPSProvider.FindPackageByFile(@"c:\test\x.testpkg", 0, Request).ToArray();
            Assert.Equal(1, pkgs.Length);
        }


        [Fact]
        public void Provider_MultiFind() {
            var id = TestPSProvider.StartFind(Request);
            Assert.NotEqual(0,id);
            var pkgs = TestPSProvider.FindPackage(@"first", null, null, null, id, Request).ToArray();
            pkgs = pkgs.Concat(TestPSProvider.FindPackage(@"second", null, null, null, id, Request)).ToArray();
            pkgs = pkgs.Concat(TestPSProvider.FindPackage(@"third", null, null, null, id, Request)).ToArray();
            pkgs = pkgs.Concat(TestPSProvider.FindPackage(@"fourth", null, null, null, id, Request)).ToArray();
            pkgs = pkgs.Concat(TestPSProvider.CompleteFind(id, Request)).ToArray();
            Assert.Equal(4,pkgs.Length);


            id = TestPSProvider.StartFind(Request);
            Assert.NotEqual(0, id);
            pkgs = TestPSProvider.FindPackageByFile(@"c:\test\a.testpkg", id, Request).ToArray();
            pkgs = pkgs.Concat(TestPSProvider.FindPackageByFile(@"c:\test\b.testpkg", id, Request)).ToArray();
            pkgs = pkgs.Concat(TestPSProvider.FindPackageByFile(@"c:\test\c.testpkg", id, Request)).ToArray();
            pkgs = pkgs.Concat(TestPSProvider.FindPackageByFile(@"c:\test\d.testpkg", id, Request)).ToArray();
            pkgs = pkgs.Concat(TestPSProvider.CompleteFind(id, Request)).ToArray();
            Assert.Equal(4, pkgs.Length);

        }

        [Fact]
        public void Provider_FindPackage() {
            var pkgs = TestPSProvider.FindPackage(@"single", null, null, null, 0, Request).ToArray();
            Assert.Equal(1, pkgs.Length);

            pkgs = TestPSProvider.FindPackage(@"multiple",null,null,null, 0, Request).ToArray();
            Assert.Equal(3, pkgs.Length);
        }

        [Fact]
        public void Provider_FindPackages() {
            var pkgs = TestPSProvider.FindPackages(new string[] { @"multiple", @"single" }, null, null, null, Request).ToArray();
            Assert.Equal(4, pkgs.Length);
        }

        [Fact]
        public void Provider_FindPackagesByUris() {
            var pkgs = TestPSProvider.FindPackagesByUris(new Uri[] { new Uri("http://foo/bar/a.testpkg"), new Uri("http://foo/bar/b.testpkg")}, Request).ToArray();
            Assert.Equal(2, pkgs.Length);
        }

        [Fact]
        public void Provider_FindPackagesByFiles() {
            var pkgs = TestPSProvider.FindPackagesByFiles(new string[] { @"c:\test\a.testpkg", @"c:\test\b.testpkg"}, Request).ToArray();
            Assert.Equal(2, pkgs.Length);
        }

      

        [Fact]
        public void Provider_GetInstalledPackages() {
            lock (_lock) {
                var pkgs = TestPSProvider.GetInstalledPackages(null, Request).ToArray();
                Assert.Equal(3, pkgs.Length);
            }
        }

        [Fact]
        public void Provider_InstallandUninstallPackage() {
            lock (_lock) {
                var pkgs = TestPSProvider.FindPackageByFile(@"c:\test\x.testpkg", 0, Request).ToArray();
                Assert.Equal(1, pkgs.Length);

                SoftwareIdentity installedPkg = null;

                foreach (var pkg in pkgs) {
                    pkgs = TestPSProvider.InstallPackage(pkg, Request).ToArray();
                    Assert.Equal(1, pkgs.Length);
                    installedPkg = pkgs.FirstOrDefault();
                }

                pkgs = TestPSProvider.GetInstalledPackages(null, Request).ToArray();
                Assert.Equal(4, pkgs.Length);

                pkgs = TestPSProvider.UninstallPackage(installedPkg, Request).ToArray();
                Assert.Equal(1, pkgs.Length);

                pkgs = TestPSProvider.GetInstalledPackages(null, Request).ToArray();
                Assert.Equal(3, pkgs.Length);
            }
        }

        [Fact]
        public void Provider_GetDynamicOptions() {
            var options = TestPSProvider.GetDynamicOptions(OptionCategory.Package, Request).ToArray();
            
            foreach (var option in options) {
                Console.WriteLine("Option: {0} {1} {2} {3} {4}", option.Name, option.Category, option.Type, option.IsRequired, option.PossibleValues.JoinWithComma());
            }

            Assert.Equal(3, options.Length );

        }

        [Fact]
        public void Provider_IsValidPackageSource() {

        }

        [Fact]
        public void Provider_IsTrustedPackageSource() {

        }

        [Fact]
        public void Provider_ResolvePackageSources() {
            lock (_lock) {
                var sources = TestPSProvider.ResolvePackageSources(new Req()).ToArray();
                DumpSources(sources);

                Assert.Equal(3, sources.Length);

                sources = TestPSProvider.ResolvePackageSources(DynamicInterfaceExtensions.Extend<IRequest>(new Req(), new {
                    GetSources = new Func<IEnumerable<string>>(() => {
                        return new string[] {
                            "source1"
                        };
                    })
                })).ToArray();

                Assert.Equal(1, sources.Length);

                DumpSources(sources);

                sources = TestPSProvider.ResolvePackageSources(DynamicInterfaceExtensions.Extend<IRequest>(new Req(), new {
                    GetSources = new Func<IEnumerable<string>>(() => {
                        return new string[] {
                            "http://foo/bar", "http://test/test"
                        };
                    })
                })).ToArray();

                Assert.Equal(1, sources.Length);

                DumpSources(sources);

            }
        }

        private static void DumpSources(PackageSource[] sources) {
            foreach (var i in sources) {
                Console.WriteLine("Source: {0} -- {1} -- {2}", i.Name, i.Location, i.IsTrusted);
            }
        }
    }
}