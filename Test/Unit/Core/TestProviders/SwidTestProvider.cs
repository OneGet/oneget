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

namespace Microsoft.OneGet.Test.Core.TestProviders {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using OneGet.Packaging;
    using Sdk;

    /// <summary>
    /// A Package provider for OneGet.
    /// 
    /// Important notes:
    ///    - Required Methods: Not all methods are required; some package providers do not support some features. If the methods isn't used or implemented it should be removed (or commented out)
    ///    - Error Handling: Avoid throwing exceptions from these methods. To properly return errors to the user, use the request.Error(...) method to notify the user of an error conditionm and then return.
    /// 
    /// todo: Give this class a proper name
    /// </summary>
    public class SwidTestProvider {
        /// <summary>
        /// The features that this package supports.
        /// todo: fill in the feature strings for this provider
        /// </summary>
        protected static Dictionary<string, string[]> Features = new Dictionary<string, string[]> {
            { Constants.Features.Test, Constants.FeaturePresent },

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
            get { return "SwidTest"; }
        }

        /// <summary>
        /// Returns the version of the Provider. 
        /// todo: Change this to the version for your package provider. 
        /// </summary>
        /// <returns>The version of this provider </returns>
        public string ProviderVersion {
            get {
                return "1.0.0.0";
            }
        }

        /// <summary>
        /// This is just here as to give us some possibility of knowing when an unexception happens...
        /// At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "#pw26506")]
        public void OnUnhandledException(string methodName, Exception exception) {
            Debug.WriteLine("Unexpected Exception thrown in '{0}::{1}' -- {2}\\{3}\r\n{4}", PackageProviderName, methodName, exception.GetType().Name, exception.Message, exception.StackTrace);
        }

        /// <summary>
        /// Performs one-time initialization of the $provider.
        /// </summary>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "#pw26506")]
        public void InitializeProvider(Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            request.Debug("Calling '{0}::InitializeProvider'", PackageProviderName);

            // todo: put any one-time initialization code here.
        }

        /// <summary>
        /// Returns a collection of strings to the client advertizing features this provider supports.
        /// </summary>
        /// <param name="request">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "#pw26506")]
        public void GetFeatures(Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            request.Debug("Calling '{0}::GetFeatures' ", PackageProviderName);

            foreach (var feature in Features) {
                request.Yield(feature);
            }
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "#pw26506")]
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Request request) {
            // Nice-to-have put a debug message in that tells what's going on.
            request.Debug("Calling '{0}::FindPackage' '{1}','{2}','{3}','{4}'", PackageProviderName, requiredVersion, minimumVersion, maximumVersion, id);

            if (name == "test") {
                // this just returns packages so we can test the calls for creating software identity objects back in the core.
                var pkg = request.YieldSoftwareIdentity("firstpackage", "first", "1.0", "multipartnumeric", "first package", "built-in", "test", "", "");
                if ( pkg != null) {
                    if (request.IsCanceled) { return; } // check early, check often.

                    // add a dependency to another package.
                    var link = request.AddDependency(PackageProviderName, "second", "1.1", null, null);
                    if (request.IsCanceled) { return; } // check early, check often.

                    var entity = request.AddEntity("garrett", "http://fearthecowboy.com", Iso19770_2.Role.Publisher, null);
                    if (request.IsCanceled) { return; } // check early, check often.

                    var icon = request.AddLink(new Uri("http://contoso.com/icon.png"), "icon", "image/icon", null, null, null, null);
                    if (request.IsCanceled) { return; } // check early, check often.

                    request.AddMetadata( "Something", "Shiny");
                    request.AddMetadata(pkg, "SomeKey", "SomeValue");
                    request.AddMetadata(pkg, new Uri("http://oneget.org/oneget"), "testkey", "testvalue");
                    if (request.IsCanceled) { return; } // check early, check often.

                    // some expected payload data 
                    var payload= request.AddPayload();
                    request.Debug("Payload:{0}", payload);
                    if (request.IsCanceled) { return; } // check early, check often.


                    var dir = request.AddDirectory(payload, "installDir", "", "PROGRAMFILES", false);
                    request.Debug("Directory:{0}", dir);
                    if (request.IsCanceled) { return; } // check early, check often.

                    var file = request.AddFile(dir, "foo.txt", null, null, true, 10240, "1.0");
                    request.Debug("File:{0}", file);
                    if (request.IsCanceled) { return; } // check early, check often.

                    var regkey = request.AddResource(payload, "regkey");
                    request.AddMetadata(regkey, "key", "hklm/software/testing/foo");
                    request.AddMetadata(regkey, "value", "hklm/software/testing/foo");
                    if (request.IsCanceled) { return; } // check early, check often.
                }

                pkg = request.YieldSoftwareIdentity("secondpackage", "second", "1.1", "multipartnumeric", "second package", "built-in", "test", "", "");
                if (pkg != null) {
                    if (request.IsCanceled) { return; } // check early, check often.

                    var entity = request.AddEntity("garrett", "http://fearthecowboy.com", Iso19770_2.Role.Publisher, null);
                    if (request.IsCanceled) { return; } // check early, check often.

                    // show some evidence (yeah, this should have been in the getinstalledpackages but ... lazy.)
                    var evidence = request.AddEvidence(DateTime.Now, "thispc");
                    request.AddProcess(evidence, "foo.exe", 100);

                    var meta = request.AddMeta(pkg);
                    request.AddMetadata(meta, "sample", "value");
                    request.AddMetadata(meta, new Uri("http://oneget.org/oneget"), "testkey", "testvalue");
                }
            }

        }

    }
}