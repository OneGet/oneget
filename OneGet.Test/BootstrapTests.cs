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
    using System.Linq;
    using Implementation;
    using Xunit;

    public class BootstrapTests {
        private static IPackageManagementService _service;
        private static PackageProvider _bootstrap;

        public static IPackageManagementService Service {
            get {
                if (_service == null) {
                    _service = new PackageManager().Instance;
                    _service.Initialize(new Req());
                }
                return _service;
            }
        }
        private static Req Request {
            get {
                return new Req();
            }
        }

        [Fact]
        public void GetProviderList() {
            var providers = Bootstrap.FindPackage(null, null, null, null, 0, Request).ToArray();
            Assert.NotEmpty( providers );
        }

        public BootstrapTests() {
            Assert.NotNull(Service);
        }

        public static PackageProvider Bootstrap {
            get {
                if (_bootstrap == null) {
                    _bootstrap = Service.SelectProviders("bootstrap",Request).FirstOrDefault();
                    Assert.NotNull(_bootstrap);
                }

                return _bootstrap;
            }
        }
    }
}