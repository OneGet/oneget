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

namespace Microsoft.OneGet.Core.Api {
    using System.Collections.Generic;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    #region declare host-apis

    /// <summary>
    ///     Used by a provider to request what metadata keys were passed from the user
    /// </summary>
    /// <returns></returns>
    public delegate IEnumerable<string> GetOptionKeys(string category);

    public delegate IEnumerable<string> GetOptionValues(string category, string key);

    public delegate IEnumerable<string> PackageSources();

    /// <summary>
    ///     Returns a string collection of values from a specified path in a hierarchal
    ///     configuration hashtable.
    /// </summary>
    /// <param name="path">
    ///     Path to the configuration key. Nodes are traversed by specifying a '/' character:
    ///     Example: "Providers/Module" ""
    /// </param>
    /// <returns>
    ///     A collection of string values from the configuration.
    ///     Returns an empty collection if no data is found for that path
    /// </returns>
    public delegate IEnumerable<string> GetConfiguration(string path);


  
    public delegate bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource);

    public delegate bool ShouldProcessPackageInstall(string packageName, string version, string source);

    public delegate bool ShouldProcessPackageUninstall(string packageName, string version);

    public delegate bool ShouldContinueAfterPackageInstallFailure(string packageName, string version, string source);

    public delegate bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source);

    public delegate bool ShouldContinueRunningInstallScript(string packageName, string version, string source, string scriptLocation);

    public delegate bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation);

    public delegate bool AskPermission(string permission);

    public delegate bool WhatIf();

    #endregion
}