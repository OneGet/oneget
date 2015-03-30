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

namespace Microsoft.PackageManagement.Api {
    using System.Collections.Generic;
    using System.Security;

    public interface IHostApi {
        bool IsCanceled {get;}

        #region declare host-apis

        /* Synced/Generated code =================================================== */
        string GetMessageString(string messageText, string defaultText);

        bool Warning(string messageText);

        bool Error(string id, string category, string targetObjectValue, string messageText);

        bool Message(string messageText);

        bool Verbose(string messageText);

        bool Debug(string messageText);

        int StartProgress(int parentActivityId, string messageText);

        bool Progress(int activityId, int progressPercentage, string messageText);

        bool CompleteProgress(int activityId, bool isSuccessful);

        /// <summary>
        ///     Used by a provider to request what metadata keys were passed from the user
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> OptionKeys {get;}

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<string> GetOptionValues(string key);

        IEnumerable<string> Sources {get;}

        string CredentialUsername {get;}

        SecureString CredentialPassword {get;}

        bool ShouldBootstrapProvider(string requestor, string providerName, string providerVersion, string providerType, string location, string destination);

        bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource);

        bool AskPermission(string permission);

        bool IsInteractive {get;}

        int CallCount {get;}

        #endregion
    }
}