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

using System.Diagnostics;

namespace OneGet.PackageProvider.Template {
    using System;
    using System.Linq;
    using System.Security;
    using RequestImpl = System.Object;

    /// <summary>
    /// A Package provider for OneGet.
    /// 
    /// 
    /// Important notes:
    ///    - Required Methods: Not all methods are required; some package providers do not support some features. If the methods isn't used or implemented it should be removed (or commented out)
    ///    - Error Handling: Avoid throwing exceptions from these methods. To properly return errors to the user, use the request.Error(...) method to notify the user of an error conditionm and then return.
    ///    - Communicating with the HOST and CORE: each method takes a RequestImpl (in reality, an alias for System.Object), which can be used in one of two ways:
    ///         - use the c# 'dynamic' keyword, and call functions on the object directly.
    ///         - use the <code><![CDATA[ .As<Request>() ]]></code> extension method to strongly-type it to the Request type (which calls upon the duck-typer to generate a strongly-typed wrapper).  The strongly-typed wrapper also implements several helper functions to make using the request object easier.
    /// 
    /// todo: Give this class a proper name
    /// </summary>
    public class SamplePackageProvider {

        // todo remove these -- they are here to illustrate things in the sample
        private bool ExampleCantFindPackageSource = false;
        private bool ExampleFoundExistingSource = false;
        private bool ExamplePackageSourceIsNotValid = false;

        /// <summary>
        /// The name of this Package Provider
        /// todo: Change this to the common name for your package provider. 
        /// </summary>
        internal const string ProviderName = "Sample";

