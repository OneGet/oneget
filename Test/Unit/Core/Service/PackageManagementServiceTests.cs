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

namespace Microsoft.OneGet.Test.Core.Service {
    using OneGet.Utility.Collections;
    using Support;
    using Xunit;
    using Xunit.Abstractions;

    public class PackageManagementServiceTests : BasePmsServiceTests {
        public PackageManagementServiceTests(ITestOutputHelper outputHelper) : base(outputHelper) {
        }

        [Fact]
        public void PackageManagerReturnsProviderNames() {
            using (CaptureConsole) {
                var providers = PackageManagementService.ProviderNames.ReEnumerable();
                Assert.NotEmpty(providers);

                foreach (var p in providers) {
                    Console.WriteLine("Provider Loaded: {0}", p);
                }

                // this list should contain our test providers
                Assert.Contains("testPSProvider", providers, IgnoreCase);
                Assert.Contains("TestFileprovider", providers, IgnoreCase);

                // and our core providers
                Assert.Contains("msi", providers, IgnoreCase);
                Assert.Contains("msu", providers, IgnoreCase);
                Assert.Contains("ARP", providers, IgnoreCase);
                Assert.Contains("Bootstrap", providers, IgnoreCase);
            }
        }

        [Fact]
        public void PackageManagerReturnsProviders() {
            using (CaptureConsole) {
                var providers = PackageManagementService.PackageProviders.ReEnumerable();
                Assert.NotEmpty(providers);

                // should not have any null elements
                foreach (var p in providers) {
                    Assert.NotNull(p);
                }
            }
        }
    }
}