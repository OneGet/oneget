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

// these are just for illustration purposes
#define EXAMPLE_SUPPORTS_PACKAGE_SOURCE_VALIDATION
#define EXAMPLE_NEED_FAVORITE_COLOR
#define EXAMPLE_NEEDS_CREDENTIALS
#define EXAMPLE_FOR_ILLUSTRATION

namespace OneGet.ProviderSDK {
    using System;
    using System.Diagnostics;
    using System.Security;
    using IRequestObject = System.Object;

    /// <summary>
    /// A Package provider for OneGet.
    /// 
    /// 
    /// Important notes:
    ///    - Required Methods: Not all methods are required; some package providers do not support some features. If the methods isn't used or implemented it should be removed (or commented out)
    ///    - Error Handling: Avoid throwing exceptions from these methods. To properly return errors to the user, use the request.Error(...) method to notify the user of an error conditionm and then return.
    ///    - Communicating with the HOST and CORE: each method takes a IRequestObject (in reality, an alias for System.Object), which can be used in one of two ways:
    ///         - use the c# 'dynamic' keyword, and call functions on the object directly.
    ///         - use the <code><![CDATA[ .As<Request>() ]]></code> extension method to strongly-type it to the Request type (which calls upon the duck-typer to generate a strongly-typed wrapper).  The strongly-typed wrapper also implements several helper functions to make using the request object easier.
    /// 
    /// todo: Give this class a proper name
    /// </summary>
    public class PackageProvider {

#if EXAMPLE_FOR_ILLUSTRATION
        // todo remove these -- they are here to illustrate things in the sample
        private bool ExampleCantFindPackageSource = false;
        private bool ExampleFoundExistingSource = false;
        private bool ExamplePackageSourceIsNotValid = false;
#endif 

        /// <summary>
        /// The name of this Package Provider
        /// todo: Change this to the common name for your package provider. 
        /// </summary>
        internal const string ProviderName = "Sample";

