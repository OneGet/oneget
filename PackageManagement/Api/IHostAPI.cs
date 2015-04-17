﻿// 
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

    /// <summary>
    /// Functions implemented by the HOST to provide contexual information and control to for the current request.
    /// </summary>
    public interface IHostApi {

        /// <summary>
        ///  The HOST should return true if the current request in progress should be cancelled.
        /// </summary>
        bool IsCanceled {get;}

        /// <summary>
        /// the HOST should return a localized string for the given messageText, or null if not localizable.
        /// </summary>
        /// <param name="messageText">
        ///     The message ID or text string to resolve.
        /// </param>
        /// <param name="defaultText">
        ///     a default message text that would be used if there is no match in local resources. 
        ///     This provides the HOST the opportunity to reformat the actual message even if they don't match it. 
        ///     (PSGet uses this)
        /// </param>
        /// <returns></returns>
        string GetMessageString(string messageText, string defaultText);

        /// <summary>
        /// Sends a formatted warning message to the HOST.
        /// </summary>
        /// <param name="messageText">
        ///     The fully formatted warning message to display to the user
        /// </param>
        /// <returns>
        ///     This should return true if the request is cancelled.
        /// </returns>
        bool Warning(string messageText);

        /// <summary>
        /// Sends a complex Error message to the HOST.
        /// </summary>
        /// <param name="id">An identifier that can be used to uniquely id the Error</param>
        /// <param name="category">A category for the message (should map to a PowerShell ErrorCategory enumeration)</param>
        /// <param name="targetObjectValue">a target object the operation has failed for.</param>
        /// <param name="messageText">
        ///     The fully formatted error message to display to the user
        /// </param>
        /// <returns>
        ///     This should return true if the request is cancelled.
        /// </returns>        /// <returns></returns>
        bool Error(string id, string category, string targetObjectValue, string messageText);

        /// <summary>
        /// Sends a status message to the HOST
        /// </summary>
        /// <param name="messageText">
        ///     The fully formatted message to display to the user
        /// </param>
        /// <returns>
        ///     This should return true if the request is cancelled.
        /// </returns>
        bool Message(string messageText);

        /// <summary>
        /// Sends a message to the verbose channel. 
        /// </summary>
        /// <param name="messageText">
        ///     The fully formatted verbose message to display to the user
        /// </param>
        /// <returns>
        ///     This should return true if the request is cancelled.
        /// </returns>
        bool Verbose(string messageText);

        /// <summary>
        /// Sends a message to the debug channel
        /// </summary>
        /// <param name="messageText">
        ///     The fully formatted debug message to display to the user
        /// </param>
        /// <returns>
        ///     This should return true if the request is cancelled.
        /// </returns>
        bool Debug(string messageText);

        /// <summary>
        /// Starts a progress indicator
        /// </summary>
        /// <param name="parentActivityId">the number of a parent progress indicator. Should be zero if there is no parent.</param>
        /// <param name="messageText">
        ///     The fully formatted progress message to display to the user
        /// </param>
        /// <returns>
        ///     The progress indicator handle for the new progress message
        /// </returns>
        int StartProgress(int parentActivityId, string messageText);

        /// <summary>
        ///     Sends a progress update
        /// </summary>
        /// <param name="activityId">
        ///     The Progress indicator ID (from StartProgress)
        /// </param>
        /// <param name="progressPercentage">
        ///     The Percentage for the progress (0-100)
        /// </param>
        /// <param name="messageText">
        ///     The fully formatted progress message to display to the user
        /// </param>
        /// <returns>
        ///     This should return true if the request is cancelled.
        /// </returns>
        bool Progress(int activityId, int progressPercentage, string messageText);

        /// <summary>
        ///     Ends a progress notification
        /// </summary>
        /// <param name="activityId">
        ///     The Progress indicator ID (from StartProgress)
        /// </param>
        /// <param name="isSuccessful">
        ///     true if the operation was successful.
        /// </param>
        /// <returns>
        ///     This should return true if the request is cancelled.
        /// </returns>
        bool CompleteProgress(int activityId, bool isSuccessful);

        /// <summary>
        ///     Used by a provider to request what metadata keys were passed from the user
        /// </summary>
        /// <returns>an collection of the keys for the specified dynamic options</returns>
        IEnumerable<string> OptionKeys {get;}

        /// <summary>
        ///     Used by a provider to request the values for a given dynamic option
        /// </summary>
        /// <param name="key">the dynamic option Key (should be present in OptionKeys)</param>
        /// <returns>an collection of the value for the specified dynamic option</returns>
        IEnumerable<string> GetOptionValues(string key);

        /// <summary>
        /// A collection of sources specified by the user. If this is null or empty, the provider should assume 'all the registered sources'
        /// </summary>
        IEnumerable<string> Sources {get;}

        /// <summary>
        /// A credential username specified by the user 
        /// </summary>
        string CredentialUsername {get;}

        /// <summary>
        /// A credential password specified by the user 
        /// </summary>
        SecureString CredentialPassword {get;}

        /// <summary>
        /// The CORE may ask the HOST if a given provider should be bootstrapped during an operation.
        /// </summary>
        /// <param name="requestor">the name of the provider or component requesting the provider.</param>
        /// <param name="providerName">the name of the requested provider</param>
        /// <param name="providerVersion">the miniumum version of the provider required</param>
        /// <param name="providerType"></param>
        /// <param name="location">the remote location that the provider is being bootstrapped from</param>
        /// <param name="destination">the target folder where the provider is to be installed.</param>
        /// <returns></returns>
        bool ShouldBootstrapProvider(string requestor, string providerName, string providerVersion, string providerType, string location, string destination);

        /// <summary>
        /// the CORE may aske the user if a given package should be allowed to install.
        /// </summary>
        /// <param name="package">the name of the package</param>
        /// <param name="packageSource">the name of the source of the package</param>
        /// <returns></returns>
        bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource);

        /// <summary>
        /// Asks an arbitrary true/false question of the user.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        bool AskPermission(string permission);

        /// <summary>
        /// The HOST should return 'True' if the current operation is executed in an interactive environment
        /// and the user should be able to respond to queries.
        /// </summary>
        bool IsInteractive {get;}

        /// <summary>
        /// The HOST should give each individual request a unique value (used to track if a particular operation has been tried before)
        /// </summary>
        int CallCount {get;}
    }
}