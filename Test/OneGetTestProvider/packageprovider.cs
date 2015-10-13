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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.PackageManagement.OneGetTestProvider.Sdk;

namespace Microsoft.PackageManagement.OneGetTestProvider {
    /// <summary>
    /// A Package provider for PM.
    ///
    /// Important notes:
    ///    - Required Methods: Not all methods are required; some package providers do not support some features. If the methods isn't used or implemented it should be removed (or commented out)
    ///    - Error Handling: Avoid throwing exceptions from these methods. To properly return errors to the user, use the request.Error(...) method to notify the user of an error conditionm and then return.
    ///    - Communicating with the HOST and CORE: each method takes a Request (in reality, an alias for System.Object), which can be used in one of two ways:
    ///         - use the c# 'dynamic' keyword, and call functions on the object directly.
    ///         - use the <code><![CDATA[ .As<Request>() ]]></code> extension method to strongly-type it to the Request type (which calls upon the duck-typer to generate a strongly-typed wrapper).  The strongly-typed wrapper also implements several helper functions to make using the request object easier.
    ///
    /// todo: Give this class a proper name
    /// </summary>
    public class PackageProvider {
        /// <summary>
        /// The features that this package supports.
        /// todo: fill in the feature strings for this provider
        /// </summary>
        protected static Dictionary<string, string[]> Features = new Dictionary<string, string[]> {

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


        /// <summary>
        /// Returns the name of the Provider.
        /// todo: Change this to the common name for your package provider.
        /// </summary>
        /// <returns>The name of this provider </returns>
        public string PackageProviderName {
            get { return "OneGetTestProvider"; }
        }

        /// <summary>
        /// Returns the version of the Provider.
        /// todo: Change this to the version for your package provider.
        /// </summary>
        /// <returns>The version of this provider </returns>
        public string ProviderVersion {
            get{
                return "1.0.0.0";
            }
        }

        /// <summary>
        /// This is just here as to give us some possibility of knowing when an unexception happens...
        /// At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
        /// </summary>
        public void OnUnhandledException(string methodName, Exception exception) {
           // Debug.WriteLine("Unexpected Exception thrown in '{0}::{1}' -- {2}\\{3}\r\n{4}", PackageProviderName, methodName, exception.GetType().Name, exception.Message, exception.StackTrace);
        }

        /// <summary>
        /// Performs one-time initialization of the $provider.
        /// </summary>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void InitializeProvider(Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            if (request == null) {
                return;
            }
            request.Debug("Calling '{0}::InitializeProvider'", PackageProviderName);

            // todo: put any one-time initialization code here.
        }

