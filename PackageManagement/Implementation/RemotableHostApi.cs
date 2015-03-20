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

namespace Microsoft.PackageManagement.Implementation {
    using System;
    using System.Collections.Generic;
    using System.Security;
    using Api;

    public class RemotableHostApi: IHostApi {
        private IHostApi _hostApi;

        internal RemotableHostApi(IHostApi host) {
            _hostApi = host;
        }

        public bool IsCanceled {
            get {
                return _hostApi.IsCanceled;
            }
        }

        public string GetMessageString(string messageText, string defaultText) {
            return _hostApi.GetMessageString(messageText,defaultText);
        }

        public bool Warning(string messageText) {
            return _hostApi.Warning(messageText);
        }

        public bool Error(string id, string category, string targetObjectValue, string messageText) {
            return _hostApi.Error(id, category, targetObjectValue, messageText);
        }

        public bool Message(string messageText) {
            return _hostApi.Message(messageText);
        }

        public bool Verbose(string messageText) {
            return _hostApi.Verbose(messageText);
        }

        public bool Debug(string messageText) {
            return _hostApi.Debug(messageText);
        }

        public int StartProgress(int parentActivityId, string messageText) {
            return _hostApi.StartProgress(parentActivityId, messageText);
        }

        public bool Progress(int activityId, int progressPercentage, string messageText) {
            return _hostApi.Progress(activityId, progressPercentage, messageText);
        }

        public bool CompleteProgress(int activityId, bool isSuccessful) {
            return _hostApi.CompleteProgress(activityId, isSuccessful);
        }

        public IEnumerable<string> OptionKeys {
            get {
                return _hostApi.OptionKeys;
            }
        }

        public IEnumerable<string> GetOptionValues(string key) {
            return _hostApi.GetOptionValues(key);
        }

        public IEnumerable<string> Sources {
            get {
                return _hostApi.Sources;
            }
        }

        public string CredentialUsername {
            get {
                return _hostApi.CredentialUsername;
            }
        }

        public SecureString CredentialPassword {
            get {
                return _hostApi.CredentialPassword;
            }
        }

        public bool ShouldBootstrapProvider(string requestor, string providerName, string providerVersion, string providerType, string location, string destination) {
            return _hostApi.ShouldBootstrapProvider(requestor, providerName, providerVersion, providerType, location, destination);
        }

        public bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
            return _hostApi.ShouldContinueWithUntrustedPackageSource(package, packageSource);
        }

        public bool AskPermission(string permission) {
            return _hostApi.AskPermission(permission);
        }

        public bool IsInteractive {
            get {
                return _hostApi.IsInteractive;
            }
        }

        public int CallCount {
            get {
                return _hostApi.CallCount;
            }
        }

    }
}
