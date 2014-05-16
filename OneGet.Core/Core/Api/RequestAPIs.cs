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
    using System.Runtime.Serialization;
    using Providers.Package;

    #region declare request-apis

    /// <summary>
    ///     The provider can query to see if the operation has been cancelled.
    ///     This provides for a gentle way for the caller to notify the callee that
    ///     they don't want any more results. It's essentially just !IsCancelled()
    /// </summary>
    /// <returns>returns FALSE if the operation has been cancelled.</returns>
    public delegate bool OkToContinue();

    /// <summary>
    ///     Used by a provider to return fields for a SoftwareIdentity.
    /// </summary>
    /// <param name="fastPath"></param>
    /// <param name="name"></param>
    /// <param name="version"></param>
    /// <param name="versionScheme"></param>
    /// <param name="summary"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public delegate bool YieldPackage(string fastPath, string name, string version, string versionScheme, string summary, string source);


    public delegate bool YieldPackageDetails(object serializablePackageDetailsObject);

    public delegate bool YieldPackageSwidtag(string fastPath, string xmlOrJsonDoc);

    /// <summary>
    ///     Used by a provider to return fields for a package source (repository)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    public delegate bool YieldSource(string name, string location, bool isTrusted);

    /// <summary>
    ///     Used by a provider to return the fields for a Metadata Definition
    ///     The cmdlets can use this to supply tab-completion for metadata to the user.
    /// </summary>
    /// <param name="category"> one of ['provider', 'source', 'package', 'install']</param>
    /// <param name="name">the provider-defined name of the option</param>
    /// <param name="expectedType"> one of ['string','int','path','switch']</param>
    /// <param name="permittedValues">either a collection of permitted values, or null for any valid value</param>
    /// <returns></returns>
    public delegate bool YieldOptionDefinition(OptionCategory category, string name, OptionType expectedType, bool isRequired, IEnumerable<string> permittedValues);

    #endregion
}