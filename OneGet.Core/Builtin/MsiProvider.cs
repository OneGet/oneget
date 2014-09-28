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
    using System.IO;
    using System.Linq;
    using Implementation;
    using Packaging;
    using Utility.Collections;
    using Utility.Deployment.WindowsInstaller;
    using Utility.Deployment.WindowsInstaller.Package;
    using Utility.Extensions;
    using Utility.Plugin;
    using RequestImpl = System.Object;

    public class MsiProvider {
        /// <summary>
        /// The name of this Package Provider
        /// </summary>
        internal const string ProviderName = "msi";

        private static readonly Dictionary<string, string[]> _features = new Dictionary<string, string[]>{
            {"extensions", new[]{"msi"} }
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
        public void InitializeProvider(RequestImpl requestImpl) {
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
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::InitializeProvider' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
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
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::GetFeatures' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
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
                            request.YieldDynamicOption( "AdditionalArguments", OptionType.StringArray.ToString(), false);
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
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::GetDynamicOptions' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
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
                    if (!file.FileExists()) {
                        request.Error(ErrorCategory.ObjectNotFound, file, Constants.Messages.UnableToResolvePackage, file);
                        return;
                    }
                    try {
                        var package = new InstallPackage(file, DatabaseOpenMode.ReadOnly);
                        YieldPackage(package, file, request);
                        package.Close();
                    } catch(Exception e ) {
                        e.Dump();
                        // any exception at this point really just means that 
                        request.Error(ErrorCategory.OpenError, file, Constants.Messages.UnableToResolvePackage, file);
                    }
                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::FindPackageByFile' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
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
                    var products = ProductInstallation.AllProducts;
                    var installed = string.IsNullOrWhiteSpace(name) ? products.Where(each => each.IsInstalled).Timid() : products.Where( each => each.IsInstalled && each.ProductName.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) > -1).Timid();

                    // dump out results.
                    if (installed.Any(p => !YieldPackage(p, name, request))) {
                        return;
                    }   
                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::GetInstalledPackages' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
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
                    var file = fastPackageReference.CanonicalizePath(false);
                    if (!file.FileExists()) {
                        request.Error(ErrorCategory.OpenError, fastPackageReference, Constants.Messages.UnableToResolvePackage, fastPackageReference);
                        return;
                    }
                    try {
                        var package = new InstallPackage(file, DatabaseOpenMode.ReadOnly);

                        Installer.SetInternalUI(InstallUIOptions.UacOnly | InstallUIOptions.Silent);

                        ExternalUIHandler handler = CreateProgressHandler(request);
                        _progressId = request.StartProgress(0, "Installing MSI '{0}'", file);
                        Installer.SetExternalUI(handler, InstallLogModes.Progress | InstallLogModes.Info);
                        Installer.InstallProduct(file, "REBOOT=REALLYSUPPRESS");
                        Installer.SetInternalUI(InstallUIOptions.Default);

                        Installer.SetExternalUI(handler, InstallLogModes.None);

                        YieldPackage(package, file, request);
                        package.Close();

                        if (Installer.RebootRequired) {
                            request.Warning("Reboot is required to complete Installation.");
                        }

                    } catch (Exception e){
                        e.Dump();
                        request.Error(ErrorCategory.InvalidOperation, file, Constants.Messages.UnableToResolvePackage, file);
                    }

                    request.CompleteProgress(_progressId, true);
                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::InstallPackage' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
        }

        private int _progressId;

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

                    try {
                        Guid guid;
                        if (!Guid.TryParse(fastPackageReference, out guid)) {
                            request.Error(ErrorCategory.InvalidArgument, fastPackageReference, Constants.Messages.UnableToResolvePackage, fastPackageReference);
                            return;
                        }
                        var product = ProductInstallation.GetProducts(fastPackageReference, null, UserContexts.All).FirstOrDefault();
                        if (product == null) {
                            request.Error(ErrorCategory.InvalidArgument, fastPackageReference, Constants.Messages.UnableToResolvePackage, fastPackageReference);
                            return;
                        }
                        var productVersion = product.ProductVersion.ToString();
                        var productName = product.ProductName;
                        var summary = product["Summary"];

                        Installer.SetInternalUI(InstallUIOptions.UacOnly | InstallUIOptions.Silent);
                        _progressId = request.StartProgress(0, "Uninstalling MSI '{0}'", productName);
                        ExternalUIHandler handler = CreateProgressHandler(request);


                        Installer.SetExternalUI(handler, InstallLogModes.Progress | InstallLogModes.Info);
                        Installer.InstallProduct(product.LocalPackage, "REMOVE=ALL REBOOT=REALLYSUPPRESS");
                        Installer.SetInternalUI(InstallUIOptions.Default);

                        Installer.SetExternalUI(handler, InstallLogModes.None);

                        // YieldPackage(product,fastPackageReference, request);
                        if (request.YieldSoftwareIdentity(fastPackageReference, productName, productVersion, "multipartnumeric", summary, "", fastPackageReference, "", "")) {
                            request.YieldSoftwareMetadata(fastPackageReference, "ProductCode", fastPackageReference);
                        }

                        request.Warning("Reboot is required to complete uninstallation.");
                    } catch (Exception e) {
                        e.Dump();
                    }
                    request.CompleteProgress(_progressId, true);
                    _progressId = 0;
                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::UninstallPackage' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
        }

        private ExternalUIHandler CreateProgressHandler(Request request) {

            int currentTotalTicks = -1;
            int currentProgress = 0;
            int progressDirection = 1;
            int actualPercent = 0;

            ExternalUIHandler handler = (type, message, buttons, icon, button) => {
                if (request.IsCancelled()) {
                    return MessageResult.Cancel;
                }

                switch (type) {
                    case InstallMessage.Progress:
                        if (message.Length >= 2) {
                            var msg = message.Split(": ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToInt32(0)).ToArray();

                            switch (msg[1]) {
                                // http://msdn.microsoft.com/en-us/library/aa370354(v=VS.85).aspx
                                case 0: //Resets progress bar and sets the expected total number of ticks in the bar.
                                    currentTotalTicks = msg[3];
                                    currentProgress = 0;
                                    if (msg.Length >= 6) {
                                        progressDirection = msg[5] == 0 ? 1 : -1;
                                    }
                                    break;
                                case 1:
                                    //Provides information related to progress messages to be sent by the current action.
                                    break;
                                case 2: //Increments the progress bar.
                                    if (currentTotalTicks == -1) {
                                        break;
                                    }
                                    currentProgress += msg[3] * progressDirection;
                                    break;
                                case 3:
                                    //Enables an action (such as CustomAction) to add ticks to the expected total number of progress of the progress bar.
                                    break;
                            }
                        }

                        if (currentTotalTicks > 0) {
                            var newPercent = (currentProgress * 100 / currentTotalTicks);
                            if (actualPercent < newPercent) {
                                actualPercent = newPercent;
                                // request.Debug("Progress : {0}", newPercent);
                                request.Progress(_progressId,actualPercent,"installing..." );
                            }
                        }
                        break;
                }

                return MessageResult.OK;
            };

            return handler;
        }

        private bool YieldPackage(InstallPackage package, string filename,  Request request) {
            /*
                       var properties = package.ExecuteStringQuery("SELECT `Property` FROM `Property` ");
                       foreach (var i in properties) {
                           Debug.WriteLine("Property {0} = {1}", i, package.Property[i]);
                       }
                       */
            if (request.YieldSoftwareIdentity(filename, package.Property["ProductName"], package.Property["ProductVersion"], "multipartnumeric", package.Property["Summary"], filename, filename, filename, Path.GetFileName(filename))) {
                var trusted = request.GetPackageManagementService().As<IPackageManagementService>().ProviderServices.IsSignedAndTrusted(filename, "");
                
                if(!request.YieldSoftwareMetadata(filename, "FromTrustedSource", trusted.ToString()) ) {
                    return false;
                }

                if (!request.YieldSoftwareMetadata(filename, "ProductCode", package.Property["ProductCode"])) {
                    return false;
                }

                if (!request.YieldSoftwareMetadata(filename, "UpgradeCode", package.Property["UpgradeCode"])) {
                    return false;
                }

                return true;
            }

            return false;
        }

        private bool YieldPackage(ProductInstallation package, string searchKey, Request request) {
            
            if (request.YieldSoftwareIdentity(package.ProductCode, package.ProductName, package.ProductVersion.ToString(), "multipartnumeric", package["Summary"], package.InstallSource, searchKey, package.InstallLocation, "?")) {
                if (!request.YieldSoftwareMetadata(package.ProductCode, "ProductCode", package.ProductCode)) {
                    return false;
                }

                if (!request.YieldSoftwareMetadata(package.ProductCode, "UpgradeCode", package["UpgradeCode"])) {
                    return false;
                }
                return true;
            }
            return false;
        }

        
    }

   
}