        /// <summary>
        /// Returns a collection of strings to the client advertizing features this provider supports.
        /// </summary>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetFeatures(Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            if (request == null) {
                return;
            }
            request.Debug("Calling '{0}::GetFeatures' ", PackageProviderName);

            foreach (var feature in Features) {
                request.Yield(feature);
            }
        }

        /// <summary>
        /// Returns dynamic option definitions to the HOST
        ///
        /// example response:
        ///     request.YieldDynamicOption( "MySwitch", OptionType.String.ToString(), false);
        ///
        /// </summary>
        /// <param name="category">The category of dynamic options that the HOST is interested in</param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetDynamicOptions(string category, Request request) {
            if (request == null) {
                return;
            }
            // Nice-to-have put a debug message in that tells what's going on.
            request.Debug("Calling '{0}::GetDynamicOptions' {1}", PackageProviderName, category);

            switch ((category ?? string.Empty).ToLowerInvariant()) {
                case "install":
                    // todo: put any options required for install/uninstall/getinstalledpackages

                    break;

                case "provider":
                    // todo: put any options used with this provider. Not currently used.

                    break;

                case "source":
                    // todo: put any options for package sources

                    break;

                case "package":
                    // todo: put any options used when searching for packages

                    break;
            }
        }

        /// <summary>
        /// Resolves and returns Package Sources to the client.
        ///
        /// Specified sources are passed in via the request object (<c>request.GetSources()</c>).
        ///
        /// Sources are returned using <c>request.YieldPackageSource(...)</c>
        /// </summary>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void ResolvePackageSources(Request request) {
            if (request == null) {
                return;
            }
            // Nice-to-have put a debug message in that tells what's going on.
            request.Debug("Calling '{0}::ResolvePackageSources'", PackageProviderName);

            // todo: resolve package sources
            if (request.Sources.Any()) {
                // the system is requesting sources that match the values passed.
                // if the value passed can be a legitimate source, but is not registered, return a package source marked unregistered.

            }
            else {
                // the system is requesting all the registered sources

            }
        }


        /// <summary>
        /// This is called when the user is adding (or updating) a package source
        ///
        /// If this PROVIDER doesn't support user-defined package sources, remove this method.
        /// </summary>
        /// <param name="name">The name of the package source. If this parameter is null or empty the PROVIDER should use the location as the name (if the PROVIDER actually stores names of package sources)</param>
        /// <param name="location">The location (ie, directory, URL, etc) of the package source. If this is null or empty, the PROVIDER should use the name as the location (if valid)</param>
        /// <param name="trusted">A boolean indicating that the user trusts this package source. Packages returned from this source should be marked as 'trusted'</param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void AddPackageSource(string name, string location, bool trusted, Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            if (request == null) {
                return;
            }
            request.Debug("Calling '{0}::AddPackageSource' '{1}','{2}','{3}'", PackageProviderName, name, location, trusted);

            // todo: add a package source
        }

        /// <summary>
        /// Removes/Unregisters a package source
        /// </summary>
        /// <param name="name">The name or location of a package source to remove.</param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void RemovePackageSource(string name, Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            if (request == null) {
                return;
            }
            request.Debug("Calling '{0}::RemovePackageSource' '{1}'", PackageProviderName, name);

            // todo: Remove a package source
        }


        /// <summary>
        /// Searches package sources given name and version information
        ///
        /// Package information must be returned using <c>request.YieldPackage(...)</c> function.
        /// </summary>
        /// <param name="name">a name or partial name of the package(s) requested</param>
        /// <param name="requiredVersion">A specific version of the package. Null or empty if the user did not specify</param>
        /// <param name="minimumVersion">A minimum version of the package. Null or empty if the user did not specify</param>
        /// <param name="maximumVersion">A maximum version of the package. Null or empty if the user did not specify</param>
        /// <param name="id">if this is greater than zero (and the number should have been generated using <c>StartFind(...)</c>, the core is calling this multiple times to do a batch search request. The operation can be delayed until <c>CompleteFind(...)</c> is called</param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            if (request == null) {
                return;
            }
            request.Debug("Calling '{0}::FindPackage' '{1}','{2}','{3}','{4}'", PackageProviderName, requiredVersion, minimumVersion, maximumVersion, id);

            // todo: find package by name
        }

        /// <summary>
        /// Finds packages given a locally-accessible filename
        ///
        /// Package information must be returned using <c>request.YieldPackage(...)</c> function.
        /// </summary>
        /// <param name="file">the full path to the file to determine if it is a package</param>
        /// <param name="id">if this is greater than zero (and the number should have been generated using <c>StartFind(...)</c>, the core is calling this multiple times to do a batch search request. The operation can be delayed until <c>CompleteFind(...)</c> is called</param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void FindPackageByFile(string file, int id, Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            if (request == null) {
                return;
            }
            request.Debug("Calling '{0}::FindPackageByFile' '{1}','{2}'", PackageProviderName, file, id);

            // todo: find a package by file
        }

        /// <summary>
        /// Finds packages given a URI.
        ///
        /// The function is responsible for downloading any content required to make this work
        ///
        /// Package information must be returned using <c>request.YieldPackage(...)</c> function.
        /// </summary>
        /// <param name="uri">the URI the client requesting a package for.</param>
        /// <param name="id">if this is greater than zero (and the number should have been generated using <c>StartFind(...)</c>, the core is calling this multiple times to do a batch search request. The operation can be delayed until <c>CompleteFind(...)</c> is called</param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void FindPackageByUri(Uri uri, int id, Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            if (request == null) {
                return;
            }
            request.Debug("Calling '{0}::FindPackageByUri' '{1}','{2}'", PackageProviderName, uri, id);

            // todo: find a package by uri
        }

        /// <summary>
        /// Downloads a remote package file to a local location.
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="location"></param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void DownloadPackage(string fastPackageReference, string location, Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            if (request == null) {
                return;
            }
            request.Debug("Calling '{0}::DownloadPackage' '{1}','{2}'", PackageProviderName, fastPackageReference, location);

        }

        /// <summary>
        /// Installs a given package.
        /// </summary>
        /// <param name="fastPackageReference">A provider supplied identifier that specifies an exact package</param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void InstallPackage(string fastPackageReference, Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            if (request == null) {
                return;
            }
            request.Debug("Calling '{0}::InstallPackage' '{1}'", PackageProviderName, fastPackageReference);

            // todo: Install a package
        }

        /// <summary>
        /// Uninstalls a package
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void UninstallPackage(string fastPackageReference, Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            if (request == null) {
                return;
            }
            request.Debug("Calling '{0}::UninstallPackage' '{1}'", PackageProviderName, fastPackageReference);

            // todo: Uninstall a package
        }

        /// <summary>
        /// Returns the packages that are installed
        /// </summary>
        /// <param name="name">the package name to match. Empty or null means match everything</param>
        /// <param name="requiredVersion">the specific version asked for. If this parameter is specified (ie, not null or empty string) then the minimum and maximum values are ignored</param>
        /// <param name="minimumVersion">the minimum version of packages to return . If the <code>requiredVersion</code> parameter is specified (ie, not null or empty string) this should be ignored</param>
        /// <param name="maximumVersion">the maximum version of packages to return . If the <code>requiredVersion</code> parameter is specified (ie, not null or empty string) this should be ignored</param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void GetInstalledPackages(string name, string requiredVersion, string minimumVersion, string maximumVersion, Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            if (request == null) {
                return;
            }
            request.Debug("Calling '{0}::GetInstalledPackages' '{1}','{2}','{3}','{4}'", PackageProviderName, name, requiredVersion, minimumVersion, maximumVersion);

            // todo: get installed packages
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetPackageDetails(string fastPackageReference, Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            if (request == null) {
                return;
            }
            request.Debug("Calling '{0}::GetPackageDetails' '{1}'", PackageProviderName, fastPackageReference);

            // todo: GetPackageDetails that are more expensive than FindPackage* can deliver
        }

        /// <summary>
        /// Initializes a batch search request.
        /// </summary>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        /// <returns></returns>
        public int StartFind(Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            if (request == null) {
                return default(int);
            }
            request.Debug("Calling '{0}::StartFind'", PackageProviderName);

            return default(int);
        }

        /// <summary>
        /// Finalizes a batch search request.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        /// <returns></returns>
        public void CompleteFind(int id, Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            if (request == null) {
                return;
            }
            request.Debug("Calling '{0}::CompleteFind' '{1}'", PackageProviderName, id);

        }

    }
}
