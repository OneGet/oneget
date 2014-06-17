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

namespace Microsoft.OneGet.Api {
    using System.Collections.Generic;
    using System.Security;

    public interface IHostApi {
        #region declare host-apis

        string GetMessageString(string message);

        bool Warning(string message);

        bool Error(string message);

        bool Message(string message);

        bool Verbose(string message);

        bool Debug(string message);

        bool ExceptionThrown(string exceptionType, string message, string stacktrace);

        int StartProgress(int parentActivityId, string message);

        bool Progress(int activityId, int progress, string message);

        bool CompleteProgress(int activityId, bool isSuccessful);

        /// <summary>
        ///     Used by a provider to request what metadata keys were passed from the user
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetOptionKeys(int category);

        IEnumerable<string> GetOptionValues(int category, string key);

        IEnumerable<string> GetSources();

        string GetCredentialUsername();

        string GetCredentialPassword();

        bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource);

        bool ShouldProcessPackageInstall(string packageName, string version, string source);

        bool ShouldProcessPackageUninstall(string packageName, string version);

        bool ShouldContinueAfterPackageInstallFailure(string packageName, string version, string source);

        bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source);

        bool ShouldContinueRunningInstallScript(string packageName, string version, string source, string scriptLocation);

        bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation);

        bool AskPermission(string permission);

        #endregion
    }
}