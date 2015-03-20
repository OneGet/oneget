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

namespace Microsoft.PackageManagement.Test.Core.Service {
    using System.Collections.Generic;
    using System.Linq;
    using Api;
    using Implementation;
    using PackageManagement.Utility.Plugin;
    using Support;
    using Xunit;
    using Xunit.Abstractions;

    public class UsingOneProvider : BasePmsServiceTests {
        private BasicHostImpl _lastCall;
        private PackageProvider _provider;
        protected readonly string _providerName;

        public UsingOneProvider(ITestOutputHelper outputHelper, string providerName) : base(outputHelper) {
            _providerName = providerName;
        }

        protected PackageProvider Provider {
            get {
                if (_provider == null) {
                    var packageProviders = PackageManagementService.SelectProviders(_providerName, Host()).ToArray();
                    AssertNoErrors();

                    Assert.NotNull(packageProviders);
                    Assert.Equal(1, packageProviders.Length);
                    _provider = packageProviders[0];

                    Assert.Equal(_providerName, _provider.Name);
                    Assert.Equal(_providerName, _provider.ProviderName);
                }
                Assert.NotNull(_provider);
                return _provider;
            }
        }

        public int ErrorCount {
            get {
                return _lastCall.Errors.Count();
            }
        }

        public int WarningCount {
            get {
                return _lastCall.Warnings.Count();
            }
        }

        protected void AssertNoErrors() {
            Assert.Equal(0, ErrorCount);
            if (WarningCount > 0) {
                Console.WriteLine("*******************\r\nWARNINGS: {0}*******************\r\n", WarningCount);
            }
        }

        /// <summary>
        ///     a means to trivially override one or more fns in the Host object.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected IHostApi Host(params object[] args) {
            _lastCall = new BasicHostImpl();
            if (args.Length == 0) {
                return _lastCall;
            }

            return new object[] {
                args,
                // everything else just pull from the basic impl
                _lastCall
            }.As<IHostApi>();
        }
    }
}
