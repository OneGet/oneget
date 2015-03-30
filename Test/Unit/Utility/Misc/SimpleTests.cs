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
    using Packaging;
    using Support;
    using Xunit;
    using Xunit.Abstractions;

    public class SimpleTests : Tests {
        public SimpleTests(ITestOutputHelper output)
            : base(output) {
        }

        [Fact]
        public void VersionTests() {
            using (CaptureConsole) {
                Console.WriteLine("Version Tests");

                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("alphanumeric", "1.0", "1.0"));
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("alphanumeric", "A", "B") < 0);
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("alphanumeric", "B", "A") > 0);

                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("decimal", "1", "1.0"));
                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("decimal", "0", "0.0"));
                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("decimal", ".0", "0.0"));

                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("decimal", "1.0", "1.0"));
                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("decimal", "1.20", "1.2"));
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("decimal", "1.1", "1.2") < 0);
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("decimal", "1.2", "1.1") > 0);

                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("multipartnumeric", "1.0", "1.0"));
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("multipartnumeric", "1.1", "1.2") < 0);
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("multipartnumeric", "1.2", "1.1") > 0);
                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("multipartnumeric", "1.2.0", "1.2.0"));
                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("multipartnumeric", "1.0.0.0.0.0.0", "1"));
                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("multipartnumeric", "1.2", "1.2.0"));
                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("multipartnumeric", "1.2.0", "1.2"));

                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("multipartnumeric+suffix", "1.0", "1.0"));
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("multipartnumeric+suffix", "1.1", "1.2") < 0);
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("multipartnumeric+suffix", "1.2", "1.1") > 0);

                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("multipartnumeric+suffix", "1.1-beta2", "1.1") > 0);
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("multipartnumeric+suffix", "1.0-beta2", "1.1") < 0);
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("multipartnumeric+suffix", "1.2-beta2", "1.1") > 0);

                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("semver", "1.0", "1.0"));
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("semver", "1.1", "1.2") < 0);
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("semver", "1.2", "1.1") > 0);

                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("unknown", "1.0", "1.0"));
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("unknown", "1.1", "1.2") < 0);
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("unknown", "1.2", "1.1") > 0);

                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("unknown", ".1", "1.2") < 0);
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("unknown", "1.0", ".1") > 0);
                Assert.True(SoftwareIdentityVersionComparer.CompareVersions("unknown", "0", "1") < 0);
                Assert.Equal(0, SoftwareIdentityVersionComparer.CompareVersions("unknown", "1", "1.0"));
            }
        }
    }
}