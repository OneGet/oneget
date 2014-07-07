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
    using System.Linq;
    using System.Management.Automation;
    using Collections;
    using Extensions;
    using Providers.Package;
    using Callback = System.Object;

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
#if DEBUG
            var r = GetMethod(methodName) != null;
            if (!r) {
                Debug.WriteLine(" -> '{0}' Not Found In PowerShell Module '{1}'".format(methodName, _module.Name));
            }
            return r;
#else
            return GetMethod(methodName) != null;
#endif
        }

        private object Call(string function, Callback c, params object[] args) {
            using (var request = Request.New(c, this, function)) {
                return request.CallPowerShell(args);
            }
        }

        #region implement PackageProvider-interface

        public void AddPackageSource(string name, string location, bool trusted, Object c) {
            Call("AddPackageSource", c, name, location, trusted);
        }
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Object c) {
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
                Call("FindPackage", c, new string[] {
                    name
                }, requiredVersion, minimumVersion, maximumVersion);
                return;
            }

            // otherwise, it has to take them one at a time and yield them anyway.
            Call("FindPackage",c, name, requiredVersion, minimumVersion, maximumVersion);
        }
        public void FindPackageByFile(string file, int id, Object c) {
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
                Call("FindPackageByFile",c, new string[] {
                    file
                });
                return;
            }

            Call("FindPackageByFile", c, file);
            return;
        }
        public void FindPackageByUri(Uri uri, int id, Object c) {
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
                Call("FindPackageByUri",c, new Uri[] {
                    uri
                });
                return;
            }

            Call("FindPackageByUri",c, uri);
        }
        public void GetInstalledPackages(string name, Object c) {
            Call("GetInstalledPackages",c, name);
        }
        public void GetDynamicOptions(int category, Callback c) {
            Call("GetDynamicOptions",c, (OptionCategory)category);
        }

        /// <summary>
        ///     Returns the name of the Provider. Doesn't need a callback .
        /// </summary>
        /// <required />
        /// <returns>the name of the package provider</returns>
        public string GetPackageProviderName() {
            return (string)CallPowerShellWithoutRequest("GetPackageProviderName");
        }
        public void ResolvePackageSources(Object c) {
            Call("ResolvePackageSources", c);
        }

        public void InitializeProvider(object dynamicInterface, Callback c) {
            Call("InitializeProvider", c);
        }
        public void InstallPackage(string fastPath, Object c) {
            Call("InstallPackage", c, fastPath);
        }
        public void RemovePackageSource(string name, Object c) {
            Call("RemovePackageSource", c, name);
        }
        public void UninstallPackage(string fastPath, Object c) {
            Call("UninstallPackage", c, fastPath);
        }
        public void GetFeatures(Callback c) {
            Call("GetFeatures", c);
        }

        // --- Optimization features -----------------------------------------------------------------------------------------------------
        public bool GetIsSourceRequired() {
            // TODO: Fill in implementation
            // Delete this method if you do not need to implement it
            // Please don't throw an not implemented exception, it's not optimal.

            return default(bool);
        }

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        public void DownloadPackage(string fastPath, string location, Object c) {
            Call("DownloadPackage", c, fastPath, location);
        }
        public void GetPackageDependencies(string fastPath, Object c) {
            Call("GetPackageDependencies", c, fastPath);
        }
        public void GetPackageDetails(string fastPath, Object c) {
            Call("GetPackageDetails", c, fastPath);
        }
        public int StartFind(Object c) {
            lock (this) {
                return ++_findId;
            }
        }
        public void CompleteFind(int id, Object c) {
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

                        Call("FindPackage", c,names, p1.Item2, p1.Item3, p1.Item4);
                    } else {
                        foreach (var each in nameBatch) {
                            Call("FindPackage",c, each.Item1, each.Item2, each.Item3, each.Item4);
                        }
                    }
                }
            }

            if (_findByFileBatches.IsValueCreated) {
                var fileBatch = _findByFileBatches.Value.TryPullValue(id);
                if (fileBatch != null) {
                    if (IsFirstParameterType<string[]>("FindPackageByFile")) {
                        // it takes a batch at a time.
                        Call("FindPackageByFile", c, new object[] {
                            fileBatch.ToArray()
                        });
                    } else {
                        foreach (var each in fileBatch) {
                            Call("FindPackageByFile",c, each);
                        }
                    }
                }
            }

            if (_findByUriBatches.IsValueCreated) {
                var uriBatch = _findByUriBatches.Value.TryPullValue(id);
                if (uriBatch != null) {
                    if (IsFirstParameterType<string[]>("FindPackageByUri")) {
                        // it takes a batch at a time.
                        Call("FindPackageByUri",c, new object[] {uriBatch.ToArray()});
                    } else {
                        foreach (var each in uriBatch) {
                            Call("FindPackageByUri",c, each);
                        }
                    }
                }
            }
        }

        #endregion

#if DYNAMIC_DELEGATE_WAY
        public Delegate CreateDelegate(string method, string[] pNames, Type[] pTypes, Type returnType) {
            var fnName = MatchFnName(method);

            if (string.IsNullOrEmpty(fnName)) {
                // no match found, return null.
                return null;
            }

            var targetDelegateType = WrappedDelegate.GetFuncOrActionType(pTypes, returnType);
            var arbitraryDelelgateType = ArbitraryDelegate.GetFuncOrActionType(pTypes, returnType);

            var cmd = Activator.CreateInstance(arbitraryDelelgateType, new Func<object[], object>((args) => {
                // put the request object in
                if (args.Length > 0) {
                    var callback = args.Last() as Callback;
                    if (callback != null) {
                        // if the last argument is a Callback delegate
                        // let's generate the Request object 
                        // 
                        using (var request = new Request(callback)) {
                            _powershell["request"] = request;

                            try {
                                // make sure we don't pass the callback to the function.
                                var result = _powershell.NewTryInvokeMemberEx(fnName, new string[0], args.Take(args.Length - 1));

                                // instead, loop thru results and get 
                                if (result == null) {
                                    // failure! 
                                    throw new Exception("Powershell script/function failed.");
                                }

                                object finalValue = null;

                                foreach (var value in result) {
                                    var y = value as IYieldable;
                                    if (y != null) {
                                        y.YieldResult(request);
                                    } else {
                                        finalValue = result;
                                    }
                                }
                                return finalValue;

                            } catch (Exception e) {
                                e.Dump();
                            } finally {
                                _powershell["request"] = null;
                            }
                        }
                    }
                }

                // otherwise, if there is no callback, or no args, just call it directly.
                return _powershell.NewTryInvokeMemberEx(fnName, new string[0], args);
            }));

            return Delegate.CreateDelegate(targetDelegateType, cmd, "Invoke");
        }
#endif
    }
}