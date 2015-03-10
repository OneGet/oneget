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
    using System;
    using System.IO;
    using OneGet.Utility.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class BasePmsServiceTests : Tests {
        private static readonly object _lockObject = new Object();
        private static IPackageManagementService _service;

        public BasePmsServiceTests(ITestOutputHelper outputHelper) : base(outputHelper) {
        }

        private static void EmptyFolder(string path) {
            try {
                if (Directory.Exists(path)) {
                    var files = Directory.EnumerateFiles(path);
                    foreach (var file in files) {
                        try {
                            file.TryHardToDelete();
                        } catch {

                        }
                    }
                }
            } catch {
            }
        }

        public static IPackageManagementService PackageManagementService {
            get {
                lock (_lockObject) {
                    if (_service == null) {
                        // we use this opportunity to make sure that there are no dynamic providers
                        // in the dynamic paths, so we can test for bootstrapping known providers

                        var appdata = Environment.GetEnvironmentVariable("APPDATA");
                        var localappdata = Environment.GetEnvironmentVariable("LOCALAPPDATA");

                        EmptyFolder(Path.Combine(appdata, "OneGet", "ProviderAssemblies"));
                        EmptyFolder(Path.Combine(localappdata, "OneGet", "ProviderAssemblies"));


                        // set the PSModulePath to just this folder so we don't pick up any other PSModules outside the system ones and the test ones
                        Environment.SetEnvironmentVariable("PSModulePath", Environment.CurrentDirectory);

                        // first time you acess this, it will test the initialization of the service 
                        var svc = PackageManager.Instance;

                        // should not permit null host object during init
                        Assert.Throws<ArgumentNullException>(() => {svc.Initialize(null);});

                        Assert.True(svc.Initialize(new BasicHostImpl()));
                        // if we got this far, the svc is good to go.

                        _service = svc;

                        // now, get nuget installed.
                        Assert.True( svc.RequirePackageProvider("testing", "nuget", "2.8.4.47", new BasicHostImpl()));

                    }
                }

                return _service;
            }
        }
    }
}