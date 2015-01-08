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

namespace Microsoft.OneGet.Implementation {
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Threading;
    using System.Threading.Tasks;
    using Api;
    using Resources;
    using Utility.Async;
    using Utility.Extensions;

    public abstract class RequestObject : AsyncAction , IRequest, IHostApi {
        private static int _c;
        protected Action<RequestObject> _action;
        private IHostApi _hostApi;
        protected Task _invocationTask;
        protected readonly ProviderBase Provider;

        internal RequestObject(ProviderBase provider, IHostApi hostApi, Action<RequestObject> action) {
            // construct request object
            _hostApi = hostApi;
            Provider = provider;
            _action = action;
        }

        internal RequestObject(ProviderBase provider, IHostApi hostApi) {
            // construct request object
            _hostApi = hostApi;
            Provider = provider;
        }

        private bool CanCallHost {
            get {
                if (IsCompleted || IsAborted) {
                    return false;
                }
                Activity();
                return _hostApi != null;
            }
        }

        protected void InvokeImpl() {
            _invocationTask = Task.Factory.StartNew(() => {
                _invocationThread = Thread.CurrentThread;
                _invocationThread.Name = Provider.ProviderName + ":" + _c++;
                try {
                    _action(this);
                } catch (ThreadAbortException) {
#if DEEP_DEBUG
                    Console.WriteLine("Thread Aborted for {0} : {1}", _invocationThread.Name, DateTime.Now.Subtract(_callStart).TotalSeconds);
#endif
                    Thread.ResetAbort();
                } catch (Exception e) {
                    e.Dump();
                }
            }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ContinueWith(antecedent => Complete());

            // start thread, call function
            StartCall();
        }

        #region HostApi Wrapper
        public string DropMsgPrefix(string messageText) {
            if (string.IsNullOrWhiteSpace(messageText)) {
                return messageText;
            }
            return messageText.StartsWith("MSG:", StringComparison.OrdinalIgnoreCase) ? messageText.Substring(4) : messageText;
        }

        public string GetMessageString(string messageText, string defaultText) {
            if (CanCallHost) {
                if (string.IsNullOrWhiteSpace(defaultText) || defaultText.StartsWith("MSG:", StringComparison.OrdinalIgnoreCase)) {
                    defaultText = Messages.ResourceManager.GetString(DropMsgPrefix(messageText));
                }

                return _hostApi.GetMessageString(messageText, defaultText);
            }
            return null;
        }

        public bool Warning(string messageText) {
            if (CanCallHost) {
                return _hostApi.Warning(messageText);
            }
            return true;
        }

        public bool Error(string id, string category, string targetObjectValue, string messageText) {
            if (CanCallHost) {
                return _hostApi.Error(id, category, targetObjectValue, messageText);
            }
            return true;
        }

        public bool Message(string messageText) {
            if (CanCallHost) {
                return _hostApi.Message(messageText);
            }
            return true;
        }

        public bool Verbose(string messageText) {
            if (CanCallHost) {
                return _hostApi.Verbose(messageText);
            }
            return true;
        }

        public bool Debug(string messageText) {
            if (CanCallHost) {
                return _hostApi.Debug(messageText);
            }
            return true;
        }

        public int StartProgress(int parentActivityId, string messageText) {
            if (CanCallHost) {
                return _hostApi.StartProgress(parentActivityId, messageText);
            }
            return 0;
        }

        public bool Progress(int activityId, int progressPercentage, string messageText) {
            if (CanCallHost) {
                return _hostApi.Progress(activityId, progressPercentage, messageText);
            }
            return true;
        }

        public bool CompleteProgress(int activityId, bool isSuccessful) {
            if (CanCallHost) {
                return _hostApi.CompleteProgress(activityId, isSuccessful);
            }
            return true;
        }

        public IEnumerable<string> OptionKeys {
            get {
                if (CanCallHost) {
                    return _hostApi.OptionKeys;
                }
                return new string[0];
            }
        }

        public IEnumerable<string> GetOptionValues(string key) {
            if (CanCallHost) {
                return _hostApi.GetOptionValues(key);
            }
            return new string[0];
        }

        public IEnumerable<string> Sources {
            get {
                if (CanCallHost) {
                    return _hostApi.Sources;
                }
                return new string[0];
            }
        }

        public string CredentialUsername {
            get {
                return CanCallHost ? _hostApi.CredentialUsername : null;
            }
        }

        public SecureString CredentialPassword {
            get {
                return CanCallHost ? _hostApi.CredentialPassword : null;
            }
        }

        public bool ShouldBootstrapProvider(string requestor, string providerName, string providerVersion, string providerType, string location, string destination) {
            if (CanCallHost) {
                return _hostApi.ShouldBootstrapProvider(requestor, providerName, providerVersion, providerType, location, destination);
            }
            return false;
        }

        public bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
            if (CanCallHost) {
                return _hostApi.ShouldContinueWithUntrustedPackageSource(package, packageSource);
            }
            return false;
        }

        public bool AskPermission(string permission) {
            if (CanCallHost) {
                return _hostApi.AskPermission(permission);
            }
            return false;
        }

        public bool IsInteractive {
            get {
                if (CanCallHost) {
                    return _hostApi.IsInteractive;
                }
                return false;
            }
        }

        public int CallCount {
            get {
                if (CanCallHost) {
                    return _hostApi.CallCount;
                }
                return 0;
            }
        }

        #endregion

        #region CoreApi implementation

        public IPackageManagementService PackageManagementService {
            get {
                Activity();
                return PackageManager._instance;
            }
        }

        public IProviderServices ProviderServices {
            get {
                Activity();
                return ProviderServicesImpl.Instance;
            }
        }

    

        #endregion

        #region response api implementation

        public virtual bool YieldSoftwareIdentity(string fastPath, string name, string version, string versionScheme, string summary, string source, string searchKey, string fullPath, string packageFileName) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        public virtual bool YieldSoftwareMetadata(string parentFastPath, string name, string value) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        public virtual bool YieldEntity(string parentFastPath, string name, string regid, string role, string thumbprint) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        public virtual bool YieldLink(string parentFastPath, string referenceUri, string relationship, string mediaType, string ownership, string use, string appliesToMedia, string artifact) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        public virtual bool YieldPackageSource(string name, string location, bool isTrusted, bool isRegistered, bool isValidated) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        public virtual bool YieldDynamicOption(string name, string expectedType, bool isRequired) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        public virtual bool YieldKeyValuePair(string key, string value) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        public virtual bool YieldValue(string value) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        #endregion

       
    }
}