        /// <summary>
        /// Performs one-time initialization of the PROVIDER.
        /// </summary>
        /// <param name="dynamicInterface">a <c>System.Type</c> that represents a remote interface for that a request needs to implement when passing the request back to methods in the CORE. (Advanced Usage)</param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void InitializeProvider(object dynamicInterface, RequestImpl requestImpl) {
            try {
                // this is used by the RequestExtensions to generate a remotable dynamic interface for cross-appdomain calls.
                // NOTE:leave this in, unless you really know what you're doing, and aren't going to use the strongly-typed request interface.
                RequestExtensions.RemoteDynamicInterface = dynamicInterface;

                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::InitializeProvider'", ProviderName);

                    // todo: put any one-time initialization code here.
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
        /// DEPRECATED -- for supporting the AUG 2014 OneGet Preview
        /// </summary>
        /// <param name="category">The category of dynamic options that the HOST is interested in</param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetDynamicOptions(int category, RequestImpl requestImpl) {
            try {
                GetDynamicOptions(((OptionCategory)category).ToString(), requestImpl);
            }
            catch {
                // meh. If it doesn't fit, move on.
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
                    OptionCategory cat;
                    if (!Enum.TryParse(category ?? "", true, out cat)) {
                        // unknown category
                        return;
                    }

                    switch (cat) {
                        case OptionCategory.Install:
                            // options required for install/uninstall/getinstalledpackages
                            break;

                        case OptionCategory.Provider:
                            // options used with this provider. Not currently used.
                            break;

                        case OptionCategory.Source:
                            // options for package sources
                            request.YieldDynamicOption(cat, Constants.FavoriteColorParameter, OptionType.String, false);
                            request.YieldDynamicOption(cat, Constants.SkipValidationParameter, OptionType.Switch, false);
                            break;

                        case OptionCategory.Package:
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
        /// This is called when the user is adding (or updating) a package source
        /// 
        /// If this PROVIDER doesn't support user-defined package sources, remove this method.
        /// </summary>
        /// <param name="name">The name of the package source. If this parameter is null or empty the PROVIDER should use the location as the name (if the PROVIDER actually stores names of package sources)</param>
        /// <param name="location">The location (ie, directory, URL, etc) of the package source. If this is null or empty, the PROVIDER should use the name as the location (if valid)</param>
        /// <param name="trusted">A boolean indicating that the user trusts this package source. Packages returned from this source should be marked as 'trusted'</param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void AddPackageSource(string name, string location, bool trusted, RequestImpl requestImpl){
            try{
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) { 
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::AddPackageSource' '{1}','{2}','{3}'", ProviderName, name, location, trusted );

                    // if they didn't pass in a name, use the location as a name. (if you support that kind of thing)
                    name = string.IsNullOrEmpty(name) ? location : name;

                    // let's make sure that they've given us everything we need.
                    if (string.IsNullOrEmpty(name)) {
                        request.Error(ErrorCategory.InvalidArgument, Constants.NameParameter, Constants.MissingRequiredParameter, Constants.NameParameter);
                        // we're done here.
                        return;
                    }

                    if (string.IsNullOrEmpty(location)) {
                        request.Error(ErrorCategory.InvalidArgument, Constants.LocationParameter, Constants.MissingRequiredParameter, Constants.LocationParameter);
                        // we're done here.
                        return;
                    }

                    // if this is supposed to be an update, there will be a dynamic parameter set for IsUpdatePackageSource
                    var isUpdate = request.GetOptionValue(OptionCategory.Source, Constants.IsUpdateParameter).IsTrue();

                    // if your source supports credentials you get get them too:
                    // string username =request.Username; 
                    // SecureString password = request.Password;
                    // feel free to send back an error here if your provider requires credentials for package sources.

#if !NEED_FAVORITE_COLOR
                    // if you have dynamic parameters that you declared, you can retrieve their values too. 
                    // this sort of thing is only needed for additional parameters outside of name, location, credentials, and istrusted.
                    var favoriteColor = request.GetOptionValue(OptionCategory.Source, Constants.FavoriteColorParameter);

                    if (string.IsNullOrEmpty(favoriteColor)) {
                        // send an error
                        request.Error(ErrorCategory.InvalidArgument,Constants.FavoriteColorParameter , Constants.MissingRequiredParameter, Constants.FavoriteColorParameter);
                        // we're done here.
                        return;
                    }
#endif

                    // check first that we're not clobbering an existing source, unless this is an update

                    // todo: insert code to look up package source (from whereever you store it)

                    if (ExampleFoundExistingSource && !isUpdate) {
                        // tell the user that there's one here already
                        request.Error(ErrorCategory.InvalidArgument, name ?? location, Constants.PackageProviderExists, name ?? location);
                        // we're done here.
                        return;
                    }

                    // conversely, if it didn't find one, and it is an update, that's bad too:
                    if (!ExampleFoundExistingSource && isUpdate) {
                        // you can't find that package source? Tell that to the user
                        request.Error(ErrorCategory.ObjectNotFound, name ?? location, Constants.UnableToResolveSource, name ?? location);
                        // we're done here.
                        return;
                    }

                    // ok, we know that we're ok to save this source
                    // next we check if the location is valid (if we support that kind of thing)

                    var validated = false;
#if !SUPPORTS_PACKAGE_SOURCE_VALIDATION
                    if (!request.GetOptionValue(OptionCategory.Source, Constants.SkipValidationParameter).IsTrue()) {
                        // the user has not opted to skip validating the package source location, so check that it's valid (talk to the url, or check if it's a valid directory, etc)
                        // todo: insert code to check if the source is valid

                        if (ExamplePackageSourceIsNotValid) {
                            request.Error(ErrorCategory.InvalidData, name ?? location, Constants.SourceLocationNotValid, location);
                            // we're done here.
                            return;
                        }

                        // we passed validation!
                        validated = true;
                    }
#endif

                    // it's good to check just before you actaully write something to see if the user has cancelled the operation
                    if (request.IsCancelled()) {
                        return;
                    }

                    // looking good -- store the package source
                    // todo: create the package source (and store it whereever you store it)

                    request.Verbose("Storing package source {0}", name);

                    // and, before you go, Yield the package source back to the caller.

                    if (!request.YieldPackageSource(name, location, trusted, true /*since we just registered it*/, validated)) {
                        // always check the return value of a yield, since if it returns false, you don't keep returning data
                        // this can happen if they have cancelled the operation.
                        return;
                    }

#if !NEED_FAVORITE_COLOR
                    // if you have additional parameters that are associated with the source
                    // you can return that data too if you'd like.
                    if (!request.YieldKeyValuePair(Constants.FavoriteColorParameter, favoriteColor)) {
                        // always check the return value of a yield, since if it returns false, you don't keep returning data
                        return;
                    }
#endif
                    // all done!

                }
            } catch( Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in {0} PackageProvider -- {1}\\{2}\r\n{3}" ),ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="requiredVersion"></param>
        /// <param name="minimumVersion"></param>
        /// <param name="maximumVersion"></param>
        /// <param name="id"></param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, RequestImpl requestImpl){
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::FindPackage' '{1}','{2}','{3}','{4}', '{5}'", ProviderName,requiredVersion,minimumVersion,maximumVersion,id);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::FindPackage' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }
        public void FindPackageByFile(string file, int id, RequestImpl requestImpl){
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
        public void FindPackageByUri(Uri uri, int id, RequestImpl requestImpl){
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::FindPackageByUri' '{1}','{2}'", ProviderName, uri,id);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::FindPackageByUri' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }
        public void GetInstalledPackages(string name, RequestImpl requestImpl){
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
        /// Returns the name of the Provider. 
        /// </summary>
        /// <returns>The name of this proivder (uses the constant declared at the top of the class)</returns>
        public string GetPackageProviderName(){
            return ProviderName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void ResolvePackageSources(RequestImpl requestImpl){
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::ResolvePackageSources'", ProviderName);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::ResolvePackageSources' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void InstallPackage(string fastPackageReference, RequestImpl requestImpl){
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
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void InstallPackageByFile(string filePath, RequestImpl requestImpl){
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::InstallPackageByFile' '{1}'", ProviderName, filePath);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::InstallPackageByFile' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="u"></param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void InstallPackageByUri(string u, RequestImpl requestImpl){
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::InstallPackageByUri' '{1}'", ProviderName, u);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::InstallPackageByUri' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void RemovePackageSource(string name, RequestImpl requestImpl){
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::RemovePackageSource' '{1}'", ProviderName, name);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::RemovePackageSource' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }

        }

        /// <summary>
        /// 
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetFeatures(RequestImpl requestImpl){
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::GetFeatures' ", ProviderName);

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
        /// 
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="location"></param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        /// <returns></returns>
        public void DownloadPackage(string fastPackageReference, string location, RequestImpl requestImpl){
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::DownloadPackage' '{1}','{2}'", ProviderName, fastPackageReference, location);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::DownloadPackage' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        /// <returns></returns>
        public void GetPackageDependencies(string fastPackageReference, RequestImpl requestImpl){
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::GetPackageDependencies' '{1}'", ProviderName, fastPackageReference);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::GetPackageDependencies' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        /// <returns></returns>
        public void GetPackageDetails(string fastPackageReference, RequestImpl requestImpl){
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::GetPackageDetails' '{1}'", ProviderName, fastPackageReference);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::GetPackageDetails' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        /// <returns></returns>
        public int StartFind(RequestImpl requestImpl){
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::StartFind'", ProviderName);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::StartFind' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }

            return  default(int);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestImpl">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        /// <returns></returns>
        public void CompleteFind(int id, RequestImpl requestImpl){
            try {
                // create a strongly-typed request object.
                using (var request = requestImpl.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::CompleteFind' '{1}'", ProviderName, id);

                }
            }
            catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::CompleteFind' -- {1}\\{2}\r\n{3}"), ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }
    }

    #region copy PackageProvider-types
/* Synced/Generated code =================================================== */

    public enum OptionCategory {
        Package = 0,
        Provider = 1,
        Source = 2,
        Install = 3
    }

    public enum OptionType {
        String = 0,
        StringArray = 1,
        Int = 2,
        Switch = 3,
        Folder = 4,
        File = 5,
        Path = 6,
        Uri = 7,
        SecureString = 8
    }

    public enum EnvironmentContext {
        All = 0,
        User = 1,
        System = 2
    }

    #endregion

}