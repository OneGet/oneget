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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using Api;
    using OneGet.Utility.Collections;
    using OneGet.Utility.Extensions;
    using Xunit;
    using Xunit.Abstractions;
    using Console = Support.Console;

    public class BasicHostImpl : IHostApi {

        private static object lockObject = new object();
        private int _count;
        private int NextNumber {
            get {
                lock (lockObject) {
                    return _count ++;
                }
            }
        }

        public BasicHostImpl() {
            _callCount = NextNumber;
        }

        public virtual bool IsCanceled {
            get {
                Console.WriteLine("[IsCancelled]");
                return false;
            }
        }

        public virtual string GetMessageString(string messageText, string defaultText) {
            Console.WriteLine("[GetMessageString],<{0}>,<{1}>",messageText,defaultText);
            return null;
        }

        public virtual bool Warning(string messageText) {
            Console.WriteLine("[Warning],<{0}>",messageText);
            return false;
        }

        public virtual bool Error(string id, string category, string targetObjectValue, string messageText) {
            Console.WriteLine("[Error],<{0}>,<{1}>,<{2}>,<{3}>",id, category, targetObjectValue,messageText);
            return false;
        }

        public virtual bool Message(string messageText) {
            Console.WriteLine("[Message],<{0}>", messageText);
            return false;
        }

        public virtual bool Verbose(string messageText) {
            Console.WriteLine("[Verbose],<{0}>", messageText);
            return false;
        }

        public virtual bool Debug(string messageText) {
            Console.WriteLine("[Debug],<{0}>", messageText);
            return false;
        }

        public virtual int StartProgress(int parentActivityId, string messageText) {
            Console.WriteLine("[StartProgress],<{0}>,<{1}>",parentActivityId, messageText);
            return 0;
        }

        public virtual bool Progress(int activityId, int progressPercentage, string messageText) {
            Console.WriteLine("[Progress],<{0}>,<{1}>,<{2}>", activityId, progressPercentage, messageText);
            return false;
        }

        public virtual bool CompleteProgress(int activityId, bool isSuccessful) {
            Console.WriteLine("[CompleteProgress],<{0}>,<{1}>", activityId, isSuccessful);
            return false;
        }

        public virtual IEnumerable<string> OptionKeys {
            get {
                Console.WriteLine("[OptionKeys]");
                return null;
            }
        }

        public virtual IEnumerable<string> GetOptionValues(string key) {
            Console.WriteLine("[GetOptionValues],<{0}>", key);
            return null;
        }

        public virtual IEnumerable<string> Sources {
            get {
                Console.WriteLine("[Sources]");
                return null;
            }
        }

        public virtual string CredentialUsername {
            get {
                Console.WriteLine("[CredentialUsername]");
                return null;
            }
        }

        public virtual SecureString CredentialPassword {
            get {
                Console.WriteLine("[CredentialPassword]");
                return null;
            }
        }

        public virtual bool ShouldBootstrapProvider(string requestor, string providerName, string providerVersion, string providerType, string location, string destination) {
            Console.WriteLine("[ShouldBootstrapProvider],<{0}>,<{1}>,<{2}>,<{3}>,<{4}>,<{5}>", requestor, providerName, providerVersion, providerType, location, destination);
            return false;
        }


        public virtual bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
            Console.WriteLine("[ShouldContinueWithUntrustedPackageSource],<{0}>,<{1}>", package, packageSource);
            return false;
        }

        public virtual bool AskPermission(string permission) {
            Console.WriteLine("[AskPermission],<{0}>", permission);
            return false;
        }

        public virtual bool IsInteractive {
            get {
                Console.WriteLine("[IsInteractive]");
                return false;
            }
        }

        private int _callCount;
        public virtual int CallCount {
            get {
                Console.WriteLine("[CallCount]");
                return _callCount;
            }
        }
    }

    public class BasePMSServiceTests : Tests {
        public BasePMSServiceTests(ITestOutputHelper outputHelper) : base(outputHelper) {
        }

        private static object _lockObject = new Object();
        private static IPackageManagementService _service;

        public static IPackageManagementService PackageManagementService {
            get {
                lock (_lockObject) {
                    if (_service == null) {
                        // set the PSModulePath to just this folder so we don't pick up any other PSModules outside the system ones and the test ones
                        Environment.SetEnvironmentVariable("PSModulePath", Environment.CurrentDirectory);

                        // first time you acess this, it will test the initialization of the service 
                        var svc = PackageManager.Instance;

                        // should not permit null host object during init
                        Assert.Throws<ArgumentNullException>(() => {
                            svc.Initialize(null);
                        });

                        Assert.True(svc.Initialize(new BasicHostImpl()));
                        // if we got this far, the svc is good to go.
                        _service = svc;
                    }
                }

                return _service;
            }
        }
    }

    public class PackageManagementServiceTests : BasePMSServiceTests {
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
                var providers = BasePMSServiceTests.PackageManagementService.PackageProviders.ReEnumerable();
                Assert.NotEmpty(providers);

                // should not have any null elements
                foreach (var p in providers) {
                    Assert.NotNull(p);
                }
            }
        }
    }
}