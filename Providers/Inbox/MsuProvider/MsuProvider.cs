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

using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.PackageManagement.Packaging;

namespace Microsoft.PackageManagement.Msu {
    using System;
    using System.Collections.Generic;
    using Archivers.Compression.Cab;
    using Implementation;
    using Utility.Extensions;

    public class MsuProvider {
        /// <summary>
        ///     The name of this Package Provider
        /// </summary>
        internal const string ProviderName = "msu";

        /// <summary>
        /// Windows Update executable
        /// </summary>
        private readonly string WusaExecutableLocation = Path.Combine(Environment.SystemDirectory, "wusa.exe");

        private static readonly Dictionary<string, string[]> _features = new Dictionary<string, string[]> {
            {Constants.Features.SupportedExtensions, new[] {"msu"}},
            {Constants.Features.MagicSignatures, new[] {Constants.Signatures.Cab}}
        };

        /// <summary>
        ///     Returns the name of the Provider.
        /// </summary>
        /// <returns>The name of this proivder (uses the constant declared at the top of the class)</returns>
        public string GetPackageProviderName() {
            return ProviderName;
        }

        /// <summary>
        ///     Performs one-time initialization of the PROVIDER.
        /// </summary>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void InitializeProvider(Request request) {
            if( request == null ) {
                throw new ArgumentNullException("request");
            }

            // Nice-to-have put a debug message in that tells what's going on.
            request.Debug("Calling '{0}::InitializeProvider'", ProviderName);
        }

        /// <summary>
        ///     Returns a collection of strings to the client advertizing features this provider supports.
        /// </summary>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void GetFeatures(Request request) {
            if( request == null ) {
                throw new ArgumentNullException("request");
            }

            // Nice-to-have put a debug message in that tells what's going on.
            request.Debug("Calling '{0}::GetFeatures' ", ProviderName);
            foreach (var feature in _features) {
                request.Yield(feature);
            }
        }

        /// <summary>
        ///     Returns dynamic option definitions to the HOST
        /// </summary>
        /// <param name="category">The category of dynamic options that the HOST is interested in</param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void GetDynamicOptions(string category, Request request) {
            if( request == null ) {
                throw new ArgumentNullException("request");
            }

            // Nice-to-have put a debug message in that tells what's going on.
            request.Debug("Calling '{0}::GetDynamicOptions' '{1}'", ProviderName, category);

            switch ((category ?? string.Empty).ToLowerInvariant()) {
                case "install":
                    // options required for install/uninstall/getinstalledpackages
                    break;

                case "provider":
                    // options used with this provider. Not currently used.
                    break;

                case "source":
                    // options for package sources
                    break;

                case "package":
                    // options used when searching for packages
                    break;
            }
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
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void FindPackageByFile(string file, int id, Request request) {
            if( request == null ) {
                throw new ArgumentNullException("request");
            }
            if( string.IsNullOrWhiteSpace(file) ) {
                throw new ArgumentNullException("file");
            }

            // Nice-to-have put a debug message in that tells what's going on.
            request.Debug("Calling '{0}::FindPackageByFile' '{1}','{2}'", ProviderName, file, id);

            if (file.FileExists())
            {
                var info = new CabInfo(file);

                request.YieldSoftwareIdentity(file, info.Name, null, null, null, null, null, file, info.Name);

                var files = info.GetFiles();
                foreach (var i in files) {
                    // read the properties file
                    if (i.FullNameExtension == ".txt")
                    {
                        request.Debug("Reading properties file {0}", i.FullName);
                        using (var reader = i.OpenText())
                        {
                            var contents = reader.ReadToEnd();
                            Dictionary<string, string> keyValuePairs = contents.Split('\n').Select(line => line.Split('=')).Where(v => v.Count() == 2).ToDictionary(pair => pair[0], pair => pair[1]);

                            foreach (var pair in keyValuePairs)
                            {
                                request.AddMetadata(pair.Key.Replace(' ', '_'), pair.Value.Replace("\"", "").Replace("\r", ""));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void GetInstalledPackages(string name, Request request) {
            if( request == null ) {
                throw new ArgumentNullException("request");
            }

            // Nice-to-have put a debug message in that tells what's going on.
            request.Debug("Calling '{0}::GetInstalledPackages' '{1}'", ProviderName, name);
        }

        /// <summary>
        ///     Installs a given package.
        /// </summary>
        /// <param name="fastPackageReference">A provider supplied identifier that specifies an exact package</param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void InstallPackage(string fastPackageReference, Request request) {
            if( request == null ) {
                throw new ArgumentNullException("request");
            }
            if( string.IsNullOrWhiteSpace(fastPackageReference) ) {
                throw new ArgumentNullException("fastPackageReference");
            }

            // Nice-to-have put a debug message in that tells what's going on.
            request.Debug("Calling '{0}::InstallPackage' '{1}'", ProviderName, fastPackageReference);

            string output;
            int exitCode = request.ProviderServices.StartProcess(WusaExecutableLocation, fastPackageReference + " /quiet /norestart", true, out output, request);
            if (exitCode == 0)
            {
                request.Verbose("Provider '{0}', Package '{1}': Installation succeeded", ProviderName, fastPackageReference);
            }
            else
            {
                request.Verbose("Provider '{0}', Package '{1}': Installation failed with Windows Update error code '{2}'.", ProviderName, fastPackageReference, String.Format(CultureInfo.CurrentCulture, "0x{0:X}", exitCode));
            }
        }

        /// <summary>
        ///     Uninstalls a package
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void UninstallPackage(string fastPackageReference, Request request) {
            if( request == null ) {
                throw new ArgumentNullException("request");
            }
            if( string.IsNullOrWhiteSpace(fastPackageReference) ) {
                throw new ArgumentNullException("fastPackageReference");
            }

            // Nice-to-have put a debug message in that tells what's going on.
            request.Debug("Calling '{0}::UninstallPackage' '{1}'", ProviderName, fastPackageReference);
        }
    }
}
