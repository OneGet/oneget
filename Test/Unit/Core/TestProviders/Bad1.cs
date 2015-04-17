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

namespace Microsoft.PackageManagement.Test.Core.TestProviders {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Sdk;
    using Console = Support.Console;

    /// <summary>
    ///     A Package provider for PackageManagement.
    ///     Important notes:
    ///     - Required Methods: Not all methods are required; some package providers do not support some features. If the
    ///     methods isn't used or implemented it should be removed (or commented out)
    ///     - Error Handling: Avoid throwing exceptions from these methods. To properly return errors to the user, use the
    ///     request.Error(...) method to notify the user of an error conditionm and then return.
    ///     todo: Give this class a proper name
    /// </summary>
    public class Bad1 {
        /// <summary>
        ///     The features that this package supports.
        ///     todo: fill in the feature strings for this provider
        /// </summary>
        protected static Dictionary<string, string[]> Features = new Dictionary<string, string[]> {
            {Constants.Features.Test, Constants.FeaturePresent},
#if FOR_EXAMPLE
    // add this if you want to 'hide' your provider by default.
            { Constants.Features.AutomationOnly, Constants.FeaturePresent },

            // specify the extensions that your provider uses for its package files (if you have any)
            { Constants.Features.SupportedExtensions, new[]{"mypkg"}},

            // you can list the URL schemes that you support searching for packages with
            { Constants.Features.SupportedSchemes, new [] {"http", "https", "file"}},

            // you can list the magic signatures (bytes at the beginning of a file) that we can use
            // to peek and see if a given file is yours.
            { Constants.Features.MagicSignatures, Constants.Signatures.ZipVariants},
#endif
        };

        internal static bool Initialized = false;

        /// <summary>
        ///     Returns the name of the Provider.
        ///     todo: Change this to the common name for your package provider.
        /// </summary>
        /// <returns>The name of this provider </returns>
        public string PackageProviderName {
            get {
                return GetType().Name;
            }
        }

        /// <summary>
        ///     Returns the version of the Provider.
        ///     todo: Change this to the version for your package provider.
        /// </summary>
        /// <returns>The version of this provider </returns>
        public string ProviderVersion {
            [SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new Exception("Misbehaving - ProviderVersion");
            }
        }

        /// <summary>
        ///     This is just here as to give us some possibility of knowing when an unexception happens...
        ///     At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for
        ///     it.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "#pw26506")]
        public void OnUnhandledException(string methodName, Exception exception) {
            Console.WriteLine("Unexpected Exception thrown in '{0}::{1}' -- {2}\\{3}\r\n{4}", PackageProviderName, methodName, exception.GetType().Name, exception.Message, exception.StackTrace);
        }

        /// <summary>
        ///     Performs one-time initialization of the $provider.
        /// </summary>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        [SuppressMessage("Microsoft.Usage", "#pw26506")]
        public void InitializeProvider(Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            request.Debug("Calling '{0}::InitializeProvider'", PackageProviderName);
            Initialized = true;
        }

        /// <summary>
        ///     Returns a collection of strings to the client advertizing features this provider supports.
        /// </summary>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        public void GetFeatures(Request request) {
            throw new Exception("Misbehaving - GetFeatures");
        }

