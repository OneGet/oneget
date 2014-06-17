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
    using System.Linq;
    using System.Management.Automation;
    using Extensions;

    public class PowerShellProviderBase : IDisposable {
        private readonly Dictionary<string, CommandInfo> _allCommands = new Dictionary<string, CommandInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, CommandInfo> _methods = new Dictionary<string, CommandInfo>(StringComparer.OrdinalIgnoreCase);
        protected PSModuleInfo _module;
        private DynamicPowershell _powershell;
        private DynamicPowershellResult _result;

        public PowerShellProviderBase(DynamicPowershell ps, PSModuleInfo module) {
            _powershell = ps;
            _module = module;

            // combine all the cmdinfos we care about
            // but normalize the keys as we go (remove any '-' '_' chars)
            foreach (var k in _module.ExportedAliases.Keys) {
                _allCommands.AddOrSet(k.Replace("-", "").Replace("_", ""), _module.ExportedFunctions[k]);
            }
            foreach (var k in _module.ExportedCmdlets.Keys) {
                _allCommands.AddOrSet(k.Replace("-", "").Replace("_", ""), _module.ExportedFunctions[k]);
            }
            foreach (var k in _module.ExportedFunctions.Keys) {
                _allCommands.AddOrSet(k.Replace("-", "").Replace("_", ""), _module.ExportedFunctions[k]);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_powershell != null) {
                    _powershell.Dispose();
                    _powershell = null;
                }
                if (_result != null) {
                    _result.Dispose();
                    _result = null;
                }
                _module = null;
            }
        }

        internal CommandInfo GetMethod(string methodName) {
            return _methods.GetOrAdd(methodName, () => {
                if (_allCommands.ContainsKey(methodName)) {
                    return _allCommands[methodName];
                }

                // try simple plurals to single
                if (methodName.EndsWith("s")) {
                    var meth = methodName.Substring(0, methodName.Length - 1);
                    if (_allCommands.ContainsKey(meth)) {
                        return _allCommands[meth];
                    }
                }

                // try words like Dependencies to Dependency
                if (methodName.EndsWith("cies")) {
                    var meth = methodName.Substring(0, methodName.Length - 4) + "cy";
                    if (_allCommands.ContainsKey(meth)) {
                        return _allCommands[meth];
                    }
                }

                // try IsFoo to Test-IsFoo 
                if (methodName.StartsWith("Is")) {
                    var meth = "test" + methodName;
                    if (_allCommands.ContainsKey(meth)) {
                        return _allCommands[meth];
                    }
                }

                // can't find one, return null.
                return null;
            });

            // hmm, it is possible to get the parameter types to match better when binding.
            // module.ExportedFunctions.FirstOrDefault().Value.Parameters.Values.First().ParameterType
        }

        internal object CallPowerShellWithoutRequest(string method, params object[] args ) {
            var cmdInfo = GetMethod(method);
            if (cmdInfo == null) {
                return null;
            }

            var result = _powershell.NewTryInvokeMemberEx(cmdInfo.Name, new string[0], args);
            if (result == null) {
                // failure! 
                throw new Exception("Powershell script/function failed.");
            }

            return result.Last();
        }

        internal object CallPowerShell(Request request, params object[] args) {
            _powershell["request"] = request;

            DynamicPowershellResult result = null;

            try {
                request.Debug("INVOKING PowerShell Fn {0} in {1}", request.CommandInfo.Name, _module.Name);
                // make sure we don't pass the callback to the function.
                result = _powershell.NewTryInvokeMemberEx(request.CommandInfo.Name, new string[0], args);

               

                // instead, loop thru results and get 
                if (result == null) {
                    // failure! 
                    throw new Exception("Powershell script/function failed.");
                }

                object finalValue = null;

                foreach (var value in result) {

                    if (result.IsFailing) {
                        foreach (var error in result.Errors) {
                            request.Debug("POWERSHELL ERROR in '{0}' in module '{1}' :\r\n{2} {3}", request.CommandInfo.Name , _module.Name, error.CategoryInfo.Category, error.Exception.Message );
                            return null;
                        }
                    }

                    var y = value as Yieldable;
                    if (y != null) {
                        y.YieldResult(request);
                    } else {
                        finalValue = value;
                    }
                }


                if (result.IsFailing) {
                    foreach (var error in result.Errors) {
                        request.Debug("POWERSHELL ERROR in '{0}' in module '{1}' :\r\n{2} {3}", request.CommandInfo.Name, _module.Name, error.CategoryInfo.Category, error.Exception.Message);
                        return null;
                    }
                }

                return finalValue;
            } catch (Exception e) {
                e.Dump();
            } finally {
                _powershell.WaitForAvailable();
                _powershell["request"] = null;
            }
            return null;
        }
    }
}