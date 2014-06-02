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
    using Core.Dynamic;
    using Xunit;

    public class PackageManagementServiceTest : MarshalByRefObject {

        public class Req {
            public bool Warning(string message) {
                Console.WriteLine("WARNING: {0}", message);
                return true;
            }

            public bool Error(string message) {
                Console.WriteLine("ERROR: {0}", string.Format(message));
                return true;
            }

            public bool Message(string message) {
                Console.WriteLine("MESSAGE: {0}", string.Format(message));
                return true;
            }

            public bool Verbose(string message) {
                Console.WriteLine("VERBOSE: {0}", string.Format(message));
                return true;
            }

            public bool Debug(string message) {
                Console.WriteLine("DEBUG: {0}", string.Format(message));
                return true;
            }

            public string GetMessageString(string message) {
                return message;
            }

            public bool ExceptionThrown(string exceptionType, string message, string stacktrace) {
                Console.WriteLine("\r\n\r\n==================================================================================");
                Console.WriteLine("{0}//{1}/{2}\r\n{3}", AppDomain.CurrentDomain.FriendlyName, exceptionType, message, stacktrace);
                Console.WriteLine("==================================================================================\r\n\r\n");
                return true;
            }

            private static int count;
            public int StartProgress(int parentActivityId, string message) {
                Console.WriteLine("STARTPROGRESS {0} // {1}", parentActivityId, string.Format(message));
                return count++;
            }

            public bool Progress(int activityId, int progress, string message) {
                Console.WriteLine("PROGRESS {0} // {1}% // {2}", activityId, progress, string.Format(message));
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
                "Chocolatey"
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
        private static Core.Providers.Package.PackageProvider _testPsProvider;

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

        public static Core.Providers.Package.PackageProvider TestPSProvider {
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

        [Fact]
        public void Provider_AddPackageSource() {

        }

        [Fact]
        public void Provider_RemovePackageSource() {

        }

        [Fact]
        public void Provider_FindPackageByUri() {

        }

        [Fact]
        public void Provider_FindPackageByFile() {

        }

        [Fact]
        public void Provider_StartFind() {

        }

        [Fact]
        public void Provider_CompleteFind() {

        }

        [Fact]
        public void Provider_FindPackages() {

        }

        [Fact]
        public void Provider_FindPackagesByUris() {

        }

        [Fact]
        public void Provider_FindPackagesByFiles() {

        }

        [Fact]
        public void Provider_FindPackage() {

        }

        [Fact]
        public void Provider_GetInstalledPackages() {

        }

        [Fact]
        public void Provider_InstallPackage() {

        }

        [Fact]
        public void Provider_UninstallPackage() {

        }

        [Fact]
        public void Provider_GetOptionDefinitons() {

        }

        [Fact]
        public void Provider_IsValidPackageSource() {

        }

        [Fact]
        public void Provider_IsTrustedPackageSource() {

        }

        [Fact]
        public void Provider_GetPackageSources() {

            var sources = TestPSProvider.GetPackageSources(new Req()).ToArray();
            foreach (var i in sources) {
                Console.WriteLine("Source: {0} -- {1} -- {2}", i.Name, i.Location , i.IsTrusted);
            }

            Assert.Equal(3, sources.Length);

            sources = TestPSProvider.GetPackageSources(new Req().Extend<IRequest>( new {
                GetSpecifiedPackageSources = new Func<IEnumerable<string>>(() => {
                    return new string[] {
                        "source1"
                    };
                })
            })).ToArray();

            Assert.Equal(1, sources.Length);

            foreach (var i in sources) {
                Console.WriteLine("Source: {0} -- {1} -- {2}", i.Name, i.Location, i.IsTrusted);
            }

            sources = TestPSProvider.GetPackageSources(new Req().Extend<IRequest>(new {
                GetSpecifiedPackageSources = new Func<IEnumerable<string>>(() => {
                    return new string[] {
                        "http://foo/bar", "http://test/test"
                    };
                })
            })).ToArray();

            Assert.Equal(1, sources.Length);

            foreach (var i in sources) {
                Console.WriteLine("Source: {0} -- {1} -- {2}", i.Name, i.Location, i.IsTrusted);
            }

        }
    }
}