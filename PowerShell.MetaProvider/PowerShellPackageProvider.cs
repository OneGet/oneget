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

namespace Microsoft.OneGet.MetaProvider.PowerShell {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using Utility.Collections;
    using Utility.Extensions;
    using Utility.PowerShell;
    using Utility.Versions;
    using IRequestObject = System.Object;

    public class PowerShellPackageProvider : PowerShellProviderBase {
        private static int _findId = 1;
        private readonly Lazy<Dictionary<int, List<string, string, string, string>>> _findByNameBatches = new Lazy<Dictionary<int, List<string, string, string, string>>>(() => new Dictionary<int, List<string, string, string, string>>());
        private readonly Lazy<Dictionary<int, List<string>>> _findByFileBatches = new Lazy<Dictionary<int, List<string>>>(() => new Dictionary<int, List<string>>());
        private readonly Lazy<Dictionary<int, List<Uri>>> _findByUriBatches = new Lazy<Dictionary<int, List<Uri>>>(() => new Dictionary<int, List<Uri>>());

        public PowerShellPackageProvider(DynamicPowershell ps, PSModuleInfo module) : base(ps, module) {
        }

        private bool IsFirstParameterType<T>(string function) {
            var method = GetMethod(function);
            if (method == null) {
                return false;
            }

            return method.Parameters.Values.First().ParameterType == typeof (T);
        }

        public bool IsMethodImplemented(string methodName) {
            if (methodName == null) {
                throw new ArgumentNullException("methodName");
            }

            if (methodName.EqualsIgnoreCase("startfind") || methodName.EqualsIgnoreCase("completeFind")) {
                return true;
            }
#if DEEP_DEBUG
            var r = GetMethod(methodName) != null;
            if (!r) {
                Debug.WriteLine(" -> '{0}' Not Found In PowerShell Module '{1}'".format(methodName, _module.Name));
            }
            return r;
#else
            return GetMethod(methodName) != null;
#endif
        }

        private object Call(string function, IRequestObject requestObject, params object[] args) {
            using (var request = Request.New(requestObject, this, function)) {
                return request.CallPowerShell(args);
            }
        }

        #region implement PackageProvider-interface

        public void AddPackageSource(string name, string location, bool trusted, IRequestObject requestObject) {
            Call("AddPackageSource", requestObject, name, location, trusted);
        }
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, IRequestObject requestObject) {
            // special case.
            // if FindPackage is implemented taking an array of strings
            // and the id > 0 then we need to hold onto the collection until 
            // CompleteFind is called.

            // if it expects multiples...
            if (IsFirstParameterType<string[]>("FindPackage")) {
                if (id > 0) {
                    _findByNameBatches.Value.GetOrAdd(id, () => new List<string, string, string, string>()).Add(name, requiredVersion, minimumVersion, maximumVersion);
                    return;
                }

                // not passed in as a set.
                Call("FindPackage", requestObject, new string[] {
                    name
                }, requiredVersion, minimumVersion, maximumVersion);
                return;
            }

            // otherwise, it has to take them one at a time and yield them anyway.
            Call("FindPackage",requestObject, name, requiredVersion, minimumVersion, maximumVersion);
        }
        public void FindPackageByFile(string file, int id, IRequestObject requestObject) {
            // special case.
            // if FindPackageByFile is implemented taking an array of strings
            // and the id > 0 then we need to hold onto the collection until 
            // CompleteFind is called.

            // if it expects multiples...
            if (IsFirstParameterType<string[]>("FindPackageByFile")) {
                if (id > 0) {
                    _findByFileBatches.Value.GetOrAdd(id, () => new List<string>()).Add(file);
                    return;
                }
                // not passed in as a set.
                Call("FindPackageByFile",requestObject, new string[] {
                    file
                });
                return;
            }

            Call("FindPackageByFile", requestObject, file);
            return;
        }
        public void FindPackageByUri(Uri uri, int id, IRequestObject requestObject) {
            // special case.
            // if FindPackageByUri is implemented taking an array of strings
            // and the id > 0 then we need to hold onto the collection until 
            // CompleteFind is called.

            // if it expects multiples...
            if (IsFirstParameterType<string[]>("FindPackageByUri")) {
                if (id > 0) {
                    _findByUriBatches.Value.GetOrAdd(id, () => new List<Uri>()).Add(uri);
                    return;
                }
                // not passed in as a set.
                Call("FindPackageByUri",requestObject, new Uri[] {
                    uri
                });
                return;
            }

            Call("FindPackageByUri",requestObject, uri);
        }
        public void GetInstalledPackages(string name, IRequestObject requestObject) {
            Call("GetInstalledPackages",requestObject, name);
        }

