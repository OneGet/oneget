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

namespace Microsoft.PackageManagement.Test.Core.Host {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Security;
    using Api;
    using Xunit.Abstractions;

    public class HostImpl : IHostApi {
        public bool IsCanceled {
            [SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new NotImplementedException();
            }
        }

        public string GetMessageString(string messageText, string defaultText) {
            throw new NotImplementedException();
        }

        public bool Warning(string messageText) {
            throw new NotImplementedException();
        }

        public bool Error(string id, string category, string targetObjectValue, string messageText) {
            throw new NotImplementedException();
        }

        public bool Message(string messageText) {
            throw new NotImplementedException();
        }

        public bool Verbose(string messageText) {
            throw new NotImplementedException();
        }

        public bool Debug(string messageText) {
            throw new NotImplementedException();
        }

        public int StartProgress(int parentActivityId, string messageText) {
            throw new NotImplementedException();
        }

        public bool Progress(int activityId, int progressPercentage, string messageText) {
            throw new NotImplementedException();
        }

        public bool CompleteProgress(int activityId, bool isSuccessful) {
            throw new NotImplementedException();
        }

        public IEnumerable<string> OptionKeys {
            [SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<string> GetOptionValues(string key) {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Sources {
            [SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new NotImplementedException();
            }
        }

        public string CredentialUsername {
            [SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new NotImplementedException();
            }
        }

        public SecureString CredentialPassword {
            [SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new NotImplementedException();
            }
        }

        public bool ShouldBootstrapProvider(string requestor, string providerName, string providerVersion, string providerType, string location, string destination) {
            throw new NotImplementedException();
        }

        public bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
            throw new NotImplementedException();
        }

        public bool AskPermission(string permission) {
            throw new NotImplementedException();
        }

        public bool IsInteractive {
            [SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new NotImplementedException();
            }
        }

        public int CallCount {
            [SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new NotImplementedException();
            }
        }
    }

    public class HostTests : Tests {
        public HostTests(ITestOutputHelper outputHelper) : base(outputHelper) {
        }
    }
}