        /// <summary>
        ///     Returns dynamic option definitions to the HOST
        ///     example response:
        ///     request.YieldDynamicOption( "MySwitch", OptionType.String.ToString(), false);
        /// </summary>
        /// <param name="category">The category of dynamic options that the HOST is interested in</param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        public void GetDynamicOptions(string category, Request request) {
            throw new Exception("Misbehaving - GetDynamicOptions");
        }

        /// <summary>
        ///     Resolves and returns Package Sources to the client.
        ///     Specified sources are passed in via the request object (<c>request.GetSources()</c>).
        ///     Sources are returned using <c>request.YieldPackageSource(...)</c>
        /// </summary>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        public void ResolvePackageSources(Request request) {
            throw new Exception("Misbehaving - ResolvePackageSources");
        }

        /// <summary>
        ///     This is called when the user is adding (or updating) a package source
        ///     If this PROVIDER doesn't support user-defined package sources, remove this method.
        /// </summary>
        /// <param name="name">
        ///     The name of the package source. If this parameter is null or empty the PROVIDER should use the
        ///     location as the name (if the PROVIDER actually stores names of package sources)
        /// </param>
        /// <param name="location">
        ///     The location (ie, directory, URL, etc) of the package source. If this is null or empty, the
        ///     PROVIDER should use the name as the location (if valid)
        /// </param>
        /// <param name="trusted">
        ///     A boolean indicating that the user trusts this package source. Packages returned from this source
        ///     should be marked as 'trusted'
        /// </param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        public void AddPackageSource(string name, string location, bool trusted, Request request) {
            throw new Exception("Misbehaving - AddPackageSource");
        }

        /// <summary>
        ///     Removes/Unregisters a package source
        /// </summary>
        /// <param name="name">The name or location of a package source to remove.</param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        public void RemovePackageSource(string name, Request request) {
            throw new Exception("Misbehaving - RemovePackageSource");
        }

        /// <summary>
        ///     Searches package sources given name and version information
        ///     Package information must be returned using <c>request.YieldPackage(...)</c> function.
        /// </summary>
        /// <param name="name">a name or partial name of the package(s) requested</param>
        /// <param name="requiredVersion">A specific version of the package. Null or empty if the user did not specify</param>
        /// <param name="minimumVersion">A minimum version of the package. Null or empty if the user did not specify</param>
        /// <param name="maximumVersion">A maximum version of the package. Null or empty if the user did not specify</param>
        /// <param name="id">
        ///     if this is greater than zero (and the number should have been generated using <c>StartFind(...)</c>,
        ///     the core is calling this multiple times to do a batch search request. The operation can be delayed until
        ///     <c>CompleteFind(...)</c> is called
        /// </param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Request request) {
            throw new Exception("Misbehaving - FindPackage");
        }

        /// <summary>
        ///     Finds packages given a locally-accessible filename
        ///     Package information must be returned using <c>request.YieldPackage(...)</c> function.
        /// </summary>
        /// <param name="file">the full path to the file to determine if it is a package</param>
        /// <param name="id">
        ///     if this is greater than zero (and the number should have been generated using <c>StartFind(...)</c>,
        ///     the core is calling this multiple times to do a batch search request. The operation can be delayed until
        ///     <c>CompleteFind(...)</c> is called
        /// </param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        public void FindPackageByFile(string file, int id, Request request) {
            throw new Exception("Misbehaving - FindPackageByFile");
        }

        /// <summary>
        ///     Finds packages given a URI.
        ///     The function is responsible for downloading any content required to make this work
        ///     Package information must be returned using <c>request.YieldPackage(...)</c> function.
        /// </summary>
        /// <param name="uri">the URI the client requesting a package for.</param>
        /// <param name="id">
        ///     if this is greater than zero (and the number should have been generated using <c>StartFind(...)</c>,
        ///     the core is calling this multiple times to do a batch search request. The operation can be delayed until
        ///     <c>CompleteFind(...)</c> is called
        /// </param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        public void FindPackageByUri(Uri uri, int id, Request request) {
            throw new Exception("Misbehaving - FindPackageByUri");
        }

        /// <summary>
        ///     Downloads a remote package file to a local location.
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="location"></param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        public void DownloadPackage(string fastPackageReference, string location, Request request) {
            throw new Exception("Misbehaving - DownloadPackage");
        }

        /// <summary>
        ///     Installs a given package.
        /// </summary>
        /// <param name="fastPackageReference">A provider supplied identifier that specifies an exact package</param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        public void InstallPackage(string fastPackageReference, Request request) {
            throw new Exception("Misbehaving - InstallPackage");
        }

        /// <summary>
        ///     Uninstalls a package
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        public void UninstallPackage(string fastPackageReference, Request request) {
            throw new Exception("Misbehaving - UninstallPackage");
        }

        /// <summary>
        ///     Returns the packages that are installed
        /// </summary>
        /// <param name="name">the package name to match. Empty or null means match everything</param>
        /// <param name="requiredVersion">
        ///     the specific version asked for. If this parameter is specified (ie, not null or empty
        ///     string) then the minimum and maximum values are ignored
        /// </param>
        /// <param name="minimumVersion">
        ///     the minimum version of packages to return . If the <code>requiredVersion</code> parameter
        ///     is specified (ie, not null or empty string) this should be ignored
        /// </param>
        /// <param name="maximumVersion">
        ///     the maximum version of packages to return . If the <code>requiredVersion</code> parameter
        ///     is specified (ie, not null or empty string) this should be ignored
        /// </param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void GetInstalledPackages(string name, string requiredVersion, string minimumVersion, string maximumVersion, Request request) {
            throw new Exception("Misbehaving - GetInstalledPackage");
        }

        /// <summary>
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        public void GetPackageDetails(string fastPackageReference, Request request) {
            throw new Exception("Misbehaving - GetPackageDetails");
        }

        /// <summary>
        ///     Initializes a batch search request.
        /// </summary>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        /// <returns></returns>
        public int StartFind(Request request) {
            throw new Exception("Misbehaving - StartFind");
        }

        /// <summary>
        ///     Finalizes a batch search request.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with the
        ///     CORE and HOST
        /// </param>
        /// <returns></returns>
        public void CompleteFind(int id, Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            throw new Exception("Misbehaving - CompleteFind");
        }
    }
}