        public void GetDynamicOptions(string category, IRequestObject requestObject) {
            OptionCategory cat;
            if (Enum.TryParse(category ?? "", true, out cat)) {
                // if this version of the plugin doesn't support that category
                // there's no point in trying to get options from that type.
                Call("GetDynamicOptions", requestObject, cat);
            }
        }

        private string _providerName;
        /// <summary>
        ///     Returns the name of the Provider. 
        /// </summary>
        /// <required />
        /// <returns>the name of the package provider</returns>
        public string GetPackageProviderName() {
            return _providerName ?? (_providerName = (string)CallPowerShellWithoutRequest("GetPackageProviderName"));
        }
        public void ResolvePackageSources(IRequestObject requestObject) {
            Call("ResolvePackageSources", requestObject);
        }

        public void InitializeProvider(IRequestObject requestObject) {
            Call("InitializeProvider", requestObject);
        }

        public string GetProviderVersion() {
            var result= (string)CallPowerShellWithoutRequest("GetProviderVersion");
            if (string.IsNullOrEmpty(result)) {

                if (_module.Version != new Version(0, 0, 0, 0)) {
                    result = _module.Version.ToString();
                } else {
                    try {
                        // use the latest date as a version number
                        return (FourPartVersion) _module.FileList.Max(each => new FileInfo(each).LastWriteTime);
                    } catch {
                        // I give up. 
                        return "0.0.0.1";
                    }
                }
            }
            return result;
        }
        public void InstallPackage(string fastPath, IRequestObject requestObject) {
            Call("InstallPackage", requestObject, fastPath);
        }
        public void RemovePackageSource(string name, IRequestObject requestObject) {
            Call("RemovePackageSource", requestObject, name);
        }
        public void UninstallPackage(string fastPath, IRequestObject requestObject) {
            Call("UninstallPackage", requestObject, fastPath);
        }
        public void GetFeatures(IRequestObject requestObject) {
            Call("GetFeatures", requestObject);
        }

        // --- Optimization features -----------------------------------------------------------------------------------------------------
        public bool GetIsSourceRequired() {
            // TODO: Fill in implementation
            // Delete this method if you do not need to implement it
            // Please don't throw an not implemented exception, it's not optimal.

            return default(bool);
        }

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        public void DownloadPackage(string fastPath, string location, IRequestObject requestObject) {
            Call("DownloadPackage", requestObject, fastPath, location);
        }
        public void GetPackageDependencies(string fastPath, IRequestObject requestObject) {
            Call("GetPackageDependencies", requestObject, fastPath);
        }
        public void GetPackageDetails(string fastPath, IRequestObject requestObject) {
            Call("GetPackageDetails", requestObject, fastPath);
        }
        public int StartFind(IRequestObject requestObject) {
            lock (this) {
                return ++_findId;
            }
        }
        public void CompleteFind(int id, IRequestObject requestObject) {
            if (id < 1) {
                return;
            }

            if (_findByNameBatches.IsValueCreated) {
                var nameBatch = _findByNameBatches.Value.TryPullValue(id);
                if (nameBatch != null) {
                    if (IsFirstParameterType<string[]>("FindPackage")) {
                        // it takes a batch at a time.

                        var names = nameBatch.Select(each => each.Item1).ToArray();
                        var p1 = nameBatch[0];

                        Call("FindPackage", requestObject,names, p1.Item2, p1.Item3, p1.Item4);
                    } else {
                        foreach (var each in nameBatch) {
                            Call("FindPackage",requestObject, each.Item1, each.Item2, each.Item3, each.Item4);
                        }
                    }
                }
            }

            if (_findByFileBatches.IsValueCreated) {
                var fileBatch = _findByFileBatches.Value.TryPullValue(id);
                if (fileBatch != null) {
                    if (IsFirstParameterType<string[]>("FindPackageByFile")) {
                        // it takes a batch at a time.
                        Call("FindPackageByFile", requestObject, new object[] {
                            fileBatch.ToArray()
                        });
                    } else {
                        foreach (var each in fileBatch) {
                            Call("FindPackageByFile",requestObject, each);
                        }
                    }
                }
            }

            if (_findByUriBatches.IsValueCreated) {
                var uriBatch = _findByUriBatches.Value.TryPullValue(id);
                if (uriBatch != null) {
                    if (IsFirstParameterType<string[]>("FindPackageByUri")) {
                        // it takes a batch at a time.
                        Call("FindPackageByUri",requestObject, new object[] {uriBatch.ToArray()});
                    } else {
                        foreach (var each in uriBatch) {
                            Call("FindPackageByUri",requestObject, each);
                        }
                    }
                }
            }
        }
        public void ExecuteElevatedAction(string payload, IRequestObject requestObject){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.

        }

        #endregion

    }
}