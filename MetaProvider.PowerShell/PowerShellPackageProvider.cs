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
    using System.Management.Automation;
    using Core;
    using Callback = System.Object;

    public interface IYieldable {
        void YieldResult(Request r);
    }

    public class SoftwareIdentity : IYieldable {
        public void YieldResult(Request r) {
            r.YieldPackage("", "", "", "", "", "");
        }
    }

    public class PackageSource : IYieldable {
        public void YieldResult(Request r) {
        }
    }

    public class DynamicOption : IYieldable {
        public void YieldResult(Request r) {
        }
    }

   

    internal class PowerShellPackageProvider : PowerShellProviderBase {

        public PowerShellPackageProvider(DynamicPowershell ps, PSModuleInfo module) : base(ps, module) {
        }

        public bool IsMethodImplemented(string methodName) {
            return GetMethod(methodName) != null;
        }

        #region implement PackageProvider-interface

        public void AddPackageSource(string name, string location, bool trusted, Callback c) {
            using (var request = Request.New(c, this, "AddPackageSource")) {
                if (request.IsMethodImplemented) {
                    CallPowerShell(request, name, location, trusted);
                }
            }
        }
        public bool FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Callback c) {
            using (var request = Request.New(c, this, "FindPackage")) {
                if (request.IsMethodImplemented) {
                }
            }
            return default(bool);
        }
        public bool FindPackageByFile(string file, int id, Callback c) {
            using (var request = Request.New(c, this, "FindPackageByFile")) {
                if (request.IsMethodImplemented) {
                }
            }

            return default(bool);
        }
        public bool FindPackageByUri(Uri uri, int id, Callback c) {
            using (var request = Request.New(c, this, "FindPackageByUri")) {
                if (request.IsMethodImplemented) {
                }
            }
            return default(bool);
        }
        public bool GetInstalledPackages(string name, Callback c) {
            using (var request = Request.New(c, this, "GetInstalledPackages")) {
                if (request.IsMethodImplemented) {
                }
            }

            return default(bool);
        }
        public void GetDynamicOptions(int category, Callback c) {
            using (var request = Request.New(c, this, "GetDynamicOptions")) {
                if (request.IsMethodImplemented) {
                }
            }
        }

        /// <summary>
        ///     Returns the name of the Provider. Doesn't need a callback .
        /// </summary>
        /// <required />
        /// <returns>the name of the package provider</returns>
        public string GetPackageProviderName() {

            return "modulename";
        }
        public bool GetPackageSources(Callback c) {
            using (var request = Request.New(c, this, "GetPackageSources")) {
                if (request.IsMethodImplemented) {
                }
            }
            return default(bool);
        }
        public void InitializeProvider(Callback c) {
            using (var request = Request.New(c, this, "InitializeProvider")) {
                if (request.IsMethodImplemented) {
                }
            }
        }
        public bool InstallPackage(string fastPath, Callback c) {
            using (var request = Request.New(c, this, "InstallPackage")) {
                if (request.IsMethodImplemented) {
                }
            }

            return default(bool);
        }
        public void RemovePackageSource(string name, Callback c) {
            using (var request = Request.New(c, this, "RemovePackageSource")) {
                if (request.IsMethodImplemented) {
                }
            }
        }
        public bool UninstallPackage(string fastPath, Callback c) {
            using (var request = Request.New(c, this, "UninstallPackage")) {
                if (request.IsMethodImplemented) {
                }
            }
            return default(bool);
        }
        public void GetFeatures(Callback c) {
            // TODO: Fill in implementation
            // Delete this method if you do not need to implement it
            // Please don't throw an not implemented exception, it's not optimal.
            using (var request = Request.New(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information", "Calling 'GetFeatures'");
            }
        }

        // --- Optimization features -----------------------------------------------------------------------------------------------------
        public IEnumerable<string> GetMagicSignatures() {
            // TODO: Fill in implementation
            // Delete this method if you do not need to implement it
            // Please don't throw an not implemented exception, it's not optimal.

            return default(IEnumerable<string>);
        }
        public IEnumerable<string> GetSchemes() {
            // TODO: Fill in implementation
            // Delete this method if you do not need to implement it
            // Please don't throw an not implemented exception, it's not optimal.

            return default(IEnumerable<string>);
        }
        public IEnumerable<string> GetFileExtensions() {
            // TODO: Fill in implementation
            // Delete this method if you do not need to implement it
            // Please don't throw an not implemented exception, it's not optimal.

            return default(IEnumerable<string>);
        }
        public bool GetIsSourceRequired() {
            // TODO: Fill in implementation
            // Delete this method if you do not need to implement it
            // Please don't throw an not implemented exception, it's not optimal.

            return default(bool);
        }

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        public bool DownloadPackage(string fastPath, string location, Callback c) {
            // TODO: Fill in implementation
            // Delete this method if you do not need to implement it
            // Please don't throw an not implemented exception, it's not optimal.
            using (var request = Request.New(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information", "Calling 'DownloadPackage'");
            }

            return default(bool);
        }
        public bool GetPackageDependencies(string fastPath, Callback c) {
            // TODO: Fill in implementation
            // Delete this method if you do not need to implement it
            // Please don't throw an not implemented exception, it's not optimal.
            using (var request = Request.New(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information", "Calling 'GetPackageDependencies'");
            }

            return default(bool);
        }
        public bool GetPackageDetails(string fastPath, Callback c) {
            // TODO: Fill in implementation
            // Delete this method if you do not need to implement it
            // Please don't throw an not implemented exception, it's not optimal.
            using (var request = Request.New(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information", "Calling 'GetPackageDetails'");
            }

            return default(bool);
        }

        private static int findId = 1;
        public int StartFind(Callback c) {
            lock (this) {
                return findId++;
            }
        }
        public bool CompleteFind(int id, Callback c) {
            if (id == 0) {
                return false;
            }

            // TODO: Fill in implementation
            // Delete this method if you do not need to implement it
            // Please don't throw an not implemented exception, it's not optimal.
            using (var request = Request.New(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information", "Calling 'CompleteFind'");
            }

            return true;
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