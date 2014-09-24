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

namespace Microsoft.OneGet.Builtin {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Implementation;
    using Packaging;
    using Utility.Plugin;
    using RequestImpl = System.Object;

    public class MsuProvider {
        /// <summary>
        /// The name of this Package Provider
        /// </summary>
        internal const string ProviderName = "msu";

        private static readonly Dictionary<string, string[]> _features = new Dictionary<string, string[]>{
            {"extensions", new[]{"msu"} }
        };


        /// <summary>
        /// Returns the name of the Provider. 
        /// </summary>
        /// <returns>The name of this proivder (uses the constant declared at the top of the class)</returns>
        public string GetPackageProviderName() {
            return ProviderName;
        }

        /// <summary>
        /// Performs one-time initialization of the PROVIDER.
        /// </summary>
        /// <param name="dynamicInterface">a <c>System.Type</c> that represents a remote interface for that a request needs to implement when passing the request back to methods in the CORE. (Advanced Usage)</param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void InitializeProvider(object dynamicInterface, RequestImpl requestImpl) {
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::InitializeProvider'", ProviderName);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::InitializeProvider' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// Returns a collection of strings to the client advertizing features this provider supports.
        /// </summary>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetFeatures(RequestImpl requestImpl) {
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::GetFeatures' ", ProviderName);
                    foreach (var feature in _features) {
                        request.Yield(feature);
                    }
                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::GetFeatures' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// Returns dynamic option definitions to the HOST
        /// </summary>
        /// <param name="category">The category of dynamic options that the HOST is interested in</param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetDynamicOptions(string category, RequestImpl requestImpl) {
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::GetDynamicOptions' '{1}'", ProviderName, category);

                    switch((category??string.Empty).ToLowerInvariant()){
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
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::GetDynamicOptions' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// Finds packages given a locally-accessible filename
        /// 
        /// Package information must be returned using <c>request.YieldPackage(...)</c> function.
        /// </summary>
        /// <param name="file">the full path to the file to determine if it is a package</param>
        /// <param name="id">if this is greater than zero (and the number should have been generated using <c>StartFind(...)</c>, the core is calling this multiple times to do a batch search request. The operation can be delayed until <c>CompleteFind(...)</c> is called</param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void FindPackageByFile(string file, int id, RequestImpl requestImpl) {
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::FindPackageByFile' '{1}','{2}'", ProviderName, file, id);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::FindPackageByFile' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetInstalledPackages(string name, RequestImpl requestImpl) {
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::GetInstalledPackages' '{1}'", ProviderName, name);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::GetInstalledPackages' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// Installs a given package.
        /// </summary>
        /// <param name="fastPackageReference">A provider supplied identifier that specifies an exact package</param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void InstallPackage(string fastPackageReference, RequestImpl requestImpl) {
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::InstallPackage' '{1}'", ProviderName, fastPackageReference);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::InstallPackage' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// Uninstalls a package 
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void UninstallPackage(string fastPackageReference, RequestImpl requestImpl) {
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::UninstallPackage' '{1}'", ProviderName, fastPackageReference);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::UninstallPackage' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }
    }
}