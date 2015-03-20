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

namespace Microsoft.PackageManagement.Test.Utility.Misc {
    using System.Linq;
    using System.Reflection;
    using PackageManagement.Utility.Platform;
    using Packaging;
    using Support;
    using Xunit;
    using Xunit.Abstractions;

    public class ManifestLoadingTests : Tests {
        public ManifestLoadingTests(ITestOutputHelper output) : base(output) {
        }

        [Fact]
        public void LoadManifests() {
            using (CaptureConsole) {
                var manifests = Manifest.LoadFrom(Assembly.GetExecutingAssembly().Location).Where( Swidtag.IsSwidtag ).ToArray();

                Assert.NotEmpty(manifests);

                foreach (var manifest in manifests) {
                    Console.WriteLine("Manifest Found:\r\n{0}", manifest.ToString());

                    Console.WriteLine(manifest.Name);
                }
            }
        }
    }
}
