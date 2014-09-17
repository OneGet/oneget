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
    using System.Globalization;
    using System.Linq;
    using Api;

    public abstract class Request : IRequest, IDisposable {
        public abstract void Dispose();
        public abstract string GetMessageString(string messageText);
        public abstract bool Warning(string messageText);
        public abstract bool Error(string id, string category, string targetObjectValue, string messageText);
        public abstract bool Message(string messageText);
        public abstract bool Verbose(string messageText);
        public abstract bool Debug(string messageText);
        public abstract int StartProgress(int parentActivityId, string messageText);
        public abstract bool Progress(int activityId, int progressPercentage, string messageText);
        public abstract bool CompleteProgress(int activityId, bool isSuccessful);
        public abstract IEnumerable<string> GetOptionValues(string category, string key);
        public abstract IEnumerable<string> GetOptionKeys(int category);
        public abstract IEnumerable<string> GetOptionValues(int category, string key);
        public abstract IEnumerable<string> GetSources();
        public abstract string GetCredentialUsername();
        public abstract string GetCredentialPassword();
        public abstract bool ShouldBootstrapProvider(string requestor, string providerName, string providerVersion, string providerType, string location, string destination);
        public abstract bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource);
        public abstract bool ShouldProcessPackageInstall(string packageName, string version, string source);
        public abstract bool ShouldProcessPackageUninstall(string packageName, string version);
        public abstract bool ShouldContinueAfterPackageInstallFailure(string packageName, string version, string source);
        public abstract bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source);
        public abstract bool ShouldContinueRunningInstallScript(string packageName, string version, string source, string scriptLocation);
        public abstract bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation);
        public abstract bool AskPermission(string permission);
        public abstract bool IsInteractive();
        public abstract int CallCount();
        public abstract bool IsCancelled();
        public abstract object GetPackageManagementService(object requestImpl);
        public abstract Type GetIRequestInterface();
        public abstract int CoreVersion();
        public abstract bool NotifyBeforePackageInstall(string packageName, string version, string source, string destination);
        public abstract bool NotifyPackageInstalled(string packageName, string version, string source, string destination);
        public abstract bool NotifyBeforePackageUninstall(string packageName, string version, string source, string destination);
        public abstract bool NotifyPackageUninstalled(string packageName, string version, string source, string destination);
        public abstract string GetCanonicalPackageId(string providerName, string packageName, string version);
        public abstract string ParseProviderName(string canonicalPackageId);
        public abstract string ParsePackageName(string canonicalPackageId);
        public abstract string ParsePackageVersion(string canonicalPackageId);
        public abstract bool OkToContinue();
        public abstract bool YieldSoftwareIdentity(string fastPath, string name, string version, string versionScheme, string summary, string source, string searchKey, string fullPath, string packageFileName);
        public abstract bool YieldSoftwareMetadata(string parentFastPath, string name, string value);
        public abstract bool YieldEntity(string parentFastPath, string name, string regid, string role, string thumbprint);
        public abstract bool YieldLink(string parentFastPath, string referenceUri, string relationship, string mediaType, string ownership, string use, string appliesToMedia, string artifact);
        public abstract bool YieldPackageSource(string name, string location, bool isTrusted, bool isRegistered, bool isValidated);
        public abstract bool YieldDynamicOption(int category, string name, int expectedType, bool isRequired);
        public abstract bool YieldDynamicOption(string category, string name, string expectedType, bool isRequired);
        public abstract bool YieldKeyValuePair(string key, string value);
        public abstract bool YieldValue(string value);

        public bool Yield(KeyValuePair<string, string[]> pair) {
            if (pair.Value.Length == 0) {
                return YieldKeyValuePair(pair.Key, null);
            }
            return pair.Value.All(each => YieldKeyValuePair(pair.Key, each));
        }

        internal bool Error(string category, string targetObjectValue, string messageText, params object[] args) {
            return Error(messageText, category, targetObjectValue, FormatMessageString(messageText, args));
        }

        public bool Warning(string messageText, params object[] args) {
            return Warning(FormatMessageString(messageText, args));
        }

        public bool Message(string messageText, params object[] args) {
            return Message(FormatMessageString(messageText, args));
        }

        public bool Verbose(string messageText, params object[] args) {
            return Verbose(FormatMessageString(messageText, args));
        }

        public bool Debug(string messageText, params object[] args) {
            return Debug(FormatMessageString(messageText, args));
        }

        public int StartProgress(int parentActivityId, string messageText, params object[] args) {
            return StartProgress(parentActivityId, FormatMessageString(messageText, args));
        }

        public bool Progress(int activityId, int progressPercentage, string messageText, params object[] args) {
            return Progress(activityId, progressPercentage, FormatMessageString(messageText, args));
        }

        private static string FixMeFormat(string formatString, object[] args) {
            if (args == null || args.Length == 0) {
                // not really any args, and not really expectng any
                return formatString.Replace('{', '\u00ab').Replace('}', '\u00bb');
            }
            return Enumerable.Aggregate(args, formatString.Replace('{', '\u00ab').Replace('}', '\u00bb'), (current, arg) => current + string.Format(CultureInfo.CurrentCulture, " \u00ab{0}\u00bb", arg));
        }

        internal string FormatMessageString(string messageText, params object[] args) {
            if (string.IsNullOrEmpty(messageText)) {
                return string.Empty;
            }

            if (messageText.StartsWith(Constants.MSGPrefix, true, CultureInfo.CurrentCulture)) {
                // check with the caller first, then with the local resources, and fallback to using the messageText itself.
                messageText = GetMessageString(messageText.Substring(Constants.MSGPrefix.Length)) ?? messageText;
            }

            // if it doesn't look like we have the correct number of parameters
            // let's return a fixmeformat string.
            var c = Enumerable.Count(Enumerable.Where(messageText.ToCharArray(), each => each == '{'));
            if (c < args.Length) {
                return FixMeFormat(messageText, args);
            }
            return string.Format(CultureInfo.CurrentCulture, messageText, args);
        }
    }
}