        /// <summary>
        /// Performs one-time initialization of the PROVIDER.
        /// </summary>
        /// <param name="dynamicInterface">a <c>System.Type</c> that represents a remote interface for that a request needs to implement when passing the request back to methods in the CORE. (Advanced Usage)</param>
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void InitializeProvider(IRequestObject requestObject) {
            try {
                // this is used by the RequestExtensions to generate a remotable dynamic interface for cross-appdomain calls.
                // NOTE:leave this in, unless you really know what you're doing, and aren't going to use the strongly-typed request interface.
                RequestExtensions.RemoteDynamicInterface = dynamicInterface;

                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::InitializeProvider'", ProviderName);

                    // todo: put any one-time initialization code here.
                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::InitializeProvider' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
        }

        /// <summary>
        /// Returns dynamic option definitions to the HOST
        /// </summary>
        /// <param name="category">The category of dynamic options that the HOST is interested in</param>
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetDynamicOptions(string category, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
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
#if EXAMPLE_FOR_ILLUSTRATION
                            request.YieldDynamicOption( ExampleConstants.FavoriteColorParameter, OptionType.String.ToString(), false);
                            request.YieldDynamicOption(ExampleConstants.SkipValidationParameter, OptionType.Switch.ToString(), false);
#endif
                            break;

                        case "package":
                            // options used when searching for packages 
                            break;
                    }
                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::GetDynamicOptions' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
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
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void AddPackageSource(string name, string location, bool trusted, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::AddPackageSource' '{1}','{2}','{3}'", ProviderName, name, location, trusted);

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
                    var isUpdate = request.GetOptionValue(Constants.IsUpdateParameter).IsTrue();

#if EXAMPLE_NEEDS_CREDENTIALS
                    // if your source supports credentials you get get them too:
                    string username =request.Username; 
                    SecureString password = request.Password;
                    // feel free to send back an error here if your provider requires credentials for package sources.
#endif 

#if EXAMPLE_NEED_FAVORITE_COLOR
                    // if you have dynamic parameters that you declared, you can retrieve their values too. 
                    // this sort of thing is only needed for additional parameters outside of name, location, credentials, and istrusted.
                    var favoriteColor = request.GetOptionValue( ExampleConstants.FavoriteColorParameter);

                    if (string.IsNullOrEmpty(favoriteColor)) {
                        // send an error
                        request.Error(ErrorCategory.InvalidArgument, ExampleConstants.FavoriteColorParameter, Constants.MissingRequiredParameter, ExampleConstants.FavoriteColorParameter);
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
#if EXAMPLE_SUPPORTS_PACKAGE_SOURCE_VALIDATION
                    if (!request.GetOptionValue( ExampleConstants.SkipValidationParameter).IsTrue()) {
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
                    if (request.IsCanceled) {
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

#if EXAMPLE_NEED_FAVORITE_COLOR
                    // if you have additional parameters that are associated with the source
                    // you can return that data too if you'd like.
                    if (!request.YieldKeyValuePair(ExampleConstants.FavoriteColorParameter, favoriteColor)) {
                        // always check the return value of a yield, since if it returns false, you don't keep returning data
                        return;
                    }
#endif
                    // all done!

                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in {0}::AddPackageSource-- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
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
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::FindPackage' '{1}','{2}','{3}','{4}'", ProviderName, requiredVersion, minimumVersion, maximumVersion, id);

                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::FindPackage' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
        }

        /// <summary>
        /// Finds packages given a locally-accessible filename
        /// 
        /// Package information must be returned using <c>request.YieldPackage(...)</c> function.
        /// </summary>
        /// <param name="file">the full path to the file to determine if it is a package</param>
        /// <param name="id">if this is greater than zero (and the number should have been generated using <c>StartFind(...)</c>, the core is calling this multiple times to do a batch search request. The operation can be delayed until <c>CompleteFind(...)</c> is called</param>
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void FindPackageByFile(string file, int id, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::FindPackageByFile' '{1}','{2}'", ProviderName, file, id);

                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::FindPackageByFile' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
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
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void FindPackageByUri(Uri uri, int id, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::FindPackageByUri' '{1}','{2}'", ProviderName, uri, id);

                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::FindPackageByUri' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetInstalledPackages(string name, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::GetInstalledPackages' '{1}'", ProviderName, name);

                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::GetInstalledPackages' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
        }

        /// <summary>
        /// Returns the name of the Provider. 
        /// </summary>
        /// <returns>The name of this proivder (uses the constant declared at the top of the class)</returns>
        public string GetPackageProviderName() {
            return ProviderName;
        }

        /// <summary>
        /// Resolves and returns Package Sources to the client.
        /// 
        /// Specified sources are passed in via the request object (<c>request.GetSources()</c>). 
        /// 
        /// Sources are returned using <c>request.YieldPackageSource(...)</c>
        /// </summary>
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void ResolvePackageSources(IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::ResolvePackageSources'", ProviderName);

                    
                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::ResolvePackageSources' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
        }

        /// <summary>
        /// Installs a given package.
        /// </summary>
        /// <param name="fastPackageReference">A provider supplied identifier that specifies an exact package</param>
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void InstallPackage(string fastPackageReference, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::InstallPackage' '{1}'", ProviderName, fastPackageReference);

                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::InstallPackage' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
        }


        /// <summary>
        /// Removes/Unregisters a package source
        /// </summary>
        /// <param name="name">The name or location of a package source to remove.</param>
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void RemovePackageSource(string name, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::RemovePackageSource' '{1}'", ProviderName, name);

                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::RemovePackageSource' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }

        }

        /// <summary>
        /// Uninstalls a package 
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void UninstallPackage(string fastPackageReference, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::UninstallPackage' '{1}'", ProviderName, fastPackageReference);

                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::UninstallPackage' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
        }

        /// <summary>
        /// Returns a collection of strings to the client advertizing features this provider supports.
        /// </summary>
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetFeatures(IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::GetFeatures' ", ProviderName);

                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::GetFeatures' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
        }

        /// <summary>
        /// Downloads a remote package file to a local location.
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="location"></param>
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void DownloadPackage(string fastPackageReference, string location, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::DownloadPackage' '{1}','{2}'", ProviderName, fastPackageReference, location);

                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::DownloadPackage' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
        }

        /// <summary>
        /// Returns package references for all the dependent packages
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetPackageDependencies(string fastPackageReference, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::GetPackageDependencies' '{1}'", ProviderName, fastPackageReference);

                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::GetPackageDependencies' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        public void GetPackageDetails(string fastPackageReference, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::GetPackageDetails' '{1}'", ProviderName, fastPackageReference);

                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::GetPackageDetails' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
        }

        /// <summary>
        /// Initializes a batch search request.
        /// </summary>
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        /// <returns></returns>
        public int StartFind(IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::StartFind'", ProviderName);

                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::StartFind' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }

            return default(int);
        }

        /// <summary>
        /// Finalizes a batch search request.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestObject">An object passed in from the CORE that contains functions that can be used to interact with the CORE and HOST</param>
        /// <returns></returns>
        public void CompleteFind(int id, IRequestObject requestObject) {
            try {
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::CompleteFind' '{1}'", ProviderName, id);

                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine(string.Format("Unexpected Exception thrown in '{0}::CompleteFind' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public static class ExampleConstants {

        internal const string FavoriteColorParameter = "FavoriteColor";

        internal const string SkipValidationParameter = "SkipValidation";
        
    }
}
